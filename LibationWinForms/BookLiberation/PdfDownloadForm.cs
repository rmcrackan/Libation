using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.BookLiberation
{
	internal class PdfDownloadForm : DownloadForm
	{
		public override void OnBegin(object sender, LibraryBook libraryBook) => InfoLogAction($"PDF Step, Begin: {libraryBook.Book}");
		public override void OnCompleted(object sender, LibraryBook libraryBook) => InfoLogAction($"PDF Step, Completed: {libraryBook.Book}");

	}
}
