using AAXClean;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using Dinah.Core.IO;
using Dinah.Core.Net.Http;
using FileManager;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileLiberator
{
    public class ConvertToMp3 : IAudioDecodable
    {

        private Mp4File m4bBook;

		public event EventHandler<TimeSpan> StreamingTimeRemaining;
		public event EventHandler<Action<byte[]>> RequestCoverArt;
		public event EventHandler<string> TitleDiscovered;
		public event EventHandler<string> AuthorsDiscovered;
		public event EventHandler<string> NarratorsDiscovered;
		public event EventHandler<byte[]> CoverImageFilepathDiscovered;
		public event EventHandler<string> StreamingBegin;
		public event EventHandler<DownloadProgress> StreamingProgressChanged;
		public event EventHandler<string> StreamingCompleted;
		public event EventHandler<LibraryBook> Begin;
		public event EventHandler<string> StatusUpdate;
		public event EventHandler<LibraryBook> Completed;

        private long fileSize;
		private string Mp3FileName(string m4bPath) => m4bPath is null ? string.Empty : PathLib.ReplaceExtension(m4bPath, ".mp3");

        public void Cancel() => m4bBook?.Cancel();

        public bool Validate(LibraryBook libraryBook)
        {
            var path = ApplicationServices.TransitionalFileLocator.Audio_GetPath(libraryBook.Book);
            return path?.ToLower()?.EndsWith(".m4b") == true && !File.Exists(Mp3FileName(path));
        }

        public async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            Begin?.Invoke(this, libraryBook);

            StreamingBegin?.Invoke(this, $"Begin converting {libraryBook} to mp3");

            try
            {
                var m4bPath = ApplicationServices.TransitionalFileLocator.Audio_GetPath(libraryBook.Book);
                m4bBook = new Mp4File(m4bPath, FileAccess.Read);
                m4bBook.ConversionProgressUpdate += M4bBook_ConversionProgressUpdate;

                fileSize = m4bBook.InputStream.Length;

                TitleDiscovered?.Invoke(this, m4bBook.AppleTags.Title);
                AuthorsDiscovered?.Invoke(this, m4bBook.AppleTags.FirstAuthor);
                NarratorsDiscovered?.Invoke(this, m4bBook.AppleTags.Narrator);
                CoverImageFilepathDiscovered?.Invoke(this, m4bBook.AppleTags.Cover);

                using var mp3File = File.OpenWrite(Path.GetTempFileName());

                var result = await Task.Run(() => m4bBook.ConvertToMp3(mp3File));
                m4bBook.InputStream.Close();
                mp3File.Close();

                var mp3Path = Mp3FileName(m4bPath);

                FileExt.SafeMove(mp3File.Name, mp3Path);

                var statusHandler = new StatusHandler();

                if (result == ConversionResult.Failed)
                    statusHandler.AddError("Conversion failed");

                return statusHandler;
            }
            finally
            {
                StreamingCompleted?.Invoke(this, $"Completed converting to mp3: {libraryBook.Book.Title}");
                Completed?.Invoke(this, libraryBook);
            }
        }

        private void M4bBook_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
        {
            var duration = m4bBook.Duration;
            double remainingSecsToProcess = (duration - e.ProcessPosition).TotalSeconds;
            double estTimeRemaining = remainingSecsToProcess / e.ProcessSpeed;

            if (double.IsNormal(estTimeRemaining))
                StreamingTimeRemaining?.Invoke(this, TimeSpan.FromSeconds(estTimeRemaining));

            double progressPercent = 100 * e.ProcessPosition.TotalSeconds / duration.TotalSeconds;

            StreamingProgressChanged?.Invoke(this,
                new DownloadProgress
                {
                    ProgressPercentage = progressPercent,
                    BytesReceived = (long)(fileSize * progressPercent),
                    TotalBytesToReceive = fileSize
                });
        }
    }
}
