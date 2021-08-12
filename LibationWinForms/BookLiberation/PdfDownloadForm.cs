using DataLayer;

namespace LibationWinForms.BookLiberation
{
	internal class PdfDownloadForm : DownloadForm
	{
		public override void OnBegin(object sender, LibraryBook libraryBook) => LogMe.Info($"PDF Step, Begin: {libraryBook.Book}");
		public override void OnCompleted(object sender, LibraryBook libraryBook) => LogMe.Info($"PDF Step, Completed: {libraryBook.Book}");
	}
}
