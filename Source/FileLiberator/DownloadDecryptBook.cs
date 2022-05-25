using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AaxDecrypter;
using ApplicationServices;
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
        public override string Name => "Download & Decrypt";
        private AudiobookDownloadBase abDownloader;

        public override bool Validate(LibraryBook libraryBook) => !libraryBook.Book.Audio_Exists();

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
                if (!success)
                {
                    foreach (var tmpFile in entries.Where(f => f.FileType != FileType.AAXC))
                        FileUtility.SaferDelete(tmpFile.Path);

                    return abDownloader?.IsCanceled == true ?
                        new StatusHandler { "Cancelled" } : 
                        new StatusHandler { "Decrypt failed" };
                }

                // moves new files from temp dir to final dest.
                // This could take a few seconds if moving hundreds of files.
                var movedAudioFile = await Task.Run(() => moveFilesToBooksDir(libraryBook, entries));

                // decrypt failed
                if (!movedAudioFile)
                    return new StatusHandler { "Cannot find final audio file after decryption" };

                if (Configuration.Instance.DownloadCoverArt)
                    DownloadCoverArt(libraryBook);

                libraryBook.Book.UpdateBookStatus(LiberatedStatus.Liberated);

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
                var config = Configuration.Instance;

                downloadValidation(libraryBook);

                var api = await libraryBook.GetApiAsync();
                var contentLic = await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId);
                var audiobookDlLic = BuildDownloadOptions(config, contentLic);

                var outFileName = AudibleFileStorage.Audio.GetInProgressFilename(libraryBook, audiobookDlLic.OutputFormat.ToString().ToLower());
                var cacheDir = AudibleFileStorage.DownloadsInProgressDirectory;

                if (contentLic.DrmType != AudibleApi.Common.DrmType.Adrm)
                    abDownloader = new UnencryptedAudiobookDownloader(outFileName, cacheDir, audiobookDlLic);
                else
                {
                    AaxcDownloadConvertBase converter
                        = config.SplitFilesByChapter ? new AaxcDownloadMultiConverter(
                            outFileName, cacheDir, audiobookDlLic,
                            AudibleFileStorage.Audio.CreateMultipartRenamerFunc(libraryBook))
                        : new AaxcDownloadSingleConverter(outFileName, cacheDir, audiobookDlLic);

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
                var success = await Task.Run(abDownloader.Run);

                return success;
            }
            finally
            {
                OnStreamingCompleted($"Completed downloading and decrypting {libraryBook.Book.Title}");
            }
        }

        private static DownloadOptions BuildDownloadOptions(Configuration config, AudibleApi.Common.ContentLicense contentLic)
        {
            //I assume if ContentFormat == "MPEG" that the delivered file is an unencrypted mp3.
            //I also assume that if DrmType != Adrm, the file will be an mp3.
            //These assumptions may be wrong, and only time and bug reports will tell.

            bool encrypted = contentLic.DrmType == AudibleApi.Common.DrmType.Adrm;

            var outputFormat = !encrypted || (config.AllowLibationFixup && config.DecryptToLossy) ?
                     OutputFormat.Mp3 : OutputFormat.M4b;

            var audiobookDlLic = new DownloadOptions
                 (
                 contentLic?.ContentMetadata?.ContentUrl?.OfflineUrl,
                 Resources.USER_AGENT
                 )
            {
                AudibleKey = contentLic?.Voucher?.Key,
                AudibleIV = contentLic?.Voucher?.Iv,
                OutputFormat = outputFormat,
                TrimOutputToChapterLength = config.AllowLibationFixup && config.StripAudibleBrandAudio,
                RetainEncryptedFile = config.RetainAaxFile && encrypted,
                StripUnabridged = config.AllowLibationFixup && config.StripUnabridged,
                Downsample = config.AllowLibationFixup && config.LameDownsampleMono,
                MatchSourceBitrate = config.AllowLibationFixup && config.LameMatchSourceBR && config.LameTargetBitrate,
                CreateCueSheet = config.CreateCueSheet
            };

            if (config.AllowLibationFixup || outputFormat == OutputFormat.Mp3)
            {

                long startMs = audiobookDlLic.TrimOutputToChapterLength ?
                    contentLic.ContentMetadata.ChapterInfo.BrandIntroDurationMs : 0;

                audiobookDlLic.ChapterInfo = new AAXClean.ChapterInfo(TimeSpan.FromMilliseconds(startMs));

                for (int i = 0; i < contentLic.ContentMetadata.ChapterInfo.Chapters.Length; i++)
                {
                    var chapter = contentLic.ContentMetadata.ChapterInfo.Chapters[i];
                    long chapLenMs = chapter.LengthMs;

                    if (i == 0)
                        chapLenMs -= startMs;

                    if (config.StripAudibleBrandAudio && i == contentLic.ContentMetadata.ChapterInfo.Chapters.Length - 1)
                        chapLenMs -= contentLic.ContentMetadata.ChapterInfo.BrandOutroDurationMs;

                    audiobookDlLic.ChapterInfo.AddChapter(chapter.Title, TimeSpan.FromMilliseconds(chapLenMs));
                }
            }

            audiobookDlLic.LameConfig = new();
            audiobookDlLic.LameConfig.Mode = NAudio.Lame.MPEGMode.Mono;

            if (config.LameTargetBitrate)
			{
                if (config.LameConstantBitrate)
                    audiobookDlLic.LameConfig.BitRate = config.LameBitrate;
                else
                {
                    audiobookDlLic.LameConfig.ABRRateKbps = config.LameBitrate;
                    audiobookDlLic.LameConfig.VBR = NAudio.Lame.VBRMode.ABR;
                    audiobookDlLic.LameConfig.WriteVBRTag = true;
                }
			}
			else
			{
                audiobookDlLic.LameConfig.VBR = NAudio.Lame.VBRMode.Default;
                audiobookDlLic.LameConfig.VBRQuality = config.LameVBRQuality;
                audiobookDlLic.LameConfig.WriteVBRTag = true;
            }

            return audiobookDlLic;
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

        private void DownloadCoverArt(LibraryBook libraryBook)
		{
            var destinationDir = AudibleFileStorage.Audio.GetDestinationDirectory(libraryBook);
            var coverPath = AudibleFileStorage.Audio.GetBooksDirectoryFilename(libraryBook, ".jpg");
            coverPath = Path.Combine(destinationDir, Path.GetFileName(coverPath));

            try
            {
                if (File.Exists(coverPath)) 
                    FileUtility.SaferDelete(coverPath);

                (string picId, PictureSize size) = libraryBook.Book.PictureLarge is null ?
                    (libraryBook.Book.PictureId, PictureSize.Native) :
                    (libraryBook.Book.PictureLarge, PictureSize.Native);

                var picBytes = PictureStorage.GetPictureSynchronously(new PictureDefinition(picId, size));
                
                if (picBytes.Length > 0)
                    File.WriteAllBytes(coverPath, picBytes);
            }
            catch (Exception ex)
            {
                //Failure to download cover art should not be
                //considered a failure to download the book
                Serilog.Log.Logger.Error(ex, $"Error downloading cover art of {libraryBook.Book.AudibleProductId} to {coverPath} catalog product.");
            }
        }

	}
}
