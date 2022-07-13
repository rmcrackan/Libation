using Avalonia.Controls;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2
	{
		private void Configure_Sorting() { }


		private void RegisterCustomColumnComparers()
		{

			removeGVColumn.CustomSortComparer = new RowComparer(removeGVColumn);
			liberateGVColumn.CustomSortComparer = new RowComparer(liberateGVColumn);
			titleGVColumn.CustomSortComparer = new RowComparer(titleGVColumn);
			authorsGVColumn.CustomSortComparer = new RowComparer(authorsGVColumn);
			narratorsGVColumn.CustomSortComparer = new RowComparer(narratorsGVColumn);
			lengthGVColumn.CustomSortComparer = new RowComparer(lengthGVColumn);
			seriesGVColumn.CustomSortComparer = new RowComparer(seriesGVColumn);
			descriptionGVColumn.CustomSortComparer = new RowComparer(descriptionGVColumn);
			categoryGVColumn.CustomSortComparer = new RowComparer(categoryGVColumn);
			productRatingGVColumn.CustomSortComparer = new RowComparer(productRatingGVColumn);
			purchaseDateGVColumn.CustomSortComparer = new RowComparer(purchaseDateGVColumn);
			myRatingGVColumn.CustomSortComparer = new RowComparer(myRatingGVColumn);
			miscGVColumn.CustomSortComparer = new RowComparer(miscGVColumn);
			tagAndDetailsGVColumn.CustomSortComparer = new RowComparer(tagAndDetailsGVColumn);
		}

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



		private DataGridColumn CurrentSortColumn;


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
