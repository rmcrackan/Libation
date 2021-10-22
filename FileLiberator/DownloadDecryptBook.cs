using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AaxDecrypter;
using AudibleApi;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using FileManager;
using LibationFileManager;

namespace FileLiberator
{
    public class DownloadDecryptBook : AudioDecodable
    {
        private AudiobookDownloadBase abDownloader;

        public override bool Validate(LibraryBook libraryBook) => !libraryBook.Book.Audio_Exists;

        public override void Cancel() => abDownloader?.Cancel();

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
                if (libraryBook.Book.Audio_Exists)
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
                if (!success)
                    return new StatusHandler { "Decrypt failed" };

                // moves new files from temp dir to final dest
                var movedAudioFile = moveFilesToBooksDir(libraryBook, entries);

                // decrypt failed
                if (!movedAudioFile)
                    return new StatusHandler { "Cannot find final audio file after decryption" };

                libraryBook.Book.UserDefinedItem.BookStatus = LiberatedStatus.Liberated;

                return new StatusHandler();
            }
            finally
            {
                OnCompleted(libraryBook);
            }
        }

        private async Task<bool> downloadAudiobookAsync(LibraryBook libraryBook)
        {
            OnStreamingBegin($"Begin decrypting {libraryBook}");

            try
            {
                downloadValidation(libraryBook);

                var api = await libraryBook.GetApiAsync();
                var contentLic = await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId);

                var audiobookDlLic = new DownloadLicense
                    (
                    contentLic?.ContentMetadata?.ContentUrl?.OfflineUrl,
                    contentLic?.Voucher?.Key,
                    contentLic?.Voucher?.Iv,
                    Resources.USER_AGENT
                    );

                //I assume if ContentFormat == "MPEG" that the delivered file is an unencrypted mp3.
                //I also assume that if DrmType != Adrm, the file will be an mp3.
                //These assumptions may be wrong, and only time and bug reports will tell.
                var outputFormat = 
                    contentLic.ContentMetadata.ContentReference.ContentFormat == "MPEG" ||
                    (Configuration.Instance.AllowLibationFixup && Configuration.Instance.DecryptToLossy) ? 
                    OutputFormat.Mp3 : OutputFormat.M4b;

                if (Configuration.Instance.AllowLibationFixup || outputFormat == OutputFormat.Mp3)
                {
                    audiobookDlLic.ChapterInfo = new AAXClean.ChapterInfo();

                    foreach (var chap in contentLic.ContentMetadata?.ChapterInfo?.Chapters)
                        audiobookDlLic.ChapterInfo.AddChapter(chap.Title, TimeSpan.FromMilliseconds(chap.LengthMs));
                }

                var outFileName = AudibleFileStorage.Audio.GetInProgressFilename(libraryBook, outputFormat.ToString().ToLower());

                var cacheDir = AudibleFileStorage.DownloadsInProgressDirectory;

                abDownloader
                    = contentLic.DrmType != AudibleApi.Common.DrmType.Adrm ? new UnencryptedAudiobookDownloader(outFileName, cacheDir, audiobookDlLic)
                    : Configuration.Instance.SplitFilesByChapter ? new AaxcDownloadMultiConverter(
                        outFileName, cacheDir, audiobookDlLic, outputFormat,
                        AudibleFileStorage.Audio.CreateMultipartRenamerFunc(libraryBook)
                        )
                    : new AaxcDownloadSingleConverter(outFileName, cacheDir, audiobookDlLic, outputFormat);
                abDownloader.DecryptProgressUpdate += (_, progress) => OnStreamingProgressChanged(progress);
                abDownloader.DecryptTimeRemaining += (_, remaining) => OnStreamingTimeRemaining(remaining);
                abDownloader.RetrievedTitle += (_, title) => OnTitleDiscovered(title);
                abDownloader.RetrievedAuthors += (_, authors) => OnAuthorsDiscovered(authors);
                abDownloader.RetrievedNarrators += (_, narrators) => OnNarratorsDiscovered(narrators);
                abDownloader.RetrievedCoverArt += AaxcDownloader_RetrievedCoverArt;
                abDownloader.FileCreated += (_, path) => OnFileCreated(libraryBook, path);

                // REAL WORK DONE HERE
                var success = await Task.Run(abDownloader.Run);
                return success;
            }
            finally
            {
                OnStreamingCompleted($"Completed downloading and decrypting {libraryBook.Book.Title}");
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

        private void AaxcDownloader_RetrievedCoverArt(object sender, byte[] e)
        {
            if (e is not null)
                OnCoverImageDiscovered(e);
            else if (Configuration.Instance.AllowLibationFixup)
                OnRequestCoverArt(abDownloader.SetCoverArt);
        }

        /// <summary>Move new files to 'Books' directory</summary>
        /// <returns>True if audiobook file(s) were successfully created and can be located on disk. Else false.</returns>
        private static bool moveFilesToBooksDir(LibraryBook libraryBook, List<FilePathCache.CacheEntry> entries)
        {
            // create final directory. move each file into it
            var destinationDir = AudibleFileStorage.Audio.GetDestinationDirectory(libraryBook);
            Directory.CreateDirectory(destinationDir);

            FilePathCache.CacheEntry getFirstAudio() => entries.FirstOrDefault(f => f.FileType == FileType.Audio);

			if (getFirstAudio() == default)
				return false;

			for (var i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];

				var realDest = FileUtility.SaferMoveToValidPath(entry.Path, Path.Combine(destinationDir, Path.GetFileName(entry.Path)));
				FilePathCache.Insert(libraryBook.Book.AudibleProductId, realDest);

				// propogate corrected path. Must update cache with corrected path. Also want updated path for cue file (after this for-loop)
				entries[i] = entry with { Path = realDest };
			}

			var cue = entries.FirstOrDefault(f => f.FileType == FileType.Cue);
			if (cue != default)
				Cue.UpdateFileName(cue.Path, getFirstAudio().Path);

			AudibleFileStorage.Audio.Refresh();

			return true;
		}
	}
}
