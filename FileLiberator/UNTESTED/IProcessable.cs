using System;
using System.Threading.Tasks;
using DataLayer;
using Dinah.Core.ErrorHandling;

namespace FileLiberator
{
    public interface IProcessable
    {
        event EventHandler<LibraryBook> Begin;

        /// <summary>General string message to display. DON'T rely on this for success, failure, or control logic</summary>
        event EventHandler<string> StatusUpdate;

        event EventHandler<LibraryBook> Completed;

		/// <returns>True == Valid</returns>
		bool Validate(LibraryBook libraryBook);

        /// <returns>True == success</returns>
        Task<StatusHandler> ProcessAsync(LibraryBook libraryBook);
    }
}
