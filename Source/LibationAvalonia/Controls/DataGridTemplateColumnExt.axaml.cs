using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationAvalonia.ViewModels;
using System;
using System.Reflection;

namespace LibationAvalonia.Controls
{	
	public class DataGridViewCellContextMenuStripNeededEventArgs
	{
		private static readonly MethodInfo GetCellValueMethod;
		static DataGridViewCellContextMenuStripNeededEventArgs()
		{
			GetCellValueMethod = typeof(DataGridColumn).GetMethod("GetCellValue", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		private static string GetCellValue(DataGridColumn column, object item)
			=> GetCellValueMethod.Invoke(column, new object[] { item, column.ClipboardContentBinding })?.ToString() ?? "";

		public string CellClipboardContents => GetCellValue(Column, GridEntry);
		public DataGridTemplateColumnExt Column { get; init; }
		public GridEntry GridEntry { get; init; }
		public ContextMenu ContextMenu { get; init; }
		public AvaloniaList<MenuItem> ContextMenuItems
			=> ContextMenu.Items as AvaloniaList<MenuItem>;
	}

	public partial class DataGridTemplateColumnExt : DataGridTemplateColumn
	{
		public event EventHandler<DataGridViewCellContextMenuStripNeededEventArgs> CellContextMenuStripNeeded;

		private static readonly ContextMenu ContextMenu = new();
		private static readonly AvaloniaList<MenuItem> MenuItems  = new();

		public DataGridTemplateColumnExt()
		{
			AvaloniaXamlLoader.Load(this);
			ContextMenu.Items = MenuItems;
		}

		private void Cell_ContextRequested(object sender, ContextRequestedEventArgs e)
		{
			if (sender is DataGridCell cell && cell.DataContext is GridEntry entry)
			{
				var args = new DataGridViewCellContextMenuStripNeededEventArgs
				{
					Column = this,
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

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			if (cell.ContextMenu is null)
			{
				cell.ContextRequested += Cell_ContextRequested;
				cell.ContextMenu = ContextMenu;
			}

			return base.GenerateElement(cell, dataItem);
		}
	}
}
