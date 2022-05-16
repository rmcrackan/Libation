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
     * by implementing IBindingListView aud using SearchEngineCommands 
     * 
     * When filtering is applied, the filtered-out items are removed
     * from the base list and added to the private FilterRemoved list.
     * All items, filtered or not, are stored in the private AllItems
     * list. When filtering is removed, items in the FilterRemoved list
     * are added back to the base list.
     * 
     * Remove and InsertItem are overridden to ensure that the current
     * filter remains applied when items are removed/added to the list.
     */
	internal class SortableFilterableBindingList : SortableBindingList<GridEntry>, IBindingListView
	{
		/// <summary>
		/// Items that were removed from the list due to filtering
		/// </summary>
		private readonly List<GridEntry> FilterRemoved = new();
		/// <summary>
		/// Tracks all items in the list, both filtered and not.
		/// </summary>
		public readonly List<GridEntry> AllItems;
		private string FilterString;
		private Action Sort;
		public SortableFilterableBindingList(IEnumerable<GridEntry> enumeration) : base(enumeration)
		{
			AllItems = new List<GridEntry>(Items);

			//This is only necessary because SortableBindingList doesn't expose Sort()
			//You should make SortableBindingList.Sort protected and remove reflection
			var method = typeof(SortableBindingList<GridEntry>).GetMethod("Sort", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			Sort = method.CreateDelegate<Action>(this);
		}

		public bool SupportsFiltering => true;
		public string Filter { get => FilterString; set => ApplyFilter(value); }

		#region Unused - Advanced Filtering
		public bool SupportsAdvancedSorting => false;


		//This ApplySort overload is only called is SupportsAdvancedSorting is true.
		//Otherwise BindingList.ApplySort() is used
		public void ApplySort(ListSortDescriptionCollection sorts) => throw new NotImplementedException();

		public ListSortDescriptionCollection SortDescriptions => throw new NotImplementedException();
		#endregion

		public new void Remove(GridEntry entry)
		{
			AllItems.Remove(entry);
			FilterRemoved.Remove(entry);
			base.Remove(entry);
		}

		protected override void InsertItem(int index, GridEntry item)
		{
			AllItems.Insert(index, item);
			base.InsertItem(index, item);
		}

		private void ApplyFilter(string filterString)
		{
			if (filterString != FilterString)
				RemoveFilter();

			FilterString = filterString;

			var searchResults = SearchEngineCommands.Search(filterString);
			var filteredOut = Items.ExceptBy(searchResults.Docs.Select(d=>d.ProductId), ge=>ge.AudibleProductId);

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
			{
				Items.Insert(i, FilterRemoved[i]);
				base.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, i));
			}

			FilterRemoved.Clear();

			if (IsSortedCore)
				Sort();
			else
				//No user-defined sort is applied, so do default sorting by date added, descending
				((List<GridEntry>)Items).Sort((i1,i2) =>i2.LibraryBook.DateAdded.CompareTo(i1.LibraryBook.DateAdded));

			FilterString = null;
		}
	}
}
