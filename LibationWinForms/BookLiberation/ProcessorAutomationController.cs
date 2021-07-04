using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileLiberator;

namespace LibationWinForms.BookLiberation
{
    // decouple serilog and form. include convenience factory method
    public class LogMe
    {
        public event EventHandler<string> LogInfo;
        public event EventHandler<string> LogErrorString;
        public event EventHandler<(Exception, string)> LogError;

        public static LogMe RegisterForm(AutomatedBackupsForm form)
        {
            var logMe = new LogMe();

            logMe.LogInfo += (_, text) => Serilog.Log.Logger.Information($"Automated backup: {text}");
            logMe.LogInfo += (_, text) => form.WriteLine(text);

            logMe.LogErrorString += (_, text) => Serilog.Log.Logger.Error(text);
            logMe.LogErrorString += (_, text) => form.WriteLine(text);

            logMe.LogError += (_, tuple) => Serilog.Log.Logger.Error(tuple.Item1, tuple.Item2 ?? "Automated backup: error");
            logMe.LogError += (_, tuple) =>
            {
                form.WriteLine(tuple.Item2 ?? "Automated backup: error");
                form.WriteLine("ERROR: " + tuple.Item1.Message);
            };

            return logMe;
        }

        public void Info(string text) => LogInfo?.Invoke(this, text);
        public void Error(string text) => LogErrorString?.Invoke(this, text);
        public void Error(Exception ex, string text = null) => LogError?.Invoke(this, (ex, text));
    }

    public static class ProcessorAutomationController
    {
        public static async Task BackupSingleBookAsync(string productId, EventHandler<LibraryBook> completedAction = null)
        {
            Serilog.Log.Logger.Information("Begin " + nameof(BackupSingleBookAsync) + " {@DebugInfo}", new { productId });

            var backupBook = getWiredUpBackupBook(completedAction);
           
            (Action unsibscribeEvents, LogMe logMe) = attachToBackupsForm(backupBook);

            var libraryBook = IProcessableExt.GetSingleLibraryBook(productId);
            // continue even if libraryBook is null. we'll display even that in the processing box
            await new BackupSingle(logMe, backupBook, libraryBook).RunBackupAsync();

            unsibscribeEvents();
        }

        public static async Task BackupAllBooksAsync(EventHandler<LibraryBook> completedAction = null)
        {
            Serilog.Log.Logger.Information("Begin " + nameof(BackupAllBooksAsync));

            var backupBook = getWiredUpBackupBook(completedAction);
            var automatedBackupsForm = new AutomatedBackupsForm();

            (Action unsibscribeEvents, LogMe logMe) = attachToBackupsForm(backupBook, automatedBackupsForm);

            await new BackupLoop(logMe, backupBook, automatedBackupsForm).RunBackupAsync();

            unsibscribeEvents();
        }

        private static BackupBook getWiredUpBackupBook(EventHandler<LibraryBook> completedAction)
        {
            var backupBook = new BackupBook();

            backupBook.DecryptBook.Begin += (_, l) => wireUpEvents(backupBook.DecryptBook, l);
            backupBook.DownloadPdf.Begin += (_, __) => wireUpEvents(backupBook.DownloadPdf);

            // must occur before completedAction. A common use case is:
            // - filter by -liberated
            // - liberate only that book
            //   completedAction is to refresh grid
            // - want to see that book disappear from grid
            // also for this to work, updateIsLiberated can NOT be async
            backupBook.DecryptBook.Completed += updateIsLiberated;
            backupBook.DownloadPdf.Completed += updateIsLiberated;

            if (completedAction != null)
            {
                backupBook.DecryptBook.Completed += completedAction;
                backupBook.DownloadPdf.Completed += completedAction;
            }

            return backupBook;
        }

        private static void updateIsLiberated(object sender, LibraryBook e) => ApplicationServices.SearchEngineCommands.UpdateIsLiberated(e.Book);

