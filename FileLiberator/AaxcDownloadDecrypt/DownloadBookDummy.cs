using DataLayer;
using Dinah.Core.ErrorHandling;
using System.Threading.Tasks;

namespace FileLiberator.AaxcDownloadDecrypt
{
    public class DownloadBookDummy : DownloadableBase
    {
        public override Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook) => Task.FromResult(new StatusHandler());

        public override bool Validate(LibraryBook libraryBook)
        {
            return true;
        }
    }
}
