using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using ScrapingDomainServices;

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
            backupBook.Download.Completed += SetBackupCountsAsync;
            backupBook.Decrypt.Completed += SetBackupCountsAsync;
            await backupBook.ProcessValidateLibraryBookAsync(libraryBook);
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
            backupBook.Download.Completed += SetBackupCountsAsync;
            backupBook.Decrypt.Completed += SetBackupCountsAsync;
            await backupBook.ProcessFirstValidAsync();
        }

        async void SetBackupCountsAsync(object obj, string str) => throw new NotImplementedException();
    }
}
