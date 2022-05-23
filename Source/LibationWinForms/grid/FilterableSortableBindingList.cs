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
		public List<GridEntry> AllItems()
		{
			var allItems = Items.Concat(FilterRemoved);

			var series = allItems.Where(i => i is SeriesEntry).Cast<SeriesEntry>().SelectMany(s => s.Children);

			return series.Concat(allItems).ToList();
		}

		private void ApplyFilter(string filterString)
		{
			if (filterString != FilterString)
				RemoveFilter();

			FilterString = filterString;

			SearchResults = SearchEngineCommands.Search(filterString);
			var filteredOut = Items.Where(i => i is LibraryBookEntry).Cast<LibraryBookEntry>().ExceptBy(SearchResults.Docs.Select(d => d.ProductId), ge => ge.AudibleProductId).Cast<GridEntry>().ToList();

			var parents = Items.Where(i => i is SeriesEntry).Cast<SeriesEntry>();

			foreach (var p in parents)
			{
				if (p.Children.Cast<LibraryBookEntry>().ExceptBy(SearchResults.Docs.Select(d => d.ProductId), ge => ge.AudibleProductId).Count() == p.Children.Count)
				{
					//Don't show series whose episodes have all been filtered out
					filteredOut.Add(p);
				}
			}

			for (int i = 0; i < filteredOut.Count; i++)
			{
				FilterRemoved.Add(filteredOut[i]);
				base.Remove(filteredOut[i]);
			}
		}

		public void CollapseItem(SeriesEntry sEntry)
		{
			foreach (var item in Items.Where(b => b is LibraryBookEntry).Cast<LibraryBookEntry>().Where(b => b.Parent == sEntry).ToList())
				base.Remove(item);

			sEntry.Liberate.Expanded = false;
		}

		public void ExpandItem(SeriesEntry sEntry)
		{
			var sindex = Items.IndexOf(sEntry);
			var children = sEntry.Children.Cast<LibraryBookEntry>().ToList();
			for (int i = 0; i < children.Count; i++)
			{
				if (SearchResults is null || SearchResults.Docs.Any(d=> d.ProductId == children[i].AudibleProductId))
					Insert(++sindex, children[i]);
				else
				{
					FilterRemoved.Add(children[i]);
				}
			}
			sEntry.Liberate.Expanded = true;
		}

		public void RemoveFilter()
		{
			if (FilterString is null) return;

			int visibleCount = Items.Count;
			for (int i = 0; i < FilterRemoved.Count; i++)
			{
				if (FilterRemoved[i].Parent is null || FilterRemoved[i].Parent.Liberate.Expanded)
					base.InsertItem(i + visibleCount, FilterRemoved[i]);
			}

			FilterRemoved.Clear();

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
