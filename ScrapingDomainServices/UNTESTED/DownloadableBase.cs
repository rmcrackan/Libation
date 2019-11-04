using System;
using System.Net;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Humanizer;

namespace ScrapingDomainServices
{
	public abstract class DownloadableBase : IDownloadable
	{
		public event EventHandler<string> Begin;

		public event EventHandler<string> StatusUpdate;
		protected void DoStatusUpdate(string message) => StatusUpdate?.Invoke(this, message);

		public event EventHandler<string> DownloadBegin;
		public event EventHandler<Dinah.Core.Net.Http.DownloadProgress> DownloadProgressChanged;
		public event EventHandler<string> DownloadCompleted;

		protected void Invoke_DownloadBegin(string downloadMessage) => DownloadBegin?.Invoke(this, downloadMessage);
		protected void Invoke_DownloadProgressChanged(object sender, Dinah.Core.Net.Http.DownloadProgress progress) => DownloadProgressChanged?.Invoke(sender, progress);
		protected void Invoke_DownloadCompleted(object sender, string str) => DownloadCompleted?.Invoke(sender, str);


		public event EventHandler<string> Completed;

        static DownloadableBase()
        {
            // https://stackoverflow.com/a/15483698
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }

        public abstract Task<bool> ValidateAsync(LibraryBook libraryBook);

        public abstract Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook);

        // do NOT use ConfigureAwait(false) on ProcessUnregistered()
        // often does a lot with forms in the UI context
        public async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            var displayMessage = $"[{libraryBook.Book.AudibleProductId}] {libraryBook.Book.Title}";

            Begin?.Invoke(this, displayMessage);

            try
            {
                return await ProcessItemAsync(libraryBook);
            }
            finally
            {
                Completed?.Invoke(this, displayMessage);
            }
        }

        // other user agents from my chrome. from: https://www.whoishostingthis.com/tools/user-agent/
        static string[] userAgents { get; } = new[]
        {
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/66.0.3359.139 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.96 Safari/537.36",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.87 Safari/537.36",
		};
        // we need a minimum delay between tries when hitting audible.com
        // in every case except decrypt (which is already long running), we hit audible.com
        static Humanizer humanizer { get; } = new Humanizer { Minimum = 5, Maximum = 20 };
        static Random rnd { get; } = new Random();
        protected async Task<WebClient> GetWebClientAsync(string downloadMessage)
        {
            await humanizer.Wait();

            var webClient = new WebClient();

            // https://towardsdatascience.com/5-strategies-to-write-unblock-able-web-scrapers-in-python-5e40c147bdaf
            var userAgentIndex = rnd.Next(0, userAgents.Length); // upper bound is exclusive
            webClient.Headers["User-Agent"] = userAgents[userAgentIndex];
            webClient.Headers["Referer"] = "https://google.com";
            webClient.Headers["Upgrade-Insecure-Requests"] = "1";
            webClient.Headers["DNT"] = "1";
            webClient.Headers["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
            webClient.Headers["Accept-Language"] = "en-US,en;q=0.9";

			// this breaks pdf download which uses: http://download.audible.com
			// weirdly, it works for book download even though it uses https://cds.audible.com
			//webClient.Headers["Host"] = "www.audible.com";

			webClient.DownloadProgressChanged += (s, e) => Invoke_DownloadProgressChanged(s, new Dinah.Core.Net.Http.DownloadProgress { BytesReceived = e.BytesReceived, ProgressPercentage = e.ProgressPercentage, TotalBytesToReceive = e.TotalBytesToReceive });
            webClient.DownloadFileCompleted += (s, e) => Invoke_DownloadCompleted(s, $"Completed: {downloadMessage}");
            webClient.DownloadDataCompleted += (s, e) => Invoke_DownloadCompleted(s, $"Completed: {downloadMessage}");
            webClient.DownloadStringCompleted += (s, e) => Invoke_DownloadCompleted(s, $"Completed: {downloadMessage}");

			Invoke_DownloadBegin(downloadMessage);

            return webClient;
        }
    }
}
