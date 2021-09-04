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
		public override void OnBegin(object sender, LibraryBook libraryBook)
		{
			LogMe.Info($"Convert Step, Begin: {libraryBook.Book}");

			base.OnBegin(sender, libraryBook);
		}
		public override void OnCompleted(object sender, LibraryBook libraryBook)
			=> LogMe.Info($"Convert Step, Completed: {libraryBook.Book}{Environment.NewLine}");

		#endregion
	}
}
