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

namespace FileLiberator.AaxcDownloadDecrypt
{
    public class DownloadDecryptBook : IDecryptable
    {
        public event EventHandler<LibraryBook> Begin;
        public event EventHandler<string> DecryptBegin;
        public event EventHandler<string> TitleDiscovered;
        public event EventHandler<string> AuthorsDiscovered;
        public event EventHandler<string> NarratorsDiscovered;
        public event EventHandler<byte[]> CoverImageFilepathDiscovered;
        public event EventHandler<int> UpdateProgress;
        public event EventHandler<string> DecryptCompleted;
        public event EventHandler<LibraryBook> Completed;
        public event EventHandler<string> StatusUpdate;

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

                // moves files and returns dest dir. Do not put inside of if(RetainAaxFiles)
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

                var dlLic = await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId);

                var aaxcDecryptDlLic = new DownloadLicense(dlLic.DownloadUrl, dlLic.AudibleKey, dlLic.AudibleIV, Resources.UserAgent);

                var destinationDirectory = Path.GetDirectoryName(destinationDir);

                AaxcDownloadConverter newDownloader;
                if (Configuration.Instance.DownloadChapters)
                {
                    var contentMetadata = await api.GetLibraryBookMetadataAsync(libraryBook.Book.AudibleProductId);

                    var aaxcDecryptChapters = new ChapterInfo();

                    foreach (var chap in contentMetadata?.ChapterInfo?.Chapters)
                        aaxcDecryptChapters.AddChapter(new Chapter(chap.Title, chap.StartOffsetMs, chap.LengthMs));

                    newDownloader = await AaxcDownloadConverter.CreateAsync(destinationDirectory, aaxcDecryptDlLic, aaxcDecryptChapters);
                }
                else
                {
                    newDownloader = await AaxcDownloadConverter.CreateAsync(destinationDirectory, aaxcDecryptDlLic);
                }

                newDownloader.AppName = "Libation";

                TitleDiscovered?.Invoke(this, newDownloader.Title);
                AuthorsDiscovered?.Invoke(this, newDownloader.Author);
                NarratorsDiscovered?.Invoke(this, newDownloader.Narrator);
                CoverImageFilepathDiscovered?.Invoke(this, newDownloader.CoverArt);

                // override default which was set in CreateAsync
                var proposedOutputFile = Path.Combine(destinationDir, $"{libraryBook.Book.Title} [{libraryBook.Book.AudibleProductId}].m4b");
                newDownloader.SetOutputFilename(proposedOutputFile);
                newDownloader.DecryptProgressUpdate += (s, progress) => UpdateProgress?.Invoke(this, progress);

                // REAL WORK DONE HERE
                var success = await Task.Run(() => newDownloader.Run());

                // decrypt failed
                if (!success)
                    return null;

                return newDownloader.outputFileName;
            }
            finally
            {
                DecryptCompleted?.Invoke(this, $"Completed downloading and decrypting {libraryBook.Book.Title}");
            }
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


    }
}
