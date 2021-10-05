using DataLayer;
using FileManager;
using System;

namespace LibationWinForms.BookLiberation
{
	class AudioConvertForm : AudioDecodeForm
	{
		public AudioConvertForm()
		{
			this.Load += (_, _) => this.RestoreSizeAndLocation(Configuration.Instance);
			this.FormClosing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);
		}

		#region AudioDecodeForm overrides
		public override string DecodeActionName => "Converting";
		#endregion

		#region IProcessable event handler overrides
		public override void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			LogMe.Info($"Convert Step, Begin: {libraryBook.Book}");

			base.Processable_Begin(sender, libraryBook);
		}
		public override void Processable_Completed(object sender, LibraryBook libraryBook)
		{
			base.Processable_Completed(sender, libraryBook);
			LogMe.Info($"Convert Step, Completed: {libraryBook.Book}{Environment.NewLine}");
		}

		#endregion
	}
}
