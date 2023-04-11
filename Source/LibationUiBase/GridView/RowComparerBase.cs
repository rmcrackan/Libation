using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace LibationUiBase.GridView
{
	/// <summary>
	/// This compare class ensures that all top-level grid entries (standalone books or series parents)
	/// are sorted by PropertyName while all episodes remain immediately beneath their parents and remain
	/// sorted by series index, ascending.
	/// </summary>
	public abstract class RowComparerBase : IComparer, IComparer<IGridEntry>, IComparer<object>
	{
		public abstract string PropertyName { get; set; }

		public int Compare(object x, object y)
		{
			if (x is null && y is not null) return -1;
			if (x is not null && y is null) return 1;
			if (x is null && y is null) return 0;

			var geA = (IGridEntry)x;
			var geB = (IGridEntry)y;

			var sortDirection = GetSortOrder();

			ISeriesEntry parentA = null;
			ISeriesEntry parentB = null;

			if (geA is ILibraryBookEntry lbA && lbA.Parent is ISeriesEntry seA)
				parentA = seA;
			if (geB is ILibraryBookEntry lbB && lbB.Parent is ISeriesEntry seB)
				parentB = seB;

			//both a and b are top-level grid entries
			if (parentA is null && parentB is null)
				return InternalCompare(geA, geB);

			//a is top-level, b is a child
			if (parentA is null && parentB is not null)
			{
				// b is a child of a, parent is always first
				if (parentB == geA)
					return sortDirection is ListSortDirection.Ascending ? -1 : 1;
				else
					return InternalCompare(geA, parentB);
			}

			//a is a child, b is a top-level
			if (parentA is not null && parentB is null)
			{
				// a is a child of b, parent is always first
				if (parentA == geB)
					return sortDirection is ListSortDirection.Ascending ? 1 : -1;
				else
					return InternalCompare(parentA, geB);
			}

			//both are children of the same series
			if (parentA == parentB)
			{
				//Podcast episodes usually all have the same PurchaseDate and DateAdded property:
				//the date that the series was added to the library. So when sorting by PurchaseDate
				//and DateAdded, compare SeriesOrder instead..
				return PropertyName switch
				{
					nameof(IGridEntry.DateAdded) or nameof (IGridEntry.PurchaseDate) => geA.SeriesOrder.CompareTo(geB.SeriesOrder),
					_ => InternalCompare(geA, geB),
				};
			}

			//a and b are children of different series.
			return InternalCompare(parentA, parentB);
		}

		protected abstract ListSortDirection GetSortOrder();

		private int InternalCompare(IGridEntry x, IGridEntry y)
		{
			var val1 = x.GetMemberValue(PropertyName);
			var val2 = y.GetMemberValue(PropertyName);

			return x.GetMemberComparer(val1.GetType()).Compare(val1, val2); ;
		}

		public int Compare(IGridEntry x, IGridEntry y)
		{
			return Compare((object)x, y);
		}
	}
}
