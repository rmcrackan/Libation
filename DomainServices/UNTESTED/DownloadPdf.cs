using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileManager;

namespace DomainServices
{
    public class DownloadPdf : DownloadableBase
    {
        public override async Task<bool> ValidateAsync(LibraryBook libraryBook)
        {
            var product = libraryBook.Book;

            if (!product.Supplements.Any())
                return false;

            return !await AudibleFileStorage.PDF.ExistsAsync(product.AudibleProductId);
        }

        public override async Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook)
        {
            var product = libraryBook.Book;

            if (product == null)
                return new StatusHandler { "Book not found" };

            var urls = product.Supplements.Select(d => d.Url).ToList();
            if (urls.Count == 0)
                return new StatusHandler { "PDF download url not found" };

            // sanity check
            if (urls.Count > 1)
                throw new Exception("Multiple PDF downloads are not currently supported. typically indicates an error");

            var url = urls.Single();

            var destinationDir = await getDestinationDirectory(product.AudibleProductId);
            if (destinationDir == null)
                return new StatusHandler { "Destination directory not found for PDF download" };

            var destinationFilename = Path.Combine(destinationDir, Path.GetFileName(url));

            using (var webClient = await GetWebClient(destinationFilename))
                await webClient.DownloadFileTaskAsync(url, destinationFilename);

            var statusHandler = new StatusHandler();
            var exists = await AudibleFileStorage.PDF.ExistsAsync(product.AudibleProductId);
            if (!exists)
                statusHandler.AddError("Downloaded PDF cannot be found");
            return statusHandler;
        }

        private async Task<string> getDestinationDirectory(string productId)
        {
            // if audio file exists, get it's dir
            var audioFile = await AudibleFileStorage.Audio.GetAsync(productId);
            if (audioFile != null)
                return Path.GetDirectoryName(audioFile);

            // else return base Book dir
            return AudibleFileStorage.PDF.StorageDirectory;
        }
    }
}
