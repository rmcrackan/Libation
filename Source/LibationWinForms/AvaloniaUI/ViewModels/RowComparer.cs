using Avalonia.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	/// <summary>
	/// This compare class ensures that all top-level grid entries (standalone books or series parents)
	/// are sorted by PropertyName while all episodes remain immediately beneath their parents and remain
	/// sorted by series index, ascending. Stable sorting is achieved by comparing the GridEntry.ListIndex
	/// properties when 2 items compare equal.
	/// </summary>
	internal class RowComparer : IComparer
	{
		private static readonly PropertyInfo HeaderCellPi = typeof(DataGridColumn).GetProperty("HeaderCell", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly PropertyInfo CurrentSortingStatePi = typeof(DataGridColumnHeader).GetProperty("CurrentSortingState", BindingFlags.NonPublic | BindingFlags.Instance);

		public DataGridColumn Column { get; init; }
		public string PropertyName { get; private set; }
		public ListSortDirection? SortDirection { get; set; }

		public RowComparer(DataGridColumn column)
		{
			Column = column;
			PropertyName = Column.SortMemberPath;
		}
		public RowComparer(ListSortDirection direction, string propertyName)
		{
			SortDirection = direction;
			PropertyName = propertyName;
		}

		public int Compare(object x, object y)
		{
			if (x is null && y is not null) return -1;
			if (x is not null && y is null) return 1;
			if (x is null && y is null) return 0;

			var geA = (GridEntry2)x;
			var geB = (GridEntry2)y;

			SortDirection ??= GetSortOrder();

			SeriesEntrys2 parentA = null;
			SeriesEntrys2 parentB = null;

			if (geA is LibraryBookEntry2 lbA && lbA.Parent is SeriesEntrys2 seA)
				parentA = seA;
			if (geB is LibraryBookEntry2 lbB && lbB.Parent is SeriesEntrys2 seB)
				parentB = seB;

			//both a and b are top-level grid entries
			if (parentA is null && parentB is null)
				return InternalCompare(geA, geB);

			//a is top-level, b is a child
			if (parentA is null && parentB is not null)
			{
				// b is a child of a, parent is always first
				if (parentB == geA)
					return SortDirection is ListSortDirection.Ascending ? -1 : 1;
				else
					return InternalCompare(geA, parentB);
			}

			//a is a child, b is a top-level
			if (parentA is not null && parentB is null)
			{
				// a is a child of b, parent is always first
				if (parentA == geB)
					return SortDirection is ListSortDirection.Ascending ? 1 : -1;
				else
					return InternalCompare(parentA, geB);
			}

			//both are children of the same series, always present in order of series index, ascending
			if (parentA == parentB)
				return geA.SeriesIndex.CompareTo(geB.SeriesIndex) * (SortDirection is ListSortDirection.Ascending ? 1 : -1);

			//a and b are children of different series.
			return Compare(parentA, parentB);
		}

		//Avalonia doesn't expose the column's CurrentSortingState, so we must get it through reflection
		private ListSortDirection? GetSortOrder()
			=> CurrentSortingStatePi.GetValue(HeaderCellPi.GetValue(Column)) as ListSortDirection?;

		private int InternalCompare(GridEntry2 x, GridEntry2 y)
		{
			var val1 = x.GetMemberValue(PropertyName);
			var val2 = y.GetMemberValue(PropertyName);

			var compareResult = x.GetMemberComparer(val1.GetType()).Compare(val1, val2);

			//If items compare equal, compare them by their positions in the the list.
			//This is how you achieve a stable sort.
			if (compareResult == 0)
				return x.ListIndex.CompareTo(y.ListIndex);
			else
				return compareResult;
		}
	}
}
