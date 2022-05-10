using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using LibationFileManager;

namespace FileLiberator
{
    public abstract class Processable : Streamable
    {
        public event EventHandler<LibraryBook> Begin;

        /// <summary>General string message to display. DON'T rely on this for success, failure, or control logic</summary>
        public event EventHandler<string> StatusUpdate;

        public event EventHandler<LibraryBook> Completed;

        /// <returns>True == Valid</returns>
        public abstract bool Validate(LibraryBook libraryBook);

        /// <returns>True == success</returns>
        public abstract Task<StatusHandler> ProcessAsync(LibraryBook libraryBook);

        // when used in foreach: stateful. deferred execution
        public IEnumerable<LibraryBook> GetValidLibraryBooks(IEnumerable<LibraryBook> library)
            => library.Where(libraryBook =>
                Validate(libraryBook)
                && (libraryBook.Book.ContentType != ContentType.Episode || LibationFileManager.Configuration.Instance.DownloadEpisodes)
                );

        public async Task<StatusHandler> ProcessSingleAsync(LibraryBook libraryBook, bool validate)
        {
            if (validate && !Validate(libraryBook))
                return new StatusHandler { "Validation failed" };

            Serilog.Log.Logger.Information("Begin " + nameof(ProcessSingleAsync) + " {@DebugInfo}", new
            {
                libraryBook.Book.Title,
                libraryBook.Book.AudibleProductId,
                libraryBook.Book.Locale,
                Account = libraryBook.Account?.ToMask() ?? "[empty]"
            });

            var status
                = (await ProcessAsync(libraryBook))
                ?? new StatusHandler { "Processable should never return a null status" };

            if (status.IsSuccess)
                DownloadCoverArt(libraryBook);

            return status;
        }

        private void DownloadCoverArt(LibraryBook libraryBook)
		{
            var destinationDir = AudibleFileStorage.Audio.GetDestinationDirectory(libraryBook);
            var coverPath = FileManager.FileUtility.GetValidFilename(System.IO.Path.Combine(destinationDir, "Cover.jpg"), "", true);

            if (System.IO.File.Exists(coverPath)) return;

            try
            {
                (string picId, PictureSize size) = libraryBook.Book.PictureId_1215 is null ?
                    (libraryBook.Book.PictureId, PictureSize._500x500) :
                    (libraryBook.Book.PictureId_1215, PictureSize._1215x1215);

                var picBytes = PictureStorage.GetPictureSynchronously(new PictureDefinition(picId, size));

                if (picBytes.Length > 0)
                    System.IO.File.WriteAllBytes(coverPath, picBytes);
            }
            catch (Exception ex)
            {
                //Failure to download cover art should not be
                //considered a failure to download the book
                Serilog.Log.Logger.Error(ex.Message);
            }
        }

        public async Task<StatusHandler> TryProcessAsync(LibraryBook libraryBook)
            => Validate(libraryBook)
            ? await ProcessAsync(libraryBook)
            : new StatusHandler();

        protected void OnBegin(LibraryBook libraryBook)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(Begin), Book = libraryBook.LogFriendly() });
            Begin?.Invoke(this, libraryBook);
        }

        protected void OnStatusUpdate(string statusUpdate)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(StatusUpdate), Status = statusUpdate });
            StatusUpdate?.Invoke(this, statusUpdate);
        }

        protected void OnCompleted(LibraryBook libraryBook)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(Completed), Book = libraryBook.LogFriendly() });
            Completed?.Invoke(this, libraryBook);
        }
    }
}
