using ApplicationServices;
using LibationSearchEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	/*
	 * Allows filtering and sorting of the underlying BindingList<GridEntry>
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
	public class GridEntryBindingList2 : ObservableCollection<GridEntry2>
	{
		public GridEntryBindingList2(IEnumerable<GridEntry2> enumeration)
			: base(new List<GridEntry2>(enumeration)) { }
		public GridEntryBindingList2(List<GridEntry2> list)
			: base(list) { }

		public List<GridEntry2> InternalList => Items as List<GridEntry2>;
		/// <returns>All items in the list, including those filtered out.</returns>
		public List<GridEntry2> AllItems() => Items.Concat(FilterRemoved).ToList();

		/// <summary>When true, itms will not be checked filtered by search criteria on item changed<summary>
		public bool SuspendFilteringOnUpdate { get; set; }
		public string Filter { get => FilterString; set => ApplyFilter(value); }

		/// <summary> Items that were removed from the base list due to filtering </summary>
		private readonly List<GridEntry2> FilterRemoved = new();
		private string FilterString;
		private SearchResultSet SearchResults;

		#region Items Management

		public new void Remove(GridEntry2 entry)
		{
			FilterRemoved.Add(entry);
			base.Remove(entry);
		}

		protected override void InsertItem(int index, GridEntry2 item)
		{
			FilterRemoved.Remove(item);
			base.InsertItem(index, item);
		}

		#endregion

		#region Filtering

		public void ResetCollection()
			=> OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

		private void ApplyFilter(string filterString)
		{
			if (filterString != FilterString)
				RemoveFilter();

			FilterString = filterString;
			SearchResults = SearchEngineCommands.Search(filterString);

			var booksFilteredIn = Items.BookEntries().Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => (GridEntry2)lbe);

			//Find all series containing children that match the search criteria
			var seriesFilteredIn = Items.SeriesEntries().Where(s => s.Children.Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => lbe).Any());

			var filteredOut = Items.Except(booksFilteredIn.Concat(seriesFilteredIn)).ToList();

			foreach (var item in filteredOut)
			{
				Remove(item);
			}
		}

		public void RemoveFilter()
		{
			if (FilterString is null) return;

			int visibleCount = Items.Count;

			foreach (var item in FilterRemoved.ToList())
			{
				if (item is SeriesEntrys2 || item is LibraryBookEntry2 lbe && (lbe.Parent is null || lbe.Parent.Liberate.Expanded))
				{
					InsertItem(visibleCount++, item);
				}
			}

			FilterString = null;
			SearchResults = null;
		}

		#endregion

		#region Expand/Collapse

		public void CollapseAll()
		{
			foreach (var series in Items.SeriesEntries().ToList())
				CollapseItem(series);
		}

		public void ExpandAll()
		{
			foreach (var series in Items.SeriesEntries().ToList())
				ExpandItem(series);
		}

		public void CollapseItem(SeriesEntrys2 sEntry)
		{
			foreach (var episode in Items.BookEntries().Where(b => b.Parent == sEntry).ToList())
			{
				Remove(episode);
			}

			sEntry.Liberate.Expanded = false;
		}

		public void ExpandItem(SeriesEntrys2 sEntry)
		{
			var sindex = Items.IndexOf(sEntry);

			foreach (var episode in FilterRemoved.BookEntries().Where(b => b.Parent == sEntry).ToList())
			{
				if (SearchResults is null || SearchResults.Docs.Any(d => d.ProductId == episode.AudibleProductId))
				{
					InsertItem(++sindex, episode);
				}
			}
			sEntry.Liberate.Expanded = true;
		}

		#endregion
	}
}
