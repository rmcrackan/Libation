using Avalonia.Controls;
using DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reflection;
using System.Collections;
using Avalonia.Threading;
using ApplicationServices;
using AudibleUtilities;
using LibationWinForms.AvaloniaUI.Views;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class ProductsDisplayViewModel : ViewModelBase
	{
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;
		public event EventHandler<int> RemovableCountChanged;
		public event EventHandler InitialLoaded;

		private DataGridColumn _currentSortColumn;
		private DataGrid productsDataGrid;

		private GridEntryBindingList2 _gridEntries;
		private bool _removeColumnVisivle;
		public GridEntryBindingList2 GridEntries { get => _gridEntries; private set => this.RaiseAndSetIfChanged(ref _gridEntries, value); }
		public bool RemoveColumnVisivle { get => _removeColumnVisivle; private set => this.RaiseAndSetIfChanged(ref _removeColumnVisivle, value); }

		public List<LibraryBook> GetVisibleBookEntries()
			=> GridEntries.InternalList
			.BookEntries()
			.Select(lbe => lbe.LibraryBook)
			.ToList();
		public IEnumerable<LibraryBookEntry2> GetAllBookEntries()
			=> GridEntries
			.AllItems()
			.BookEntries();
		public ProductsDisplayViewModel() { }
		public ProductsDisplayViewModel(List<GridEntry2> items)
		{
			GridEntries = new GridEntryBindingList2(items);
		}

		#region Display Functions

		/// <summary>
		/// Call once on load so we can modify access a private member with reflection
		/// </summary>
		public void RegisterCollectionChanged(ProductsDisplay2 productsDisplay = null)
		{
			productsDataGrid ??= productsDisplay?.productsGrid;

			if (GridEntries is null)
				return;

			//Avalonia displays items in the DataConncetion from an internal copy of
			//the bound list, not the actual bound list. So we need to reflect to get
			//the current display order and set each GridEntry.ListIndex correctly.
			var DataConnection_PI = typeof(DataGrid).GetProperty("DataConnection", BindingFlags.NonPublic | BindingFlags.Instance);
			var DataSource_PI = DataConnection_PI.PropertyType.GetProperty("DataSource", BindingFlags.Public | BindingFlags.Instance);

			GridEntries.CollectionChanged += (s, e) =>
			{
				if (s != GridEntries) return;

				var displayListGE = ((IEnumerable)DataSource_PI.GetValue(DataConnection_PI.GetValue(productsDataGrid))).Cast<GridEntry2>();
				int index = 0;
				foreach (var di in displayListGE)
				{
					di.ListIndex = index++;
				}
			};
		}

		/// <summary>
		/// Only call once per lifetime
		/// </summary>
		public void InitialDisplay(List<LibraryBook> dbBooks)
		{
			try
			{
				GridEntries = new GridEntryBindingList2(CreateGridEntries(dbBooks));
				GridEntries.CollapseAll();

				int bookEntryCount = GridEntries.BookEntries().Count();

				InitialLoaded?.Invoke(this, EventArgs.Empty);
				VisibleCountChanged?.Invoke(this, bookEntryCount);

				RegisterCollectionChanged();
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error displaying library in {0}", nameof(ProductsDisplayViewModel));
			}
		}

		/// <summary>
		/// Call when there's been a change to the library
		/// </summary>
		public async Task DisplayBooks(List<LibraryBook> dbBooks)
		{
			try
			{
				//List is already displayed. Replace all items with new ones, refilter, and re-sort
				string existingFilter = GridEntries?.Filter;
				var newEntries = CreateGridEntries(dbBooks);

				var existingSeriesEntries = GridEntries.AllItems().SeriesEntries().ToList();

				await Dispatcher.UIThread.InvokeAsync(() => GridEntries.ReplaceList(newEntries));

				//We're replacing the list, so preserve usere's existing collapse/expand
				//state. When resetting a list, default state is open.
				foreach (var series in existingSeriesEntries)
				{
					var sEntry = GridEntries.InternalList.FirstOrDefault(ge => ge.AudibleProductId == series.AudibleProductId);
					if (sEntry is SeriesEntrys2 se && !series.Liberate.Expanded)
						await Dispatcher.UIThread.InvokeAsync(() => GridEntries.CollapseItem(se));
				}
				await Dispatcher.UIThread.InvokeAsync(() =>
				{
					GridEntries.Filter = existingFilter;
					ReSort();
				});
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error displaying library in {0}", nameof(ProductsDisplayViewModel));
			}
		}

		private static IEnumerable<GridEntry2> CreateGridEntries(IEnumerable<LibraryBook> dbBooks)
		{
			var geList = dbBooks
				.Where(lb => lb.Book.IsProduct())
				.Select(b => new LibraryBookEntry2(b))
				.Cast<GridEntry2>()
				.ToList();

			var episodes = dbBooks.Where(lb => lb.Book.IsEpisodeChild());

			var seriesBooks = dbBooks.Where(lb => lb.Book.IsEpisodeParent()).ToList();

			foreach (var parent in seriesBooks)
			{
				var seriesEpisodes = episodes.FindChildren(parent);

				if (!seriesEpisodes.Any()) continue;

				var seriesEntry = new SeriesEntrys2(parent, seriesEpisodes);

				geList.Add(seriesEntry);
				geList.AddRange(seriesEntry.Children);
			}
			return geList.OrderByDescending(e => e.DateAdded);
		}

		public void ToggleSeriesExpanded(SeriesEntrys2 seriesEntry)
		{
			if (seriesEntry.Liberate.Expanded)
				GridEntries.CollapseItem(seriesEntry);
			else
				GridEntries.ExpandItem(seriesEntry);

			VisibleCountChanged?.Invoke(this, GridEntries.BookEntries().Count());
		}

		#endregion

		#region Filtering
		public async Task Filter(string searchString)
		{
			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				int visibleCount = GridEntries.Count;

				if (string.IsNullOrEmpty(searchString))
					GridEntries.RemoveFilter();
				else
					GridEntries.Filter = searchString;

				if (visibleCount != GridEntries.Count)
					VisibleCountChanged?.Invoke(this, GridEntries.BookEntries().Count());

				//Re-sort after filtering
				ReSort();
			});
		}

		#endregion

		#region Sorting

		public void Sort(DataGridColumn sortColumn)
		{
			//Force the comparer to get the current sort order. We can't
			//retrieve it from inside this event handler because Avalonia
			//doesn't set the property until after this event.
			var comparer = sortColumn.CustomSortComparer as RowComparer;
			comparer.SortDirection = null;

			_currentSortColumn = sortColumn;
		}

		//Must be invoked on UI thread
		private void ReSort()
		{
			if (_currentSortColumn is null)
			{
				//Sort ascending and reverse. That's how the comparer is designed to work to be compatible with Avalonia.
				var defaultComparer = new RowComparer(ListSortDirection.Descending, nameof(GridEntry2.DateAdded));
				GridEntries.InternalList.Sort(defaultComparer);
				GridEntries.InternalList.Reverse();
				GridEntries.ResetCollection();
			}
			else
			{
				_currentSortColumn.Sort(((RowComparer)_currentSortColumn.CustomSortComparer).SortDirection ?? ListSortDirection.Ascending);
			}
		}

		#endregion

		#region Scan and Remove Books

		public void DoneRemovingBooks()
		{
			foreach (var item in GridEntries.AllItems())
				item.PropertyChanged -= Item_PropertyChanged;
			RemoveColumnVisivle = false;
		}

		public async Task RemoveCheckedBooksAsync()
		{
			var selectedBooks = GetAllBookEntries().Where(lbe => lbe.Remove == true).ToList();

			if (selectedBooks.Count == 0)
				return;

			var libraryBooks = selectedBooks.Select(rge => rge.LibraryBook).ToList();
			var result = await MessageBox.ShowConfirmationDialog(
				null,
				libraryBooks,
				$"Are you sure you want to remove {selectedBooks.Count} books from Libation's library?",
				"Remove books from Libation?");

			if (result != DialogResult.Yes)
				return;

			foreach (var book in selectedBooks)
				book.PropertyChanged -= Item_PropertyChanged;

			var idsToRemove = libraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();
			GridEntries.CollectionChanged += BindingList_CollectionChanged;

			//The RemoveBooksAsync will fire LibrarySizeChanged, which calls ProductsDisplay2.Display(),
			//so there's no need to remove books from the grid display here.
			var removeLibraryBooks = await LibraryCommands.RemoveBooksAsync(idsToRemove);

			foreach (var b in GetAllBookEntries())
				b.Remove = false;

			RemovableCountChanged?.Invoke(this, 0);
		}

		void BindingList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				return;

			//After ProductsDisplay2.Display() re-creates the list,
			//re-subscribe to all items' PropertyChanged events.

			foreach (var b in GetAllBookEntries())
				b.PropertyChanged += Item_PropertyChanged;

			GridEntries.CollectionChanged -= BindingList_CollectionChanged;
		}

		public async Task ScanAndRemoveBooksAsync(params Account[] accounts)
		{
			foreach (var item in GridEntries.AllItems())
			{
				item.Remove = false;
				item.PropertyChanged += Item_PropertyChanged;
			}

			RemoveColumnVisivle = true;
			RemovableCountChanged?.Invoke(this, 0);

			try
			{
				if (accounts is null || accounts.Length == 0)
					return;

				var allBooks = GetAllBookEntries();

				foreach (var b in allBooks)
					b.Remove = false;

				var lib = allBooks
					.Select(lbe => lbe.LibraryBook)
					.Where(lb => !lb.Book.HasLiberated());

				var removedBooks = await LibraryCommands.FindInactiveBooks(Login.WinformLoginChoiceEager.ApiExtendedFunc, lib, accounts);

				var removable = allBooks.Where(lbe => removedBooks.Any(rb => rb.Book.AudibleProductId == lbe.AudibleProductId)).ToList();

				foreach (var r in removable)
					r.Remove = true;
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(
					null,
					"Error scanning library. You may still manually select books to remove from Libation's library.",
					"Error scanning library",
					ex);
			}
		}

		private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(GridEntry2.Remove) && sender is LibraryBookEntry2 lbEntry)
			{
				int removeCount = GetAllBookEntries().Count(lbe => lbe.Remove is true);
				RemovableCountChanged?.Invoke(this, removeCount);
			}
		}

		#endregion
	}
}
