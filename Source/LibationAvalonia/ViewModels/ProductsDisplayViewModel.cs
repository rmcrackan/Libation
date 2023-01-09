using DataLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Avalonia.Threading;
using ApplicationServices;
using AudibleUtilities;
using LibationAvalonia.Dialogs.Login;
using Avalonia.Collections;
using LibationSearchEngine;
using Octokit.Internal;

namespace LibationAvalonia.ViewModels
{
	public class ProductsDisplayViewModel : ViewModelBase
	{
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;
		public event EventHandler<int> RemovableCountChanged;

		/// <summary>Backing list of all grid entries</summary>
		private readonly List<GridEntry> SOURCE = new();
		/// <summary>Grid entries included in the filter set. If null, all grid entries are shown</summary>
		private List<GridEntry> FilteredInGridEntries;
		public string FilterString { get; private set; }
		public DataGridCollectionView GridEntries { get; }

		private bool _removeColumnVisivle;
		public bool RemoveColumnVisivle { get => _removeColumnVisivle; private set => this.RaiseAndSetIfChanged(ref _removeColumnVisivle, value); }

		public List<LibraryBook> GetVisibleBookEntries()
			=> GridEntries
			.OfType<LibraryBookEntry>()
			.Select(lbe => lbe.LibraryBook)
			.ToList();

		private IEnumerable<LibraryBookEntry> GetAllBookEntries()
			=> SOURCE
			.BookEntries();

		public ProductsDisplayViewModel()
		{
			SearchEngineCommands.SearchEngineUpdated += SearchEngineCommands_SearchEngineUpdated;
			GridEntries = new(SOURCE);
			GridEntries.Filter = CollectionFilter;

			GridEntries.CollectionChanged += (s, e)
				=> VisibleCountChanged?.Invoke(this, GridEntries.OfType<LibraryBookEntry>().Count());
		}

		#region Display Functions

		/// <summary>
		/// Call when there's been a change to the library
		/// </summary>
		public async Task DisplayBooksAsync(List<LibraryBook> dbBooks)
		{
			try
			{
				var existingSeriesEntries = SOURCE.SeriesEntries().ToList();

				SOURCE.Clear();
				SOURCE.AddRange(CreateGridEntries(dbBooks));

				//If replacing the list, preserve user's existing collapse/expand
				//state. When resetting a list, default state is cosed.
				foreach (var series in existingSeriesEntries)
				{
					var sEntry = SOURCE.FirstOrDefault(ge => ge.AudibleProductId == series.AudibleProductId);
					if (sEntry is SeriesEntry se)
						se.Liberate.Expanded = series.Liberate.Expanded;
				}

				//Run query on new list
				FilteredInGridEntries = QueryResults(SOURCE, FilterString);

				await Dispatcher.UIThread.InvokeAsync(GridEntries.Refresh);

			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error displaying library in {0}", nameof(ProductsDisplayViewModel));
			}
		}

		private static List<GridEntry> CreateGridEntries(IEnumerable<LibraryBook> dbBooks)
		{
			var geList = dbBooks
				.Where(lb => lb.Book.IsProduct())
				.Select(b => new LibraryBookEntry(b))
				.Cast<GridEntry>()
				.ToList();

			var episodes = dbBooks.Where(lb => lb.Book.IsEpisodeChild());

			foreach (var parent in dbBooks.Where(lb => lb.Book.IsEpisodeParent()))
			{
				var seriesEpisodes = episodes.FindChildren(parent);

				if (!seriesEpisodes.Any()) continue;

				var seriesEntry = new SeriesEntry(parent, seriesEpisodes);

				geList.Add(seriesEntry);
				geList.AddRange(seriesEntry.Children);
			}

			var bookList =  geList.OrderByDescending(e => e.DateAdded).ToList();

			//ListIndex is used by RowComparer to make column sort stable
			int index = 0;
			foreach (GridEntry di in bookList)
				di.ListIndex = index++;

			return bookList;
		}

		public void ToggleSeriesExpanded(SeriesEntry seriesEntry)
		{
			seriesEntry.Liberate.Expanded = !seriesEntry.Liberate.Expanded;
			GridEntries.Refresh();
		}

		#endregion

		#region Filtering

		public async Task Filter(string searchString)
		{
			if (searchString == FilterString)
				return;

			FilterString = searchString;

			if (SOURCE.Count == 0)
				return;

			FilteredInGridEntries = QueryResults(SOURCE, searchString);

			await Dispatcher.UIThread.InvokeAsync(GridEntries.Refresh);
		}

