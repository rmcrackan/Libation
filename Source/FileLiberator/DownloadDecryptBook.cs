using AaxDecrypter;
using ApplicationServices;
using AudibleApi.Common;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;
using FileManager;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace FileLiberator
{
	public class DownloadDecryptBook : AudioDecodable
	{
		public override string Name => "Download & Decrypt";
		private CancellationTokenSource? cancellationTokenSource;
		private AudiobookDownloadBase? abDownloader;

		/// <summary>
		/// Optional override to supply license info directly instead of querying the api based on Configuration options
		/// </summary>
		public DownloadOptions.LicenseInfo? LicenseInfo { get; set; }

		public override bool Validate(LibraryBook libraryBook) => !libraryBook.Book.Audio_Exists();
		public override async Task CancelAsync()
		{
			if (abDownloader is not null) await abDownloader.CancelAsync();
			if (cancellationTokenSource is not null) await cancellationTokenSource.CancelAsync();
		}

		public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
		{
			OnBegin(libraryBook);
			cancellationTokenSource = new CancellationTokenSource();
			var cancellationToken = cancellationTokenSource.Token;

			try
			{
				if (libraryBook.Book.Audio_Exists())
					return new StatusHandler { "Cannot find decrypt. Final audio file already exists" };

				DownloadValidation(libraryBook);

				var api = await libraryBook.GetApiAsync();

				LicenseInfo ??= await DownloadOptions.GetDownloadLicenseAsync(api, libraryBook, Configuration.Instance, cancellationToken);
				using var downloadOptions = DownloadOptions.BuildDownloadOptions(libraryBook, Configuration.Instance, LicenseInfo);
				var result = await DownloadAudiobookAsync(api, downloadOptions, cancellationToken);

				if (!result.Success || getFirstAudioFile(result.ResultFiles) is not TempFile audioFile)
				{
					// decrypt failed. Delete all output entries but leave the cache files.
					result.ResultFiles.ForEach(f => FileUtility.SaferDelete(f.FilePath));
					cancellationToken.ThrowIfCancellationRequested();
					return new StatusHandler { "Decrypt failed" };
				}

				if (Configuration.Instance.RetainAaxFile)
				{
					//Add the cached aaxc and key files to the entries list to be moved to the Books directory.
					result.ResultFiles.AddRange(getAaxcFiles(result.CacheFiles));
				}

				//Set the last downloaded information on the book so that it can be used in the naming templates,
				//but don't persist it until everything completes successfully (in the finally block)
				var audioFormat = GetFileFormatInfo(downloadOptions, audioFile);
				var audioVersion = downloadOptions.ContentMetadata.ContentReference.Version;
				libraryBook.Book.UserDefinedItem.SetLastDownloaded(Configuration.LibationVersion, audioFormat, audioVersion);

				var finalStorageDir = getDestinationDirectory(libraryBook);

				//post-download tasks done in parallel.
				var moveFilesTask = Task.Run(() => MoveFilesToBooksDir(libraryBook, finalStorageDir, result.ResultFiles, cancellationToken));
				Task[] finalTasks =
				[
					moveFilesTask,
					Task.Run(() => DownloadCoverArt(finalStorageDir, downloadOptions, cancellationToken)),
					Task.Run(() => DownloadRecordsAsync(api, finalStorageDir, downloadOptions, cancellationToken)),
					Task.Run(() => DownloadMetadataAsync(api, finalStorageDir, downloadOptions, cancellationToken)),
					Task.Run(() => WindowsDirectory.SetCoverAsFolderIcon(libraryBook.Book.PictureId, finalStorageDir, cancellationToken))
				];

				try
				{
					await Task.WhenAll(finalTasks);
				}
				catch when (!moveFilesTask.IsFaulted)
				{
					//Swallow DownloadCoverArt, DownloadRecordsAsync, DownloadMetadataAsync, and SetCoverAsFolderIcon exceptions.
					//Only fail if the downloaded audio files failed to move to Books directory
				}
				finally
				{
					if (moveFilesTask.IsCompletedSuccessfully && !cancellationToken.IsCancellationRequested)
					{
						await libraryBook.UpdateBookStatusAsync(LiberatedStatus.Liberated, Configuration.LibationVersion, audioFormat, audioVersion);
						SetDirectoryTime(libraryBook, finalStorageDir);
						foreach (var cacheFile in result.CacheFiles.Where(f => File.Exists(f.FilePath)))
						{
							//Delete cache files only after the download/decrypt operation completes successfully.
							FileUtility.SaferDelete(cacheFile.FilePath);
						}
					}
				}

				return new StatusHandler();
			}
			catch when (cancellationToken.IsCancellationRequested)
			{
				Serilog.Log.Logger.Information("Download/Decrypt was cancelled. {@Book}", libraryBook.LogFriendly());
				return new StatusHandler { "Cancelled" };
			}
			finally
			{
				OnCompleted(libraryBook);
				cancellationTokenSource.Dispose();
				cancellationTokenSource = null;
			}
		}

		private record AudiobookDecryptResult(bool Success, List<TempFile> ResultFiles, List<TempFile> CacheFiles);

		private async Task<AudiobookDecryptResult> DownloadAudiobookAsync(AudibleApi.Api api, DownloadOptions dlOptions, CancellationToken cancellationToken)
		{
			var outpoutDir = AudibleFileStorage.DecryptInProgressDirectory;
			var cacheDir = AudibleFileStorage.DownloadsInProgressDirectory;
			var result = new AudiobookDecryptResult(false, [], []);

			try
			{
				if (dlOptions.DrmType is not DrmType.Adrm and not DrmType.Widevine)
					abDownloader = new UnencryptedAudiobookDownloader(outpoutDir, cacheDir, dlOptions);
				else
				{
					AaxcDownloadConvertBase converter
						= dlOptions.Config.SplitFilesByChapter && dlOptions.ChapterInfo.Count > 1 ?
						new AaxcDownloadMultiConverter(outpoutDir, cacheDir, dlOptions) :
						new AaxcDownloadSingleConverter(outpoutDir, cacheDir, dlOptions);

					if (dlOptions.Config.AllowLibationFixup)
						converter.RetrievedMetadata += Converter_RetrievedMetadata;

					abDownloader = converter;
				}

				abDownloader.DecryptProgressUpdate += OnStreamingProgressChanged;
				abDownloader.DecryptTimeRemaining += OnStreamingTimeRemaining;
				abDownloader.RetrievedTitle += OnTitleDiscovered;
				abDownloader.RetrievedAuthors += OnAuthorsDiscovered;
				abDownloader.RetrievedNarrators += OnNarratorsDiscovered;
				abDownloader.RetrievedCoverArt += AaxcDownloader_RetrievedCoverArt;
				abDownloader.TempFileCreated += AbDownloader_TempFileCreated;

				// REAL WORK DONE HERE
				bool success = await abDownloader.RunAsync();
				return result with { Success = success };
			}
			catch (Exception ex)
			{
				if (!cancellationToken.IsCancellationRequested)
					Serilog.Log.Logger.Error(ex, "Error downloading audiobook {@Book}", dlOptions.LibraryBook.LogFriendly());
				//don't throw any exceptions so the caller can delete any temp files.
				return result;
			}
			finally
			{
				OnStreamingProgressChanged(new() { ProgressPercentage = 100 });
			}

			void AbDownloader_TempFileCreated(object? sender, TempFile e)
			{
				if (Path.GetDirectoryName(e.FilePath) == outpoutDir)
				{
					result.ResultFiles.Add(e);
				}
				else if (Path.GetDirectoryName(e.FilePath) == cacheDir)
				{
					result.CacheFiles.Add(e);
					// Notify that the aaxc file has been created so that
					// the UI can know about partially-downloaded files
					if (getFileType(e) is FileType.AAXC)
						OnFileCreated(dlOptions.LibraryBook, e.FilePath);
				}
			}
		}

		#region Decryptor event handlers
		private void Converter_RetrievedMetadata(object? sender, AAXClean.AppleTags tags)
		{
			if (sender is not AaxcDownloadConvertBase converter ||
				converter.AaxFile is not AAXClean.Mp4File aaxFile ||
				converter.DownloadOptions is not DownloadOptions options ||
				options.ChapterInfo.Chapters is not List<AAXClean.Chapter> chapters)
				return;

			#region Prevent erroneous truncation due to incorrect chapter info

			//Sometimes the chapter info is not accurate. Since AAXClean trims audio
			//files to the chapters start and end, if the last chapter's end time is
			//before the end of the audio file, the file will be truncated to match
			//the chapter. This is never desirable, so pad the last chapter to match
			//the original audio length.

			var fileDuration = aaxFile.Duration;
			if (options.Config.StripAudibleBrandAudio)
				fileDuration -= TimeSpan.FromMilliseconds(options.ContentMetadata.ChapterInfo.BrandOutroDurationMs);

			var durationDelta = fileDuration - options.ChapterInfo.EndOffset;
			//Remove the last chapter and re-add it with the durationDelta that will
			//make the chapter's end coincide with the end of the audio file.
			var lastChapter = chapters[^1];

			chapters.Remove(lastChapter);
			options.ChapterInfo.Add(lastChapter.Title, lastChapter.Duration + durationDelta);
			
			#endregion

			tags.Title ??= options.LibraryBookDto.TitleWithSubtitle;
			tags.Album ??= tags.Title;
			tags.Artist ??= string.Join("; ", options.LibraryBook.Book.Authors.Select(a => a.Name));
			tags.AlbumArtists ??= tags.Artist;
			tags.Generes = string.Join(", ", options.LibraryBook.Book.LowestCategoryNames());
			tags.ProductID ??= options.ContentMetadata.ContentReference.Sku;
			tags.Comment ??= options.LibraryBook.Book.Description;
			tags.LongDescription ??= tags.Comment;
			tags.Publisher ??= options.LibraryBook.Book.Publisher;
			tags.Narrator ??= string.Join("; ", options.LibraryBook.Book.Narrators.Select(n => n.Name));
			tags.Asin = options.LibraryBook.Book.AudibleProductId;
			tags.Acr = options.ContentMetadata.ContentReference.Acr;
			tags.Version = options.ContentMetadata.ContentReference.Version;
			if (options.LibraryBook.Book.DatePublished is DateTime pubDate)
			{
				tags.Year ??= pubDate.Year.ToString();
				tags.ReleaseDate ??= pubDate.ToString("dd-MMM-yyyy");
			}
		}

		private void AaxcDownloader_RetrievedCoverArt(object? sender, byte[]? e)
		{
			if (Configuration.Instance.AllowLibationFixup && sender is AaxcDownloadConvertBase downloader)
			{
				try
				{
					e = OnRequestCoverArt();
					downloader.SetCoverArt(e);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Failed to retrieve cover art from server.");
				}
			}

			if (e is not null)
				OnCoverImageDiscovered(e);
		}
		#endregion

		#region Validation

		private static void DownloadValidation(LibraryBook libraryBook)
		{
			string errorString(string field)
				=> $"{errorTitle()}\r\nCannot download book. {field} is not known. Try re-importing the account which owns this book.";

			string errorTitle()
			{
				var title
					= (libraryBook.Book.TitleWithSubtitle.Length > 53)
					? $"{libraryBook.Book.TitleWithSubtitle.Truncate(50)}..."
					: libraryBook.Book.TitleWithSubtitle;
				var errorBookTitle = $"{title} [{libraryBook.Book.AudibleProductId}]";
				return errorBookTitle;
			};

			if (string.IsNullOrWhiteSpace(libraryBook.Account))
				throw new InvalidOperationException(errorString("Account"));

			if (string.IsNullOrWhiteSpace(libraryBook.Book.Locale))
				throw new InvalidOperationException(errorString("Locale"));
		}
		#endregion

		#region Post-success routines
		/// <summary>Read the audio format from the audio file's metadata.</summary>
		public AudioFormat GetFileFormatInfo(DownloadOptions options, TempFile firstAudioFile)
		{
			try
			{
				return firstAudioFile.Extension.ToLowerInvariant() switch
				{
					".m4b" or ".m4a" or ".mp4" => GetMp4AudioFormat(),
					".mp3" => AudioFormatDecoder.FromMpeg3(firstAudioFile.FilePath),
					_ => AudioFormat.Default
				};
			}
			catch (Exception ex)
			{
				//Failure to determine output audio format should not be considered a failure to download the book
				Serilog.Log.Logger.Error(ex, "Error determining output audio format for {@Book}. File = '{@audioFile}'", options.LibraryBook, firstAudioFile);
				return AudioFormat.Default;
			}

			AudioFormat GetMp4AudioFormat()
				=> abDownloader is AaxcDownloadConvertBase converter && converter.AaxFile is AAXClean.Mp4File mp4File
				? AudioFormatDecoder.FromMpeg4(mp4File)
				: AudioFormatDecoder.FromMpeg4(firstAudioFile.FilePath);
		}

		/// <summary>Move new files to 'Books' directory</summary>
		/// <returns>Return directory if audiobook file(s) were successfully created and can be located on disk. Else null.</returns>
		private void MoveFilesToBooksDir(LibraryBook libraryBook, LongPath destinationDir, List<TempFile> entries, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			AverageSpeed averageSpeed = new();

			var totalSizeToMove = entries.Sum(f => new FileInfo(f.FilePath).Length);
			long totalBytesMoved = 0;

			for (var i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];

				var destFileName
					= AudibleFileStorage.Audio.GetCustomDirFilename(
						libraryBook,
						destinationDir,
						entry.Extension,
						entry.PartProperties,
						Configuration.Instance.OverwriteExisting);

				var realDest
					= FileUtility.SaferMoveToValidPath(
						entry.FilePath,
						destFileName,
						Configuration.Instance.ReplacementCharacters,
						entry.Extension,
						Configuration.Instance.OverwriteExisting);

				#region File Move Progress
				totalBytesMoved += new FileInfo(realDest).Length;
				averageSpeed.AddPosition(totalBytesMoved);
				var estSecsRemaining = (totalSizeToMove - totalBytesMoved) / averageSpeed.Average;

				if (double.IsNormal(estSecsRemaining))
					OnStreamingTimeRemaining(TimeSpan.FromSeconds(estSecsRemaining));

				OnStreamingProgressChanged(new DownloadProgress
					{
						ProgressPercentage = 100d * totalBytesMoved / totalSizeToMove,
						BytesReceived = totalBytesMoved,
						TotalBytesToReceive = totalSizeToMove
					});
				#endregion

				// propagate corrected path for cue file (after this for-loop)
				entries[i] = entry with { FilePath = realDest };

				SetFileTime(libraryBook, realDest);
				OnFileCreated(libraryBook, realDest);
				cancellationToken.ThrowIfCancellationRequested();
			}

			if (entries.FirstOrDefault(f => getFileType(f) is FileType.Cue) is TempFile cue
				&& getFirstAudioFile(entries)?.FilePath is LongPath audioFilePath)
			{
				Cue.UpdateFileName(cue.FilePath, audioFilePath);
				SetFileTime(libraryBook, cue.FilePath);
			}

			cancellationToken.ThrowIfCancellationRequested();
			AudibleFileStorage.Audio.Refresh();
		}

		private void DownloadCoverArt(LongPath destinationDir, DownloadOptions options, CancellationToken cancellationToken)
		{
			if (!options.Config.DownloadCoverArt) return;

			var coverPath = "[null]";

			try
			{
				coverPath
					= AudibleFileStorage.Audio.GetCustomDirFilename(
						options.LibraryBook,
						destinationDir,
						extension: ".jpg",
						returnFirstExisting: Configuration.Instance.OverwriteExisting);

				if (File.Exists(coverPath))
					FileUtility.SaferDelete(coverPath);

				var picBytes = PictureStorage.GetPictureSynchronously(new(options.LibraryBook.Book.PictureLarge ?? options.LibraryBook.Book.PictureId, PictureSize.Native), cancellationToken);
				if (picBytes.Length > 0)
				{
					File.WriteAllBytes(coverPath, picBytes);
					SetFileTime(options.LibraryBook, coverPath);
					OnFileCreated(options.LibraryBook, coverPath);
				}
			}
			catch (Exception ex)
			{
				//Failure to download cover art should not be considered a failure to download the book
				if (!cancellationToken.IsCancellationRequested)
					Serilog.Log.Logger.Error(ex, "Error downloading cover art for {@Book} to {@metadataFile}.", options.LibraryBook, coverPath);
				throw;
			}
		}

		public async Task DownloadRecordsAsync(AudibleApi.Api api, LongPath destinationDir, DownloadOptions options, CancellationToken cancellationToken)
		{
			if (!options.Config.DownloadClipsBookmarks) return;

			var recordsPath = "[null]";
			var format = options.Config.ClipsBookmarksFileFormat;
			var formatExtension = FileUtility.GetStandardizedExtension(format.ToString().ToLowerInvariant());

			try
			{
				recordsPath
					= AudibleFileStorage.Audio.GetCustomDirFilename(
						options.LibraryBook,
						destinationDir,
						extension: formatExtension,
						returnFirstExisting: Configuration.Instance.OverwriteExisting);

				if (File.Exists(recordsPath))
					FileUtility.SaferDelete(recordsPath);

				var records = await api.GetRecordsAsync(options.AudibleProductId);

				switch (format)
				{
					case Configuration.ClipBookmarkFormat.CSV:
						RecordExporter.ToCsv(recordsPath, records);
						break;
					case Configuration.ClipBookmarkFormat.Xlsx:
						RecordExporter.ToXlsx(recordsPath, records);
						break;
					case Configuration.ClipBookmarkFormat.Json:
						RecordExporter.ToJson(recordsPath, options.LibraryBook, records);
						break;
					default:
						throw new NotSupportedException($"Unsupported record export format: {format}");
				}

				SetFileTime(options.LibraryBook, recordsPath);
				OnFileCreated(options.LibraryBook, recordsPath);
			}
			catch (Exception ex)
			{
				//Failure to download records should not be considered a failure to download the book
				if (!cancellationToken.IsCancellationRequested)
					Serilog.Log.Logger.Error(ex, "Error downloading clips and bookmarks for {@Book} to {@recordsPath}.", options.LibraryBook, recordsPath);
				throw;
			}
		}

		private async Task DownloadMetadataAsync(AudibleApi.Api api, LongPath destinationDir, DownloadOptions options, CancellationToken cancellationToken)
		{
			if (!options.Config.SaveMetadataToFile) return;

			string metadataPath = "[null]";

			try
			{
				metadataPath
					= AudibleFileStorage.Audio.GetCustomDirFilename(
						options.LibraryBook,
						destinationDir,
						extension: ".metadata.json",
						returnFirstExisting: Configuration.Instance.OverwriteExisting);

				if (File.Exists(metadataPath))
					FileUtility.SaferDelete(metadataPath);

				var item = await api.GetCatalogProductAsync(options.LibraryBook.Book.AudibleProductId, AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
				item.SourceJson.Add(nameof(ContentMetadata.ChapterInfo), Newtonsoft.Json.Linq.JObject.FromObject(options.ContentMetadata.ChapterInfo));
				item.SourceJson.Add(nameof(ContentMetadata.ContentReference), Newtonsoft.Json.Linq.JObject.FromObject(options.ContentMetadata.ContentReference));

				cancellationToken.ThrowIfCancellationRequested();
				File.WriteAllText(metadataPath, item.SourceJson.ToString());
				SetFileTime(options.LibraryBook, metadataPath);
				OnFileCreated(options.LibraryBook, metadataPath);
			}
			catch (Exception ex)
			{
				//Failure to download metadata should not be considered a failure to download the book
				if (!cancellationToken.IsCancellationRequested)
					Serilog.Log.Logger.Error(ex, "Error downloading metdatat of {@Book} to {@metadataFile}.", options.LibraryBook, metadataPath);
				throw;
			}
		}
		#endregion

		#region Macros
		private static string getDestinationDirectory(LibraryBook libraryBook)
		{
			var destinationDir = AudibleFileStorage.Audio.GetDestinationDirectory(libraryBook);
			if (!Directory.Exists(destinationDir))
				Directory.CreateDirectory(destinationDir);
			return destinationDir;
		}

		private static FileType getFileType(TempFile file)
			=> FileTypes.GetFileTypeFromPath(file.FilePath);
		private static TempFile? getFirstAudioFile(IEnumerable<TempFile> entries)
			=> entries.FirstOrDefault(f => File.Exists(f.FilePath) && getFileType(f) is FileType.Audio);
		private static IEnumerable<TempFile> getAaxcFiles(IEnumerable<TempFile> entries)
			=> entries.Where(f => File.Exists(f.FilePath) && (getFileType(f) is FileType.AAXC || f.Extension.Equals(".key", StringComparison.OrdinalIgnoreCase)));
		#endregion
	}
}
