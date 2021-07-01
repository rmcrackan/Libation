using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using FileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AaxDecrypter;
using AudibleApi;

namespace FileLiberator
{
    public class DownloadDecryptBook : IDecryptable
    {
        public event EventHandler<Action<byte[]>> RequestCoverArt;
        public event EventHandler<LibraryBook> Begin;
        public event EventHandler<string> DecryptBegin;
        public event EventHandler<string> TitleDiscovered;
        public event EventHandler<string> AuthorsDiscovered;
        public event EventHandler<string> NarratorsDiscovered;
        public event EventHandler<byte[]> CoverImageFilepathDiscovered;
        public event EventHandler<int> UpdateProgress;
        public event EventHandler<TimeSpan> UpdateRemainingTime;
        public event EventHandler<string> DecryptCompleted;
        public event EventHandler<LibraryBook> Completed;
        public event EventHandler<string> StatusUpdate;

        private AaxcDownloadConverter aaxcDownloader;
        public async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            Begin?.Invoke(this, libraryBook);

            try
            {
                if (AudibleFileStorage.Audio.Exists(libraryBook.Book.AudibleProductId))
                    return new StatusHandler { "Cannot find decrypt. Final audio file already exists" };

                var outputAudioFilename = await aaxToM4bConverterDecryptAsync(AudibleFileStorage.DecryptInProgress, libraryBook);

                // decrypt failed
                if (outputAudioFilename is null)
                    return new StatusHandler { "Decrypt failed" };

                // moves files and returns dest dir
                _ = moveFilesToBooksDir(libraryBook.Book, outputAudioFilename);

                var finalAudioExists = AudibleFileStorage.Audio.Exists(libraryBook.Book.AudibleProductId);
                if (!finalAudioExists)
                    return new StatusHandler { "Cannot find final audio file after decryption" };

                return new StatusHandler();
            }
            finally
            {
                Completed?.Invoke(this, libraryBook);
            }
        }

        private async Task<string> aaxToM4bConverterDecryptAsync(string destinationDir, LibraryBook libraryBook)
        {
            DecryptBegin?.Invoke(this, $"Begin decrypting {libraryBook}");

            try
            {
                validate(libraryBook);

                var api = await InternalUtilities.AudibleApiActions.GetApiAsync(libraryBook.Account, libraryBook.Book.Locale);

                var contentLic = await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId);

                var aaxcDecryptDlLic = new DownloadLicense
                    (
                    contentLic.ContentMetadata?.ContentUrl?.OfflineUrl,
                    contentLic.Voucher?.Key,
                    contentLic.Voucher?.Iv,
                    Resources.UserAgent
                    );

                if (Configuration.Instance.AllowLibationFixup)
                {
                    aaxcDecryptDlLic.ChapterInfo = new ChapterInfo();

                    foreach (var chap in contentLic.ContentMetadata?.ChapterInfo?.Chapters)
                        aaxcDecryptDlLic.ChapterInfo.AddChapter(
                            new Chapter(
                                chap.Title,
                                chap.StartOffsetMs,
                                chap.LengthMs
                                ));
                }

                aaxcDownloader = AaxcDownloadConverter.Create(destinationDir, aaxcDecryptDlLic);

                aaxcDownloader.AppName = "Libation";                              

                // override default which was set in CreateAsync
                var proposedOutputFile = Path.Combine(destinationDir, $"{PathLib.ToPathSafeString(libraryBook.Book.Title)} [{libraryBook.Book.AudibleProductId}].m4b");
                aaxcDownloader.SetOutputFilename(proposedOutputFile);
                aaxcDownloader.DecryptProgressUpdate += (s, progress) => UpdateProgress?.Invoke(this, progress);
                aaxcDownloader.DecryptTimeRemaining += (s, remaining) => UpdateRemainingTime?.Invoke(this, remaining);
                aaxcDownloader.RetrievedCoverArt += AaxcDownloader_RetrievedCoverArt;
                aaxcDownloader.RetrievedTags += aaxcDownloader_RetrievedTags;

                // REAL WORK DONE HERE
                var success = await Task.Run(() => aaxcDownloader.Run());

                // decrypt failed
                if (!success)
                    return null;

                return aaxcDownloader.outputFileName;
            }
            finally
            {
                DecryptCompleted?.Invoke(this, $"Completed downloading and decrypting {libraryBook.Book.Title}");
            }
        }

        private void AaxcDownloader_RetrievedCoverArt(object sender, byte[] e)
        {
            if (e is null && Configuration.Instance.AllowLibationFixup)
            {
                RequestCoverArt?.Invoke(this, aaxcDownloader.SetCoverArt);
            }

            if (e is not null)
            {
                CoverImageFilepathDiscovered?.Invoke(this, e);
            }
        }

        private void aaxcDownloader_RetrievedTags(object sender, AaxcTagLibFile e)
        {
            TitleDiscovered?.Invoke(this, e.TitleSansUnabridged);
            AuthorsDiscovered?.Invoke(this, e.FirstAuthor ?? "[unknown]");
            NarratorsDiscovered?.Invoke(this, e.Narrator ?? "[unknown]");           
        }

        private static string moveFilesToBooksDir(Book product, string outputAudioFilename)
        {
            // create final directory. move each file into it. MOVE AUDIO FILE LAST
            // new dir: safetitle_limit50char + " [" + productId + "]"

            var destinationDir = AudibleFileStorage.Audio.GetDestDir(product.Title, product.AudibleProductId);
            Directory.CreateDirectory(destinationDir);

            var sortedFiles = getProductFilesSorted(product, outputAudioFilename);

            var musicFileExt = Path.GetExtension(outputAudioFilename).Trim('.');

            // audio filename: safetitle_limit50char + " [" + productId + "]." + audio_ext
            var audioFileName = FileUtility.GetValidFilename(destinationDir, product.Title, musicFileExt, product.AudibleProductId);

            foreach (var f in sortedFiles)
            {
                var dest
                    = AudibleFileStorage.Audio.IsFileTypeMatch(f)
                    ? audioFileName
                    // non-audio filename: safetitle_limit50char + " [" + productId + "][" + audio_ext +"]." + non_audio_ext
                    : FileUtility.GetValidFilename(destinationDir, product.Title, f.Extension, product.AudibleProductId, musicFileExt);

                if (Path.GetExtension(dest).Trim('.').ToLower() == "cue")
                    Cue.UpdateFileName(f, audioFileName);

                File.Move(f.FullName, dest);
            }

            return destinationDir;
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

        public bool Validate(LibraryBook libraryBook)
            => !AudibleFileStorage.Audio.Exists(libraryBook.Book.AudibleProductId)
            && !AudibleFileStorage.AAX.Exists(libraryBook.Book.AudibleProductId);

        public void Cancel()
        {
            aaxcDownloader.Cancel();
        }
    }
}
