using LibationUiBase.GridView;
using System.ComponentModel;

namespace LibationWinForms.GridView
{
	internal class RowComparer : RowComparerBase
	{
		public ListSortDirection? SortOrder { get; set; }
		public override string PropertyName { get; set; }
		protected override ListSortDirection? GetSortOrder() => SortOrder;
	}
}
