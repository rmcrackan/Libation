using Avalonia.Input;
using LibationWinForms.AvaloniaUI.Views.Dialogs;
using LibationWinForms.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		protected void Configure_Filter() { }

		public void filterHelpBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> new SearchSyntaxDialog().ShowDialog();
		
		public async void filterSearchTb_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				await performFilter(this.filterSearchTb.Text);

				// silence the 'ding'
				e.Handled = true;
			}
		}

		public async void filterBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await performFilter(this.filterSearchTb.Text);

		private string lastGoodFilter = "";
		private async Task performFilter(string filterString)
		{
			this.filterSearchTb.Text = filterString;

			try
			{
				productsDisplay.Filter(filterString);
				lastGoodFilter = filterString;
			}
			catch (Exception ex)
			{
				await MessageBox.Show($"Bad filter string:\r\n\r\n{ex.Message}", "Bad filter string", MessageBoxButtons.OK, MessageBoxIcon.Error);

				// re-apply last good filter
				await performFilter(lastGoodFilter);
			}
		}
	}
}
