using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.BookLiberation
{
	class AudioConvertForm : AudioDecodeBaseForm
	{
		#region AudioDecodeBaseForm overrides
		public override string DecodeActionName => "Converting";
		#endregion

		#region IProcessable event handler overrides
		public override void OnBegin(object sender, LibraryBook libraryBook)
		{
			InfoLogAction($"Convert Step, Begin: {libraryBook.Book}");

			base.OnBegin(sender, libraryBook);
		}
		public override void OnCompleted(object sender, LibraryBook libraryBook)
			=> InfoLogAction($"Convert Step, Completed: {libraryBook.Book}{Environment.NewLine}");

		#endregion
	}
}
