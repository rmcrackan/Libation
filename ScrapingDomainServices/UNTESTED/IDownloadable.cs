using System;
using System.Net;

namespace DomainServices
{
    public interface IDownloadable : IProcessable
    {
        event EventHandler<string> DownloadBegin;
        event DownloadProgressChangedEventHandler DownloadProgressChanged;
        event EventHandler<string> DownloadCompleted;
    }
}
