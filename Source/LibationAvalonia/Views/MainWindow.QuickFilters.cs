using Avalonia;
using Avalonia.Controls;
using LibationFileManager;
using System.Linq;
using Avalonia.Data;

namespace LibationAvalonia.Views
{
	public partial class MainWindow
	{
		private void Configure_QuickFilters()
		{
			_viewModel.FirstFilterIsDefault = QuickFilters.UseDefault;
			Load += updateFiltersMenu;
			QuickFilters.Updated += updateFiltersMenu;

			//We need to be able to dynamically add and remove menu items from the Quick Filters menu.
			//To do that, we need quick filter's menu items source to be writable, which we can only
			//achieve by creating the list ourselves (instead of allowing Avalonia to create it from the xaml)
			
			var startWithFilterMenuItem = new MenuItem
			{
				Header = "Start Libation with 1st filter _Default",
				Icon = new CheckBox
				{
					BorderThickness = new Thickness(0),
					IsHitTestVisible = false,
					[!CheckBox.IsCheckedProperty] = new Binding(nameof(_viewModel.FirstFilterIsDefault))
				}
			};

			var editFiltersMenuItem = new MenuItem { Header = "_Edit quick filters..." };

			startWithFilterMenuItem.Click += firstFilterIsDefaultToolStripMenuItem_Click;
			editFiltersMenuItem.Click += editQuickFiltersToolStripMenuItem_Click;

			_viewModel.QuickFilterMenuItems.Add(startWithFilterMenuItem);
			_viewModel.QuickFilterMenuItems.Add(editFiltersMenuItem);
			_viewModel.QuickFilterMenuItems.Add(new Separator());
		}

		private async void QuickFiltersMenuItem_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
		{
			int keyNum = (int)e.Key - 34;

			if (keyNum <=9 && keyNum >= 1)
			{
				var menuItem = _viewModel.QuickFilterMenuItems
					.OfType<MenuItem>()
					.FirstOrDefault(i => i.Header is string h && h.StartsWith($"_{keyNum}"));

				if (menuItem is not null)
				{
					await performFilter(menuItem.Tag as string);
					e.Handled = true;
				}
			}
		}

		private void updateFiltersMenu(object _ = null, object __ = null)
		{
			//Clear all filters
			_viewModel.QuickFilterMenuItems.RemoveAll(_viewModel.QuickFilterMenuItems.Where(i => i.Tag is string).ToList());

			// re-populate
			var index = 0;
			foreach (var filter in QuickFilters.Filters)
			{
				var quickFilterMenuItem = new MenuItem
				{
					Tag = filter,
					Header = $"_{++index}: {filter}"
				};
				quickFilterMenuItem.Click += async (_, __) => await performFilter(filter);
				_viewModel.QuickFilterMenuItems.Add(quickFilterMenuItem);
			}
		}

		private void firstFilterIsDefaultToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_viewModel.FirstFilterIsDefault = !_viewModel.FirstFilterIsDefault;
		}

		private void addQuickFilterBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> QuickFilters.Add(_viewModel.FilterString);

		private async void editQuickFiltersToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await new Dialogs.EditQuickFilters().ShowDialog(this);
	}
}