		private bool CollectionFilter(object item)
		{
			if (item is LibraryBookEntry lbe
				&& lbe.IsEpisode
				&& lbe.Parent?.Liberate?.Expanded != true)
				return false;

			if (FilteredInGridEntries is null) return true;

			return FilteredInGridEntries.Contains(item);
		}

		private static List<GridEntry> QueryResults(List<GridEntry> entries, string searchString)
		{
			if (string.IsNullOrEmpty(searchString)) return null;

			var searchResultSet = SearchEngineCommands.Search(searchString);

			var booksFilteredIn = entries.BookEntries().Join(searchResultSet.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => (GridEntry)lbe);

			//Find all series containing children that match the search criteria
			var seriesFilteredIn = entries.SeriesEntries().Where(s => s.Children.Join(searchResultSet.Docs, lbe => lbe.AudibleProductId, d => d.ProductId, (lbe, d) => lbe).Any());

			return booksFilteredIn.Concat(seriesFilteredIn).ToList();
		}

		private async void SearchEngineCommands_SearchEngineUpdated(object sender, EventArgs e)
		{
			var filterResults = QueryResults(SOURCE, FilterString);

			if (filterResults is not null && FilteredInGridEntries.Intersect(filterResults).Count() != FilteredInGridEntries.Count)
			{
				FilteredInGridEntries = filterResults;

				if (GridEntries.IsEditingItem)
					await Dispatcher.UIThread.InvokeAsync(GridEntries.CommitEdit);

				await Dispatcher.UIThread.InvokeAsync(GridEntries.Refresh);
			}
		}

		#endregion

		#region Scan and Remove Books

		public void DoneRemovingBooks()
		{
			foreach (var item in SOURCE)
				item.PropertyChanged -= GridEntry_PropertyChanged;
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
				// do not use `$` string interpolation. See impl.
				"Are you sure you want to remove {0} from Libation's library?",
				"Remove books from Libation?");

			if (result != DialogResult.Yes)
				return;

			foreach (var book in selectedBooks)
				book.PropertyChanged -= GridEntry_PropertyChanged;

			var idsToRemove = libraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();

			void BindingList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
					return;

				//After DisplayBooks() re-creates the list,
				//re-subscribe to all items' PropertyChanged events.

				foreach (var b in GetAllBookEntries())
					b.PropertyChanged += GridEntry_PropertyChanged;

				GridEntries.CollectionChanged -= BindingList_CollectionChanged;
			}

			GridEntries.CollectionChanged += BindingList_CollectionChanged;

			//The RemoveBooksAsync will fire LibrarySizeChanged, which calls ProductsDisplay2.Display(),
			//so there's no need to remove books from the grid display here.
			var removeLibraryBooks = await LibraryCommands.RemoveBooksAsync(idsToRemove);

			RemovableCountChanged?.Invoke(this, 0);
		}

		public async Task ScanAndRemoveBooksAsync(params Account[] accounts)
		{
			foreach (var item in SOURCE)
			{
				item.Remove = false;
				item.PropertyChanged += GridEntry_PropertyChanged;
			}

			RemoveColumnVisivle = true;
			RemovableCountChanged?.Invoke(this, 0);

			try
			{
				if (accounts is null || accounts.Length == 0)
					return;

				var allBooks = GetAllBookEntries();

				var lib = allBooks
					.Select(lbe => lbe.LibraryBook)
					.Where(lb => !lb.Book.HasLiberated());

				var removedBooks = await LibraryCommands.FindInactiveBooks(AvaloniaLoginChoiceEager.ApiExtendedFunc, lib, accounts);

				var removable = allBooks.Where(lbe => removedBooks.Any(rb => rb.Book.AudibleProductId == lbe.AudibleProductId)).ToList();

				foreach (var r in removable)
					r.Remove = true;
			}
			catch (Exception ex)
			{
				await MessageBox.ShowAdminAlert(
					null,
					"Error scanning library. You may still manually select books to remove from Libation's library.",
					"Error scanning library",
					ex);
			}
		}

		private void GridEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(GridEntry.Remove) && sender is LibraryBookEntry lbEntry)
			{
				int removeCount = GetAllBookEntries().Count(lbe => lbe.Remove is true);
				RemovableCountChanged?.Invoke(this, removeCount);
			}
		}

		#endregion
	}
}
