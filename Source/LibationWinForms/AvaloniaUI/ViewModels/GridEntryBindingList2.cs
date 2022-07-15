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

		public void ReplaceList(IEnumerable<GridEntry2> newItems)
		{
			Items.Clear();
			((List<GridEntry2>)Items).AddRange(newItems);
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

			var booksFilteredIn = Items.BookEntries().Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => (GridEntry2)lbe);

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
				if (item is SeriesEntrys2 || item is LibraryBookEntry2 lbe && (lbe.Parent is null || lbe.Parent.Liberate.Expanded))
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

		public void CollapseItem(SeriesEntrys2 sEntry)
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

		public void ExpandItem(SeriesEntrys2 sEntry)
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
