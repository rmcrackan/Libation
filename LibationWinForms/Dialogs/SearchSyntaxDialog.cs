using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class SearchSyntaxDialog : Form
	{
		public SearchSyntaxDialog()
		{
			InitializeComponent();

			label2.Text += "\r\n\r\n" + string.Join("\r\n", LibationSearchEngine.SearchEngine.GetSearchStringFields());
			label3.Text += "\r\n\r\n" + string.Join("\r\n", LibationSearchEngine.SearchEngine.GetSearchNumberFields());
			label4.Text += "\r\n\r\n" + string.Join("\r\n", LibationSearchEngine.SearchEngine.GetSearchBoolFields());
			label5.Text += "\r\n\r\n" + string.Join("\r\n", LibationSearchEngine.SearchEngine.GetSearchIdFields());
		}

		private void CloseBtn_Click(object sender, EventArgs e) => this.Close();
	}
}
