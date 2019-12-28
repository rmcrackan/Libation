using System;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
    public interface IDownloadable 
    {
        event EventHandler<string> DownloadBegin;
        event EventHandler<DownloadProgress> DownloadProgressChanged;
        event EventHandler<string> DownloadCompleted;
    }
}
