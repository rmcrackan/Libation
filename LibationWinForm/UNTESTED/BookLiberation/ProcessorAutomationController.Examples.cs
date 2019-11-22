using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileLiberator;

namespace LibationWinForm.BookLiberation
{
    public class BookLiberatorControllerExamples
    {
        async Task BackupBookAsync(string productId)
        {
			using var context = LibationContext.Create();

            var libraryBook = context
                .Library
                .GetLibrary()
                .SingleOrDefault(lb => lb.Book.AudibleProductId == productId);

            if (libraryBook == null)
                return;

            var backupBook = new BackupBook();
            backupBook.DownloadBook.Completed += SetBackupCountsAsync;
            backupBook.DecryptBook.Completed += SetBackupCountsAsync;
            await ProcessValidateLibraryBookAsync(backupBook, libraryBook);
		}

		static async Task<StatusHandler> ProcessValidateLibraryBookAsync(IProcessable processable, LibraryBook libraryBook)
		{
			if (!processable.Validate(libraryBook))
				return new StatusHandler { "Validation failed" };
			return await processable.ProcessAsync(libraryBook);
		}

		// Download First Book (Download encrypted/DRM file)
		async Task DownloadFirstBookAsync()
        {
            var downloadBook = ProcessorAutomationController.GetWiredUpDownloadBook();
            downloadBook.Completed += SetBackupCountsAsync;
            await downloadBook.ProcessFirstValidAsync();
        }

        // Decrypt First Book (Remove DRM from downloaded file)
        async Task DecryptFirstBookAsync()
        {
            var decryptBook = ProcessorAutomationController.GetWiredUpDecryptBook();
            decryptBook.Completed += SetBackupCountsAsync;
            await decryptBook.ProcessFirstValidAsync();
        }

        // Backup First Book (Decrypt a non-liberated book. Download if needed)
        async Task BackupFirstBookAsync()
        {
            var backupBook = ProcessorAutomationController.GetWiredUpBackupBook();
            backupBook.DownloadBook.Completed += SetBackupCountsAsync;
            backupBook.DecryptBook.Completed += SetBackupCountsAsync;
            await backupBook.ProcessFirstValidAsync();
        }

        async void SetBackupCountsAsync(object obj, string str) => throw new NotImplementedException();
    }
}