        private static (Action unsibscribeEvents, LogMe) attachToBackupsForm(BackupBook backupBook, AutomatedBackupsForm automatedBackupsForm = null)
        {
            #region create logger
            var logMe = automatedBackupsForm is null? new LogMe() : LogMe.RegisterForm(automatedBackupsForm);
            #endregion

            #region define how model actions will affect form behavior
            void statusUpdate(object _, string str) => logMe.Info("- " + str);
            void decryptBookBegin(object _, LibraryBook libraryBook) => logMe.Info($"Decrypt Step, Begin: {libraryBook.Book}");
            // extra line after book is completely finished
            void decryptBookCompleted(object _, LibraryBook libraryBook) => logMe.Info($"Decrypt Step, Completed: {libraryBook.Book}{Environment.NewLine}");
            void downloadPdfBegin(object _, LibraryBook libraryBook) => logMe.Info($"PDF Step, Begin: {libraryBook.Book}");
            // extra line after book is completely finished
            void downloadPdfCompleted(object _, LibraryBook libraryBook) => logMe.Info($"PDF Step, Completed: {libraryBook.Book}{Environment.NewLine}");
            #endregion

            #region subscribe new form to model's events
            backupBook.DecryptBook.Begin += decryptBookBegin;
            backupBook.DecryptBook.StatusUpdate += statusUpdate;
            backupBook.DecryptBook.Completed += decryptBookCompleted;
            backupBook.DownloadPdf.Begin += downloadPdfBegin;
            backupBook.DownloadPdf.StatusUpdate += statusUpdate;
            backupBook.DownloadPdf.Completed += downloadPdfCompleted;
            #endregion

            #region when form closes, unsubscribe from model's events
            // unsubscribe so disposed forms aren't still trying to receive notifications
            Action unsibscribe = () =>
            {
                backupBook.DecryptBook.Begin -= decryptBookBegin;
                backupBook.DecryptBook.StatusUpdate -= statusUpdate;
                backupBook.DecryptBook.Completed -= decryptBookCompleted;
                backupBook.DownloadPdf.Begin -= downloadPdfBegin;
                backupBook.DownloadPdf.StatusUpdate -= statusUpdate;
                backupBook.DownloadPdf.Completed -= downloadPdfCompleted;
            };
            #endregion

            return (unsibscribe, logMe);
        }

