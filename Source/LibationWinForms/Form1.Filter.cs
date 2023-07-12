using System;
using System.Windows.Forms;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
    public partial class Form1
    {
		protected void Configure_Filter() { }

		private void filterHelpBtn_Click(object sender, EventArgs e) => new SearchSyntaxDialog().ShowDialog();

		private void filterSearchTb_TextCleared(object sender, EventArgs e)
		{
			performFilter(string.Empty);
		}
		private void filterSearchTb_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Return)
			{
				performFilter(this.filterSearchTb.Text);

				// silence the 'ding'
				e.Handled = true;
			}
		}

		private void filterBtn_Click(object sender, EventArgs e) => performFilter(this.filterSearchTb.Text);

		private string lastGoodFilter = "";
		private void performFilter(string filterString)
		{
			this.filterSearchTb.Text = filterString;

			try
			{
				productsDisplay.Filter(filterString);
				lastGoodFilter = filterString;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Bad filter string:\r\n\r\n{ex.Message}", "Bad filter string", MessageBoxButtons.OK, MessageBoxIcon.Error);

				// re-apply last good filter
				performFilter(lastGoodFilter);
			}
		}
	}
}
