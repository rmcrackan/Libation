using Avalonia.Controls;
using Avalonia.Interactivity;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Reflection;

namespace LibationWinForms.AvaloniaUI.Controls
{
	/// <summary> The purpose of this extension it to immediately commit any check
	/// state changes to the viewmodel. There must be a better way to do this, but
	/// I sure as shit can't find it. </summary>
	public partial class DataGridCheckBoxColumnExt : DataGridCheckBoxColumn
	{
		Func<DataGrid> _owningGrid_get;
		Func<DataGridEditAction, bool, bool, bool, bool> _endCellEdit;
		Func<Action, bool> _waitForLostFocus;
		public DataGrid OwningGrid
		{
			get
			{
				if (_owningGrid_get == null)
				{
					var pi = typeof(DataGridColumn).GetProperty(nameof(OwningGrid), BindingFlags.NonPublic | BindingFlags.Instance);
					var mi = pi.GetGetMethod(true);
					_owningGrid_get = mi.CreateDelegate<Func<DataGrid>>(this);
				}
				return _owningGrid_get();
			}
		}

		public Func<Action, bool> WaitForLostFocus
		{
			get
			{
				if (_endCellEdit == null)
				{
					var mi = typeof(DataGrid).GetMethod(nameof(WaitForLostFocus), BindingFlags.NonPublic | BindingFlags.Instance);
					_waitForLostFocus = mi.CreateDelegate<Func<Action, bool>>(OwningGrid);
				}
				return _waitForLostFocus;
			}
		}

		public Func<DataGridEditAction, bool, bool, bool, bool> EndCellEdit
		{
			get
			{
				if (_endCellEdit == null)
				{
					var mi = typeof(DataGrid).GetMethod(nameof(EndCellEdit), BindingFlags.NonPublic | BindingFlags.Instance);
					_endCellEdit = mi.CreateDelegate<Func<DataGridEditAction, bool, bool, bool, bool>>(OwningGrid);
				}
				return _endCellEdit;
			}
		}

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
				var check = cbox.IsChecked;
				WaitForLostFocus(() =>
				{
					EndCellEdit(DataGridEditAction.Cancel, true, true, false);
					gentry.Remove = check;
				});
			}
		}
	}
}
