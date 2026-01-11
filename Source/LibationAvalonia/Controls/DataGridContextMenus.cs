using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Controls;

public class DataGridCellContextMenu<TContext> where TContext : class
{
	public static DataGridCellContextMenu<TContext>? Create(ContextMenu? contextMenu)
	{
		DataGrid? grid = null;
		DataGridCell? cell = null;
		var parent = contextMenu?.Parent;
		while (parent is not null && grid is null)
		{
			grid ??= parent as DataGrid;
			cell ??= parent as DataGridCell;

			parent = parent.Parent;
		}

		if (grid is null || cell is null || cell.Tag is not DataGridColumn column || contextMenu!.DataContext is not TContext clickedEntry)
			return null;

		var allSelected = grid.SelectedItems.OfType<TContext>().ToArray();
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

		return new DataGridCellContextMenu<TContext>(contextMenu, grid, column, allSelected);
	}

	public string CellClipboardContents
	{
		get
		{
			var lines = GetClipboardLines(getClickedCell: true);
			return lines.Count >= 1 ? lines[0] : string.Empty;
		}
	}
	public string GetRowClipboardContents() => string.Join(Environment.NewLine, GetClipboardLines(false));

	public ContextMenu ContextMenu { get; }
	public DataGrid Grid { get; }
	public DataGridColumn Column { get; }
	public TContext[] RowItems { get; }
	public AvaloniaList<Control> ContextMenuItems { get; }

	private DataGridCellContextMenu(ContextMenu contextMenu, DataGrid grid, DataGridColumn column, TContext[] rowItems)
	{
		Grid = grid;
		Column = column;
		RowItems = rowItems;
		ContextMenu = contextMenu;
		ContextMenuItems = contextMenu.ItemsSource as AvaloniaList<Control> ?? new();
		contextMenu.ItemsSource = ContextMenuItems;
		ContextMenuItems.Clear();
	}

	private List<string> GetClipboardLines(bool getClickedCell)
	{
		if (RowItems is null || RowItems.Length == 0)
			return [];

		List<string> lines = [];
		Grid.CopyingRowClipboardContent += Grid_CopyingRowClipboardContent;
		Grid.RaiseEvent(GetCopyEventArgs());
		Grid.CopyingRowClipboardContent -= Grid_CopyingRowClipboardContent;
		return lines;

		void Grid_CopyingRowClipboardContent(object? sender, DataGridRowClipboardEventArgs e)
		{
			if (getClickedCell)
			{
				if (e.IsColumnHeadersRow)
					return;
				var cellContent = e.ClipboardRowContent.FirstOrDefault(c => c.Column == Column);
				if (cellContent.Column is not null)
				{
					lines.Add(cellContent.Content?.ToString() ?? string.Empty);
				}
			}
			else if (e.Item == RowItems[0])
				lines.Insert(1, FormatClipboardRowContent(e));
			else
				lines.Add(FormatClipboardRowContent(e));

			//Clear so that the DataGrid copy implementation doesn't set the clipboard
			e.ClipboardRowContent.Clear();
		}
	}

	private static KeyEventArgs GetCopyEventArgs() => new()
	{
		Key = Key.C,
		KeyModifiers = KeyModifiers.Control,
		Route = Avalonia.Interactivity.RoutingStrategies.Bubble,
		PhysicalKey = PhysicalKey.C,
		KeySymbol = "c",
		KeyDeviceType = KeyDeviceType.Keyboard,
		RoutedEvent = InputElement.KeyDownEvent
	};

	private string FormatClipboardRowContent(DataGridRowClipboardEventArgs e)
		=> string.Join("\t", e.ClipboardRowContent.Select(c => RemoveLineBreaks(c.Content?.ToString())));
	private static string RemoveLineBreaks(string? text)
		=> text?.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ') ?? "";

}
