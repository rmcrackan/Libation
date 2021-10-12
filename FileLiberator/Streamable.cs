using System;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
    public abstract class Streamable
    {
        public event EventHandler<string> StreamingBegin;
        public event EventHandler<DownloadProgress> StreamingProgressChanged;
        public event EventHandler<TimeSpan> StreamingTimeRemaining;
        public event EventHandler<string> StreamingCompleted;
        /// <summary>Fired when a file is successfully saved to disk</summary>
        public event EventHandler<(string id, string path)> FileCreated;

        protected void OnStreamingBegin(string filePath)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(StreamingBegin), Message = filePath });
            StreamingBegin?.Invoke(this, filePath);
        }

        protected void OnStreamingProgressChanged(DownloadProgress progress)
        {
            StreamingProgressChanged?.Invoke(this, progress);
        }

        protected void OnStreamingTimeRemaining(TimeSpan timeRemaining)
        {
            StreamingTimeRemaining?.Invoke(this, timeRemaining);
        }

        protected void OnStreamingCompleted(string filePath)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(StreamingCompleted), Message = filePath });
            StreamingCompleted?.Invoke(this, filePath);
        }

        protected void OnFileCreated(DataLayer.LibraryBook libraryBook, string path) => OnFileCreated(libraryBook.Book.AudibleProductId, path);
        protected void OnFileCreated(string id, string path)
        {
            Serilog.Log.Logger.Information("File created {@DebugInfo}", new { Name = nameof(FileCreated), id, path });
            LibationFileManager.FilePathCache.Insert(id, path);
            FileCreated?.Invoke(this, (id, path));
        }
    }
}