        public static async Task BackupAllPdfsAsync(EventHandler<LibraryBook> completedAction = null)
        {
            Serilog.Log.Logger.Information("Begin " + nameof(BackupAllPdfsAsync));

            var downloadPdf = getWiredUpDownloadPdf(completedAction);

            (AutomatedBackupsForm automatedBackupsForm, LogMe logMe) = attachToBackupsForm(downloadPdf);
            await new BackupLoop(logMe, downloadPdf, automatedBackupsForm).RunBackupAsync();
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
        private static void wireUpEvents(IDecryptable decryptBook, LibraryBook libraryBook)
        {
            #region create form
            var decryptDialog = new DecryptForm();
            #endregion

            #region Set initially displayed book properties from library info.
            decryptDialog.SetTitle(libraryBook.Book.Title);
            decryptDialog.SetAuthorNames(string.Join(", ", libraryBook.Book.Authors));
            decryptDialog.SetNarratorNames(string.Join(", ", libraryBook.Book.NarratorNames));
            decryptDialog.SetCoverImage(
                WindowsDesktopUtilities.WinAudibleImageServer.GetImage(
                    libraryBook.Book.PictureId,
                    FileManager.PictureSize._80x80
                    ));
            #endregion

            #region define how model actions will affect form behavior
            void decryptBegin(object _, string __) => decryptDialog.Show();

            void titleDiscovered(object _, string title) => decryptDialog.SetTitle(title);
            void authorsDiscovered(object _, string authors) => decryptDialog.SetAuthorNames(authors);
            void narratorsDiscovered(object _, string narrators) => decryptDialog.SetNarratorNames(narrators);
            void coverImageFilepathDiscovered(object _, byte[] coverBytes) => decryptDialog.SetCoverImage(Dinah.Core.Drawing.ImageReader.ToImage(coverBytes));
            void updateProgress(object _, int percentage) => decryptDialog.UpdateProgress(percentage);
            void updateRemainingTime(object _, TimeSpan remaining) => decryptDialog.UpdateRemainingTime(remaining);
            void decryptCompleted(object _, string __) => decryptDialog.Close();
           
            void requestCoverArt(object _, Action<byte[]> setCoverArtDelegate)
            {
                var picDef = new FileManager.PictureDefinition(libraryBook.Book.PictureId, FileManager.PictureSize._500x500);
                (bool isDefault, byte[] picture) = FileManager.PictureStorage.GetPicture(picDef);

                if (isDefault)
                {
                    void pictureCached(object _, string pictureId)
                    {
                        if (pictureId == libraryBook.Book.PictureId)
                        {
                            FileManager.PictureStorage.PictureCached -= pictureCached;

                            var picDef = new FileManager.PictureDefinition(libraryBook.Book.PictureId, FileManager.PictureSize._500x500);
                            (_, picture) = FileManager.PictureStorage.GetPicture(picDef);

                            setCoverArtDelegate(picture);
                        }
                    };
                    FileManager.PictureStorage.PictureCached += pictureCached;
                }
                else
                    setCoverArtDelegate(picture);
            }
            #endregion

            #region subscribe new form to model's events
            decryptBook.DecryptBegin += decryptBegin;

            decryptBook.TitleDiscovered += titleDiscovered;
            decryptBook.AuthorsDiscovered += authorsDiscovered;
            decryptBook.NarratorsDiscovered += narratorsDiscovered;
            decryptBook.CoverImageFilepathDiscovered += coverImageFilepathDiscovered;
            decryptBook.UpdateProgress += updateProgress;
            decryptBook.UpdateRemainingTime += updateRemainingTime;
            decryptBook.RequestCoverArt += requestCoverArt;

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
                decryptBook.UpdateRemainingTime -= updateRemainingTime;
                decryptBook.RequestCoverArt -= requestCoverArt;

                decryptBook.DecryptCompleted -= decryptCompleted;
                decryptBook.Cancel();
            };
            #endregion
        }
        
        private static (AutomatedBackupsForm, LogMe) attachToBackupsForm(IDownloadableProcessable downloadable)
        {
            #region create form and logger
            var automatedBackupsForm = new AutomatedBackupsForm();
            var logMe = LogMe.RegisterForm(automatedBackupsForm);
            #endregion

            #region define how model actions will affect form behavior
            void begin(object _, LibraryBook libraryBook) => logMe.Info($"Begin: {libraryBook.Book}");
            void statusUpdate(object _, string str) => logMe.Info("- " + str);
            // extra line after book is completely finished
            void completed(object _, LibraryBook libraryBook) => logMe.Info($"Completed: {libraryBook.Book}{Environment.NewLine}");
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

            return (automatedBackupsForm, logMe);
        }
    }

    abstract class BackupRunner
    {
        protected LogMe LogMe { get; }
        protected IProcessable Processable { get; }
        protected AutomatedBackupsForm AutomatedBackupsForm { get; }

        protected BackupRunner(LogMe logMe, IProcessable processable, AutomatedBackupsForm automatedBackupsForm = null)
        {
            LogMe = logMe;
            Processable = processable;
            AutomatedBackupsForm = automatedBackupsForm;
        }

        protected abstract Task RunAsync();

        protected abstract string SkipDialogText { get; }
        protected abstract MessageBoxButtons SkipDialogButtons { get; }
        protected abstract DialogResult CreateSkipFileResult { get; }

