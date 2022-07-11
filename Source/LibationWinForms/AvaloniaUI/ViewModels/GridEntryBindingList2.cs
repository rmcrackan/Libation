using ApplicationServices;
using Dinah.Core.DataBinding;
using LibationSearchEngine;
using LibationWinForms.GridView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
		public GridEntryBindingList2(IEnumerable<GridEntry2> enumeration) : base(new List<GridEntry2>(enumeration))
		{
			foreach (var item in enumeration)
				item.PropertyChanged += Item_PropertyChanged;
		}
		/// <returns>All items in the list, including those filtered out.</returns>
		public List<GridEntry2> AllItems() => Items.Concat(FilterRemoved).ToList();

		/// <summary>When true, itms will not be checked filtered by search criteria on item changed<summary>
		public bool SuspendFilteringOnUpdate { get; set; }
		public string Filter { get => FilterString; set => ApplyFilter(value); }
		protected MemberComparer<GridEntry2> Comparer { get; } = new();

		/// <summary> Items that were removed from the base list due to filtering </summary>
		private readonly List<GridEntry2> FilterRemoved = new();
		private string FilterString;
		private SearchResultSet SearchResults;
		private bool isSorted;

		#region Items Management

		public new void Remove(GridEntry2 entry)
		{
			entry.PropertyChanged -= Item_PropertyChanged;
			FilterRemoved.Add(entry);
			base.Remove(entry);
		}

		protected override void RemoveItem(int index)
		{
			var item = Items[index];
			item.PropertyChanged -= Item_PropertyChanged;
			base.RemoveItem(index);
		}

		protected override void ClearItems()
		{
			foreach (var item in Items)
				item.PropertyChanged -= Item_PropertyChanged;
			base.ClearItems();
		}

		protected override void InsertItem(int index, GridEntry2 item)
		{
			item.PropertyChanged += Item_PropertyChanged;
			FilterRemoved.Remove(item);
			base.InsertItem(index, item);
		}

		private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Don't audo-sort Remove column or else Avalonia will crash.
			if (isSorted && e.PropertyName == Comparer.PropertyName && e.PropertyName != nameof(GridEntry.Remove))
			{
				Sort();

				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				return;
			}
		}

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

			if (isSorted)
				Sort();
			else
			{
				//No user sort is applied, so do default sorting by DateAdded, descending
				Comparer.PropertyName = nameof(GridEntry.DateAdded);
				Comparer.Direction = ListSortDirection.Descending;
				Sort();
			}

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

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

		#region Sorting

		public void DoSortCore(string propertyName)
		{
			if (isSorted && Comparer.PropertyName == propertyName)
			{
				Comparer.Direction = ~Comparer.Direction & ListSortDirection.Descending;
			}
			else
			{
				Comparer.PropertyName = propertyName;
				Comparer.Direction = ListSortDirection.Descending;
			}

			Sort();

			isSorted = true;

			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected void Sort()
		{
			var itemsList = (List<GridEntry2>)Items;

			var children = itemsList.BookEntries().Where(i => i.Parent is not null).ToList();

			var sortedItems = itemsList.Except(children).OrderBy(ge => ge, Comparer).ToList();

			itemsList.Clear();

			//Only add parentless items at this stage. After these items are added in the
			//correct sorting order, go back and add the children beneath their parents.
			itemsList.AddRange(sortedItems);

			foreach (var parent in children.Select(c => c.Parent).Distinct())
			{
				var pIndex = itemsList.IndexOf(parent);

				//children should always be sorted by series index.
				foreach (var c in children.Where(c => c.Parent == parent).OrderBy(c => c.SeriesIndex))
					itemsList.Insert(++pIndex, c);
			}
		}

		#endregion
	}
}
