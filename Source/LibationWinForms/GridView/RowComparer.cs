using LibationUiBase.GridView;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.GridView
{
	internal class RowComparer : RowComparerBase
	{
		public ListSortDirection SortOrder { get; set; } = ListSortDirection.Descending;
		public override string PropertyName { get; set; } = nameof(IGridEntry.DateAdded);
		protected override ListSortDirection GetSortOrder() => SortOrder;

		/// <summary>
		/// Helper method for ordering grid entries
		/// </summary>
		public IOrderedEnumerable<IGridEntry> OrderEntries(IEnumerable<IGridEntry> entries)
			=> SortOrder is ListSortDirection.Descending ? entries.OrderDescending(this) : entries.Order(this);
	}
}
