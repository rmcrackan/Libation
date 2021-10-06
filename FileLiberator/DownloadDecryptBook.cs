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

namespace FileLiberator
{
    public class DownloadDecryptBook : AudioDecodable
    {
        private AudiobookDownloadBase abDownloader;

        public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            OnBegin(libraryBook);

            try
            {
                if (libraryBook.Book.Audio_Exists)
                    return new StatusHandler { "Cannot find decrypt. Final audio file already exists" };

                var outputAudioFilename = await downloadAudiobookAsync(AudibleFileStorage.DownloadsInProgress, AudibleFileStorage.DecryptInProgress, libraryBook);

                // decrypt failed
                if (outputAudioFilename is null)
                    return new StatusHandler { "Decrypt failed" };

                // moves files and returns dest dir
                var moveResults = MoveFilesToBooksDir(libraryBook.Book, outputAudioFilename);

                if (!moveResults.movedAudioFile)
                    return new StatusHandler { "Cannot find final audio file after decryption" };

                libraryBook.Book.UserDefinedItem.BookStatus = LiberatedStatus.Liberated;

                return new StatusHandler();
            }
            finally
            {
                OnCompleted(libraryBook);
            }
        }

        private async Task<string> downloadAudiobookAsync(string cacheDir, string destinationDir, LibraryBook libraryBook)
        {
            OnStreamingBegin($"Begin decrypting {libraryBook}");

            try
            {
                validate(libraryBook);

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
                
                var outFileName = Path.Combine(destinationDir, $"{PathLib.ToPathSafeString(libraryBook.Book.Title)} [{libraryBook.Book.AudibleProductId}].{outputFormat.ToString().ToLower()}");

                abDownloader = contentLic.DrmType == AudibleApi.Common.DrmType.Adrm
                    ? new AaxcDownloadConverter(outFileName, cacheDir, audiobookDlLic, outputFormat, Configuration.Instance.SplitFilesByChapter)
                    : new UnencryptedAudiobookDownloader(outFileName, cacheDir, audiobookDlLic);
                abDownloader.AppName = "Libation";
                abDownloader.DecryptProgressUpdate += (_, progress) => OnStreamingProgressChanged(progress);
                abDownloader.DecryptTimeRemaining += (_, remaining) => OnStreamingTimeRemaining(remaining);
                abDownloader.RetrievedTitle += (_, title) => OnTitleDiscovered(title);
                abDownloader.RetrievedAuthors += (_, authors) => OnAuthorsDiscovered(authors);
                abDownloader.RetrievedNarrators += (_, narrators) => OnNarratorsDiscovered(narrators);
                abDownloader.RetrievedCoverArt += AaxcDownloader_RetrievedCoverArt;

                // REAL WORK DONE HERE
                var success = await Task.Run(abDownloader.Run);

                // decrypt failed
                if (!success)
                    return null;

                return outFileName;
            }
            finally
            {
                OnStreamingCompleted($"Completed downloading and decrypting {libraryBook.Book.Title}");
            }
        }

        private void AaxcDownloader_RetrievedCoverArt(object sender, byte[] e)
        {
            if (e is null && Configuration.Instance.AllowLibationFixup)
            {
                OnRequestCoverArt(abDownloader.SetCoverArt);
            }

            if (e is not null)
            {
                OnCoverImageDiscovered(e);
            }
        }

        private static (string destinationDir, bool movedAudioFile) MoveFilesToBooksDir(Book product, string outputAudioFilename)
        {
            // create final directory. move each file into it. MOVE AUDIO FILE LAST
            // new dir: safetitle_limit50char + " [" + productId + "]"
            // TODO make this method handle multiple audio files or a single audio file.
            var destinationDir = AudibleFileStorage.Audio.GetDestDir(product.Title, product.AudibleProductId);
            Directory.CreateDirectory(destinationDir);

            var sortedFiles = getProductFilesSorted(product, outputAudioFilename);

            var musicFileExt = Path.GetExtension(outputAudioFilename).Trim('.');

            // audio filename: safetitle_limit50char + " [" + productId + "]." + audio_ext
            var audioFileName = FileUtility.GetValidFilename(destinationDir, product.Title, musicFileExt, product.AudibleProductId);

            bool movedAudioFile = false;
            foreach (var f in sortedFiles)
            {
                var dest
                    = AudibleFileStorage.Audio.IsFileTypeMatch(f)//f.Extension.Equals($".{musicFileExt}", StringComparison.OrdinalIgnoreCase)
                    ? Path.Join(destinationDir, f.Name)
                    // non-audio filename: safetitle_limit50char + " [" + productId + "][" + audio_ext + "]." + non_audio_ext
                    : FileUtility.GetValidFilename(destinationDir, product.Title, f.Extension, product.AudibleProductId, musicFileExt);

                if (Path.GetExtension(dest).Trim('.').ToLower() == "cue")
                    Cue.UpdateFileName(f, audioFileName);

                File.Move(f.FullName, dest);

                movedAudioFile |= AudibleFileStorage.Audio.IsFileTypeMatch(f);
            }

            AudibleFileStorage.Audio.Refresh();

            return (destinationDir, movedAudioFile);
        }

        private static List<FileInfo> getProductFilesSorted(Book product, string outputAudioFilename)
        {
            // files are: temp path\author\[asin].ext
            var m4bDir = new FileInfo(outputAudioFilename).Directory;
            var files = m4bDir
                .EnumerateFiles()
                .Where(f => f.Name.ContainsInsensitive(product.AudibleProductId))
                .ToList();

            // move audio files to the end of the collection so these files are moved last
            var musicFiles = files.Where(f => AudibleFileStorage.Audio.IsFileTypeMatch(f));
            var sortedFiles = files
                .Except(musicFiles)
                .Concat(musicFiles)
                .ToList();

            return sortedFiles;
        }

        private static void validate(LibraryBook libraryBook)
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

        public override bool Validate(LibraryBook libraryBook) => !libraryBook.Book.Audio_Exists;

        public override void Cancel()
        {
            abDownloader?.Cancel();
        }
    }
}
