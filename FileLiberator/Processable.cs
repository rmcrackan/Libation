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
    public abstract class Processable : IStreamable
    {
        public event EventHandler<LibraryBook> Begin;

        /// <summary>General string message to display. DON'T rely on this for success, failure, or control logic</summary>
        public event EventHandler<string> StatusUpdate;

        public event EventHandler<LibraryBook> Completed;
		public event EventHandler<string> StreamingBegin;
		public event EventHandler<DownloadProgress> StreamingProgressChanged;
		public event EventHandler<TimeSpan> StreamingTimeRemaining;
		public event EventHandler<string> StreamingCompleted;



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
            Begin?.Invoke(this, libraryBook);
        }
        public virtual void OnCompleted(LibraryBook libraryBook)
        {
            Completed?.Invoke(this, libraryBook);
        }
        public virtual void OnStatusUpdate(string status)
        {
            StatusUpdate?.Invoke(this, status);
        }
        public virtual void OnStreamingBegin(string filePath)
        {
            StreamingBegin?.Invoke(this, filePath);
        }
        public virtual void OnStreamingCompleted(string filePath)
        {
            StreamingCompleted?.Invoke(this, filePath);
        }
        public virtual void OnStreamingProgressChanged(DownloadProgress progress)
        {
            StreamingProgressChanged?.Invoke(this, progress);
        }
        public virtual void OnStreamingTimeRemaining(TimeSpan timeRemaining)
        {
            StreamingTimeRemaining?.Invoke(this, timeRemaining);
        }
    }
}
