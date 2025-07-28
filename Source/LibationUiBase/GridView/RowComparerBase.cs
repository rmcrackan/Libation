using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

#nullable enable
namespace LibationUiBase.GridView
{
	/// <summary>
	/// This compare class ensures that all top-level grid entries (standalone books or series parents)
	/// are sorted by PropertyName while all episodes remain immediately beneath their parents and remain
	/// sorted by series index, ascending.
	/// </summary>
	public abstract class RowComparerBase : IComparer, IComparer<GridEntry>, IComparer<object>
	{
		public abstract string? PropertyName { get; set; }

		public int Compare(object? x, object? y)
			=> Compare(x as GridEntry, y as GridEntry);

		protected abstract ListSortDirection GetSortOrder();

		private int InternalCompare(GridEntry x, GridEntry y)
		{
			//Default values (e.g. empty strings) always sort to the end of the list.
			var val1IsDefault = x.MemberValueIsDefault(PropertyName);
			var val2IsDefault = y.MemberValueIsDefault(PropertyName);

			if (val1IsDefault && val2IsDefault) return 0;
			else if (val1IsDefault && !val2IsDefault) return GetSortOrder() is ListSortDirection.Ascending ? 1 : -1;
			else if (!val1IsDefault && val2IsDefault) return GetSortOrder() is ListSortDirection.Ascending ? -1 : 1;
			else
			{
				var val1 = x.GetMemberValue(PropertyName);
				var val2 = y.GetMemberValue(PropertyName);
				var compare = x.GetMemberComparer(val1.GetType()).Compare(val1, val2);

				return compare == 0 && x.Liberate.IsSeries && y.Liberate.IsSeries
					//Both a and b are series parents and compare as equal, so break the tie.
					? x.AudibleProductId.CompareTo(y.AudibleProductId)
					: compare;
			}
		}

		public int Compare(GridEntry? geA, GridEntry? geB)
		{
			if (geA is null && geB is not null) return -1;
			if (geA is not null && geB is null) return 1;
			if (geA is null || geB is null) return 0;

			var sortDirection = GetSortOrder();

			SeriesEntry? parentA = null;
			SeriesEntry? parentB = null;

			if (geA is LibraryBookEntry lbA && lbA.Parent is SeriesEntry seA)
				parentA = seA;
			if (geB is LibraryBookEntry lbB && lbB.Parent is SeriesEntry seB)
				parentB = seB;

			//both entries are children
			if (parentA != null && parentB != null)
			{
				//both are children of the same series
				if (parentA == parentB)
				{
					//Podcast episodes usually all have the same PurchaseDate and DateAdded property:
					//the date that the series was added to the library. So when sorting by PurchaseDate
					//and DateAdded, compare SeriesOrder instead..
					return PropertyName switch
					{
						nameof(GridEntry.DateAdded) or nameof(GridEntry.PurchaseDate) => geA.SeriesOrder.CompareTo(geB.SeriesOrder),
						_ => InternalCompare(geA, geB),
					};
				}
				else
					//a and b are children of different series.
					return InternalCompare(parentA, parentB);
			}

			//a is top-level, b is a child
			else if (parentA is null && parentB is not null)
			{
				// b is a child of a, parent is always first
				if (parentB == geA)
					return sortDirection is ListSortDirection.Ascending ? -1 : 1;
				else
					return InternalCompare(geA, parentB);
			}
			//a is a child, b is a top-level
			else if (parentA is not null && parentB is null)
			{
				// a is a child of b, parent is always first
				if (parentA == geB)
					return sortDirection is ListSortDirection.Ascending ? 1 : -1;
				else
					return InternalCompare(parentA, geB);
			}
			//parentA and parentB are null
			else
			{
				//both a and b are top-level grid entries
				return InternalCompare(geA, geB);
			}
		}
	}
}
