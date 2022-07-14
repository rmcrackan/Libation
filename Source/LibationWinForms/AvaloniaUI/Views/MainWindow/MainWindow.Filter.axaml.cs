using Avalonia.Input;
using LibationWinForms.Dialogs;
using System;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		protected void Configure_Filter() { }

		public void filterHelpBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> new SearchSyntaxDialog().ShowDialog();
		
		public void filterSearchTb_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				performFilter(this.filterSearchTb.Text);

				// silence the 'ding'
				e.Handled = true;
			}
		}

		public void filterBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> performFilter(this.filterSearchTb.Text);

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
				System.Windows.Forms.MessageBox.Show($"Bad filter string:\r\n\r\n{ex.Message}", "Bad filter string", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

				// re-apply last good filter
				performFilter(lastGoodFilter);
			}
		}
	}
}
