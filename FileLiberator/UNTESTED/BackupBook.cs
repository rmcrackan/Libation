using System;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileManager;

namespace FileLiberator
{
    /// <summary>
    /// Download DRM book and decrypt audiobook files
    /// 
    /// Processes:
    /// Download: download aax file: the DRM encrypted audiobook
    /// Decrypt: remove DRM encryption from audiobook. Store final book
    /// Backup: perform all steps (downloaded, decrypt) still needed to get final book
    /// </summary>
    public class BackupBook : IProcessable
    {
        public event EventHandler<string> Begin;
        public event EventHandler<string> StatusUpdate;
        public event EventHandler<string> Completed;

        public DownloadBook DownloadBook { get; } = new DownloadBook();
        public DecryptBook DecryptBook { get; } = new DecryptBook();
		public DownloadPdf DownloadPdf { get; } = new DownloadPdf();

		// ValidateAsync() doesn't need UI context
		public async Task<bool> ValidateAsync(LibraryBook libraryBook)
            => await validateAsync_ConfigureAwaitFalse(libraryBook.Book.AudibleProductId).ConfigureAwait(false);
        private async Task<bool> validateAsync_ConfigureAwaitFalse(string productId)
            => !await AudibleFileStorage.Audio.ExistsAsync(productId);

        // do NOT use ConfigureAwait(false) on ProcessUnregistered()
        // often does a lot with forms in the UI context
        public async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
			var productId = libraryBook.Book.AudibleProductId;
            var displayMessage = $"[{productId}] {libraryBook.Book.Title}";

            Begin?.Invoke(this, displayMessage);

            try
			{
				{
					var statusHandler = await processAsync(libraryBook, AudibleFileStorage.AAX, DownloadBook);
					if (statusHandler.Any())
						return statusHandler;
				}

				{
					var statusHandler = await processAsync(libraryBook, AudibleFileStorage.Audio, DecryptBook);
					if (statusHandler.Any())
						return statusHandler;
				}

				{
					var statusHandler = await processAsync(libraryBook, AudibleFileStorage.PDF, DownloadPdf);
					if (statusHandler.Any())
						return statusHandler;
				}

				return new StatusHandler();
			}
			finally
            {
                Completed?.Invoke(this, displayMessage);
            }
        }

		private static async Task<StatusHandler> processAsync(LibraryBook libraryBook, AudibleFileStorage afs, IProcessable processable)
			=> !await afs.ExistsAsync(libraryBook.Book.AudibleProductId)
			? await processable.ProcessAsync(libraryBook)
			: new StatusHandler();
	}
}
