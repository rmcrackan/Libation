using Avalonia.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	/// <summary>
	/// This compare class ensures that all top-level grid entries (standalone books or series parents)
	/// are sorted by PropertyName while all episodes remain immediately beneath their parents and remain
	/// sorted by series index, ascending.
	/// </summary>
	internal class RowComparer : IComparer, IComparer<GridEntry2>
	{
		private static readonly System.Reflection.PropertyInfo HeaderCellPi = typeof(DataGridColumn).GetProperty("HeaderCell", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		private static readonly System.Reflection.PropertyInfo CurrentSortingStatePi = typeof(DataGridColumnHeader).GetProperty("CurrentSortingState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

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

			if (geA is SeriesEntrys2 && geB is SeriesEntrys2)
			{
				//Both are parents. Make sure they never compare equal.
				var comparison = InternalCompare(geA, geB);
				if (comparison == 0)
				{
					var propBackup = PropertyName;
					PropertyName = nameof(GridEntry2.Series);
					comparison = InternalCompare(geA, geB);
					PropertyName = propBackup;
					return comparison;
				}
				return comparison;
			}



			//both a and b are standalone
			if (parentA is null && parentB is null)
				return InternalCompare(geA, geB);

			//a is a standalone, b is a child
			if (parentA is null && parentB is not null)
			{
				// b is a child of a, parent is always first
				if (parentB == geA)
					return SortDirection is ListSortDirection.Ascending ? -1 : 1;
				else if (geA is SeriesEntrys2)
				{
					//Both are parents. Make sure they never compare equal.
					var comparison = InternalCompare(geA, parentB);
					if (comparison == 0)
					{
						var propBackup = PropertyName;
						PropertyName = nameof(GridEntry2.Series);
						comparison = InternalCompare(geA, parentB);
						PropertyName = propBackup;
						return comparison;
					}
					return comparison;
				}
				else
					return InternalCompare(geA, parentB);
			}

			//a is a child, b is a standalone
			if (parentA is not null && parentB is null)
			{
				// a is a child of b, parent is always first
				if (parentA == geB)
					return SortDirection is ListSortDirection.Ascending ? 1 : -1;
				else if (geB is SeriesEntrys2)
				{
					//Both are parents. Make sure they never compare equal.
					var comparison = InternalCompare(parentA, geB);
					if (comparison == 0)
					{
						var propBackup = PropertyName;
						PropertyName = nameof(GridEntry2.Series);
						comparison = InternalCompare(parentA, geB);
						PropertyName = propBackup;
						return comparison;
					}
					return comparison;
				}
				else
					return InternalCompare(parentA, geB);
			}

			//both are children of the same series, always present in order of series index, ascending
			if (parentA == parentB)
				return geA.SeriesIndex.CompareTo(geB.SeriesIndex) * (SortDirection is ListSortDirection.Ascending ? 1 : -1);

			//a and b are children of different series. Make sure their parents never compare equal.
			var comparison2 = InternalCompare(parentA, parentB);
			if (comparison2 == 0)
			{
				var propBackup = PropertyName;
				PropertyName = nameof(GridEntry2.Series);
				comparison2 = InternalCompare(parentA, parentB);
				PropertyName = propBackup;
				return comparison2;
			}
			return comparison2;
		}

		//Avalonia doesn't expose the column's CurrentSortingState, so we must get it through reflection
		private ListSortDirection? GetSortOrder()
			=> CurrentSortingStatePi.GetValue(HeaderCellPi.GetValue(Column)) as ListSortDirection?;

		private int InternalCompare(GridEntry2 x, GridEntry2 y)
		{
			var val1 = x.GetMemberValue(PropertyName);
			var val2 = y.GetMemberValue(PropertyName);

			return x.GetMemberComparer(val1.GetType()).Compare(val1, val2);
		}

		public int CompareTo(GridEntry2 other)
		{
			return Compare(this, other);
		}

		public int Compare(GridEntry2 x, GridEntry2 y)
		{
			return Compare((object)x, (object)y) * (SortDirection is ListSortDirection.Ascending ? 1 : -1);
		}
	}
}
