using Avalonia.Controls;
using LibationAvalonia.ViewModels;
using System;

namespace LibationAvalonia.Controls
{
	public partial class DataGridCheckBoxColumnExt : DataGridCheckBoxColumn
	{
		protected override IControl GenerateEditingElementDirect(DataGridCell cell, object dataItem)
		{
			//Only SeriesEntry types have three-state checks, individual LibraryEntry books are binary.
			var ele = base.GenerateEditingElementDirect(cell, dataItem) as CheckBox;
			ele.IsThreeState = dataItem is SeriesEntry;
			return ele;
		}
	}
}
