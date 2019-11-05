using System;
using System.Threading.Tasks;
using FileLiberator;

namespace LibationWinForm.BookLiberation
{
    // matches a file processor with a form
    public static class ProcessorAutomationController
    {
        //
        // these utility methods ensure proper wiring
        // 1) we can't forget to do it
        // 2) we can't accidentally do it mult times becaues we lost track of complexity
        //
        public static BackupBook GetWiredUpBackupBook()
        {
            var backupBook = new BackupBook();

            backupBook.Download.Begin += (_, __) => wireUpDownloadable(backupBook.Download);
            backupBook.Decrypt.Begin += (_, __) => wireUpDecryptable(backupBook.Decrypt);

            return backupBook;
        }
        public static DecryptBook GetWiredUpDecryptBook()
        {
            var decryptBook = new DecryptBook();
            decryptBook.Begin += (_, __) => wireUpDecryptable(decryptBook);
            return decryptBook;
        }
        public static DownloadBook GetWiredUpDownloadBook()
        {
            var downloadBook = new DownloadBook();
            downloadBook.Begin += (_, __) => wireUpDownloadable(downloadBook);
            return downloadBook;
        }
        public static DownloadPdf GetWiredUpDownloadPdf()
        {
            var downloadPdf = new DownloadPdf();
            downloadPdf.Begin += (_, __) => wireUpDownloadable(downloadPdf);
            return downloadPdf;
        }

        // subscribed to Begin event because a new form should be created+processed+closed on each iteration
        private static void wireUpDownloadable(IDownloadable downloadable)
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
                => downloadDialog.DownloadProgressChanged(progress.BytesReceived, progress.TotalBytesToReceive.Value);

            void unsubscribe(object _ = null, EventArgs __ = null)
            {
                downloadable.DownloadBegin -= downloadBegin;
                downloadable.DownloadCompleted -= fileDownloadCompleted;
                downloadable.DownloadProgressChanged -= downloadProgressChanged;
                downloadable.Completed -= dialogDispose;
            }

            // unless we dispose, if the form is created but un-used/never-shown then weird UI stuff can happen
            // also, since event unsubscribe occurs on FormClosing and an unused form is never closed, then the events will never be unsubscribed
            void dialogDispose(object _, string __)
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
        private static void wireUpDecryptable(IDecryptable decryptBook)
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

        public static async Task RunAutomaticDownload(IDownloadable downloadable)
        {
            #region create form
            var automatedBackupsForm = new AutomatedBackupsForm();
            #endregion

            #region define how model actions will affect form behavior
            void begin(object _, string str) => automatedBackupsForm.AppendText("Begin: " + str);
            void statusUpdate(object _, string str) => automatedBackupsForm.AppendText("- " + str);
            // extra line after book is completely finished
            void completed(object _, string str) => automatedBackupsForm.AppendText("Completed: " + str + Environment.NewLine);
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

            await runBackupLoop(downloadable, automatedBackupsForm);
        }

        public static async Task RunAutomaticBackup(BackupBook backupBook)
        {
            #region create form
            var automatedBackupsForm = new AutomatedBackupsForm();
            #endregion

            #region define how model actions will affect form behavior
            void downloadBegin(object _, string str) => automatedBackupsForm.AppendText("DownloadStep_Begin: " + str);
            void statusUpdate(object _, string str) => automatedBackupsForm.AppendText("- " + str);
            void downloadCompleted(object _, string str) => automatedBackupsForm.AppendText("DownloadStep_Completed: " + str);
            void decryptBegin(object _, string str) => automatedBackupsForm.AppendText("DecryptStep_Begin: " + str);
            // extra line after book is completely finished
            void decryptCompleted(object _, string str) => automatedBackupsForm.AppendText("DecryptStep_Completed: " + str + Environment.NewLine);
            #endregion

            #region subscribe new form to model's events
            backupBook.Download.Begin += downloadBegin;
            backupBook.Download.StatusUpdate += statusUpdate;
            backupBook.Download.Completed += downloadCompleted;
            backupBook.Decrypt.Begin += decryptBegin;
            backupBook.Decrypt.StatusUpdate += statusUpdate;
            backupBook.Decrypt.Completed += decryptCompleted;
            #endregion

            #region when form closes, unsubscribe from model's events
            // unsubscribe so disposed forms aren't still trying to receive notifications
            automatedBackupsForm.FormClosing += (_, __) =>
            {
                backupBook.Download.Begin -= downloadBegin;
                backupBook.Download.StatusUpdate -= statusUpdate;
                backupBook.Download.Completed -= downloadCompleted;
                backupBook.Decrypt.Begin -= decryptBegin;
                backupBook.Decrypt.StatusUpdate -= statusUpdate;
                backupBook.Decrypt.Completed -= decryptCompleted;
            };
            #endregion

            await runBackupLoop(backupBook, automatedBackupsForm);
        }

        // automated backups looper feels like a composible IProcessable: logic, UI, begin + process child + end
        // however the process step doesn't follow the pattern: Validate(product) + Process(product)
        private static async Task runBackupLoop(IProcessable processable, AutomatedBackupsForm automatedBackupsForm)
        {
            automatedBackupsForm.Show();

            try
            {
                do
                {
                    var statusHandler = await processable.ProcessFirstValidAsync();

                    if (statusHandler == null)
                    {
                        automatedBackupsForm.AppendText("Done. All books have been processed");
                        break;
                    }

                    if (statusHandler.HasErrors)
                    {
                        automatedBackupsForm.AppendText("ERROR. All books have not been processed. Most recent valid book: processing failed");
                        foreach (var errorMessage in statusHandler.Errors)
                            automatedBackupsForm.AppendText(errorMessage);
                        break;
                    }

                    if (!automatedBackupsForm.KeepGoingIsChecked)
                    {
                        automatedBackupsForm.AppendText("'Keep going' is unchecked");
                        break;
                    }
                }
                while (automatedBackupsForm.KeepGoingIsChecked);
            }
            catch (Exception ex)
            {
                automatedBackupsForm.AppendError(ex);
            }

            automatedBackupsForm.FinalizeUI();
        }
    }
}
