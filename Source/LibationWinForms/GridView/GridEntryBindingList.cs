using ApplicationServices;
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
		public GridEntryBindingList(IEnumerable<IGridEntry> enumeration) : base(new List<IGridEntry>(enumeration))
		{
			SearchEngineCommands.SearchEngineUpdated += SearchEngineCommands_SearchEngineUpdated;

			refreshEntries();
		}


		/// <returns>All items in the list, including those filtered out.</returns>
		public List<IGridEntry> AllItems() => Items.Concat(FilterRemoved).ToList();

		/// <summary>All items that pass the current filter</summary>
		public IEnumerable<ILibraryBookEntry> GetFilteredInItems()
			=> FilteredInGridEntries?
				.OfType<ILibraryBookEntry>()
			?? FilterRemoved
				.OfType<ILibraryBookEntry>()
				.Union(Items.OfType<ILibraryBookEntry>());

		public bool SupportsFiltering => true;
		public string Filter
		{
			get => FilterString;
			set
			{
				FilterString = value;

				if (Items.Count + FilterRemoved.Count == 0)
					return;

				FilteredInGridEntries = Items.Concat(FilterRemoved).FilterEntries(FilterString);
				refreshEntries();
			}
		}

		/// <summary>When true, itms will not be checked filtered by search criteria on item changed<summary>
		public bool SuspendFilteringOnUpdate { get; set; }

		protected RowComparer Comparer { get; } = new();
		protected override bool SupportsSortingCore => true;
		protected override bool SupportsSearchingCore => true;
		protected override bool IsSortedCore => isSorted;
		protected override PropertyDescriptor SortPropertyCore => propertyDescriptor;
		protected override ListSortDirection SortDirectionCore => listSortDirection;

		/// <summary> Items that were removed from the base list due to filtering </summary>
		private readonly List<IGridEntry> FilterRemoved = new();
		private string FilterString;
		private bool isSorted;
		private ListSortDirection listSortDirection;
		private PropertyDescriptor propertyDescriptor;
		/// <summary> All GridEntries present in the current filter set. If null, no filter is applied and all entries are filtered in.(This was a performance choice)</summary>
		private HashSet<IGridEntry> FilteredInGridEntries;


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

		/// <summary>
		/// This method should be called whenever there's been a change to the
		/// set of all GridEntries that affects sort order or filter status
		/// </summary>
		private void refreshEntries()
		{
			if (FilteredInGridEntries is null)
			{
				addRemovedItemsBack(FilterRemoved.ToList());
			}
			else
			{
				var addBackEntries = FilterRemoved.Intersect(FilteredInGridEntries).ToList();
				var toRemoveEntries = Items.Except(FilteredInGridEntries).ToList();

				addRemovedItemsBack(addBackEntries);

				foreach (var newRemove in toRemoveEntries)
				{
					FilterRemoved.Add(newRemove);
					Items.Remove(newRemove);
				}
			}

			if (IsSortedCore)
				Sort();
			else
			{
				//No user sort is applied, so do default sorting by DateAdded, descending
				Comparer.PropertyName = nameof(IGridEntry.DateAdded);
				Comparer.SortOrder = ListSortDirection.Descending;
				Sort();
			}

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));

			void addRemovedItemsBack(List<IGridEntry> addBackEntries)
			{
				//Add removed entries back into Items so they are displayed
				//(except for episodes that are collapsed)
				foreach (var addBack in addBackEntries)
				{
					if (addBack is ILibraryBookEntry lbe && lbe.Parent is ISeriesEntry se && !se.Liberate.Expanded)
						continue;

					FilterRemoved.Remove(addBack);
					Items.Add(addBack);
				}
			}
		}

		private void SearchEngineCommands_SearchEngineUpdated(object sender, EventArgs e)
		{
			var filterResults = AllItems().FilterEntries(FilterString);

			if (filterResults is not null && FilteredInGridEntries.Intersect(filterResults).Count() != FilteredInGridEntries.Count)
			{
				FilteredInGridEntries = filterResults;
				refreshEntries();
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
			foreach (var episode in sEntry.Children.Intersect(Items.BookEntries()).ToList())
			{
				FilterRemoved.Add(episode);
				base.Remove(episode);
			}

			sEntry.Liberate.Expanded = false;
		}

		public void ExpandItem(ISeriesEntry sEntry)
		{
			var sindex = Items.IndexOf(sEntry);

			foreach (var episode in sEntry.Children.Intersect(FilterRemoved.BookEntries()).ToList())
			{
				if (FilteredInGridEntries?.Contains(episode) ?? true)
				{
					FilterRemoved.Remove(episode);
					InsertItem(++sindex, episode);
				}
			}
			sEntry.Liberate.Expanded = true;
		}

		public void RemoveFilter()
		{
			FilterString = null;
			FilteredInGridEntries = null;
			refreshEntries();
		}

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			Comparer.PropertyName = property.Name;
			Comparer.SortOrder = direction;

			Sort();

			propertyDescriptor = property;
			listSortDirection = direction;
			isSorted = true;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		private void Sort()
		{
			var itemsList = (List<IGridEntry>)Items;
			//User Order/OrderDescending and replace items in list instead of using List.Sort() to achieve stable sorting.
			var sortedItems = Comparer.OrderEntries(itemsList).ToList();

			itemsList.Clear();
			itemsList.AddRange(sortedItems);
		}

		protected override void OnListChanged(ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemChanged && isSorted && e.PropertyDescriptor == SortPropertyCore)
				refreshEntries();
			else
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
