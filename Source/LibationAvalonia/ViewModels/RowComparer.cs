using Avalonia.Controls;
using LibationUiBase.GridView;
using System.ComponentModel;
using System.Reflection;

namespace LibationAvalonia.ViewModels
{
	internal class RowComparer : RowComparerBase
	{
		private static readonly PropertyInfo HeaderCellPi = typeof(DataGridColumn).GetProperty("HeaderCell", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly PropertyInfo CurrentSortingStatePi = typeof(DataGridColumnHeader).GetProperty("CurrentSortingState", BindingFlags.NonPublic | BindingFlags.Instance);

		private DataGridColumn Column { get; init; }
		public override string PropertyName { get; set; }

		public RowComparer(DataGridColumn column)
		{
			Column = column;
			PropertyName = Column?.SortMemberPath ?? nameof(IGridEntry.DateAdded);
		}

		//Avalonia doesn't expose the column's CurrentSortingState, so we must get it through reflection
		protected override ListSortDirection GetSortOrder()
			=> Column is null ? ListSortDirection.Descending
			: CurrentSortingStatePi.GetValue(HeaderCellPi.GetValue(Column)) is ListSortDirection lsd ? lsd
			: ListSortDirection.Descending;
	}
}
