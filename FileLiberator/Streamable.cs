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
        public void OnStreamingBegin(string filePath)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(StreamingBegin), Message = filePath });
            StreamingBegin?.Invoke(this, filePath);
        }
        public void OnStreamingCompleted(string filePath)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(StreamingCompleted), Message = filePath });
            StreamingCompleted?.Invoke(this, filePath);
        }
        public void OnStreamingProgressChanged(DownloadProgress progress)
        {
            StreamingProgressChanged?.Invoke(this, progress);
        }
        public void OnStreamingTimeRemaining(TimeSpan timeRemaining)
        {
            StreamingTimeRemaining?.Invoke(this, timeRemaining);
        }
    }
}
