using System;

namespace FileLiberator
{
    public interface IDownloadable : IProcessable
    {
        event EventHandler<string> DownloadBegin;
        event EventHandler<Dinah.Core.Net.Http.DownloadProgress> DownloadProgressChanged;
        event EventHandler<string> DownloadCompleted;
    }
}
