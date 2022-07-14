using Avalonia.Controls;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2
	{
		private DataGridColumn CurrentSortColumn;
		private void Configure_Sorting() { }

		private void ReSort()
		{
			if (CurrentSortColumn is null)
			{
				bindingList.InternalList.Sort((i1, i2) => i2.DateAdded.CompareTo(i1.DateAdded));
				bindingList.ResetCollection();
			}
			else
			{
				CurrentSortColumn.Sort(((RowComparer)CurrentSortColumn.CustomSortComparer).SortDirection ?? ListSortDirection.Ascending);
			}
		}

		private void ProductsGrid_Sorting(object sender, DataGridColumnEventArgs e)
		{
			//Force the comparer to get the current sort order. We can't
			//retrieve it from inside this event handler because Avalonia
			//doesn't set the property until after this event.
			var comparer = e.Column.CustomSortComparer as RowComparer;
			comparer.SortDirection = null;

			CurrentSortColumn = e.Column;
		}
	}
}
