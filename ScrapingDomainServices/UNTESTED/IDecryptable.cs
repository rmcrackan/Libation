using System;

namespace ScrapingDomainServices
{
    public interface IDecryptable : IProcessable
    {
        event EventHandler<string> DecryptBegin;

        event EventHandler<string> TitleDiscovered;
        event EventHandler<string> AuthorsDiscovered;
        event EventHandler<string> NarratorsDiscovered;
        event EventHandler<byte[]> CoverImageFilepathDiscovered;
        event EventHandler<int> UpdateProgress;

        event EventHandler<string> DecryptCompleted;
    }
}
