using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudibleDotCom;

namespace AudibleDotComAutomation
{
    public interface IPageRetriever : IDisposable
    {
        Task<IEnumerable<AudiblePageSource>> GetPageSourcesAsync(AudiblePageType audiblePage, string pageId = null);
    }
}
