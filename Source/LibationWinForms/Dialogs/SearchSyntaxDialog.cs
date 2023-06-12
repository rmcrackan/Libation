using LibationSearchEngine;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class SearchSyntaxDialog : Form
	{
		public SearchSyntaxDialog()
		{
			InitializeComponent();

			label2.Text += "\r\n\r\n" + string.Join("\r\n", SearchEngine.FieldIndexRules.StringFieldNames);
			label3.Text += "\r\n\r\n" + string.Join("\r\n", SearchEngine.FieldIndexRules.NumberFieldNames);
			label4.Text += "\r\n\r\n" + string.Join("\r\n", SearchEngine.FieldIndexRules.BoolFieldNames);
			label5.Text += "\r\n\r\n" + string.Join("\r\n", SearchEngine.FieldIndexRules.IdFieldNames);
			this.SetLibationIcon();
		}

		private void CloseBtn_Click(object sender, EventArgs e) => this.Close();
	}
}
