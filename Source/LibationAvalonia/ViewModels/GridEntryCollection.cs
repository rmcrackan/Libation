using ApplicationServices;
using LibationSearchEngine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace LibationAvalonia.ViewModels
{
	/*
	 * Allows filtering of the underlying ObservableCollection<GridEntry>
	 * 
	 * When filtering is applied, the filtered-out items are removed
	 * from the base list and added to the private FilterRemoved list.
	 * When filtering is removed, items in the FilterRemoved list are
	 * added back to the base list.
	 * 
	 * Items are added and removed to/from the ObservableCollection's
	 * internal list instead of the ObservableCollection itself to
	 * avoid ObservableCollection firing CollectionChanged for every
	 * item. Editing the list this way improve's display performance,
	 * but requires ResetCollection() to be called after all changes
	 * have been made.
	 */
	public class GridEntryCollection : ObservableCollection<GridEntry>
	{
		public GridEntryCollection(IEnumerable<GridEntry> enumeration)
			: base(new List<GridEntry>(enumeration)) { }
		public GridEntryCollection(List<GridEntry> list)
			: base(list) { }

		public List<GridEntry> InternalList => Items as List<GridEntry>;
		/// <returns>All items in the list, including those filtered out.</returns>
		public List<GridEntry> AllItems() => Items.Concat(FilterRemoved).ToList();

		/// <summary>When true, itms will not be checked filtered by search criteria on item changed<summary>
		public bool SuspendFilteringOnUpdate { get; set; }
		public string Filter { get => FilterString; set => ApplyFilter(value); }

		/// <summary> Items that were removed from the base list due to filtering </summary>
		private readonly List<GridEntry> FilterRemoved = new();
		private string FilterString;
		private SearchResultSet SearchResults;

		#region Items Management

		/// <summary>
		/// Removes all items from the collection, both visible and hidden, adds new items to the visible collection.
		/// </summary>
		public void ReplaceList(IEnumerable<GridEntry> newItems)
		{
			Items.Clear();
			FilterRemoved.Clear();
			((List<GridEntry>)Items).AddRange(newItems);
			ResetCollection();
		}
		public void ResetCollection()
			=> OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

		#endregion

		#region Filtering


		private void ApplyFilter(string filterString)
		{
			if (filterString != FilterString)
				RemoveFilter();

			FilterString = filterString;
			SearchResults = SearchEngineCommands.Search(filterString);

			var booksFilteredIn = Items.BookEntries().Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => (GridEntry)lbe);

			//Find all series containing children that match the search criteria
			var seriesFilteredIn = Items.SeriesEntries().Where(s => s.Children.Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => lbe).Any());

			var filteredOut = Items.Except(booksFilteredIn.Concat(seriesFilteredIn)).ToList();

			foreach (var item in filteredOut)
			{
				FilterRemoved.Add(item);
				Items.Remove(item);
			}
			ResetCollection();
		}

		public void RemoveFilter()
		{
			if (FilterString is null) return;

			int visibleCount = Items.Count;

			foreach (var item in FilterRemoved.ToList())
			{
				if (item is SeriesEntry || item is LibraryBookEntry lbe && (lbe.Parent is null || lbe.Parent.Liberate.Expanded))
				{

					FilterRemoved.Remove(item);
					Items.Insert(visibleCount++, item);
				}
			}

			FilterString = null;
			SearchResults = null;
			ResetCollection();
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

		public void CollapseItem(SeriesEntry sEntry)
		{
			foreach (var episode in Items.BookEntries().Where(b => b.Parent == sEntry).OrderByDescending(lbe => lbe.SeriesIndex).ToList())
			{
				/*
				 * Bypass ObservationCollection's InsertItem method so that CollectionChanged isn't
				 * fired. When adding or removing many items at once, Avalonia's CollectionChanged
				 * event handler causes serious performance problems. And unfotrunately, Avalonia
				 * doesn't respect the NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList? changedItems)
				 * overload that would fire only once for all changed items.
				 * 
				 * Doing this requires resetting the list so the view knows it needs to rebuild its display.
				 */

				FilterRemoved.Add(episode);
				Items.Remove(episode);
			}

			sEntry.Liberate.Expanded = false;
			ResetCollection();
		}

		public void ExpandItem(SeriesEntry sEntry)
		{
			var sindex = Items.IndexOf(sEntry);

			foreach (var episode in FilterRemoved.BookEntries().Where(b => b.Parent == sEntry).OrderByDescending(lbe => lbe.SeriesIndex).ToList())
			{
				if (SearchResults is null || SearchResults.Docs.Any(d => d.ProductId == episode.AudibleProductId))
				{
					/*
					 * Bypass ObservationCollection's InsertItem method so that CollectionChanged isn't
					 * fired. When adding or removing many items at once, Avalonia's CollectionChanged
					 * event handler causes serious performance problems. And unfotrunately, Avalonia
					 * doesn't respect the NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList? changedItems)
					 * overload that would fire only once for all changed items.
					 * 
					 * Doing this requires resetting the list so the view knows it needs to rebuild its display.
					 */

					FilterRemoved.Remove(episode);
					Items.Insert(++sindex, episode);
				}
			}

			sEntry.Liberate.Expanded = true;
			ResetCollection();
		}

		#endregion
	}
}
