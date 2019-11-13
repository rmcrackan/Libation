using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileManager;

namespace FileLiberator
{
	public class DownloadPdf : DownloadableBase
	{
		static DownloadPdf()
		{
			// https://stackoverflow.com/a/15483698
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
		}

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
				throw new Exception("Multiple PDF downloads are not currently supported. Typically indicates an error");

			var destinationDir = await getDestinationDirectoryAsync(product.AudibleProductId);
			if (destinationDir == null)
				return new StatusHandler { "Destination directory not found for PDF download" };

			var url = urls.Single();
			var destinationFilename = Path.Combine(destinationDir, Path.GetFileName(url));
			await performDownloadAsync(url, destinationFilename);

			var statusHandler = new StatusHandler();
			var exists = await AudibleFileStorage.PDF.ExistsAsync(product.AudibleProductId);
			if (!exists)
				statusHandler.AddError("Downloaded PDF cannot be found");
			return statusHandler;
		}

		private async Task<string> getDestinationDirectoryAsync(string productId)
		{
			// if audio file exists, get it's dir
			var audioFile = await AudibleFileStorage.Audio.GetAsync(productId);
			if (audioFile != null)
				return Path.GetDirectoryName(audioFile);

			// else return base Book dir
			return AudibleFileStorage.PDF.StorageDirectory;
		}

		// other user agents from my chrome. from: https://www.whoishostingthis.com/tools/user-agent/
		private static string[] userAgents { get; } = new[]
		{
			"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.96 Safari/537.36",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.87 Safari/537.36",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.97 Safari/537.36",
		};

		private async Task performDownloadAsync(string url, string destinationFilename)
		{
			using var webClient = new WebClient();

			var userAgentIndex = new Random().Next(0, userAgents.Length); // upper bound is exclusive
			webClient.Headers["User-Agent"] = userAgents[userAgentIndex];
			webClient.Headers["Referer"] = "https://google.com";
			webClient.Headers["Upgrade-Insecure-Requests"] = "1";
			webClient.Headers["DNT"] = "1";
			webClient.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
			webClient.Headers["Accept-Language"] = "en-US,en;q=0.9";

			webClient.DownloadProgressChanged += (s, e) => Invoke_DownloadProgressChanged(s, new Dinah.Core.Net.Http.DownloadProgress { BytesReceived = e.BytesReceived, ProgressPercentage = e.ProgressPercentage, TotalBytesToReceive = e.TotalBytesToReceive });
			webClient.DownloadFileCompleted += (s, e) => Invoke_DownloadCompleted(s, $"Completed: {destinationFilename}");
			webClient.DownloadDataCompleted += (s, e) => Invoke_DownloadCompleted(s, $"Completed: {destinationFilename}");
			webClient.DownloadStringCompleted += (s, e) => Invoke_DownloadCompleted(s, $"Completed: {destinationFilename}");

			Invoke_DownloadBegin(destinationFilename);

			await webClient.DownloadFileTaskAsync(url, destinationFilename);
		}
	}
}
