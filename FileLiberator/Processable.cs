using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
    public abstract class Processable : Streamable
    {
        public event EventHandler<LibraryBook> Begin;

        /// <summary>General string message to display. DON'T rely on this for success, failure, or control logic</summary>
        public event EventHandler<string> StatusUpdate;

        public event EventHandler<LibraryBook> Completed;

        // when used in foreach: stateful. deferred execution
        public IEnumerable<LibraryBook> GetValidLibraryBooks(IEnumerable<LibraryBook> library)
            => library.Where(libraryBook =>
                Validate(libraryBook)
                && (libraryBook.Book.ContentType != ContentType.Episode || FileManager.Configuration.Instance.DownloadEpisodes)
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

            return status;
        }

        public async Task<StatusHandler> TryProcessAsync( LibraryBook libraryBook)
            => Validate(libraryBook)
            ? await ProcessAsync(libraryBook)
            : new StatusHandler();

        /// <returns>True == Valid</returns>
        public abstract bool Validate(LibraryBook libraryBook);

        /// <returns>True == success</returns>
        public abstract Task<StatusHandler> ProcessAsync(LibraryBook libraryBook);

        public virtual void OnBegin(LibraryBook libraryBook)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(Begin), Book = libraryBook.LogFriendly() });
            Begin?.Invoke(this, libraryBook);
        }
        public virtual void OnCompleted(LibraryBook libraryBook)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(Completed), Book = libraryBook.LogFriendly() });
            Completed?.Invoke(this, libraryBook);
        }
        public virtual void OnStatusUpdate(string statusUpdate)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(StatusUpdate), Status = statusUpdate });
            StatusUpdate?.Invoke(this, statusUpdate);
        }
    }
}
