using Avalonia.Controls;
using Avalonia.Interactivity;
using LibationWinForms.AvaloniaUI.ViewModels;

namespace LibationWinForms.AvaloniaUI.Controls
{
	/// <summary> The purpose of this extension is to immediately commit any check state changes to the viewmodel </summary>
	public partial class DataGridCheckBoxColumnExt : DataGridCheckBoxColumn
	{
		protected override IControl GenerateEditingElementDirect(DataGridCell cell, object dataItem)
		{
			var ele = base.GenerateEditingElementDirect(cell, dataItem) as CheckBox;
			ele.Checked += EditingElement_Checked;
			ele.Unchecked += EditingElement_Checked;
			ele.Indeterminate += EditingElement_Checked;
			return ele;
		}

		private void EditingElement_Checked(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox cbox && cbox.DataContext is GridEntry2 gentry)
			{
				gentry.Remove = cbox.IsChecked;
				FindDataGridParent(cbox)?.CommitEdit(DataGridEditingUnit.Cell, false);
			}
		}

		DataGrid? FindDataGridParent(IControl? control)
		{
			if (control?.Parent is null) return null;
			else if (control?.Parent is DataGrid dg) return dg;
			else return FindDataGridParent(control?.Parent);
		}
	}
}
