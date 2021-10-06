using DataLayer;
using FileManager;
using System;

namespace LibationWinForms.BookLiberation
{
	class AudioDecryptForm : AudioDecodeForm
	{
		public AudioDecryptForm()
		{
			this.Load += (_, _) => this.RestoreSizeAndLocation(Configuration.Instance);
			this.FormClosing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);
		}

		#region AudioDecodeForm overrides
		public override string DecodeActionName => "Decrypting";
		#endregion

		#region Processable event handler overrides
		public override void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			LogMe.Info($"Download & Decrypt Step, Begin: {libraryBook.Book}");

			base.Processable_Begin(sender, libraryBook);
		}
		public override void Processable_Completed(object sender, LibraryBook libraryBook)
		{
			base.Processable_Completed(sender, libraryBook);
			LogMe.Info($"Download & Decrypt Step, Completed: {libraryBook.Book}{Environment.NewLine}");
		}

		#endregion
	}
}
