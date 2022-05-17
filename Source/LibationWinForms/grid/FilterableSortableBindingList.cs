using ApplicationServices;
using Dinah.Core.DataBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms
{
	/*
     * Allows filtering of the underlying SortableBindingList<GridEntry>
     * by implementing IBindingListView and using SearchEngineCommands 
     * 
     * When filtering is applied, the filtered-out items are removed
     * from the base list and added to the private FilterRemoved list.
     * When filtering is removed, items in the FilterRemoved list are
     * added back to the base list.
     * 
     * Remove is overridden to ensure that removed items are removed from
     * the base list (visible items) as well as the FilterRemoved list.
     */
	internal class FilterableSortableBindingList : SortableBindingList<GridEntry>, IBindingListView
	{
		/// <summary>
		/// Items that were removed from the base list due to filtering
		/// </summary>
		private readonly List<GridEntry> FilterRemoved = new();
		private string FilterString;
		public FilterableSortableBindingList(IEnumerable<GridEntry> enumeration) : base(enumeration) { }

		public bool SupportsFiltering => true;
		public string Filter { get => FilterString; set => ApplyFilter(value); }

		#region Unused - Advanced Filtering
		public bool SupportsAdvancedSorting => false;

		//This ApplySort overload is only called if SupportsAdvancedSorting is true.
		//Otherwise BindingList.ApplySort() is used
		public void ApplySort(ListSortDescriptionCollection sorts) => throw new NotImplementedException();

		public ListSortDescriptionCollection SortDescriptions => throw new NotImplementedException();
		#endregion

		public new void Remove(GridEntry entry)
		{
			FilterRemoved.Remove(entry);
			base.Remove(entry);
		}

		/// <returns>All items in the list, including those filtered out.</returns>
		public List<GridEntry> AllItems() => Items.Concat(FilterRemoved).ToList();

		private void ApplyFilter(string filterString)
		{
			if (filterString != FilterString)
				RemoveFilter();

			FilterString = filterString;

			var searchResults = SearchEngineCommands.Search(filterString);
			var filteredOut = Items.ExceptBy(searchResults.Docs.Select(d => d.ProductId), ge => ge.AudibleProductId);

			for (int i = Items.Count - 1; i >= 0; i--)
			{
				if (filteredOut.Contains(Items[i]))
				{
					FilterRemoved.Add(Items[i]);
					Items.RemoveAt(i);
					base.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, i));
				}
			}
		}

		public void RemoveFilter()
		{
			if (FilterString is null) return;

			for (int i = 0; i < FilterRemoved.Count; i++)
				base.InsertItem(i, FilterRemoved[i]);

			FilterRemoved.Clear();

			if (IsSortedCore)
				Sort();
			else
				//No user-defined sort is applied, so do default sorting by date added, descending
				((List<GridEntry>)Items).Sort((i1, i2) => i2.LibraryBook.DateAdded.CompareTo(i1.LibraryBook.DateAdded));

			FilterString = null;
		}
	}
}
