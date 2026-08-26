using AaxDecrypter;
using ApplicationServices;
using AudibleApi.Common;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;
using FileManager;
using LibationFileManager;
using LibationFileManager.Templates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileLiberator;

public class DownloadDecryptBook : AudioDecodable, IProcessable<DownloadDecryptBook>, ILicensedDownload
{
	public override string Name => "Download & Decrypt";
	private CancellationTokenSource? cancellationTokenSource;
	private AudiobookDownloadBase? abDownloader;

	/// <inheritdoc/>
	public DownloadOptions.LicenseInfo? LicenseInfo { get; set; }

	/// <inheritdoc/>
	public DownloadOptions.LicenseInfo? ObtainedLicense { get; private set; }

	public override bool Validate(LibraryBook libraryBook) => !libraryBook.Book.AudioExists;
	protected override bool RecordsAttemptFailures => true;

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
		ObtainedLicense = null;

		try
		{
			if (libraryBook.Book.AudioExists)
				return new StatusHandler { "Cannot find decrypt. Final audio file already exists" };

			DownloadValidation(libraryBook);

			var api = await libraryBook.GetApiAsync();

			//Processable instances are reusable, so don't set LicenseInfo
			//override from within a DownloadDecryptBook instance.
			var license = LicenseInfo ?? await DownloadOptions.GetDownloadLicenseAsync(api, libraryBook, Configuration, cancellationToken);

			// Published before the download begins so the supplement step gets it even from a run that fails
			// later on: what it needs is the license, and this one is already granted.
			ObtainedLicense = license;

			using var downloadOptions = DownloadOptions.BuildDownloadOptions(libraryBook, Configuration, license);
			var result = await DownloadAudiobookAsync(api, downloadOptions, cancellationToken);

			if (!result.Success || getFirstAudioFile(result.ResultFiles) is not TempFile audioFile)
			{
				// decrypt failed. Delete all output entries but leave the cache files.
				result.ResultFiles.ForEach(f => FileUtility.SaferDelete(f.FilePath));
				cancellationToken.ThrowIfCancellationRequested();
				return new StatusHandler { "Decrypt failed" };
			}

			if (Configuration.RetainAaxFile)
			{
				//Add the cached aaxc and key files to the entries list to be moved to the Books directory.
				result.ResultFiles.AddRange(getAaxcFiles(result.CacheFiles));
			}

			//Set the last downloaded information on the book so that it can be used in the naming templates,
			//but don't persist it until everything completes successfully (in the finally block)
			Serilog.Log.Verbose("Setting last downloaded info for {@Book}", libraryBook.LogFriendly());
			var audioFormat = GetFileFormatInfo(downloadOptions, audioFile);
			var audioVersion = downloadOptions.ContentMetadata.ContentReference.Version;
			libraryBook.Book.UserDefinedItem.SetLastDownloaded(Configuration.LibationVersion, audioFormat, audioVersion);

			//Verbose logging inside getDestinationDirectory
			var finalStorageDir = getDestinationDirectory(libraryBook);

			// Post-download tasks (parallel). Folder icon runs afterward so it does not race cover/metadata work
			// or a still-warming Amazon image cache for the 300x300 asset.
			Serilog.Log.Verbose("Starting post-liberation finalization tasks");
			var moveFilesTask = Task.Run(() => MoveFilesToBooksDir(libraryBook, finalStorageDir, result.ResultFiles, cancellationToken));
			Task[] finalTasks =
			[
				moveFilesTask,
				Task.Run(() => DownloadCoverArt(finalStorageDir, downloadOptions, cancellationToken)),
				Task.Run(() => DownloadRecordsAsync(api, finalStorageDir, downloadOptions, cancellationToken)),
				Task.Run(() => DownloadMetadataAsync(api, finalStorageDir, downloadOptions, cancellationToken)),
			];

			try
			{
				Serilog.Log.Verbose("Awaiting post-liberation finalization tasks");
				await Task.WhenAll(finalTasks);
			}
			catch (Exception ex)
			{
				Serilog.Log.Verbose(ex, "An error occurred in the post-liberation finalization tasks");
				//Swallow DownloadCoverArt, DownloadRecordsAsync, and DownloadMetadataAsync exceptions.
				//Only fail if the downloaded audio files failed to move to Books directory
				if (moveFilesTask.IsFaulted)
					throw;
			}

			if (moveFilesTask.IsCompletedSuccessfully && !cancellationToken.IsCancellationRequested)
			{
				// Same picture key as DownloadCoverArt so we hit the same Amazon asset (and Images cache) the parallel task likely just populated.
				var folderIconPictureId = libraryBook.Book.PictureLarge ?? libraryBook.Book.PictureId;
				await Task.Run(() => WindowsDirectory.SetCoverAsFolderIcon(folderIconPictureId, finalStorageDir, cancellationToken), cancellationToken);

				Serilog.Log.Verbose("Updating liberated status for {@Book}", libraryBook.LogFriendly());
				await libraryBook.UpdateBookStatusAsync(LiberatedStatus.Liberated, Configuration.LibationVersion, audioFormat, audioVersion);
				RecordDownloadForDailyLimit(libraryBook, result.ResultFiles);
				Serilog.Log.Verbose("Setting directory time for {@Book}", libraryBook.LogFriendly());
				SetDirectoryTime(libraryBook, finalStorageDir);
				Serilog.Log.Verbose("Deleting cache files for {@Book}", libraryBook.LogFriendly());
				foreach (var cacheFile in result.CacheFiles.Where(f => File.Exists(f.FilePath)))
				{
					//Delete cache files only after the download/decrypt operation completes successfully.
					FileUtility.SaferDelete(cacheFile.FilePath);
				}
			}

			Serilog.Log.Verbose("Returning successful status handler for {@Book}", libraryBook.LogFriendly());
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
		//Directories are validated prior to beginning download/decrypt
		var outputDir = AudibleFileStorage.DecryptInProgressDirectory!;
		var cacheDir = AudibleFileStorage.DownloadsInProgressDirectory!;
		var result = new AudiobookDecryptResult(false, [], []);

		try
		{
			if (dlOptions.DrmType is not DrmType.Adrm and not DrmType.Widevine)
				abDownloader = new UnencryptedAudiobookDownloader(outputDir, cacheDir, dlOptions);
			else
			{
				AaxcDownloadConvertBase converter
					= dlOptions.Config.SplitFilesByChapter && dlOptions.ChapterInfo.Count > 1 ?
					new AaxcDownloadMultiConverter(outputDir, cacheDir, dlOptions) :
					new AaxcDownloadSingleConverter(outputDir, cacheDir, dlOptions);

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
			if (Path.GetDirectoryName(e.FilePath) == outputDir)
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
	private void Converter_RetrievedMetadata(object? sender, Mpeg4Lib.MetadataItems tags)
	{
		if (sender is not AaxcDownloadConvertBase converter ||
			converter.AaxFile is not Mpeg4Lib.Mpeg4File aaxFile ||
			converter.DownloadOptions is not DownloadOptions options ||
			options.ChapterInfo.Chapters is not List<Mpeg4Lib.Chapter> chapters)
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

		FillMissingTags(tags, options.LibraryBook.Book, options.LibraryBookDto, options.ContentMetadata.ContentReference, options.DrmType);
	}

	/// <summary>Audible's own format for <c>rldt</c>, which Libation matches when it supplies the tag itself.</summary>
	private const string ReleaseDateFormat = "dd-MMM-yyyy";

	/// <summary>
	/// Audible's ADRM (.aaxc) downloads arrive with most of these tags already written; its Widevine
	/// (DASH) downloads arrive with almost none of them, so the library's own data has to stand in.
	/// </summary>
	internal static void FillMissingTags(Mpeg4Lib.MetadataItems tags, Book book, LibraryBookDto dto, ContentReference contentReference, DrmType drmType)
	{
		tags.Title ??= dto.TitleWithSubtitle;
		tags.Album ??= tags.Title;
		tags.Artist ??= string.Join("; ", book.Authors.Select(a => a.Name));
		tags.AlbumArtists ??= tags.Artist;
		tags.Genres = string.Join(", ", book.LowestCategoryNames());
		tags.ProductID ??= contentReference.Sku;
		//Book.Description is Audible's publisher_summary verbatim, which is HTML. The .aaxc files
		//embed the same text already flattened, so flatten it here too rather than writing markup
		//into a tag that no player will render.
		tags.Comment ??= book.DescriptionAsPlainText();
		tags.LongDescription ??= tags.Comment;
		tags.Publisher ??= book.Publisher;
		tags.Narrator ??= string.Join("; ", book.Narrators.Select(n => n.Name));
		tags.Asin = book.AudibleProductId;
		tags.Acr = contentReference.Acr;
		tags.Version = contentReference.Version;
		if (book.DatePublished is DateTime pubDate)
		{
			tags.Year ??= pubDate.Year.ToString();
			tags.ReleaseDate ??= pubDate.ToString(ReleaseDateFormat);

			//Audible's .aaxc files frequently carry 01-Jan-2000 where the real release date belongs.
			//The file's own tag is otherwise the better source - it is what Audible shipped with this
			//particular recording - so it still wins everywhere except for that one placeholder value.
			//
			//Only the exact sentinel is overridden, and only when the catalog disagrees with it, so a
			//title genuinely released on 01-Jan-2000 comes out the same either way. Two broader rules
			//were considered and rejected: always preferring the catalog date would discard Audible's
			//date on re-releases the catalog dates wrong, and treating any year below some floor as
			//bogus has no defensible floor, since real recordings predate 2000.
			if (IsPlaceholderReleaseDate(tags.ReleaseDate) && pubDate.Date != PlaceholderReleaseDate)
			{
				tags.ReleaseDate = pubDate.ToString(ReleaseDateFormat);
				//A file carrying the placeholder rldt carries the matching placeholder year.
				if (tags.Year == PlaceholderReleaseDate.Year.ToString())
					tags.Year = pubDate.Year.ToString();
			}
		}

		const string tagDomain = "org.libation";
		tags.AppleListBox.EditOrAddFreeformTag(tagDomain, "AUDIBLE_ACR", tags.Acr);
		tags.AppleListBox.EditOrAddFreeformTag(tagDomain, "AUDIBLE_DRM_TYPE", drmType.ToString());
		tags.AppleListBox.EditOrAddFreeformTag(tagDomain, "AUDIBLE_LOCALE", book.Locale);
	}

	private static readonly DateTime PlaceholderReleaseDate = new(2000, 1, 1);

	internal static bool IsPlaceholderReleaseDate(string? releaseDate)
		=> DateTime.TryParse(releaseDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
		&& parsed.Date == PlaceholderReleaseDate;

	private void AaxcDownloader_RetrievedCoverArt(object? sender, byte[]? e)
	{
		if (Configuration.AllowLibationFixup && sender is AaxcDownloadConvertBase downloader)
		{
			try
			{
				e = OnRequestCoverArt();
				if (e is not null)
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
		}
		;

		if (string.IsNullOrWhiteSpace(libraryBook.Account))
			throw new InvalidOperationException(errorString("Account"));

		if (string.IsNullOrWhiteSpace(libraryBook.Book.Locale))
			throw new InvalidOperationException(errorString("Locale"));
	}
	#endregion

	#region Post-success routines
	/// <summary>
	/// Records the finished download for the daily download limit. Always recorded, even when no limit is
	/// configured, so that turning the limit on later reflects downloads already performed.
	/// </summary>
	private static void RecordDownloadForDailyLimit(LibraryBook libraryBook, List<TempFile> movedFiles)
	{
		try
		{
			// Files have already been moved, so these paths are their final locations in the Books directory.
			var bytes = movedFiles.Sum(f => File.Exists(f.FilePath) ? new FileInfo(f.FilePath).Length : 0);
			DownloadHistoryStore.Record(libraryBook.Book.AudibleProductId, libraryBook.IsAudiblePlus, bytes);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to measure a completed download for the daily download limit. The download itself succeeded. {@Book}", libraryBook.LogFriendly());
		}
	}

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
			Serilog.Log.Logger.Error(ex, "Error determining output audio format for {@Book}. File = '{@audioFile}'", options.LibraryBook.LogFriendly(), firstAudioFile);
			return AudioFormat.Default;
		}

		AudioFormat GetMp4AudioFormat()
			=> abDownloader is AaxcDownloadConvertBase converter && converter.AaxFile is AAXClean.Mp4File mp4File
			? AudioFormatDecoder.FromMpeg4(mp4File)
			: AudioFormatDecoder.FromMpeg4(firstAudioFile.FilePath);
	}

	/// <summary>Move new files to 'Books' directory</summary>
	/// <returns>Return directory if audiobook file(s) were successfully created and can be located on disk. Else null.</returns>
	private async Task MoveFilesToBooksDir(LibraryBook libraryBook, LongPath destinationDir, List<TempFile> entries, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		AverageSpeed averageSpeed = new();
		MoveWithProgress moveWithProgress = new();

		var totalSizeToMove = entries.Sum(f => new FileInfo(f.FilePath).Length);
		long totalBytesMoved = 0;
		moveWithProgress.MoveProgress += onMovefileProgress;

		for (var i = 0; i < entries.Count; i++)
		{
			var entry = entries[i];

			var destFileName
				= AudibleFileStorage.Audio.GetCustomDirFilename(
					libraryBook,
					destinationDir,
					entry.Extension,
					entry.PartProperties,
					Configuration.OverwriteExisting);

			var realDest
				= FileUtility.GetValidFilename(
					destFileName,
					Configuration.ReplacementCharacters,
					entry.Extension,
					Configuration.OverwriteExisting);

			await moveWithProgress.MoveAsync(entry.FilePath, realDest, Configuration.OverwriteExisting, cancellationToken);

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

		void onMovefileProgress(object? sender, MoveFileProgressEventArgs e)
		{
			totalBytesMoved += e.BytesMoved;
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
		}
		;
	}

	private void DownloadCoverArt(LongPath destinationDir, DownloadOptions options, CancellationToken cancellationToken)
	{
		if (!options.Config.DownloadCoverArt) return;

		var coverPath = "[null]";
		var picId = options.LibraryBook.Book.PictureLarge ?? options.LibraryBook.Book.PictureId;
		if (picId is null)
		{
			Serilog.Log.Logger.Warning("No cover art available for {@Book}.", options.LibraryBook.LogFriendly());
			return;
		}

		try
		{
			coverPath
				= AudibleFileStorage.Audio.GetCustomDirFilename(
					options.LibraryBook,
					destinationDir,
					extension: ".jpg",
					returnFirstExisting: Configuration.OverwriteExisting);

			if (File.Exists(coverPath))
				FileUtility.SaferDelete(coverPath);


			var picBytes = PictureStorage.GetPictureSynchronously(new(picId, PictureSize.Native), cancellationToken);
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
				Serilog.Log.Logger.Error(ex, "Error downloading cover art for {@Book} to {coverPath}.", options.LibraryBook.LogFriendly(), coverPath);
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
					returnFirstExisting: Configuration.OverwriteExisting);

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
				Serilog.Log.Logger.Error(ex, "Error downloading clips and bookmarks for {@Book} to {recordsPath}.", options.LibraryBook.LogFriendly(), recordsPath);
			throw;
		}
	}

	private async Task DownloadMetadataAsync(AudibleApi.Api api, LongPath destinationDir, DownloadOptions options, CancellationToken cancellationToken)
	{
		if (!options.Config.SaveMetadataToFile) return;

		string metadataPath = "[null]";

		try
		{
			// Fetched before the destination is touched. A storefront that no longer lists the title answers
			// with an empty product, and the file already on disk is then the better of the two.
			var item = await api.GetCatalogProductAsync(options.LibraryBook.Book.AudibleProductId, AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);

			if (item?.SourceJson is not { } sourceJson)
			{
				Serilog.Log.Logger.Error("Failed to retrieve metadata from server for {@Book}.", options.LibraryBook.LogFriendly());
				return;
			}

			if (CatalogProductIsEmpty(sourceJson))
			{
				Serilog.Log.Logger.Warning(
					"Audible's catalog holds no details for {@Book}, so its metadata file was left as it was. This happens when the title is no longer listed in the storefront of the account that owns it.",
					options.LibraryBook.LogFriendly());
				return;
			}

			metadataPath
				= AudibleFileStorage.Audio.GetCustomDirFilename(
					options.LibraryBook,
					destinationDir,
					extension: ".metadata.json",
					returnFirstExisting: Configuration.OverwriteExisting);

			if (File.Exists(metadataPath))
				FileUtility.SaferDelete(metadataPath);

			sourceJson.Add(nameof(ContentMetadata.ChapterInfo), Newtonsoft.Json.Linq.JObject.FromObject(options.ContentMetadata.ChapterInfo));
			sourceJson.Add(nameof(ContentMetadata.ContentReference), Newtonsoft.Json.Linq.JObject.FromObject(options.ContentMetadata.ContentReference));

			cancellationToken.ThrowIfCancellationRequested();
			File.WriteAllText(metadataPath, sourceJson.ToString());
			SetFileTime(options.LibraryBook, metadataPath);
			OnFileCreated(options.LibraryBook, metadataPath);
		}
		catch (Exception ex)
		{
			//Failure to download metadata should not be considered a failure to download the book
			if (!cancellationToken.IsCancellationRequested)
				Serilog.Log.Logger.Error(ex, "Error downloading metadata of {@Book} to {metadataFile}.", options.LibraryBook.LogFriendly(), metadataPath);
			throw;
		}
	}

	/// <summary>
	/// Whether a catalog product carries no details at all.
	/// <para>
	/// A storefront that no longer lists a title still answers a request for it: HTTP 200, total_results 1,
	/// and a product holding an asin and a handful of always-returned flags. Nothing between the request and
	/// the file notices, so a re-download of such a title used to replace a metadata file written while the
	/// title was still listed - the only copy of that data - with the placeholder. Reported in issue #1947,
	/// where a Canada-only title produced
	/// <c>{"asin":"...","asset_details":[],"is_preview_enabled":false,"is_vvab":false,"rating":{...zeros...}}</c>.
	/// </para>
	/// <para>
	/// Keyed off the title because every response group that returns anything descriptive returns one, and
	/// the placeholder's own fields are Audible's to change.
	/// </para>
	/// </summary>
	internal static bool CatalogProductIsEmpty(Newtonsoft.Json.Linq.JObject sourceJson)
		=> string.IsNullOrWhiteSpace(sourceJson.Value<string>("title"));
	#endregion

	#region Macros
	private string getDestinationDirectory(LibraryBook libraryBook)
	{
		Serilog.Log.Verbose("Getting destination directory for {@Book}", libraryBook.LogFriendly());
		var destinationDir = AudibleFileStorage.Audio.GetDestinationDirectory(libraryBook, Configuration);
		Serilog.Log.Verbose("Got destination directory for {@Book}. {Directory}", libraryBook.LogFriendly(), destinationDir);
		if (!Directory.Exists(destinationDir))
		{
			Serilog.Log.Verbose("Creating destination {Directory}", destinationDir);
			Directory.CreateDirectory(destinationDir);
			Serilog.Log.Verbose("Created destination {Directory}", destinationDir);
		}
		return destinationDir;
	}

	private static FileType getFileType(TempFile file)
		=> FileTypes.GetFileTypeFromPath(file.FilePath);
	private static TempFile? getFirstAudioFile(IEnumerable<TempFile> entries)
		=> entries.FirstOrDefault(f => File.Exists(f.FilePath) && getFileType(f) is FileType.Audio);
	private static IEnumerable<TempFile> getAaxcFiles(IEnumerable<TempFile> entries)
		=> entries.Where(f => File.Exists(f.FilePath) && (getFileType(f) is FileType.AAXC || f.Extension.Equals(".key", StringComparison.OrdinalIgnoreCase)));
	#endregion

	public static DownloadDecryptBook Create(Configuration config) => new() { Configuration = config };
	private DownloadDecryptBook() { }
}
