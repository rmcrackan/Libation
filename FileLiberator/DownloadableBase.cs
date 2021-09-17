using System;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
	public abstract class DownloadableBase : IProcessable
	{
		public event EventHandler<LibraryBook> Begin;
		public event EventHandler<LibraryBook> Completed;

		public event EventHandler<string> StreamingBegin;
		public event EventHandler<DownloadProgress> StreamingProgressChanged;
		public event EventHandler<string> StreamingCompleted;

		public event EventHandler<string> StatusUpdate;
		public event EventHandler<TimeSpan> StreamingTimeRemaining;

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

		protected static Task<InternalUtilities.ApiExtended> GetApiExtendedAsync(LibraryBook libraryBook)
			=> InternalUtilities.ApiExtended.CreateAsync(libraryBook.Account, libraryBook.Book.Locale);

		protected async Task<string> PerformDownloadAsync(string proposedDownloadFilePath, Func<Progress<DownloadProgress>, Task<string>> func)
		{
			var progress = new Progress<DownloadProgress>();
			progress.ProgressChanged += (_, e) => StreamingProgressChanged?.Invoke(this, e);

			StreamingBegin?.Invoke(this, proposedDownloadFilePath);

			try
			{
				var result = await func(progress);
				StatusUpdate?.Invoke(this, result);

				return result;
			}
			finally
			{
				StreamingCompleted?.Invoke(this, proposedDownloadFilePath);
			}
		}
	}
}
