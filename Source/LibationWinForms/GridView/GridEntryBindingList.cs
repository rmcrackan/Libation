using ApplicationServices;
using Dinah.Core.DataBinding;
using LibationSearchEngine;
using LibationUiBase.GridView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.GridView
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
	internal class GridEntryBindingList : BindingList<IGridEntry>, IBindingListView
	{
		public GridEntryBindingList() : base(new List<IGridEntry>()) { }
		public GridEntryBindingList(IEnumerable<IGridEntry> enumeration) : base(new List<IGridEntry>(enumeration)) { }

		/// <returns>All items in the list, including those filtered out.</returns>
		public List<IGridEntry> AllItems() => Items.Concat(FilterRemoved).ToList();

		/// <summary>All items that pass the current filter</summary>
		public IEnumerable<ILibraryBookEntry> GetFilteredInItems()
			=> SearchResults is null
			? FilterRemoved
				.OfType<ILibraryBookEntry>()
				.Union(Items.OfType<ILibraryBookEntry>())
			: FilterRemoved
				.OfType<ILibraryBookEntry>()
				.Join(SearchResults.Docs, o => o.Book.AudibleProductId, i => i.ProductId, (o, _) => o)
				.Union(Items.OfType<ILibraryBookEntry>());

		public bool SupportsFiltering => true;
		public string Filter { get => FilterString; set => ApplyFilter(value); }

		/// <summary>When true, itms will not be checked filtered by search criteria on item changed<summary>
		public bool SuspendFilteringOnUpdate { get; set; }

		protected MemberComparer<IGridEntry> Comparer { get; } = new();
		protected override bool SupportsSortingCore => true;
		protected override bool SupportsSearchingCore => true;
		protected override bool IsSortedCore => isSorted;
		protected override PropertyDescriptor SortPropertyCore => propertyDescriptor;
		protected override ListSortDirection SortDirectionCore => listSortDirection;

		/// <summary> Items that were removed from the base list due to filtering </summary>
		private readonly List<IGridEntry> FilterRemoved = new();
		private string FilterString;
		private SearchResultSet SearchResults;
		private bool isSorted;
		private ListSortDirection listSortDirection;
		private PropertyDescriptor propertyDescriptor;


		#region Unused - Advanced Filtering
		public bool SupportsAdvancedSorting => false;

		//This ApplySort overload is only called if SupportsAdvancedSorting is true.
		//Otherwise BindingList.ApplySort() is used
		public void ApplySort(ListSortDescriptionCollection sorts) => throw new NotImplementedException();

		public ListSortDescriptionCollection SortDescriptions => throw new NotImplementedException();
		#endregion

		public new void Remove(IGridEntry entry)
		{
			FilterRemoved.Remove(entry);
			base.Remove(entry);
		}

		private void ApplyFilter(string filterString)
		{
			if (filterString != FilterString)
				RemoveFilter();

			FilterString = filterString;
			SearchResults = SearchEngineCommands.Search(filterString);

			var booksFilteredIn = Items.BookEntries().Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => (IGridEntry)lbe);

			//Find all series containing children that match the search criteria
			var seriesFilteredIn = Items.SeriesEntries().Where(s => s.Children.Join(SearchResults.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => lbe).Any());

			var filteredOut = Items.Except(booksFilteredIn.Concat(seriesFilteredIn)).ToList();

			foreach (var item in filteredOut)
			{
				FilterRemoved.Add(item);
				base.Remove(item);
			}
		}

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

		public void CollapseItem(ISeriesEntry sEntry)
		{
			foreach (var episode in sEntry.Children.Join(Items.BookEntries(), o => o, i => i, (_, i) => i).ToList())
			{
				FilterRemoved.Add(episode);
				base.Remove(episode);
			}

			sEntry.Liberate.Expanded = false;
		}

		public void ExpandItem(ISeriesEntry sEntry)
		{
			var sindex = Items.IndexOf(sEntry);

			foreach (var episode in sEntry.Children.Join(FilterRemoved.BookEntries(), o => o, i => i, (_, i) => i).ToList())
			{
				if (SearchResults is null || SearchResults.Docs.Any(d => d.ProductId == episode.AudibleProductId))
				{
					FilterRemoved.Remove(episode);
					InsertItem(++sindex, episode);
				}
			}
			sEntry.Liberate.Expanded = true;
		}

		public void RemoveFilter()
		{
			if (FilterString is null) return;

			int visibleCount = Items.Count;

			foreach (var item in FilterRemoved.ToList())
			{
				if (item is ISeriesEntry || (item is ILibraryBookEntry lbe && (lbe.Liberate.IsBook || lbe.Parent.Liberate.Expanded)))
				{
					FilterRemoved.Remove(item);
					InsertItem(visibleCount++, item);
				}
			}

			if (IsSortedCore)
				Sort();
			else
			//No user sort is applied, so do default sorting by DateAdded, descending
			{
				Comparer.PropertyName = nameof(IGridEntry.DateAdded);
				Comparer.Direction = ListSortDirection.Descending;
				Sort();
			}

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

			FilterString = null;
			SearchResults = null;
		}

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			Comparer.PropertyName = property.Name;
			Comparer.Direction = direction;

			Sort();

			propertyDescriptor = property;
			listSortDirection = direction;
			isSorted = true;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected void Sort()
		{
			var itemsList = (List<IGridEntry>)Items;

			var children = itemsList.BookEntries().Where(i => i.Liberate.IsEpisode).ToList();

			var sortedItems = itemsList.Except(children).OrderBy(ge => ge, Comparer).ToList();

			itemsList.Clear();

			//Only add parentless items at this stage. After these items are added in the
			//correct sorting order, go back and add the children beneath their parents.
			itemsList.AddRange(sortedItems);

			foreach (var parent in children.Select(c => c.Parent).Distinct())
			{
				var pIndex = itemsList.IndexOf(parent);

				//children are sorted beneath their series parent
				foreach (var c in children.Where(c => c.Parent == parent).OrderBy(c => c, Comparer))
					itemsList.Insert(++pIndex, c);
			}
		}

		protected override void OnListChanged(ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemChanged)
			{
				if (FilterString is not null && !SuspendFilteringOnUpdate && Items[e.NewIndex] is ILibraryBookEntry lbItem)
				{
					SearchResults = SearchEngineCommands.Search(FilterString);
					if (!SearchResults.Docs.Any(d => d.ProductId == lbItem.AudibleProductId))
					{
						FilterRemoved.Add(lbItem);
						base.Remove(lbItem);
						return;
					}
				}

				if (isSorted && e.PropertyDescriptor == SortPropertyCore)
				{
					var item = Items[e.NewIndex];
					Sort();
					var newIndex = Items.IndexOf(item);

					base.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemMoved, newIndex, e.NewIndex));
					return;
				}
			}
			base.OnListChanged(e);
		}

		protected override void RemoveSortCore()
		{
			isSorted = false;
			propertyDescriptor = base.SortPropertyCore;
			listSortDirection = base.SortDirectionCore;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
	}
}
