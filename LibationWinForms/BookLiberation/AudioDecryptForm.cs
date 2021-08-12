using DataLayer;
using System;

namespace LibationWinForms.BookLiberation
{
	class AudioDecryptForm : AudioDecodeBaseForm
	{
		#region AudioDecodeBaseForm overrides
		public override string DecodeActionName => "Decrypting";
		#endregion

		#region IProcessable event handler overrides
		public override void OnBegin(object sender, LibraryBook libraryBook)
		{
			InfoLogAction($"Download & Decrypt Step, Begin: {libraryBook.Book}");

			base.OnBegin(sender, libraryBook);
		}
		public override void OnCompleted(object sender, LibraryBook libraryBook)
			=> InfoLogAction($"Download & Decrypt Step, Completed: {libraryBook.Book}{Environment.NewLine}");

		#endregion
	}
}
