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
using InternalUtilities;

namespace FileLiberator
{
    /// <summary>
    /// Decrypt audiobook files
    /// 
    /// Processes:
    /// Download: download aax file: the DRM encrypted audiobook
    /// Decrypt: remove DRM encryption from audiobook. Store final book
    /// Backup: perform all steps (downloaded, decrypt) still needed to get final book
    /// </summary>
    public class DecryptBook : IDecryptable
    {
        public event EventHandler<LibraryBook> Begin;
        public event EventHandler<string> StatusUpdate;
        public event EventHandler<string> DecryptBegin;

        public event EventHandler<string> TitleDiscovered;
        public event EventHandler<string> AuthorsDiscovered;
        public event EventHandler<string> NarratorsDiscovered;
        public event EventHandler<byte[]> CoverImageFilepathDiscovered;
        public event EventHandler<int> UpdateProgress;

        public event EventHandler<string> DecryptCompleted;
        public event EventHandler<LibraryBook> Completed;

        public bool Validate(LibraryBook libraryBook)
            => AudibleFileStorage.AAX.Exists(libraryBook.Book.AudibleProductId)
            && !AudibleFileStorage.Audio.Exists(libraryBook.Book.AudibleProductId);

		// do NOT use ConfigureAwait(false) on ProcessAsync()
		// often calls events which prints to forms in the UI context
		public async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            Begin?.Invoke(this, libraryBook);

            try
            {
                var aaxFilename = AudibleFileStorage.AAX.GetPath(libraryBook.Book.AudibleProductId);

                if (aaxFilename == null)
                    return new StatusHandler { "aaxFilename parameter is null" };
                if (!File.Exists(aaxFilename))
                    return new StatusHandler { $"Cannot find AAX file: {aaxFilename}" };
                if (AudibleFileStorage.Audio.Exists(libraryBook.Book.AudibleProductId))
                    return new StatusHandler { "Cannot find decrypt. Final audio file already exists" };

                var chapters = await downloadChapterNamesAsync(libraryBook);

                var outputAudioFilename = await aaxToM4bConverterDecryptAsync(aaxFilename, libraryBook, chapters);

                // decrypt failed
                if (outputAudioFilename == null)
                    return new StatusHandler { "Decrypt failed" };

                // moves files and returns dest dir. Do not put inside of if(RetainAaxFiles)
                var destinationDir = moveFilesToBooksDir(libraryBook.Book, outputAudioFilename);

                var jsonFilename = PathLib.ReplaceExtension(aaxFilename, "json");
                if (Configuration.Instance.RetainAaxFiles)
                {
                    var newAaxFilename = FileUtility.GetValidFilename(
                        destinationDir,
                        Path.GetFileNameWithoutExtension(aaxFilename),
                        "aax");
                    File.Move(aaxFilename, newAaxFilename);

                    var newJsonFilename = PathLib.ReplaceExtension(newAaxFilename, "json");
                    File.Move(jsonFilename, newJsonFilename);
                }
                else
                {
                    Dinah.Core.IO.FileExt.SafeDelete(aaxFilename);
                    Dinah.Core.IO.FileExt.SafeDelete(jsonFilename);
                }

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

        private static async Task<Chapters> downloadChapterNamesAsync(LibraryBook libraryBook)
        {
            try
            {
                var api = await AudibleApiActions.GetApiAsync(libraryBook.Account, libraryBook.Book.Locale);
                var contentMetadata = await api.GetLibraryBookMetadataAsync(libraryBook.Book.AudibleProductId);
                if (contentMetadata?.ChapterInfo is null)
                    return null;

                return new DownloadedChapters(contentMetadata.ChapterInfo);
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> aaxToM4bConverterDecryptAsync(string aaxFilename, LibraryBook libraryBook, Chapters chapters)
        {
            DecryptBegin?.Invoke(this, $"Begin decrypting {aaxFilename}");

            try
            {
                var jsonPath = PathLib.ReplaceExtension(aaxFilename, "json");
                var jsonContents = File.ReadAllText(jsonPath);
                var dlLic = Newtonsoft.Json.JsonConvert.DeserializeObject<AudibleApiDTOs.DownloadLicense>(jsonContents);

                var converter = await AaxToM4bConverter.CreateAsync(aaxFilename, dlLic.AudibleKey, dlLic.AudibleIV, chapters);
                converter.AppName = "Libation";

                TitleDiscovered?.Invoke(this, converter.tags.title);
                AuthorsDiscovered?.Invoke(this, converter.tags.author);
                NarratorsDiscovered?.Invoke(this, converter.tags.narrator);
                CoverImageFilepathDiscovered?.Invoke(this, converter.coverBytes);

                // override default which was set in CreateAsync
                var proposedOutputFile = Path.Combine(AudibleFileStorage.DecryptInProgress, $"[{libraryBook.Book.AudibleProductId}].m4b");
                converter.SetOutputFilename(proposedOutputFile);
                converter.DecryptProgressUpdate += (s, progress) => UpdateProgress?.Invoke(this, progress);

                // REAL WORK DONE HERE
                var success = await Task.Run(() => converter.Run());

                // decrypt failed
                if (!success)
                    return null;

                return converter.outputFileName;
            }
            finally
            {
                DecryptCompleted?.Invoke(this, $"Completed decrypting {aaxFilename}");
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
	}
}
