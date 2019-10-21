using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InternalUtilities;
using AudibleDotCom;
using AudibleDotComAutomation;
using FileManager;
using Scraping;

namespace DomainServices
{
    public static class DownloadLibrary
    {
        /// <summary>scrape all library pages. save htm files. save json files</summary>
        /// <returns>paths of json files</returns>
        public static async Task<List<FileInfo>> DownloadLibraryAsync(IPageRetriever pageRetriever)
        {
            var batchName = WebpageStorage.GetLibraryBatchName();

            // library webpages => AudiblePageSource objects
            var libraryAudiblePageSources = await pageRetriever.GetPageSourcesAsync(AudiblePageType.Library);

            var jsonFiles = new List<FileInfo>();
            foreach (var libraryAudiblePageSource in libraryAudiblePageSources)
            {
                // good habit to persist htm before attempting to parse it. this way, if there's a parse error, we can test errors on a local copy
                var htmFile = DataConverter.AudiblePageSource_2_HtmFile_Batch(libraryAudiblePageSource, batchName);

                var libraryDTOs = AudibleScraper.ScrapeLibrarySources(libraryAudiblePageSource);
                var jsonFile = DataConverter.Value_2_JsonFile(libraryDTOs, Path.ChangeExtension(htmFile.FullName, "json"));

                jsonFiles.Add(jsonFile);
            }

            return jsonFiles;
        }
    }
}
