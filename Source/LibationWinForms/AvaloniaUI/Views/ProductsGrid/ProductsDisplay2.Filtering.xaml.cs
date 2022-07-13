using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2
	{
		private void Configure_Filtering() { }

		public void Filter(string searchString)
		{
			int visibleCount = bindingList.Count;

			if (string.IsNullOrEmpty(searchString))
				bindingList.RemoveFilter();
			else
				bindingList.Filter = searchString;

			if (visibleCount != bindingList.Count)
				VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());

			//Re-sort after filtering
			ReSort();
		}
	}
}
