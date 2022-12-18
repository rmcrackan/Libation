using Avalonia.Controls;
using LibationFileManager;
using System;
using System.Linq;

namespace LibationAvalonia.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_QuickFilters()
		{
			_viewModel.FirstFilterIsDefault = QuickFilters.UseDefault;
			Load += updateFiltersMenu;
			QuickFilters.Updated += updateFiltersMenu;
		}

		private object quickFilterTag { get; } = new();
		private void updateFiltersMenu(object _ = null, object __ = null)
		{
			var allItems = quickFiltersToolStripMenuItem
				.Items
				.Cast<Control>()
				.ToList();

			var toRemove = allItems
				.OfType<MenuItem>()
				.Where(mi => mi.Tag == quickFilterTag)
				.ToList();

			allItems = allItems
				.Except(toRemove)
				.ToList();

			// re-populate
			var index = 0;
			foreach (var filter in QuickFilters.Filters)
			{
				var quickFilterMenuItem = new MenuItem
				{
					Tag = quickFilterTag,
					Header = $"_{++index}: {filter}"
				};
				quickFilterMenuItem.Click += async (_, __) => await performFilter(filter);
				allItems.Add(quickFilterMenuItem);
			}
			quickFiltersToolStripMenuItem.Items = allItems;
		}

		public void firstFilterIsDefaultToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (sender is MenuItem mi && mi.Icon is CheckBox checkBox)
			{
				checkBox.IsChecked = !(checkBox.IsChecked ?? false);
			}
		}

		public void addQuickFilterBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> QuickFilters.Add(_viewModel.FilterString);

		public async void editQuickFiltersToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await new Dialogs.EditQuickFilters().ShowDialog(this);
	}
}
