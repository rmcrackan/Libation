using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AaxDecrypter;
using ApplicationServices;
using AudibleApi.Common;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using FileManager;
using LibationFileManager;

namespace FileLiberator
{
    public class DownloadDecryptBook : AudioDecodable
    {
        public override string Name => "Download & Decrypt";
        private AudiobookDownloadBase abDownloader;

        public override bool Validate(LibraryBook libraryBook) => !libraryBook.Book.Audio_Exists();

        public override Task CancelAsync() => abDownloader?.CancelAsync() ?? Task.CompletedTask;

        public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            var entries = new List<FilePathCache.CacheEntry>();
            // these only work so minimally b/c CacheEntry is a record.
            // in case of parallel decrypts, only capture the ones for this book id.
            // if user somehow starts multiple decrypts of the same book in parallel: on their own head be it
            void FilePathCache_Inserted(object sender, FilePathCache.CacheEntry e)
            {
                if (e.Id.EqualsInsensitive(libraryBook.Book.AudibleProductId))
                    entries.Add(e);
            }
            void FilePathCache_Removed(object sender, FilePathCache.CacheEntry e)
            {
                if (e.Id.EqualsInsensitive(libraryBook.Book.AudibleProductId))
                    entries.Remove(e);
            }

            OnBegin(libraryBook);

            try
            {
                if (libraryBook.Book.Audio_Exists())
                    return new StatusHandler { "Cannot find decrypt. Final audio file already exists" };

                downloadValidation(libraryBook);
				var api = await libraryBook.GetApiAsync();
                var config = Configuration.Instance;
				using var downloadOptions = await DownloadOptions.InitiateDownloadAsync(api, config, libraryBook);

                bool success = false;
                try
                {
                    FilePathCache.Inserted += FilePathCache_Inserted;
                    FilePathCache.Removed += FilePathCache_Removed;

                    success = await downloadAudiobookAsync(api, config, downloadOptions);
                }
                finally
                {
                    FilePathCache.Inserted -= FilePathCache_Inserted;
                    FilePathCache.Removed -= FilePathCache_Removed;
                }

                // decrypt failed
                if (!success || getFirstAudioFile(entries) == default)
                {
                    await Task.WhenAll(
                        entries
                        .Where(f => f.FileType != FileType.AAXC)
                        .Select(f => Task.Run(() => FileUtility.SaferDelete(f.Path))));

                    return
                        abDownloader?.IsCanceled is true
                        ? new StatusHandler { "Cancelled" }
                        : new StatusHandler { "Decrypt failed" };
                }

                var finalStorageDir = getDestinationDirectory(libraryBook);

                var moveFilesTask = Task.Run(() => moveFilesToBooksDir(libraryBook, entries));
                Task[] finalTasks =
                [
                    Task.Run(() => downloadCoverArt(downloadOptions)),
                    moveFilesTask,
                    Task.Run(() => WindowsDirectory.SetCoverAsFolderIcon(libraryBook.Book.PictureId, finalStorageDir))
                ];

				try
                {
                    await Task.WhenAll(finalTasks);
                }
                catch
                {
					//Swallow downloadCoverArt and SetCoverAsFolderIcon exceptions.
                    //Only fail if the downloaded audio files failed to move to Books directory
					if (moveFilesTask.IsFaulted)
                    {
                        throw;
                    }
                }
                finally
                {
                    if (moveFilesTask.IsCompletedSuccessfully)
                    {
                        await Task.Run(() => libraryBook.UpdateBookStatus(LiberatedStatus.Liberated, Configuration.LibationVersion));

						SetDirectoryTime(libraryBook, finalStorageDir);
					}
				}

				return new StatusHandler();
            }
            finally
            {
                OnCompleted(libraryBook);
            }
        }



