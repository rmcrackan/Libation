using Avalonia.Controls;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2
	{
		private ContextMenu contextMenuStrip1 = new ContextMenu();
		private void Configure_ColumnCustomization()
		{
			if (Design.IsDesignMode) return;

			productsGrid.ColumnDisplayIndexChanged += ProductsGrid_ColumnDisplayIndexChanged;

			var config = Configuration.Instance;
			var gridColumnsVisibilities = config.GridColumnsVisibilities;
			var displayIndices = config.GridColumnsDisplayIndices;

			var contextMenu = new ContextMenu();
			contextMenu.MenuClosed += ContextMenu_MenuClosed;
			contextMenu.ContextMenuOpening += ContextMenu_ContextMenuOpening;
			List<Control> menuItems = new();
			contextMenu.Items = menuItems;

			menuItems.Add(new MenuItem { Header = "Show / Hide Columns" });
			menuItems.Add(new MenuItem { Header = "-" });

			var HeaderCell_PI = typeof(DataGridColumn).GetProperty("HeaderCell", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			foreach (var column in productsGrid.Columns)
			{
				var itemName = column.SortMemberPath;

				if (itemName == nameof(ViewModels.GridEntry2.Remove))
					continue;

				menuItems.Add
					(
						new MenuItem
						{
							Header = ((string)column.Header).Replace((char)0xa,' '),
							Tag = column,
							Margin = new Avalonia.Thickness(6 ,0),
							Icon = new CheckBox
							{
								Width = 50,								
							}
						}
					);

				var headercell = HeaderCell_PI.GetValue(column) as DataGridColumnHeader;
				headercell.ContextMenu = contextMenu;

				column.IsVisible = gridColumnsVisibilities.GetValueOrDefault(itemName, true);
			}

			//We must set DisplayIndex properties in ascending order
			foreach (var itemName in displayIndices.OrderBy(i => i.Value).Select(i => i.Key))
			{
				if (!productsGrid.Columns.Any(c => c.SortMemberPath == itemName))
					continue;

				var column = productsGrid.Columns
					.Single(c => c.SortMemberPath == itemName);

				column.DisplayIndex = displayIndices.GetValueOrDefault(itemName, productsGrid.Columns.IndexOf(column));
			}
		}

		private void ContextMenu_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var contextMenu = sender as ContextMenu;
			foreach (var mi in contextMenu.Items.OfType<MenuItem>())
			{
				if (mi.Tag is DataGridColumn column)
				{
					var cbox = mi.Icon as CheckBox;
					cbox.IsChecked = column.IsVisible;
				}
			}
		}

		private void ContextMenu_MenuClosed(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var contextMenu = sender as ContextMenu;
			var config = Configuration.Instance;
			var dictionary = config.GridColumnsVisibilities;
			
			foreach (var mi in contextMenu.Items.OfType<MenuItem>())
			{
				if (mi.Tag is DataGridColumn column)
				{
					var cbox = mi.Icon as CheckBox;
					column.IsVisible = cbox.IsChecked == true;
					dictionary[column.SortMemberPath] = cbox.IsChecked == true;
				}
			}

			//If all columns are hidden, register the context menu on the grid so users can unhide.
			if (!productsGrid.Columns.Any(c => c.IsVisible))
				productsGrid.ContextMenu = contextMenu;
			else
				productsGrid.ContextMenu = null;

			config.GridColumnsVisibilities = dictionary;
		}

		private void ProductsGrid_ColumnDisplayIndexChanged(object sender, Avalonia.Controls.DataGridColumnEventArgs e)
		{
			var config = Configuration.Instance;

			var dictionary = config.GridColumnsDisplayIndices;
			dictionary[e.Column.SortMemberPath] = e.Column.DisplayIndex;
			config.GridColumnsDisplayIndices = dictionary;
		}
	}
}
