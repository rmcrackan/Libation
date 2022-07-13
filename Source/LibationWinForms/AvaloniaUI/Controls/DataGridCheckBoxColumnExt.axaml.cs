using Avalonia.Controls;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;

namespace LibationWinForms.AvaloniaUI.Controls
{
	public partial class DataGridCheckBoxColumnExt : DataGridCheckBoxColumn
	{
		protected override IControl GenerateEditingElementDirect(DataGridCell cell, object dataItem)
		{
			//Only SeriesEntry types have three-state checks, individual LibraryEntry books are binary.
			var ele = base.GenerateEditingElementDirect(cell, dataItem) as CheckBox;
			ele.IsThreeState = dataItem is SeriesEntrys2;
			return ele;
		}
	}
}
