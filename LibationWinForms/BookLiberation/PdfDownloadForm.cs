using DataLayer;

namespace LibationWinForms.BookLiberation
{
	internal class PdfDownloadForm : DownloadForm
	{
		public override void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			base.Processable_Begin(sender, libraryBook);
			LogMe.Info($"PDF Step, Begin: {libraryBook.Book}");
		}
		public override void Processable_Completed(object sender, LibraryBook libraryBook)
		{
			base.Processable_Completed(sender, libraryBook);
			LogMe.Info($"PDF Step, Completed: {libraryBook.Book}");
		}
	}
}
