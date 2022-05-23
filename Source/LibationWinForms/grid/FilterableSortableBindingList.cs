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
	internal class FilterableSortableBindingList : SortableBindingList1<GridEntry>, IBindingListView
	{
		/// <summary>
		/// Items that were removed from the base list due to filtering
		/// </summary>
		private readonly List<GridEntry> FilterRemoved = new();
		private string FilterString;
		private LibationSearchEngine.SearchResultSet SearchResults;
		public FilterableSortableBindingList(IEnumerable<GridEntry> enumeration) : base(enumeration) { }
		public FilterableSortableBindingList() : base(new List<GridEntry>()) { }

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
			SearchResults = SearchEngineCommands.Search(filterString);

			var booksFilteredIn = Items.LibraryBooks().Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => (GridEntry)lbe);

			//Find all series containing children that match the search criteria
			var seriesFilteredIn = Items.Series().Where(s => s.Children.Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => lbe).Any());

			var filteredOut = Items.Except(booksFilteredIn.Concat(seriesFilteredIn)).ToList();

			foreach (var item in filteredOut)
			{
				FilterRemoved.Add(item);
				base.Remove(item);
			}
		}

		public void CollapseAll()
		{
			foreach (var series in Items.Series().ToList())
				CollapseItem(series);
		}

		public void ExpandAll()
		{
			foreach (var series in Items.Series().ToList())
				ExpandItem(series);
		}

		public void CollapseItem(SeriesEntry sEntry)
		{
			foreach (var episode in Items.Where(b => b.Parent == sEntry).Cast<LibraryBookEntry>().ToList())
			{
				FilterRemoved.Add(episode);
				base.Remove(episode);
			}

			sEntry.Liberate.Expanded = false;
		}

		public void ExpandItem(SeriesEntry sEntry)
		{
			var sindex = Items.IndexOf(sEntry);

			foreach (var episode in FilterRemoved.Where(b => b.Parent == sEntry).Cast<LibraryBookEntry>().ToList())
			{
				if (SearchResults is null || SearchResults.Docs.Any(d => d.ProductId == episode.AudibleProductId))
				{
					FilterRemoved.Remove(episode);
					Items.Insert(++sindex, episode);
				}
			}

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			sEntry.Liberate.Expanded = true;
		}

		public void RemoveFilter()
		{
			if (FilterString is null) return;

			int visibleCount = Items.Count;

			foreach (var item in FilterRemoved.ToList())
			{
				if (item.Parent is null || item.Parent.Liberate.Expanded)
				{
					FilterRemoved.Remove(item);
					base.InsertItem(visibleCount++, item);
				}
			}

			if (IsSortedCore)
				Sort();
			else
			//No user sort is applied, so do default sorting by PurchaseDate, descending
			{
				Comparer.PropertyName = nameof(GridEntry.DateAdded);
				Comparer.Direction = ListSortDirection.Descending;
				Sort();
			}

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

			FilterString = null;
			SearchResults = null;
		}
	}
}
