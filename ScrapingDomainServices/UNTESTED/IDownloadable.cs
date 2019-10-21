using System;
using System.Net;

namespace ScrapingDomainServices
{
    public interface IDownloadable : IProcessable
    {
        event EventHandler<string> DownloadBegin;
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        event EventHandler<string> DownloadCompleted;
    }
}
