using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServices;
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
        public static IEnumerable<LibraryBook> GetValidLibraryBooks(this IProcessable processable)
            => DbContexts.GetContext()
            .GetLibrary_Flat_NoTracking()
            .Where(libraryBook => processable.Validate(libraryBook));

        public static async Task<StatusHandler> ProcessSingleAsync(this IProcessable processable, LibraryBook libraryBook)
        {
            if (!processable.Validate(libraryBook))
                return new StatusHandler { "Validation failed" };

            return await processable.ProcessBookAsync_NoValidation(libraryBook);
        }

        public static async Task<StatusHandler> ProcessBookAsync_NoValidation(this IProcessable processable, LibraryBook libraryBook)
        {
            Serilog.Log.Logger.Information("Begin " + nameof(ProcessBookAsync_NoValidation) + " {@DebugInfo}", new
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
