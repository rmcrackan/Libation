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
			(TotalBooksProcessed, NewBooksAdded) = await LibraryCommands.IndexLibraryAsync(new Login.WinformResponder());

			this.Close();
		}
	}
}
