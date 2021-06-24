using DataLayer;
using Dinah.Core.ErrorHandling;
using System.Threading.Tasks;

namespace FileLiberator.AaxcDownloadDecrypt
{
    public class DownloadBookDummy : DownloadableBase
    {
        public override async Task<StatusHandler> ProcessItemAsync(LibraryBook libraryBook)
        {
            return new StatusHandler();
        }

        public override bool Validate(LibraryBook libraryBook)
        {
            return true;
        }
    }
}
