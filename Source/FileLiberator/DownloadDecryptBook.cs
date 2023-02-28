using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AaxDecrypter;
using ApplicationServices;
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

                bool success = false;
                try
                {
                    FilePathCache.Inserted += FilePathCache_Inserted;
                    FilePathCache.Removed += FilePathCache_Removed;

                    success = await downloadAudiobookAsync(libraryBook);
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

				Task[] finalTasks = new[]
                {
					Task.Run(() => downloadCoverArt(libraryBook)),
					Task.Run(() => moveFilesToBooksDir(libraryBook, entries)),
					Task.Run(() => libraryBook.Book.UpdateBookStatus(LiberatedStatus.Liberated)),
					Task.Run(() => WindowsDirectory.SetCoverAsFolderIcon(libraryBook.Book.PictureId, finalStorageDir))
				};

				await Task.WhenAll(finalTasks);

                return new StatusHandler();
            }
            finally
            {
                OnCompleted(libraryBook);
            }
        }

        private async Task<bool> downloadAudiobookAsync(LibraryBook libraryBook)
        {
            var config = Configuration.Instance;

            downloadValidation(libraryBook);

            var api = await libraryBook.GetApiAsync();
            var contentLic = await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId);
            using var dlOptions = BuildDownloadOptions(libraryBook, config, contentLic);

            var outFileName = AudibleFileStorage.Audio.GetInProgressFilename(libraryBook, dlOptions.OutputFormat.ToString().ToLower());
            var cacheDir = AudibleFileStorage.DownloadsInProgressDirectory;

            if (contentLic.DrmType != AudibleApi.Common.DrmType.Adrm)
                abDownloader = new UnencryptedAudiobookDownloader(outFileName, cacheDir, dlOptions);
            else
            {
                AaxcDownloadConvertBase converter
                    = config.SplitFilesByChapter ?
                    new AaxcDownloadMultiConverter(outFileName, cacheDir, dlOptions) :
                    new AaxcDownloadSingleConverter(outFileName, cacheDir, dlOptions);

                if (config.AllowLibationFixup)
                    converter.RetrievedMetadata += (_, tags) => tags.Generes = string.Join(", ", libraryBook.Book.CategoriesNames());

                abDownloader = converter;
            }

            abDownloader.DecryptProgressUpdate += OnStreamingProgressChanged;
            abDownloader.DecryptTimeRemaining += OnStreamingTimeRemaining;
            abDownloader.RetrievedTitle += OnTitleDiscovered;
            abDownloader.RetrievedAuthors += OnAuthorsDiscovered;
            abDownloader.RetrievedNarrators += OnNarratorsDiscovered;
            abDownloader.RetrievedCoverArt += AaxcDownloader_RetrievedCoverArt;
            abDownloader.FileCreated += (_, path) => OnFileCreated(libraryBook, path);

			// REAL WORK DONE HERE
			return await abDownloader.RunAsync();
        }

        private DownloadOptions BuildDownloadOptions(LibraryBook libraryBook, Configuration config, AudibleApi.Common.ContentLicense contentLic)
        {
			//If DrmType != Adrm the delivered file is an unencrypted mp3.

            var outputFormat
                = contentLic.DrmType != AudibleApi.Common.DrmType.Adrm || (config.AllowLibationFixup && config.DecryptToLossy)
                ? OutputFormat.Mp3
                : OutputFormat.M4b;

            long chapterStartMs
                = config.StripAudibleBrandAudio
                ? contentLic.ContentMetadata.ChapterInfo.BrandIntroDurationMs
                : 0;

            var dlOptions = new DownloadOptions(config, libraryBook, contentLic?.ContentMetadata?.ContentUrl?.OfflineUrl)
            {
                AudibleKey = contentLic?.Voucher?.Key,
                AudibleIV = contentLic?.Voucher?.Iv,
				OutputFormat = outputFormat,
				LameConfig = GetLameOptions(config),
				ChapterInfo = new AAXClean.ChapterInfo(TimeSpan.FromMilliseconds(chapterStartMs)),
				RuntimeLength = TimeSpan.FromMilliseconds(contentLic?.ContentMetadata?.ChapterInfo?.RuntimeLengthMs ?? 0),
			};

            var chapters = flattenChapters(contentLic.ContentMetadata.ChapterInfo.Chapters).OrderBy(c => c.StartOffsetMs).ToList();

            if (config.MergeOpeningAndEndCredits)
                combineCredits(chapters);

            for (int i = 0; i < chapters.Count; i++)
            {
                var chapter = chapters[i];
                long chapLenMs = chapter.LengthMs;

                if (i == 0)
                    chapLenMs -= chapterStartMs;

                if (config.StripAudibleBrandAudio && i == chapters.Count - 1)
                    chapLenMs -= contentLic.ContentMetadata.ChapterInfo.BrandOutroDurationMs;

                dlOptions.ChapterInfo.AddChapter(chapter.Title, TimeSpan.FromMilliseconds(chapLenMs));
            }

            return dlOptions;
        }

        /*

		Flatten Audible's new hierarchical chapters, combining children into parents.

		Audible may deliver chapters like this:

		00:00 - 00:10	Opening Credits
		00:10 - 00:12	Book 1
		00:12 - 00:14	|	Part 1
		00:14 - 01:40	|	|	Chapter 1
		01:40 - 03:20	|	|	Chapter 2
		03:20 - 03:22	|	Part 2
		03:22 - 05:00	|	|	Chapter 3
		05:00 - 06:40	|	|	Chapter 4
		06:40 - 06:42	Book 2
		06:42 - 06:44	|	Part 3
		06:44 - 08:20	|	|	Chapter 5
		08:20 - 10:00	|	|	Chapter 6
		10:00 - 10:02	|	Part 4
		10:02 - 11:40	|	|	Chapter 7
		11:40 - 13:20	|	|	Chapter 8
		13:20 - 13:30	End Credits

		And flattenChapters will combine them into this:

		00:00 - 00:10	Opening Credits
		00:10 - 01:40	Book 1: Part 1: Chapter 1
		01:40 - 03:20	Book 1: Part 1: Chapter 2
		03:20 - 05:00	Book 1: Part 2: Chapter 3
		05:00 - 06:40	Book 1: Part 2: Chapter 4
		06:40 - 08:20	Book 2: Part 3: Chapter 5
		08:20 - 10:00	Book 2: Part 3: Chapter 6
		10:00 - 11:40	Book 2: Part 4: Chapter 7
		11:40 - 13:20	Book 2: Part 4: Chapter 8
		13:20 - 13:40	End Credits

		However, if one of the parent chapters is longer than 10000 milliseconds, it's kept as its own
		chapter. A duration longer than a few seconds implies that the chapter contains more than just
		the narrator saying the chapter title, so it should probably be preserved as a separate chapter.
		Using the example above, if "Book 1" was 15 seconds long and "Part 3" was 20 seconds long:

		00:00 - 00:10	Opening Credits
		00:10 - 00:25	Book 1
		00:25 - 00:27	|	Part 1
		00:27 - 01:40	|	|	Chapter 1
		01:40 - 03:20	|	|	Chapter 2
		03:20 - 03:22	|	Part 2
		03:22 - 05:00	|	|	Chapter 3
		05:00 - 06:40	|	|	Chapter 4
		06:40 - 06:42	Book 2
		06:42 - 07:02	|	Part 3
		07:02 - 08:20	|	|	Chapter 5
		08:20 - 10:00	|	|	Chapter 6
		10:00 - 10:02	|	Part 4
		10:02 - 11:40	|	|	Chapter 7
		11:40 - 13:20	|	|	Chapter 8
		13:20 - 13:30	End Credits

		then flattenChapters will combine them into this:

		00:00 - 00:10	Opening Credits
		00:10 - 00:25	Book 1
		00:25 - 01:40	Book 1: Part 1: Chapter 1
		01:40 - 03:20	Book 1: Part 1: Chapter 2
		03:20 - 05:00	Book 1: Part 2: Chapter 3
		05:00 - 06:40	Book 1: Part 2: Chapter 4
		06:40 - 07:02	Book 2: Part 3
		07:02 - 08:20	Book 2: Part 3: Chapter 5
		08:20 - 10:00	Book 2: Part 3: Chapter 6
		10:00 - 11:40	Book 2: Part 4: Chapter 7
		11:40 - 13:20	Book 2: Part 4: Chapter 8
		13:20 - 13:40	End Credits

		*/

        public static List<AudibleApi.Common.Chapter> flattenChapters(IList<AudibleApi.Common.Chapter> chapters, string titleConcat = ": ")
        {
            List<AudibleApi.Common.Chapter> chaps = new();

            foreach (var c in chapters)
            {
                if (c.Chapters is null)
					chaps.Add(c);
				else
				{
                    if (c.LengthMs < 10000)
                    {
                        c.Chapters[0].StartOffsetMs = c.StartOffsetMs;
                        c.Chapters[0].StartOffsetSec = c.StartOffsetSec;
                        c.Chapters[0].LengthMs += c.LengthMs;
                    }
                    else
                        chaps.Add(c);

                    var children = flattenChapters(c.Chapters);

                    foreach (var child in children)
                        child.Title = $"{c.Title}{titleConcat}{child.Title}";

                    chaps.AddRange(children);
                    c.Chapters = null;
                }
            }
            return chaps;
        }

        public static void combineCredits(IList<AudibleApi.Common.Chapter> chapters)
        {
            if (chapters.Count > 1 && chapters[0].Title == "Opening Credits")
            {
                chapters[1].StartOffsetMs = chapters[0].StartOffsetMs;
                chapters[1].StartOffsetSec = chapters[0].StartOffsetSec;
                chapters[1].LengthMs += chapters[0].LengthMs;
                chapters.RemoveAt(0);
            }
            if (chapters.Count > 1 && chapters[^1].Title == "End Credits")
            {
                chapters[^2].LengthMs += chapters[^1].LengthMs;
                chapters.Remove(chapters[^1]);
            }
        }

        private static void downloadValidation(LibraryBook libraryBook)
        {
            string errorString(string field)
                => $"{errorTitle()}\r\nCannot download book. {field} is not known. Try re-importing the account which owns this book.";

            string errorTitle()
            {
                var title
                    = (libraryBook.Book.Title.Length > 53)
                    ? $"{libraryBook.Book.Title.Truncate(50)}..."
                    : libraryBook.Book.Title;
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
            if (e is not null)
                OnCoverImageDiscovered(e);
            else if (Configuration.Instance.AllowLibationFixup)
                abDownloader.SetCoverArt(OnRequestCoverArt());
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

                var realDest = FileUtility.SaferMoveToValidPath(entry.Path, Path.Combine(destinationDir, Path.GetFileName(entry.Path)), Configuration.Instance.ReplacementCharacters);
                FilePathCache.Insert(libraryBook.Book.AudibleProductId, realDest);

                // propagate corrected path. Must update cache with corrected path. Also want updated path for cue file (after this for-loop)
                entries[i] = entry with { Path = realDest };
            }

			var cue = entries.FirstOrDefault(f => f.FileType == FileType.Cue);
            if (cue != default)
                Cue.UpdateFileName(cue.Path, getFirstAudioFile(entries).Path);

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

		private static void downloadCoverArt(LibraryBook libraryBook)
        {
            if (!Configuration.Instance.DownloadCoverArt) return;

            var coverPath = "[null]";

            try
            {
                var destinationDir = getDestinationDirectory(libraryBook);
                coverPath = AudibleFileStorage.Audio.GetBooksDirectoryFilename(libraryBook, ".jpg");
                coverPath = Path.Combine(destinationDir, Path.GetFileName(coverPath));

                if (File.Exists(coverPath))
                    FileUtility.SaferDelete(coverPath);

                var picBytes = PictureStorage.GetPictureSynchronously(new(libraryBook.Book.PictureLarge ?? libraryBook.Book.PictureId, PictureSize.Native));
                if (picBytes.Length > 0)
                    File.WriteAllBytes(coverPath, picBytes);
            }
            catch (Exception ex)
            {
                //Failure to download cover art should not be considered a failure to download the book
                Serilog.Log.Logger.Error(ex, $"Error downloading cover art of {libraryBook.Book.AudibleProductId} to {coverPath} catalog product.");
            }
        }
    }
}
