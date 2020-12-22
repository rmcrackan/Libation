using System;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using FileLiberator;

namespace LibationWinForms.BookLiberation
{
    public static class ProcessorAutomationController
    {
        public static async Task BackupSingleBookAsync(string productId, EventHandler<LibraryBook> completedAction = null)
        {
            Serilog.Log.Logger.Information("Begin " + nameof(BackupSingleBookAsync) + " {@DebugInfo}", new { productId });

            var backupBook = getWiredUpBackupBook(completedAction);

            var automatedBackupsForm = attachToBackupsForm(backupBook);
            automatedBackupsForm.KeepGoingVisible = false;

            await runSingleBackupAsync(backupBook, automatedBackupsForm, productId);
        }

        public static async Task BackupAllBooksAsync(EventHandler<LibraryBook> completedAction = null)
        {
            Serilog.Log.Logger.Information("Begin " + nameof(BackupAllBooksAsync));

            var backupBook = getWiredUpBackupBook(completedAction);

            var automatedBackupsForm = attachToBackupsForm(backupBook);

            await runBackupLoopAsync(backupBook, automatedBackupsForm);
        }

        private static BackupBook getWiredUpBackupBook(EventHandler<LibraryBook> completedAction)
        {
            var backupBook = new BackupBook();

            backupBook.DownloadBook.Begin += (_, __) => wireUpEvents(backupBook.DownloadBook);
            backupBook.DecryptBook.Begin += (_, __) => wireUpEvents(backupBook.DecryptBook);
			backupBook.DownloadPdf.Begin += (_, __) => wireUpEvents(backupBook.DownloadPdf);

            if (completedAction != null)
            {
                backupBook.DownloadBook.Completed += completedAction;
                backupBook.DecryptBook.Completed += completedAction;
                backupBook.DownloadPdf.Completed += completedAction;
            }

            return backupBook;
        }

        private static AutomatedBackupsForm attachToBackupsForm(BackupBook backupBook)
        {
            #region create form
            var automatedBackupsForm = new AutomatedBackupsForm();
            #endregion

            #region define how model actions will affect form behavior
            void downloadBookBegin(object _, LibraryBook libraryBook) => automatedBackupsForm.AppendText($"Download Step, Begin: {libraryBook.Book}");
            void statusUpdate(object _, string str) => automatedBackupsForm.AppendText("- " + str);
            void downloadBookCompleted(object _, LibraryBook libraryBook) => automatedBackupsForm.AppendText($"Download Step, Completed: {libraryBook.Book}");
            void decryptBookBegin(object _, LibraryBook libraryBook) => automatedBackupsForm.AppendText($"Decrypt Step, Begin: {libraryBook.Book}");
            // extra line after book is completely finished
            void decryptBookCompleted(object _, LibraryBook libraryBook) => automatedBackupsForm.AppendText($"Decrypt Step, Completed: {libraryBook.Book}{Environment.NewLine}");
            void downloadPdfBegin(object _, LibraryBook libraryBook) => automatedBackupsForm.AppendText($"PDF Step, Begin: {libraryBook.Book}");
            // extra line after book is completely finished
            void downloadPdfCompleted(object _, LibraryBook libraryBook) => automatedBackupsForm.AppendText($"PDF Step, Completed: {libraryBook.Book}{Environment.NewLine}");
            #endregion

            #region subscribe new form to model's events
            backupBook.DownloadBook.Begin += downloadBookBegin;
            backupBook.DownloadBook.StatusUpdate += statusUpdate;
            backupBook.DownloadBook.Completed += downloadBookCompleted;
            backupBook.DecryptBook.Begin += decryptBookBegin;
            backupBook.DecryptBook.StatusUpdate += statusUpdate;
            backupBook.DecryptBook.Completed += decryptBookCompleted;
            backupBook.DownloadPdf.Begin += downloadPdfBegin;
            backupBook.DownloadPdf.StatusUpdate += statusUpdate;
            backupBook.DownloadPdf.Completed += downloadPdfCompleted;
            #endregion

            #region when form closes, unsubscribe from model's events
            // unsubscribe so disposed forms aren't still trying to receive notifications
            automatedBackupsForm.FormClosing += (_, __) =>
            {
                backupBook.DownloadBook.Begin -= downloadBookBegin;
                backupBook.DownloadBook.StatusUpdate -= statusUpdate;
                backupBook.DownloadBook.Completed -= downloadBookCompleted;
                backupBook.DecryptBook.Begin -= decryptBookBegin;
                backupBook.DecryptBook.StatusUpdate -= statusUpdate;
                backupBook.DecryptBook.Completed -= decryptBookCompleted;
                backupBook.DownloadPdf.Begin -= downloadPdfBegin;
                backupBook.DownloadPdf.StatusUpdate -= statusUpdate;
                backupBook.DownloadPdf.Completed -= downloadPdfCompleted;
            };
            #endregion

            return automatedBackupsForm;
        }

