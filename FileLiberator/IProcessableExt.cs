using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core;
using Dinah.Core.ErrorHandling;

namespace FileLiberator
{
    public static class IProcessableExt
    {
        //
        // DO NOT USE ConfigureAwait(false) WITH ProcessAsync() unless ensuring ProcessAsync() implementation is cross-thread compatible
        // ProcessAsync() often does a lot with forms in the UI context
        //


        // when used in foreach: stateful. deferred execution
        public static IEnumerable<LibraryBook> GetValidLibraryBooks(this IProcessable processable, IEnumerable<LibraryBook> library)
            => library.Where(libraryBook => processable.Validate(libraryBook));

        public static async Task<StatusHandler> ProcessSingleAsync(this IProcessable processable, LibraryBook libraryBook, bool validate)
        {
            if (validate && !processable.Validate(libraryBook))
                return new StatusHandler { "Validation failed" };

            Serilog.Log.Logger.Information("Begin " + nameof(ProcessSingleAsync) + " {@DebugInfo}", new
            {
                libraryBook.Book.Title,
                libraryBook.Book.AudibleProductId,
                libraryBook.Book.Locale,
                Account = libraryBook.Account?.ToMask() ?? "[empty]"
            });

            var status
                = (await processable.ProcessAsync(libraryBook))
                ?? new StatusHandler { "Processable should never return a null status" };

            return status;
        }

		public static async Task<StatusHandler> TryProcessAsync(this IProcessable processable, LibraryBook libraryBook)
			=> processable.Validate(libraryBook)
			? await processable.ProcessAsync(libraryBook)
			: new StatusHandler();
	}
}
