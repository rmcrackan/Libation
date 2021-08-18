using DataLayer;
using System;

namespace LibationWinForms.BookLiberation
{
	class AudioDecryptForm : AudioDecodeForm
	{
		#region AudioDecodeForm overrides
		public override string DecodeActionName => "Decrypting";
		#endregion

		#region IProcessable event handler overrides
		public override void OnBegin(object sender, LibraryBook libraryBook)
		{
			LogMe.Info($"Download & Decrypt Step, Begin: {libraryBook.Book}");

			base.OnBegin(sender, libraryBook);
		}
		public override void OnCompleted(object sender, LibraryBook libraryBook)
			=> LogMe.Info($"Download & Decrypt Step, Completed: {libraryBook.Book}{Environment.NewLine}");

		#endregion
	}
}
