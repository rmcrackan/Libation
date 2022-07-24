using Avalonia.Input;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		protected void Configure_Filter() { }

		public async void filterHelpBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await (new LibationAvalonia.AvaloniaUI.Views.Dialogs.SearchSyntaxDialog()).ShowDialog(this);
		
		public async void filterSearchTb_KeyPress(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				await performFilter(_viewModel.FilterString);

				// silence the 'ding'
				e.Handled = true;
			}
		}

		public async void filterBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await performFilter(_viewModel.FilterString);

		private string lastGoodFilter = "";
		private async Task performFilter(string filterString)
		{
			_viewModel.FilterString = filterString;

			try
			{
				await _viewModel.ProductsDisplay.Filter(filterString);
				lastGoodFilter = filterString;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Bad filter string:\r\n\r\n{ex.Message}", "Bad filter string", MessageBoxButtons.OK, MessageBoxIcon.Error);

				// re-apply last good filter
				await performFilter(lastGoodFilter);
			}
		}
	}
}
