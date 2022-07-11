using Avalonia.Controls;
using LibationFileManager;
using LibationWinForms.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_QuickFilters()
		{
			Opened += updateFirstFilterIsDefaultToolStripMenuItem;
			Opened += updateFiltersMenu;
			QuickFilters.UseDefaultChanged += updateFirstFilterIsDefaultToolStripMenuItem;
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
				quickFilterMenuItem.Click += (_, __) => performFilter(filter);
				allItems.Add(quickFilterMenuItem);
			}
			quickFiltersToolStripMenuItem.Items = allItems;
		}

		private void updateFirstFilterIsDefaultToolStripMenuItem(object sender, EventArgs e)
			=> firstFilterIsDefaultToolStripMenuItem_Checkbox.IsChecked = QuickFilters.UseDefault;

		public void firstFilterIsDefaultToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> QuickFilters.UseDefault = firstFilterIsDefaultToolStripMenuItem_Checkbox.IsChecked != true;

		public void addQuickFilterBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> QuickFilters.Add(this.filterSearchTb.Text);

		public void editQuickFiltersToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e) => new EditQuickFilters().ShowDialog();

		public void productsDisplay_Initialized(object sender, EventArgs e)
		{
			if (QuickFilters.UseDefault)
				performFilter(QuickFilters.Filters.FirstOrDefault());
		}
	}
}
