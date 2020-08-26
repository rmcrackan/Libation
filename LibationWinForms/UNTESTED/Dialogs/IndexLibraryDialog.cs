using System;
using System.Windows.Forms;
using ApplicationServices;
using LibationWinForms.Login;

namespace LibationWinForms.Dialogs
{
	public partial class IndexLibraryDialog : Form
	{
		public int NewBooksAdded { get; private set; }
		public int TotalBooksProcessed { get; private set; }

		public IndexLibraryDialog()
		{
			InitializeComponent();
			this.Shown += IndexLibraryDialog_Shown;
		}

		private async void IndexLibraryDialog_Shown(object sender, EventArgs e)
		{
			try
			{
				(TotalBooksProcessed, NewBooksAdded) = await LibraryCommands.ImportAccountAsync(InternalUtilities.AudibleApiStorage.TEST_GetFirstAccount(), new WinformResponder());
			}
			catch
			{
				var msg = "Error importing library. Please try again. If this still happens after 2 or 3 tries, stop and contact administrator";
				MessageBox.Show(msg, "Error importing library", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			this.Close();
		}
	}
}