        private async Task<bool> downloadAudiobookAsync(AudibleApi.Api api, Configuration config, DownloadOptions dlOptions)
        {
			var outFileName = AudibleFileStorage.Audio.GetInProgressFilename(dlOptions.LibraryBookDto, dlOptions.OutputFormat.ToString().ToLower());
            var cacheDir = AudibleFileStorage.DownloadsInProgressDirectory;

            if (dlOptions.DrmType is not DrmType.Adrm and not DrmType.Widevine)
                abDownloader = new UnencryptedAudiobookDownloader(outFileName, cacheDir, dlOptions);
            else
            {
                AaxcDownloadConvertBase converter
                    = config.SplitFilesByChapter ?
                    new AaxcDownloadMultiConverter(outFileName, cacheDir, dlOptions) :
                    new AaxcDownloadSingleConverter(outFileName, cacheDir, dlOptions);

                if (config.AllowLibationFixup)
                    converter.RetrievedMetadata += Converter_RetrievedMetadata;

                abDownloader = converter;
            }

            abDownloader.DecryptProgressUpdate += OnStreamingProgressChanged;
            abDownloader.DecryptTimeRemaining += OnStreamingTimeRemaining;
            abDownloader.RetrievedTitle += OnTitleDiscovered;
            abDownloader.RetrievedAuthors += OnAuthorsDiscovered;
            abDownloader.RetrievedNarrators += OnNarratorsDiscovered;
            abDownloader.RetrievedCoverArt += AaxcDownloader_RetrievedCoverArt;
            abDownloader.FileCreated += (_, path) => OnFileCreated(dlOptions.LibraryBook, path);

            // REAL WORK DONE HERE
            var success = await abDownloader.RunAsync();

			if (success && config.SaveMetadataToFile)
            {
                var metadataFile = LibationFileManager.Templates.Templates.File.GetFilename(dlOptions.LibraryBookDto, Path.GetDirectoryName(outFileName), ".metadata.json");

                var item = await api.GetCatalogProductAsync(dlOptions.LibraryBook.Book.AudibleProductId, AudibleApi.CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
				item.SourceJson.Add(nameof(ContentMetadata.ChapterInfo), Newtonsoft.Json.Linq.JObject.FromObject(dlOptions.ContentMetadata.ChapterInfo));
				item.SourceJson.Add(nameof(ContentMetadata.ContentReference), Newtonsoft.Json.Linq.JObject.FromObject(dlOptions.ContentMetadata.ContentReference));

                File.WriteAllText(metadataFile, item.SourceJson.ToString());
                OnFileCreated(dlOptions.LibraryBook, metadataFile);
            }
			return success;
		}

		private void Converter_RetrievedMetadata(object sender, AAXClean.AppleTags tags)
		{
            if (sender is not AaxcDownloadConvertBase converter || converter.DownloadOptions is not DownloadOptions options)
				return;

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

        private static void downloadValidation(LibraryBook libraryBook)
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
                throw new Exception(errorString("Account"));

            if (string.IsNullOrWhiteSpace(libraryBook.Book.Locale))
                throw new Exception(errorString("Locale"));
        }

        private void AaxcDownloader_RetrievedCoverArt(object _, byte[] e)
        {
            if (Configuration.Instance.AllowLibationFixup)
            {
                try
                {
                    e = OnRequestCoverArt();
                    abDownloader.SetCoverArt(e);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Logger.Error(ex, "Failed to retrieve cover art from server.");
                }
            }
            
            if (e is not null)
                OnCoverImageDiscovered(e);
		}

        /// <summary>Move new files to 'Books' directory</summary>
        /// <returns>Return directory if audiobook file(s) were successfully created and can be located on disk. Else null.</returns>
        private static void moveFilesToBooksDir(LibraryBook libraryBook, List<FilePathCache.CacheEntry> entries)
        {
            // create final directory. move each file into it
            var destinationDir = getDestinationDirectory(libraryBook);

			for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                var realDest
                    = FileUtility.SaferMoveToValidPath(
                        entry.Path,
                        Path.Combine(destinationDir, Path.GetFileName(entry.Path)),
                        Configuration.Instance.ReplacementCharacters,
                        overwrite: Configuration.Instance.OverwriteExisting);

                SetFileTime(libraryBook, realDest);
				FilePathCache.Insert(libraryBook.Book.AudibleProductId, realDest);

                // propagate corrected path. Must update cache with corrected path. Also want updated path for cue file (after this for-loop)
                entries[i] = entry with { Path = realDest };
            }

			var cue = entries.FirstOrDefault(f => f.FileType == FileType.Cue);
            if (cue != default)
            {
                Cue.UpdateFileName(cue.Path, getFirstAudioFile(entries).Path);
				SetFileTime(libraryBook, cue.Path);
			}

            AudibleFileStorage.Audio.Refresh();
        }

        private static string getDestinationDirectory(LibraryBook libraryBook)
        {
			var destinationDir = AudibleFileStorage.Audio.GetDestinationDirectory(libraryBook);
            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);
            return destinationDir;
		}

		private static FilePathCache.CacheEntry getFirstAudioFile(IEnumerable<FilePathCache.CacheEntry> entries)
            => entries.FirstOrDefault(f => f.FileType == FileType.Audio);

		private static void downloadCoverArt(DownloadOptions options)
        {
			if (!Configuration.Instance.DownloadCoverArt) return;

            var coverPath = "[null]";

            try
            {
                var destinationDir = getDestinationDirectory(options.LibraryBook);
                coverPath = AudibleFileStorage.Audio.GetBooksDirectoryFilename(options.LibraryBookDto, ".jpg");
                coverPath = Path.Combine(destinationDir, Path.GetFileName(coverPath));

                if (File.Exists(coverPath))
                    FileUtility.SaferDelete(coverPath);

                var picBytes = PictureStorage.GetPictureSynchronously(new(options.LibraryBook.Book.PictureLarge ?? options.LibraryBook.Book.PictureId, PictureSize.Native));
                if (picBytes.Length > 0)
                {
                    File.WriteAllBytes(coverPath, picBytes);
                    SetFileTime(options.LibraryBook, coverPath);
                }
            }
            catch (Exception ex)
            {
                //Failure to download cover art should not be considered a failure to download the book
                Serilog.Log.Logger.Error(ex, $"Error downloading cover art of {options.LibraryBook.Book.AudibleProductId} to {coverPath} catalog product.");
            }
        }
    }
}
