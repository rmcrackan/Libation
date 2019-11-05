using System;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.Collections.Generic;
using Dinah.Core.ErrorHandling;

namespace ScrapingDomainServices
{
    public static class IProcessableExt
    {
        //
        // DO NOT USE ConfigureAwait(false) WITH ProcessAsync() unless ensuring ProcessAsync() implementation is cross-thread compatible
        // - ValidateAsync() doesn't need UI context. however, each class already uses ConfigureAwait(false)
        // - ProcessAsync() often does a lot with forms in the UI context
        //


        /// <summary>Process the first valid product. Create default context</summary>
        /// <returns>Returns either the status handler from the process, or null if all books have been processed</returns>
        public static async Task<StatusHandler> ProcessFirstValidAsync(this IProcessable processable)
        {
            var libraryBook = await processable.GetNextValidAsync();
            if (libraryBook == null)
                return null;

            var status = await processable.ProcessAsync(libraryBook);
            if (status == null)
                throw new Exception("Processable should never return a null status");

            return status;
        }

        // i'd love to turn this into Task<IEnumerable<LibraryBook>>
        // since enumeration is a blocking operation, this won't be possible until
        // 2019's C# 8 async streams, aka async enumerables, aka async iterators: https://blogs.msdn.microsoft.com/dotnet/2018/11/12/building-c-8-0/
        public static async Task<LibraryBook> GetNextValidAsync(this IProcessable processable)
        {
            var libraryBooks = LibraryQueries.GetLibrary_Flat_NoTracking();

            foreach (var libraryBook in libraryBooks)
                if (await processable.ValidateAsync(libraryBook))
                    return libraryBook;

            return null;
        }

        public static async Task<StatusHandler> ProcessValidateLibraryBookAsync(this IProcessable processable, LibraryBook libraryBook)
        {
            if (!await processable.ValidateAsync(libraryBook))
                return new StatusHandler { "Validation failed" };
            return await processable.ProcessAsync(libraryBook);
        }
    }
}
