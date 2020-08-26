using System;
using System.Windows.Forms;
using ApplicationServices;
using InternalUtilities;
using LibationWinForms.Login;

namespace LibationWinForms.Dialogs
{
	public partial class IndexLibraryDialog : Form
	{
		private Account[] _accounts { get; }

		public int NewBooksAdded { get; private set; }
		public int TotalBooksProcessed { get; private set; }

		public IndexLibraryDialog(params Account[] accounts)
		{
			_accounts = accounts;
			InitializeComponent();
			this.Shown += IndexLibraryDialog_Shown;
		}

		private async void IndexLibraryDialog_Shown(object sender, EventArgs e)
		{
			if (_accounts != null && _accounts.Length > 0)
			{
				this.label1.Text
					= (_accounts.Length == 1)
					? "Scanning Audible library. This may take a few minutes."
					: $"Scanning Audible library: {_accounts.Length} accounts. This may take a few minutes per account.";

				try
				{
					(TotalBooksProcessed, NewBooksAdded) = await LibraryCommands.ImportAccountAsync(new WinformResponder(), _accounts);
				}
				catch
				{
					var msg = "Error importing library. Please try again. If this still happens after 2 or 3 tries, stop and contact administrator";
					MessageBox.Show(msg, "Error importing library", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}

			this.Close();
		}
	}
}