        public static async Task BackupAllPdfsAsync(EventHandler<LibraryBook> completedAction = null)
        {
            Serilog.Log.Logger.Information("Begin " + nameof(BackupAllPdfsAsync));

            var downloadPdf = getWiredUpDownloadPdf(completedAction);

            var automatedBackupsForm = attachToBackupsForm(downloadPdf);

            await runBackupLoopAsync(downloadPdf, automatedBackupsForm);
        }

        private static DownloadPdf getWiredUpDownloadPdf(EventHandler<LibraryBook> completedAction)
        {
            var downloadPdf = new DownloadPdf();

            downloadPdf.Begin += (_, __) => wireUpEvents(downloadPdf);

            if (completedAction != null)
                downloadPdf.Completed += completedAction;

            return downloadPdf;
        }

        public static async Task DownloadFileAsync(string url, string destination)
        {
            var downloadFile = new DownloadFile();

            // frustratingly copy pasta from wireUpEvents(IDownloadable downloadable) due to Completed being EventHandler<LibraryBook>
            var downloadDialog = new DownloadForm();
            downloadFile.DownloadBegin += (_, str) =>
            {
                downloadDialog.UpdateFilename(str);
                downloadDialog.Show();
            };
            downloadFile.DownloadProgressChanged += (_, progress) => downloadDialog.DownloadProgressChanged(progress.BytesReceived, progress.TotalBytesToReceive);
            downloadFile.DownloadCompleted += (_, __) => downloadDialog.Close();

            await downloadFile.PerformDownloadFileAsync(url, destination);
        }

        // subscribed to Begin event because a new form should be created+processed+closed on each iteration
        private static void wireUpEvents(IDownloadableProcessable downloadable)
        {
            #region create form
            var downloadDialog = new DownloadForm();
            #endregion

            // extra complexity for wiring up download form:
            // case 1: download is needed
            //   dialog created. subscribe to events
            //   downloadable.DownloadBegin fires. shows dialog
            //   downloadable.DownloadCompleted fires. closes dialog. which fires FormClosing, FormClosed, Disposed
            //   Disposed unsubscribe from events
            // case 2: download is not needed
            //   dialog created. subscribe to events
            //   dialog is never shown nor closed
            //   downloadable.Completed fires. disposes dialog and unsubscribes from events

            #region define how model actions will affect form behavior
            void downloadBegin(object _, string str)
            {
                downloadDialog.UpdateFilename(str);
                downloadDialog.Show();
            }

            // close form on DOWNLOAD completed, not final Completed. Else for BackupBook this form won't close until DECRYPT is also complete
            void fileDownloadCompleted(object _, string __) => downloadDialog.Close();

            void downloadProgressChanged(object _, Dinah.Core.Net.Http.DownloadProgress progress)
                => downloadDialog.DownloadProgressChanged(progress.BytesReceived, progress.TotalBytesToReceive);

            void unsubscribe(object _ = null, EventArgs __ = null)
            {
                downloadable.DownloadBegin -= downloadBegin;
                downloadable.DownloadCompleted -= fileDownloadCompleted;
                downloadable.DownloadProgressChanged -= downloadProgressChanged;
                downloadable.Completed -= dialogDispose;
            }

            // unless we dispose, if the form is created but un-used/never-shown then weird UI stuff can happen
            // also, since event unsubscribe occurs on FormClosing and an unused form is never closed, then the events will never be unsubscribed
            void dialogDispose(object _, object __)
            {
                if (!downloadDialog.IsDisposed)
                    downloadDialog.Dispose();
            }
            #endregion

            #region subscribe new form to model's events
            downloadable.DownloadBegin += downloadBegin;
            downloadable.DownloadCompleted += fileDownloadCompleted;
            downloadable.DownloadProgressChanged += downloadProgressChanged;
            downloadable.Completed += dialogDispose;
            #endregion

            #region when form closes, unsubscribe from model's events
            // unsubscribe so disposed forms aren't still trying to receive notifications
            // FormClosing is more UI safe but won't fire unless the form is shown and closed
            //   if form was shown, Disposed will fire for FormClosing, FormClosed, and Disposed
            //   if not shown, it will still fire for Disposed
            downloadDialog.Disposed += unsubscribe;
            #endregion
        }

