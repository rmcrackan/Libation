using System;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;

namespace FileLiberator
{
	public abstract class DownloadableBase : IDownloadable
	{
		public event EventHandler<string> Begin;
		public event EventHandler<string> Completed;

		public event EventHandler<string> StatusUpdate;
		public event EventHandler<string> DownloadBegin;
		public event EventHandler<Dinah.Core.Net.Http.DownloadProgress> DownloadProgressChanged;
		public event EventHandler<string> DownloadCompleted;
		protected void Invoke_StatusUpdate(string message) => StatusUpdate?.Invoke(this, message);
		protected void Invoke_DownloadBegin(string downloadMessage) => DownloadBegin?.Invoke(this, downloadMessage);
		protected void Invoke_DownloadProgressChanged(object sender, Dinah.Core.Net.Http.DownloadProgress progress) => DownloadProgressChanged?.Invoke(sender, progress);
		protected void Invoke_DownloadCompleted(object sender, string str) => DownloadCompleted?.Invoke(sender, str);


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
    }
}
