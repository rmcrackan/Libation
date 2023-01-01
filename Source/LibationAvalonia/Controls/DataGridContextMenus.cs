using Avalonia.Collections;
using Avalonia.Controls;
using LibationAvalonia.ViewModels;
using System;
using System.Reflection;

namespace LibationAvalonia.Controls
{
	internal static class DataGridContextMenus
	{
		public static event EventHandler<DataGridCellContextMenuStripNeededEventArgs> CellContextMenuStripNeeded;
		private static readonly ContextMenu ContextMenu = new();
		private static readonly AvaloniaList<MenuItem> MenuItems = new();
		private static readonly PropertyInfo OwningColumnProperty;

		static DataGridContextMenus()
		{
			ContextMenu.Items = MenuItems;
			OwningColumnProperty = typeof(DataGridCell).GetProperty("OwningColumn", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		public static void AttachContextMenuToCell(this DataGridCell cell)
		{
			if (cell is not null && cell.ContextMenu is null)
			{
				cell.ContextRequested += Cell_ContextRequested;
				cell.ContextMenu = ContextMenu;
			}
		}

		private static void Cell_ContextRequested(object sender, ContextRequestedEventArgs e)
		{
			if (sender is DataGridCell cell && cell.DataContext is GridEntry entry)
			{
				var args = new DataGridCellContextMenuStripNeededEventArgs
				{
					Column = OwningColumnProperty.GetValue(cell) as DataGridColumn,
					GridEntry = entry,
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
			GetCellValueMethod = typeof(DataGridColumn).GetMethod("GetCellValue", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		private static string GetCellValue(DataGridColumn column, object item)
			=> GetCellValueMethod.Invoke(column, new object[] { item, column.ClipboardContentBinding })?.ToString() ?? "";

		public string CellClipboardContents => GetCellValue(Column, GridEntry);
		public DataGridColumn Column { get; init; }
		public GridEntry GridEntry { get; init; }
		public ContextMenu ContextMenu { get; init; }
		public AvaloniaList<MenuItem> ContextMenuItems
			=> ContextMenu.Items as AvaloniaList<MenuItem>;
	}
}