        // subscribed to Begin event because a new form should be created+processed+closed on each iteration
        private static void wireUpEvents(IDecryptable decryptBook)
        {
            #region create form
            var decryptDialog = new DecryptForm();
            #endregion

            #region define how model actions will affect form behavior
            void decryptBegin(object _, string __) => decryptDialog.Show();

            void titleDiscovered(object _, string title) => decryptDialog.SetTitle(title);
            void authorsDiscovered(object _, string authors) => decryptDialog.SetAuthorNames(authors);
            void narratorsDiscovered(object _, string narrators) => decryptDialog.SetNarratorNames(narrators);
            void coverImageFilepathDiscovered(object _, byte[] coverBytes) => decryptDialog.SetCoverImage(coverBytes);
            void updateProgress(object _, int percentage) => decryptDialog.UpdateProgress(percentage);

            void decryptCompleted(object _, string __) => decryptDialog.Close();
            #endregion

            #region subscribe new form to model's events
            decryptBook.DecryptBegin += decryptBegin;

            decryptBook.TitleDiscovered += titleDiscovered;
            decryptBook.AuthorsDiscovered += authorsDiscovered;
            decryptBook.NarratorsDiscovered += narratorsDiscovered;
            decryptBook.CoverImageFilepathDiscovered += coverImageFilepathDiscovered;
            decryptBook.UpdateProgress += updateProgress;

            decryptBook.DecryptCompleted += decryptCompleted;
            #endregion

            #region when form closes, unsubscribe from model's events
            // unsubscribe so disposed forms aren't still trying to receive notifications
            decryptDialog.FormClosing += (_, __) =>
            {
                decryptBook.DecryptBegin -= decryptBegin;

                decryptBook.TitleDiscovered -= titleDiscovered;
                decryptBook.AuthorsDiscovered -= authorsDiscovered;
                decryptBook.NarratorsDiscovered -= narratorsDiscovered;
                decryptBook.CoverImageFilepathDiscovered -= coverImageFilepathDiscovered;
                decryptBook.UpdateProgress -= updateProgress;

                decryptBook.DecryptCompleted -= decryptCompleted;
            };
            #endregion
        }

        private static AutomatedBackupsForm attachToBackupsForm(IDownloadableProcessable downloadable)
        {
            #region create form
            var automatedBackupsForm = new AutomatedBackupsForm();
            #endregion

            #region define how model actions will affect form behavior
            void begin(object _, LibraryBook libraryBook) => automatedBackupsForm.AppendText($"Begin: {libraryBook.Book}");
            void statusUpdate(object _, string str) => automatedBackupsForm.AppendText("- " + str);
            // extra line after book is completely finished
            void completed(object _, LibraryBook libraryBook) => automatedBackupsForm.AppendText($"Completed: {libraryBook.Book}{Environment.NewLine}");
            #endregion

            #region subscribe new form to model's events
            downloadable.Begin += begin;
            downloadable.StatusUpdate += statusUpdate;
            downloadable.Completed += completed;
            #endregion

            #region when form closes, unsubscribe from model's events
            // unsubscribe so disposed forms aren't still trying to receive notifications
            automatedBackupsForm.FormClosing += (_, __) =>
            {
                downloadable.Begin -= begin;
                downloadable.StatusUpdate -= statusUpdate;
                downloadable.Completed -= completed;
            };
            #endregion

            return automatedBackupsForm;
        }

