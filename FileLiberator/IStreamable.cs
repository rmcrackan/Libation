using System;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
    public interface IStreamable : IFileLiberator
    {
        event EventHandler<string> StreamingBegin;
		event EventHandler<DownloadProgress> StreamingProgressChanged;
        event EventHandler<TimeSpan> StreamingTimeRemaining;
        event EventHandler<string> StreamingCompleted;
    }
}
