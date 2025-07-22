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
	 * 
	 * Using BindingList.Add/Insert and BindingList.Remove will cause the
	 * BindingList to subscribe/unsibscribe to/from the item's PropertyChanged
	 * event. Adding or removing from the underlying list will not change the
	 * BindingList's subscription to that item.
	 */
	internal class GridEntryBindingList : BindingList<GridEntry>, IBindingListView
	{
		public GridEntryBindingList(IEnumerable<GridEntry> enumeration) : base(new List<GridEntry>(enumeration))
		{
			SearchEngineCommands.SearchEngineUpdated += SearchEngineCommands_SearchEngineUpdated;
			ListChanged += GridEntryBindingList_ListChanged;
		}

		/// <returns>All items in the list, including those filtered out.</returns>
		public List<GridEntry> AllItems() => Items.Concat(FilterRemoved).ToList();

		/// <summary>All items that pass the current filter</summary>
		public IEnumerable<LibraryBookEntry> GetFilteredInItems()
			=> FilteredInGridEntries?
				.OfType<LibraryBookEntry>()
			?? FilterRemoved
				.OfType<LibraryBookEntry>()
				.Union(Items.OfType<LibraryBookEntry>());

		public bool SupportsFiltering => true;
		public string Filter
		{
			get => FilterString;
			set
			{
				FilterString = value;

				if (Items.Count + FilterRemoved.Count == 0)
					return;

				FilteredInGridEntries = AllItems().FilterEntries(FilterString);
				refreshEntries();
			}
		}

		protected RowComparer Comparer { get; } = new();
		protected override bool SupportsSortingCore => true;
		protected override bool SupportsSearchingCore => true;
		protected override bool IsSortedCore => isSorted;
		protected override PropertyDescriptor SortPropertyCore => propertyDescriptor;
		protected override ListSortDirection SortDirectionCore => Comparer.SortOrder;

		/// <summary> Items that were removed from the base list due to filtering </summary>
		private readonly List<GridEntry> FilterRemoved = new();
		private string FilterString;
		private bool isSorted;
		private PropertyDescriptor propertyDescriptor;
		/// <summary> All GridEntries present in the current filter set. If null, no filter is applied and all entries are filtered in.(This was a performance choice)</summary>
		private HashSet<GridEntry> FilteredInGridEntries;

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

		/// <summary>
		/// This method should be called whenever there's been a change to the
		/// set of all GridEntries that affects sort order or filter status
		/// </summary>
		private void refreshEntries()
		{
			var priorState = RaiseListChangedEvents;
			RaiseListChangedEvents = false;

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
					base.Remove(newRemove);
				}
			}

			SortInternal();

			ResetList();
			RaiseListChangedEvents = priorState;

			void addRemovedItemsBack(List<GridEntry> addBackEntries)
			{
				//Add removed entries back into Items so they are displayed
				//(except for episodes that are collapsed)
				foreach (var addBack in addBackEntries)
				{
					if (addBack is LibraryBookEntry lbe && lbe.Parent is SeriesEntry se && !se.Liberate.Expanded)
						continue;

					FilterRemoved.Remove(addBack);
					Add(addBack);
				}
			}
		}

		private void SearchEngineCommands_SearchEngineUpdated(object sender, EventArgs e)
		{
			var filterResults = AllItems().FilterEntries(FilterString);

			if (FilteredInGridEntries.SearchSetsDiffer(filterResults))
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

		public void CollapseItem(SeriesEntry sEntry)
		{
			foreach (var episode in sEntry.Children.Intersect(Items.BookEntries()).ToList())
			{
				FilterRemoved.Add(episode);
				base.Remove(episode);
			}

			sEntry.Liberate.Expanded = false;
		}

		public void ExpandItem(SeriesEntry sEntry)
		{
			var sindex = Items.IndexOf(sEntry);

			foreach (var episode in Comparer.OrderEntries(sEntry.Children.Intersect(FilterRemoved.BookEntries())).ToList())
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

			SortInternal();

			propertyDescriptor = property;
			isSorted = true;

			ResetList();
		}

		private void SortInternal()
		{
			var itemsList = (List<GridEntry>)Items;
			//User Order/OrderDescending and replace items in list instead of using List.Sort() to achieve stable sorting.
			var sortedItems = Comparer.OrderEntries(itemsList).ToList();

			itemsList.Clear();
			itemsList.AddRange(sortedItems);
		}

		private void GridEntryBindingList_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemChanged && IsSortedCore && e.PropertyDescriptor == SortPropertyCore)
				refreshEntries();
		}

		protected override void RemoveSortCore()
		{
			isSorted = false;
			propertyDescriptor = base.SortPropertyCore;
			Comparer.SortOrder = base.SortDirectionCore;
			ResetList();
		}

		private void ResetList()
		{
			var priorState = RaiseListChangedEvents;
			RaiseListChangedEvents = true;
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
			RaiseListChangedEvents = priorState;
		}
	}
}
