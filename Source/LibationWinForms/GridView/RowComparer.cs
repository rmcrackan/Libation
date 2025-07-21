using LibationUiBase.GridView;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.GridView
{
	internal class RowComparer : RowComparerBase
	{
		public ListSortDirection SortOrder { get; set; } = ListSortDirection.Descending;
		public override string PropertyName { get; set; } = nameof(GridEntry.DateAdded);
		protected override ListSortDirection GetSortOrder() => SortOrder;

		/// <summary>
		/// Helper method for ordering grid entries
		/// </summary>
		public IOrderedEnumerable<GridEntry> OrderEntries(IEnumerable<GridEntry> entries)
			=> SortOrder is ListSortDirection.Descending ? entries.OrderDescending(this) : entries.Order(this);
	}
}
