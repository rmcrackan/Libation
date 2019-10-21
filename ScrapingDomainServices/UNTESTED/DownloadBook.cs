using System;
using System.IO;
using System.Threading.Tasks;
using FileManager;
using DataLayer;
using Dinah.Core.ErrorHandling;

namespace ScrapingDomainServices
{
    /// <summary>
    /// Download DRM book and decrypt audiobook files.
    /// 
    /// Processes:
    /// Download: download aax file: the DRM encrypted audiobook
    /// Decrypt: remove DRM encryption from audiobook. Store final book
    /// Backup: perform all steps (downloaded, decrypt) still needed to get final book
    /// </summary>
    public class DownloadBook : DownloadableBase
    {
        public override async Task<bool> ValidateAsync(LibraryBook libraryBook)
            => !await AudibleFileStorage.Audio.ExistsAsync(libraryBook.Book.AudibleProductId)
            && !await AudibleFileStorage.AAX.ExistsAsync(libraryBook.Book.AudibleProductId);

        public override async Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook)
        {
            var tempAaxFilename = FileUtility.GetValidFilename(
                AudibleFileStorage.DownloadsInProgress,
                libraryBook.Book.Title,
                "aax",
                libraryBook.Book.AudibleProductId);

            // if getting from full title:
            // '?' is allowed
            // colons are inconsistent but not problematic to just leave them
            // - 1 colon: sometimes full title is used. sometimes only the part before the colon is used
            // - multple colons: only the part before the final colon is used
            //   e.g. Alien: Out of the Shadows: An Audible Original Drama => Alien: Out of the Shadows
            // in cases where title includes '&', just use everything before the '&' and ignore the rest
            //// var adhTitle = product.Title.Split('&')[0]

            var aaxDownloadLink = libraryBook.DownloadBookLink
                .Replace("/admhelper", "")
                .Replace("&DownloadType=Now", "")
                + "&asin=&source=audible_adm&size=&browser_type=&assemble_url=http://cds.audible.com/download";
            var uri = new Uri(aaxDownloadLink);

			using var webClient = await GetWebClient(tempAaxFilename);
            // for book downloads only: pretend to be the audible download manager. from inAudible:
            webClient.Headers["User-Agent"] = "Audible ADM 6.6.0.15;Windows Vista Service Pack 1 Build 7601";
            await webClient.DownloadFileTaskAsync(uri, tempAaxFilename);

            // move
            var aaxFilename = FileUtility.GetValidFilename(
                AudibleFileStorage.DownloadsFinal,
                libraryBook.Book.Title,
                "aax",
                libraryBook.Book.AudibleProductId);
            File.Move(tempAaxFilename, aaxFilename);

            var statusHandler = new StatusHandler();
            var isDownloaded = await AudibleFileStorage.AAX.ExistsAsync(libraryBook.Book.AudibleProductId);
            if (isDownloaded)
                DoStatusUpdate($"Downloaded: {aaxFilename}");
            else
                statusHandler.AddError("Downloaded AAX file cannot be found");
            return statusHandler;
        }
    }
}
