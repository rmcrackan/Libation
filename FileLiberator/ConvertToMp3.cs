using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAXClean;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using Dinah.Core.IO;
using Dinah.Core.Net.Http;
using LibationFileManager;

namespace FileLiberator
{
    public class ConvertToMp3 : AudioDecodable
    {
        private Mp4File m4bBook;

		private long fileSize;
		private static string Mp3FileName(string m4bPath) => m4bPath is null ? string.Empty : PathLib.ReplaceExtension(m4bPath, ".mp3");

        public override void Cancel() => m4bBook?.Cancel();

        public override bool Validate(LibraryBook libraryBook)
        {
            var path = AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId);
            return path?.ToLower()?.EndsWith(".m4b") == true && !File.Exists(Mp3FileName(path));
        }

        public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            OnBegin(libraryBook);

            OnStreamingBegin($"Begin converting {libraryBook} to mp3");

            try
            {
                var m4bPath = AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId);
                m4bBook = new Mp4File(m4bPath, FileAccess.Read);
                m4bBook.ConversionProgressUpdate += M4bBook_ConversionProgressUpdate;

                fileSize = m4bBook.InputStream.Length;

                OnTitleDiscovered(m4bBook.AppleTags.Title);
                OnAuthorsDiscovered(m4bBook.AppleTags.FirstAuthor);
                OnNarratorsDiscovered(m4bBook.AppleTags.Narrator);
                OnCoverImageDiscovered(m4bBook.AppleTags.Cover);

                using var mp3File = File.OpenWrite(Path.GetTempFileName());

                var result = await Task.Run(() => m4bBook.ConvertToMp3(mp3File));
                m4bBook.InputStream.Close();
                mp3File.Close();

                var mp3Path = Mp3FileName(m4bPath);

                FileExt.SafeMove(mp3File.Name, mp3Path);
                OnFileCreated(libraryBook.Book.AudibleProductId, mp3Path);

                var statusHandler = new StatusHandler();

                if (result == ConversionResult.Failed)
                    statusHandler.AddError("Conversion failed");

                return statusHandler;
            }
            finally
            {
                OnStreamingCompleted($"Completed converting to mp3: {libraryBook.Book.Title}");
                OnCompleted(libraryBook);
            }
        }

        private void M4bBook_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
        {
            var duration = m4bBook.Duration;
            double remainingSecsToProcess = (duration - e.ProcessPosition).TotalSeconds;
            double estTimeRemaining = remainingSecsToProcess / e.ProcessSpeed;

            if (double.IsNormal(estTimeRemaining))
                OnStreamingTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

            double progressPercent = 100 * e.ProcessPosition.TotalSeconds / duration.TotalSeconds;

            OnStreamingProgressChanged(
                new DownloadProgress
                {
                    ProgressPercentage = progressPercent,
                    BytesReceived = (long)(fileSize * progressPercent),
                    TotalBytesToReceive = fileSize
                });
        }
    }
}