        public async Task RunBackupAsync()
        {
            AutomatedBackupsForm?.Show();

            try
            {
                await RunAsync();
            }
            catch (Exception ex)
            {
                LogMe.Error(ex);
            }

            AutomatedBackupsForm?.FinalizeUI();
            LogMe.Info("DONE");
        }

        protected async Task<bool> ProcessOneAsync(Func<LibraryBook, Task<StatusHandler>> func, LibraryBook libraryBook)
        {
            string logMessage;

            try
            {
                var statusHandler = await func(libraryBook);

                if (statusHandler.IsSuccess)
                    return true;

                foreach (var errorMessage in statusHandler.Errors)
                    LogMe.Error(errorMessage);

                logMessage = statusHandler.Errors.Aggregate((a, b) => $"{a}\r\n{b}");
            }
            catch (Exception ex)
            {
                LogMe.Error(ex);

                logMessage = ex.Message + "\r\n|\r\n" + ex.StackTrace;
            }

            LogMe.Error("ERROR. All books have not been processed. Most recent book: processing failed");

            var dialogResult = MessageBox.Show(SkipDialogText, "Skip importing this book?", SkipDialogButtons, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Abort)
                return false;

            if (dialogResult == CreateSkipFileResult)
            {
                var path = FileManager.AudibleFileStorage.Audio.CreateSkipFile(libraryBook.Book.Title, libraryBook.Book.AudibleProductId, logMessage);
                LogMe.Info($@"
Created new 'skip' file
  [{libraryBook.Book.AudibleProductId}] {libraryBook.Book.Title}
  {path}
".Trim());
            }

            return true;
        }
    }
    class BackupSingle : BackupRunner
    {
        private LibraryBook _libraryBook { get; }

        protected override string SkipDialogText => @"
An error occurred while trying to process this book. Skip this book permanently?

- Click YES to skip this book permanently.

- Click NO to skip the book this time only. We'll try again later.
".Trim();
        protected override MessageBoxButtons SkipDialogButtons => MessageBoxButtons.YesNo;
        protected override DialogResult CreateSkipFileResult => DialogResult.Yes;

        public BackupSingle(LogMe logMe, IProcessable processable, LibraryBook libraryBook)
            : base(logMe, processable)
        {
            _libraryBook = libraryBook;
        }

        protected override async Task RunAsync()
        {
            if (_libraryBook is not null)
                await ProcessOneAsync(Processable.ProcessSingleAsync, _libraryBook);
        }
    }
    class BackupLoop : BackupRunner
    {
        protected override string SkipDialogText => @"
An error occurred while trying to process this book

- ABORT: stop processing books.

- RETRY: retry this book later. Just skip it for now. Continue processing books. (Will try this book again later.)

- IGNORE: Permanently ignore this book. Continue processing books. (Will not try this book again later.)
".Trim();
        protected override MessageBoxButtons SkipDialogButtons => MessageBoxButtons.AbortRetryIgnore;
        protected override DialogResult CreateSkipFileResult => DialogResult.Ignore;

        public BackupLoop(LogMe logMe, IProcessable processable, AutomatedBackupsForm automatedBackupsForm)
            : base(logMe, processable, automatedBackupsForm) { }

        protected override async Task RunAsync()
        {
            // support for 'skip this time only' requires state. iterators provide this state for free. therefore: use foreach/iterator here
            foreach (var libraryBook in Processable.GetValidLibraryBooks())
            {
                var keepGoing = await ProcessOneAsync(Processable.ProcessBookAsync_NoValidation, libraryBook);
                if (!keepGoing)
                    return;

                if (AutomatedBackupsForm.IsDisposed)
                    break;

                if (!AutomatedBackupsForm.KeepGoing)
                {
                    if (AutomatedBackupsForm.KeepGoingVisible && !AutomatedBackupsForm.KeepGoingChecked)
                        LogMe.Info("'Keep going' is unchecked");
                    return;
                }
            }

            LogMe.Info("Done. All books have been processed");
        }
    }
}
