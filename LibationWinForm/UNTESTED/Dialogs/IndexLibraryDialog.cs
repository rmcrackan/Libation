using System.Windows.Forms;
using ApplicationServices;

namespace LibationWinForm
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

		private async void IndexLibraryDialog_Shown(object sender, System.EventArgs e)
		{
			try
			{
				(TotalBooksProcessed, NewBooksAdded) = await LibraryCommands.IndexLibraryAsync(new Login.WinformResponder());
			}
			catch
			{
				MessageBox.Show("Error importing library. Please try again. If this happens after 2 or 3 tries, contact administrator", "Error importing library", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			this.Close();
		}
	}
}
