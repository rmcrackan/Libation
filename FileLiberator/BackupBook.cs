using System;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;

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
        public event EventHandler<LibraryBook> Begin;
        public event EventHandler<string> StatusUpdate;
        public event EventHandler<LibraryBook> Completed;

        public DownloadDecryptBook DownloadDecryptBook { get; }
		public DownloadPdf DownloadPdf { get; }

        public BackupBook(DownloadDecryptBook downloadDecryptBook, DownloadPdf downloadPdf)
		{
            DownloadDecryptBook = downloadDecryptBook;
            DownloadPdf = downloadPdf;
        }

        public bool Validate(LibraryBook libraryBook)
            => !ApplicationServices.TransitionalFileLocator.Audio_Exists(libraryBook.Book);

		// do NOT use ConfigureAwait(false) on ProcessAsync()
		// often calls events which prints to forms in the UI context
		public async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
        {
            Begin?.Invoke(this, libraryBook);

            try
			{
				{
                    var statusHandler = await DownloadDecryptBook.TryProcessAsync(libraryBook);
                    if (statusHandler.HasErrors)
						return statusHandler;
				}

                {
                    var statusHandler = await DownloadPdf.TryProcessAsync(libraryBook);
					if (statusHandler.HasErrors)
						return statusHandler;
                }

				return new StatusHandler();
			}
			finally
            {
                Completed?.Invoke(this, libraryBook);
            }
        }
	}
}
