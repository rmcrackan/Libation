
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Styling;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;

namespace LibationWinForms.AvaloniaUI.Controls
{
	public partial class DataGridCheckBoxColumnExt : DataGridCheckBoxColumn
	{
		protected override object PrepareCellForEdit(IControl editingElement, RoutedEventArgs editingEventArgs)
		{
			return base.PrepareCellForEdit(editingElement, editingEventArgs);
		}
		protected override IControl GenerateEditingElementDirect(DataGridCell cell, object dataItem)
		{
			var ele = base.GenerateEditingElementDirect(cell, dataItem) as CheckBox;
			ele.Checked += EditingElement_Checked;
			return ele;
		}

		private void EditingElement_Checked(object sender, RoutedEventArgs e)
		{
			var cbox = sender as CheckBox;
			var gEntry = cbox.DataContext as GridEntry2;
			gEntry.Remove = cbox.IsChecked;
		}
	}
}
