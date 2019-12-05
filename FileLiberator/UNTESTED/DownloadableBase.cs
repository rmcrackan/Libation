using System;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
	public abstract class DownloadableBase : IDownloadable
	{
		public event EventHandler<LibraryBook> Begin;
		public event EventHandler<LibraryBook> Completed;

		public event EventHandler<string> DownloadBegin;
		public event EventHandler<DownloadProgress> DownloadProgressChanged;
		public event EventHandler<string> DownloadCompleted;

		public event EventHandler<string> StatusUpdate;
		protected void Invoke_StatusUpdate(string message) => StatusUpdate?.Invoke(this, message);

        public abstract bool Validate(LibraryBook libraryBook);

        public abstract Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook);

		// do NOT use ConfigureAwait(false) on ProcessAsync()
		// often calls events which prints to forms in the UI context
		public async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            Begin?.Invoke(this, libraryBook);

            try
            {
                return await ProcessItemAsync(libraryBook);
            }
            finally
            {
                Completed?.Invoke(this, libraryBook);
            }
        }

		protected async Task<string> PerformDownloadAsync(string proposedDownloadFilePath, Func<Progress<DownloadProgress>, Task<string>> func)
		{
			var progress = new Progress<DownloadProgress>();
			progress.ProgressChanged += (_, e) => DownloadProgressChanged?.Invoke(this, e);

			DownloadBegin?.Invoke(this, proposedDownloadFilePath);

			try
			{
				var result = await func(progress);
				StatusUpdate?.Invoke(this, result);

				return result;
			}
			finally
			{
				DownloadCompleted?.Invoke(this, proposedDownloadFilePath);
			}
		}
	}
}
