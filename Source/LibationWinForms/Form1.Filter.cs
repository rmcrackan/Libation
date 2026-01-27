using System;
using System.Windows.Forms;
using LibationWinForms.Dialogs;

#nullable enable
namespace LibationWinForms
{
    public partial class Form1
    {
		protected void Configure_Filter() { }

		private void filterHelpBtn_Click(object sender, EventArgs e) => ShowSearchSyntaxDialog();

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

		private string? lastGoodFilter = null;
		private void performFilter(string? filterString)
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

		public SearchSyntaxDialog ShowSearchSyntaxDialog()
		{
			var dialog = new SearchSyntaxDialog();
			dialog.TagDoubleClicked += Dialog_TagDoubleClicked;
			dialog.FormClosed += Dialog_Closed;
			filterHelpBtn.Enabled = false;
			dialog.Show(this);
			return dialog;

			void Dialog_Closed(object? sender, FormClosedEventArgs e)
			{
				dialog.TagDoubleClicked -= Dialog_TagDoubleClicked;
				filterHelpBtn.Enabled = true;
			}
			void Dialog_TagDoubleClicked(object? sender, string tag)
			{
				if (string.IsNullOrEmpty(tag)) return;

				var text = filterSearchTb.Text;
				var selStart = Math.Min(Math.Max(0, filterSearchTb.SelectionStart), text.Length);

				filterSearchTb.Text = text.Insert(selStart, tag);
				filterSearchTb.SelectionStart = selStart + tag.Length;
				filterSearchTb.Focus();
			}
		}
	}
}
