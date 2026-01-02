using Avalonia.Collections;
using Avalonia.Controls;
using LibationUiBase.GridView;
using System;
using System.Linq;
using System.Reflection;

namespace LibationAvalonia.Controls
{
	internal static class DataGridContextMenus
	{
		public static event EventHandler<DataGridCellContextMenuStripNeededEventArgs>? CellContextMenuStripNeeded;
		private static readonly ContextMenu ContextMenu = new();
		public static readonly AvaloniaList<Control> MenuItems = new();
		private static readonly PropertyInfo OwningColumnProperty;
		private static readonly PropertyInfo OwningGridProperty;

		static DataGridContextMenus()
		{
			ContextMenu.ItemsSource = MenuItems;
			OwningColumnProperty = typeof(DataGridCell).GetProperty("OwningColumn", BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new InvalidOperationException("Could not find OwningColumn property on DataGridCell");
			OwningGridProperty = typeof(DataGridColumn).GetProperty("OwningGrid", BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new InvalidOperationException("Could not find OwningGrid property on DataGridColumn");
		}

		public static void AttachContextMenu(this DataGridCell cell)
		{
			if (cell is not null && cell.ContextMenu is null)
			{
				cell.ContextRequested += Cell_ContextRequested;
				cell.ContextMenu = ContextMenu;
			}
		}

		private static void Cell_ContextRequested(object? sender, ContextRequestedEventArgs e)
		{
			if (sender is DataGridCell cell &&
				cell.DataContext is GridEntry clickedEntry &&
				OwningColumnProperty.GetValue(cell) is DataGridColumn column &&
				OwningGridProperty.GetValue(column) is DataGrid grid)
			{
				var allSelected = grid.SelectedItems.OfType<GridEntry>().ToArray();
				var clickedIndex = Array.IndexOf(allSelected, clickedEntry);
				if (clickedIndex == -1)
				{
					//User didn't right-click on a selected cell
					grid.SelectedItem = clickedEntry;
					allSelected = [clickedEntry];
				}
				else if (clickedIndex > 0)
				{
					//Ensure the clicked entry is first in the list
					(allSelected[0], allSelected[clickedIndex]) = (allSelected[clickedIndex], allSelected[0]);
				}

				var args = new DataGridCellContextMenuStripNeededEventArgs
				{
					Column = column,
					Grid = grid,
					GridEntries = allSelected,
					ContextMenu = ContextMenu
				};

				args.ContextMenuItems.Clear();
				CellContextMenuStripNeeded?.Invoke(sender, args);
				e.Handled = args.ContextMenuItems.Count == 0;
			}
			else
				e.Handled = true;
		}
	}

	public class DataGridCellContextMenuStripNeededEventArgs
	{
		private static readonly MethodInfo GetCellValueMethod;
		static DataGridCellContextMenuStripNeededEventArgs()
		{
			GetCellValueMethod = typeof(DataGridColumn).GetMethod("GetCellValue", BindingFlags.NonPublic | BindingFlags.Instance)
				?? throw new InvalidOperationException("Could not find GetCellValue method on DataGridColumn");
		}

		private static string GetCellValue(DataGridColumn column, object item)
			=> GetCellValueMethod.Invoke(column, new object[] { item, column.ClipboardContentBinding })?.ToString() ?? "";

		public string CellClipboardContents => GetCellValue(Column, GridEntries[0]);
		public string GetRowClipboardContents()
		{
			if (GridEntries is null || GridEntries.Length == 0)
				return string.Empty;
			else if (GridEntries.Length == 1)
				return HeaderNames + Environment.NewLine + GetRowClipboardContents(GridEntries[0]);
			else
				return string.Join(Environment.NewLine, GridEntries.Select(GetRowClipboardContents).Prepend(HeaderNames));
		}

		private string HeaderNames
			=> string.Join("\t",
				Grid.Columns
				.Where(c => c.IsVisible)
				.OrderBy(c => c.DisplayIndex)
				.Select(c => RemoveLineBreaks(c.Header.ToString() ?? "")));

		private static string RemoveLineBreaks(string text)
			=> text.Replace("\r\n", "").Replace('\r', ' ').Replace('\n', ' ');

		private string GetRowClipboardContents(GridEntry gridEntry)
		{
			var contents = Grid.Columns.Where(c => c.IsVisible).OrderBy(c => c.DisplayIndex).Select(c => RemoveLineBreaks(GetCellValue(c, gridEntry))).ToArray();
			return string.Join("\t", contents);
		}

		public required DataGrid Grid { get; init; }
		public required DataGridColumn Column { get; init; }
		public required GridEntry[] GridEntries { get; init; }
		public required ContextMenu ContextMenu { get; init; }
		public AvaloniaList<Control> ContextMenuItems => DataGridContextMenus.MenuItems;
	}
}
