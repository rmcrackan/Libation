using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;
using LibationFileManager;

namespace FileLiberator
{
    public abstract class Processable
    {
        public abstract string Name { get; }
        public event EventHandler<LibraryBook> Begin;

        /// <summary>General string message to display. DON'T rely on this for success, failure, or control logic</summary>
        public event EventHandler<string> StatusUpdate;
        /// <summary>Fired when a file is successfully saved to disk</summary>
        public event EventHandler<(string id, string path)> FileCreated;
        public event EventHandler<DownloadProgress> StreamingProgressChanged;
        public event EventHandler<TimeSpan> StreamingTimeRemaining;

        public event EventHandler<LibraryBook> Completed;

        /// <returns>True == Valid</returns>
        public abstract bool Validate(LibraryBook libraryBook);

        /// <returns>True == success</returns>
        public abstract Task<StatusHandler> ProcessAsync(LibraryBook libraryBook);

        // when used in foreach: stateful. deferred execution
        public IEnumerable<LibraryBook> GetValidLibraryBooks(IEnumerable<LibraryBook> library)
            => library.Where(libraryBook =>
                Validate(libraryBook)
                && (!libraryBook.Book.IsEpisodeChild() || Configuration.Instance.DownloadEpisodes)
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

        protected void OnFileCreated(LibraryBook libraryBook, string path)
        {
            Serilog.Log.Logger.Information("File created {@DebugInfo}", new { Name = nameof(FileCreated), libraryBook.Book.AudibleProductId, path });
            FilePathCache.Insert(libraryBook.Book.AudibleProductId, path);
            FileCreated?.Invoke(this, (libraryBook.Book.AudibleProductId, path));
        }

        protected void OnStreamingProgressChanged(DownloadProgress progress)
            => OnStreamingProgressChanged(null, progress);
        protected void OnStreamingProgressChanged(object _, DownloadProgress progress)
            => StreamingProgressChanged?.Invoke(this, progress);

        protected void OnStreamingTimeRemaining(TimeSpan timeRemaining)
            => OnStreamingTimeRemaining(null, timeRemaining);
        protected void OnStreamingTimeRemaining(object _, TimeSpan timeRemaining)
            => StreamingTimeRemaining?.Invoke(this, timeRemaining);

        protected void OnCompleted(LibraryBook libraryBook)
        {
            Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(Completed), Book = libraryBook.LogFriendly() });
            Completed?.Invoke(this, libraryBook);
        }
    }
}