        // automated backups looper feels like a composible IProcessable: logic, UI, begin + process child + end
        // however the process step doesn't follow the pattern: Validate(product) + Process(product)
        private static async Task runBackupLoopAsync(IProcessable processable, AutomatedBackupsForm automatedBackupsForm)
        {
            automatedBackupsForm.Show();

            // processable.ProcessFirstValidAsync used to be encapsulated elsewhere. however, support for 'skip this time only' requires state. iterators provide this state for free. therefore: use foreach/iterator here
            try
            {
                foreach (var libraryBook in processable.GetValidLibraryBooks())
                {
                    try
                    {
                        var statusHandler = await processable.ProcessBookAsync_NoValidation(libraryBook);
                        if (!validateStatus(statusHandler, automatedBackupsForm))
                            break;
                    }
                    catch (Exception e)
                    {
                        automatedBackupsForm.AppendError(e);
                        DisplaySkipDialog(automatedBackupsForm, libraryBook, e);
                    }
                }
            }
            catch (Exception ex)
            {
                automatedBackupsForm.AppendError(ex);
            }

            automatedBackupsForm.FinalizeUI();
        }

        private static async Task runSingleBackupAsync(IProcessable processable, AutomatedBackupsForm automatedBackupsForm, string productId)
        {
            automatedBackupsForm.Show();

            try
            {
                var libraryBook = IProcessableExt.GetSingleLibraryBook(productId);

                try
                {
                    var statusHandler = await processable.ProcessSingleAsync(libraryBook);
                    validateStatus(statusHandler, automatedBackupsForm);
                }
                catch (Exception ex)
                {
                    automatedBackupsForm.AppendError(ex);
                    DisplaySkipDialog(automatedBackupsForm, libraryBook, ex);
                }
            }
            catch (Exception e)
            {
                automatedBackupsForm.AppendError(e);
            }

            automatedBackupsForm.FinalizeUI();
        }

        private static void DisplaySkipDialog(AutomatedBackupsForm automatedBackupsForm, LibraryBook libraryBook, Exception ex)
        {

            try
            {
                const int TRUNC = 150;

                var errMsg = ex.Message.Truncate(TRUNC);
                if (ex.Message.Length > TRUNC)
                    errMsg += "...";

                var text = @$"
The below error occurred while trying to process this book. Skip this book permanently?

- Click YES to skip this book permanently.

- Click NO to skip the book this time only. We'll try again later.

Error:
{errMsg}
".Trim();
                var dialogResult = System.Windows.Forms.MessageBox.Show(
                    text,
                    "Skip importing this book?",
                    System.Windows.Forms.MessageBoxButtons.YesNo,
                    System.Windows.Forms.MessageBoxIcon.Question);

                if (dialogResult != System.Windows.Forms.DialogResult.Yes)
                {
                    FileManager.AudibleFileStorage.Audio.CreateSkipFile(
                        libraryBook.Book.Title,
                        libraryBook.Book.AudibleProductId,
                        ex.Message + "\r\n|\r\n" + ex.StackTrace);
                }
            }
            catch (Exception exc)
            {
                automatedBackupsForm.AppendText($"Error attempting to display {nameof(DisplaySkipDialog)}");
                automatedBackupsForm.AppendError(exc);
            }
        }

        private static bool validateStatus(StatusHandler statusHandler, AutomatedBackupsForm automatedBackupsForm)
        {
            if (statusHandler == null)
            {
                automatedBackupsForm.AppendText("Done. All books have been processed");
                return false;
            }

            if (statusHandler.HasErrors)
            {
                automatedBackupsForm.AppendText("ERROR. All books have not been processed. Most recent valid book: processing failed");
                foreach (var errorMessage in statusHandler.Errors)
                    automatedBackupsForm.AppendText(errorMessage);
                return false;
            }

            if (!automatedBackupsForm.KeepGoing)
            {
                if (automatedBackupsForm.KeepGoingVisible && !automatedBackupsForm.KeepGoingChecked)
                    automatedBackupsForm.AppendText("'Keep going' is unchecked");
                return false;
            }

            return true;
        }
    }
}
