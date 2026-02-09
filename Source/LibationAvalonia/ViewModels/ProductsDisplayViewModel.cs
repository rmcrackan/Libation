using ApplicationServices;
using AudibleUtilities;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using DataLayer;
using Dinah.Core.Collections.Generic;
using LibationFileManager;
using LibationUiBase.Forms;
using LibationUiBase.GridView;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

public class ProductsDisplayViewModel : ViewModelBase
{
	/// <summary>Number of visible rows has changed</summary>
	public event EventHandler<int>? VisibleCountChanged;
	public event EventHandler<int>? RemovableCountChanged;

	/// <summary>Backing list of all grid entries</summary>
	private readonly AvaloniaList<GridEntry> SOURCE = new();
	/// <summary>Grid entries included in the filter set. If null, all grid entries are shown</summary>
	private HashSet<GridEntry>? FilteredInGridEntries;
	public string? FilterString { get; private set; }

	public DataGridCollectionView? GridEntries { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool RemoveColumnVisible { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }

	public List<LibraryBook> GetVisibleBookEntries()
		=> GetVisibleGridEntries().Select(lbe => lbe.LibraryBook).ToList();

	public IEnumerable<LibraryBookEntry> GetVisibleGridEntries()
		=> (FilteredInGridEntries as IEnumerable<GridEntry> ?? SOURCE).OfType<LibraryBookEntry>();

	private IEnumerable<LibraryBookEntry> GetAllBookEntries() => SOURCE.BookEntries();

	public ProductsDisplayViewModel()
	{
		SearchEngineCommands.SearchEngineUpdated += SearchEngineCommands_SearchEngineUpdated;
		VisibleCountChanged?.Invoke(this, 0);
	}

	public ISearchEngine? SearchEngine { get; set; }

	private static readonly System.Reflection.MethodInfo? SetFlagsMethod;

	/// <summary>
	/// Tells the <see cref="DataGridCollectionView"/> whether it should process changes to the underlying collection
	/// </summary>
	/// <remarks> DataGridCollectionView.CollectionViewFlags.ShouldProcessCollectionChanged = 4</remarks>
	private void SetShouldProcessCollectionChanged(bool flagSet)
		=> SetFlagsMethod?.Invoke(GridEntries, new object[] { 4, flagSet });

	static ProductsDisplayViewModel()
	{
		/*
		 * When a book is removed from the library, SearchEngineUpdated is fired before LibrarySizeChanged, so
		 * the book is removed from the filtered set and the grid is refreshed before RemoveBooks() is ever
		 * called.
		 * 
		 * To remove an item from DataGridCollectionView, it must be be in the current filtered view. If it's
		 * not and you try to remove the book from the source list, the source will fire NotifyCollectionChanged
		 * on an invalid item and the DataGridCollectionView will throw an exception. There are two ways to
		 * remove an item that is filtered out of the DataGridCollectionView:
		 * 
		 * (1)  Re-add the item to the filtered-in list and refresh the grid so DataGridCollectionView knows
		 *		that the item is present. This causes the whole grid to flicker to refresh twice in rapid
		 *		succession, which is undesirable.
		 *		
		 * (2)  Remove it from the underlying collection and suppress NotifyCollectionChanged. This is the
		 *		method used. Steps to complete a removal using this method:
		 *		
		 *		(a)	Set DataGridCollectionView.CollectionViewFlags.ShouldProcessCollectionChanged to false.
		 *		(b) Remove the item from the source list. The source will fire NotifyCollectionChanged, but the
		 *			DataGridCollectionView will ignore it.
		 *		(c) Reset the flag to true.
		 */

		SetFlagsMethod =
			typeof(DataGridCollectionView)
			.GetMethod("SetFlag", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
	}

	#region Display Functions

	internal async Task BindToGridAsync(List<LibraryBook> dbBooks)
	{
		ArgumentNullException.ThrowIfNull(dbBooks, nameof(dbBooks));

		//Get the UI thread's synchronization context and set it on the current thread to ensure
		//it's available for GetAllProductsAsync and GetAllSeriesEntriesAsync
		var sc = await Dispatcher.UIThread.InvokeAsync(() => AvaloniaSynchronizationContext.Current);
		AvaloniaSynchronizationContext.SetSynchronizationContext(sc);

		var geList = await LibraryBookEntry.GetAllProductsAsync(dbBooks);
		var seriesEntries = await SeriesEntry.GetAllSeriesEntriesAsync(dbBooks);

		//Add all IGridEntries to the SOURCE list. Note that SOURCE has not yet been linked to the UI via
		//the GridEntries property, so adding items to SOURCE will not trigger any refreshes or UI action.
		//This this can be done on any thread.
		SOURCE.AddRange(geList.Concat(seriesEntries).OrderDescending(new RowComparer(null)));

		//Add all children beneath their parent
		foreach (var series in seriesEntries)
		{
			var seriesIndex = SOURCE.IndexOf(series);
			foreach (var child in series.Children)
				SOURCE.Insert(++seriesIndex, child);
		}

		//Create the filtered-in list before adding entries to GridEntries to avoid a refresh or UI action
		var searchResultSet = SearchEngine?.GetSearchResultSet(FilterString);
		FilteredInGridEntries = geList.Union(seriesEntries.SelectMany(s => s.Children)).FilterEntries(searchResultSet);

		// Adding SOURCE to the DataGridViewCollection _after_ building the SOURCE list 
		//Saves ~500 ms on a library of ~4500 books.
		//Perform on UI thread for safety, but at this time, merely setting the DataGridCollectionView
		//does not trigger UI actions in the way that modifying the list after it's been linked does.
		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			GridEntries = new(SOURCE) { Filter = CollectionFilter };
			GridEntries.CollectionChanged += GridEntries_CollectionChanged;
		});

		GridEntries_CollectionChanged();
	}

	private void GridEntries_CollectionChanged(object? sender = null, EventArgs? e = null)
	{
		var count
			= FilteredInGridEntries?.OfType<LibraryBookEntry>().Count()
			?? SOURCE.OfType<LibraryBookEntry>().Count();

		VisibleCountChanged?.Invoke(this, count);
	}

	/// <summary>
	/// Call when there's been a change to the library
	/// </summary>
	internal async Task UpdateGridAsync(List<LibraryBook> dbBooks)
	{
		ArgumentNullException.ThrowIfNull(dbBooks, nameof(dbBooks));

		if (GridEntries == null)
			throw new InvalidOperationException($"Must call {nameof(BindToGridAsync)} before calling {nameof(UpdateGridAsync)}");

		//CollectionChanged fires for every book added, and the handler invokes
		//VisibleCountChanged which triggers Libation to re-count all books.
		GridEntries.CollectionChanged -= GridEntries_CollectionChanged;

		#region Add new or update existing grid entries

		//Add absent entries to grid, or update existing entry
		var allEntries = SOURCE.BookEntries().ToDictionarySafe(b => b.AudibleProductId);
		var seriesEntries = SOURCE.SeriesEntries().ToList();
		var parentedEpisodes = dbBooks.ParentedEpisodes().ToHashSet();

		await Dispatcher.UIThread.InvokeAsync(() =>
		{
			foreach (var libraryBook in dbBooks.OrderBy(e => e.DateAdded))
			{
				var existingEntry = allEntries.TryGetValue(libraryBook.Book.AudibleProductId, out var e) ? e : null;

				if (libraryBook.Book.IsProduct())
					UpsertBook(libraryBook, existingEntry);
				else if (parentedEpisodes.Contains(libraryBook))
					//Only try to add or update is this LibraryBook is a know child of a parent
					UpsertEpisode(libraryBook, existingEntry, seriesEntries, dbBooks);
			}
		});

		#endregion

		#region Remove entries no longer in the library

		//Rapid successive book removals will cause changes to SOURCE after the update has
		//begun but before it has completed, so perform all updates on a copy of the list.
		var sourceSnapshot = SOURCE.ToList();

		// remove deleted from grid.
		// note: actual deletion from db must still occur via the RemoveBook feature. deleting from audible will not trigger this
		var removedBooks =
			sourceSnapshot
			.BookEntries()
			.ExceptBy(dbBooks.Select(lb => lb.Book.AudibleProductId), ge => ge.AudibleProductId);

		//Remove books in series from their parents' Children list
		foreach (var removed in removedBooks.Where(b => b.Liberate?.IsEpisode is true))
			removed.Parent?.RemoveChild(removed);

		//Remove series that have no children
		var removedSeries = sourceSnapshot.EmptySeries();

		await Dispatcher.UIThread.InvokeAsync(() => RemoveBooks(removedBooks, removedSeries));

		#endregion

		await Filter(FilterString);
		GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

		//Resubscribe after all changes to the list have been made
		GridEntries.CollectionChanged += GridEntries_CollectionChanged;
		GridEntries_CollectionChanged();
	}

	private void RemoveBooks(IEnumerable<LibraryBookEntry> removedBooks, IEnumerable<SeriesEntry> removedSeries)
	{
		foreach (var removed in removedBooks.Cast<GridEntry>().Concat(removedSeries).Where(b => b is not null).ToList())
		{
			if (GridEntries?.PassesFilter(removed) ?? false)
				GridEntries.Remove(removed);
			else
			{
				SetShouldProcessCollectionChanged(false);
				SOURCE.Remove(removed);
				SetShouldProcessCollectionChanged(true);
			}
		}
	}

	private void UpsertBook(LibraryBook book, LibraryBookEntry? existingBookEntry)
	{
		if (existingBookEntry is null)
			// Add the new product to top
			SOURCE.Insert(0, new LibraryBookEntry(book));
		else
			// update existing
			existingBookEntry.UpdateLibraryBook(book);
	}

	private void UpsertEpisode(LibraryBook episodeBook, LibraryBookEntry? existingEpisodeEntry, List<SeriesEntry> seriesEntries, IEnumerable<LibraryBook> dbBooks)
	{
		if (existingEpisodeEntry is null)
		{
			LibraryBookEntry episodeEntry;

			var seriesEntry = seriesEntries.FindSeriesParent(episodeBook);

			if (seriesEntry is null)
			{
				//Series doesn't exist yet, so create and add it
				var seriesBook = dbBooks.FindSeriesParent(episodeBook);

				if (seriesBook is null)
				{
					//This is only possible if the user's db  has some malformed
					//entries from earlier Libation releases that could not be
					//automatically fixed. Log, but don't throw.
					Serilog.Log.Logger.Error("Episode={0}, Episode Series: {1}", episodeBook, episodeBook.Book.SeriesNames());
					return;
				}

				seriesEntry = new SeriesEntry(seriesBook, episodeBook);
				seriesEntries.Add(seriesEntry);

				episodeEntry = seriesEntry.Children[0];
				seriesEntry.Liberate?.Expanded = true;
				SOURCE.Insert(0, seriesEntry);
			}
			else
			{
				//Series exists. Create and add episode child then update the SeriesEntry
				episodeEntry = new LibraryBookEntry(episodeBook, seriesEntry);
				seriesEntry.Children.Add(episodeEntry);
				seriesEntry.Children.Sort((c1, c2) => c1.SeriesIndex.CompareTo(c2.SeriesIndex));
				var seriesBook = dbBooks.Single(lb => lb.Book.AudibleProductId == seriesEntry.LibraryBook.Book.AudibleProductId);
				seriesEntry.UpdateLibraryBook(seriesBook);
			}

			//Add episode to the grid beneath the parent
			int seriesIndex = SOURCE.IndexOf(seriesEntry);
			int episodeIndex = seriesEntry.Children.IndexOf(episodeEntry);
			SOURCE.Insert(seriesIndex + 1 + episodeIndex, episodeEntry);
		}
		else
			existingEpisodeEntry.UpdateLibraryBook(episodeBook);
	}

	private async Task refreshGrid()
	{
		if (GridEntries != null)
		{
			if (GridEntries.IsEditingItem)
				await Dispatcher.UIThread.InvokeAsync(GridEntries.CommitEdit);

			await Dispatcher.UIThread.InvokeAsync(GridEntries.Refresh);
		}
	}

	public async Task ToggleSeriesExpanded(SeriesEntry seriesEntry)
	{
		seriesEntry.Liberate?.Expanded = !seriesEntry.Liberate.Expanded;

		await refreshGrid();
	}

	#endregion

	#region Filtering

	public async Task Filter(string? searchString)
	{
		FilterString = searchString;

		if (SOURCE.Count == 0)
			return;

		var results = SearchEngine?.GetSearchResultSet(searchString);
		FilteredInGridEntries = SOURCE.FilterEntries(results);

		await refreshGrid();
	}

	private bool CollectionFilter(object item)
	{
		if (item is LibraryBookEntry lbe
			&& lbe.Liberate?.IsEpisode is true
			&& lbe.Parent?.Liberate?.Expanded != true)
			return false;

		if (FilteredInGridEntries is null) return true;

		return FilteredInGridEntries.Contains(item);
	}

	private async void SearchEngineCommands_SearchEngineUpdated(object? sender, EventArgs? e)
	{

		var searchResultSet = SearchEngine?.GetSearchResultSet(FilterString);
		var filterResults = SOURCE.FilterEntries(searchResultSet);

		if (FilteredInGridEntries.SearchSetsDiffer(filterResults))
		{
			FilteredInGridEntries = filterResults;
			await refreshGrid();
		}
	}

	#endregion

	#region Scan and Remove Books

	public void DoneRemovingBooks()
	{
		foreach (var item in SOURCE)
			item.PropertyChanged -= GridEntry_PropertyChanged;
		RemoveColumnVisible = false;
	}

	public async Task RemoveCheckedBooksAsync()
	{
		var selectedBooks = GetAllBookEntries().Where(lbe => lbe.Remove == true).ToList();

		if (selectedBooks.Count == 0)
			return;

		var booksToRemove = selectedBooks.Select(rge => rge.LibraryBook).ToList();
		var result = await MessageBox.ShowConfirmationDialog(
			null,
			booksToRemove,
			// do not use `$` string interpolation. See impl.
			"Are you sure you want to remove {0} from Libation's library?",
			"Remove books from Libation?");

		if (result != DialogResult.Yes)
			return;

		foreach (var book in selectedBooks)
			book.PropertyChanged -= GridEntry_PropertyChanged;

		void BindingList_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs? e)
		{
			if (e?.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				return;

			//After DisplayBooks() re-creates the list,
			//re-subscribe to all items' PropertyChanged events.

			foreach (var b in GetAllBookEntries())
				b.PropertyChanged += GridEntry_PropertyChanged;

			GridEntries.CollectionChanged -= BindingList_CollectionChanged;
		}

		if (GridEntries != null)
			GridEntries.CollectionChanged += BindingList_CollectionChanged;

		//The RemoveBooksAsync will fire LibrarySizeChanged, which calls ProductsDisplay2.Display(),
		//so there's no need to remove books from the grid display here.
		await booksToRemove.RemoveBooksAsync();

		RemovableCountChanged?.Invoke(this, 0);
	}

	public async Task ScanAndRemoveBooksAsync(params Account[] accounts)
	{
		foreach (var item in SOURCE)
		{
			item.Remove = false;
			item.PropertyChanged += GridEntry_PropertyChanged;
		}

		RemoveColumnVisible = true;
		RemovableCountChanged?.Invoke(this, 0);

		try
		{
			if (accounts is null || accounts.Length == 0)
				return;

			var allBooks = GetAllBookEntries();

			var lib = allBooks
				.Select(lbe => lbe.LibraryBook)
				.Where(lb => !lb.Book.HasLiberated());

			var removedBooks = await LibraryCommands.FindInactiveBooks(lib, accounts);

			var removable = allBooks.Where(lbe => removedBooks.Any(rb => rb.Book.AudibleProductId == lbe.AudibleProductId)).ToList();

			foreach (var r in removable)
				r.Remove = true;
		}
		catch (OperationCanceledException)
		{
			Serilog.Log.Information("Audible login attempt cancelled by user");
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

	private void GridEntry_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
	{
		if (e?.PropertyName == nameof(GridEntry.Remove) && sender is LibraryBookEntry)
		{
			int removeCount = GetAllBookEntries().Count(lbe => lbe.Remove is true);
			RemovableCountChanged?.Invoke(this, removeCount);
		}
	}

	#endregion

	#region Column Widths
	public bool DisablePersistColumnWidths { get; set; }
	public DataGridLength TitleWidth { get => getColumnWidth("Title", 200); set => setColumnWidth("Title", value); }
	public DataGridLength AuthorsWidth { get => getColumnWidth("Authors", 100); set => setColumnWidth("Authors", value); }
	public DataGridLength NarratorsWidth { get => getColumnWidth("Narrators", 100); set => setColumnWidth("Narrators", value); }
	public DataGridLength LengthWidth { get => getColumnWidth("Length", 80); set => setColumnWidth("Length", value); }
	public DataGridLength SeriesWidth { get => getColumnWidth("Series", 100); set => setColumnWidth("Series", value); }
	public DataGridLength SeriesOrderWidth { get => getColumnWidth("SeriesOrder", 60); set => setColumnWidth("SeriesOrder", value); }
	public DataGridLength DescriptionWidth { get => getColumnWidth("Description", 100); set => setColumnWidth("Description", value); }
	public DataGridLength CategoryWidth { get => getColumnWidth("Category", 100); set => setColumnWidth("Category", value); }
	public DataGridLength ProductRatingWidth { get => getColumnWidth("ProductRating", 95); set => setColumnWidth("ProductRating", value); }
	public DataGridLength PurchaseDateWidth { get => getColumnWidth("PurchaseDate", 75); set => setColumnWidth("PurchaseDate", value); }
	public DataGridLength MyRatingWidth { get => getColumnWidth("MyRating", 95); set => setColumnWidth("MyRating", value); }
	public DataGridLength MiscWidth { get => getColumnWidth("Misc", 140); set => setColumnWidth("Misc", value); }
	public DataGridLength IncludedUntilWidth { get => getColumnWidth("IncludedUntil", 140); set => setColumnWidth("IncludedUntil", value); }
	public DataGridLength LastDownloadWidth { get => getColumnWidth("LastDownload", 100); set => setColumnWidth("LastDownload", value); }
	public DataGridLength BookTagsWidth { get => getColumnWidth("BookTags", 100); set => setColumnWidth("BookTags", value); }
	public DataGridLength IsSpatialWidth { get => getColumnWidth("IsSpatial", 100); set => setColumnWidth("IsSpatial", value); }
	public DataGridLength AccountWidth { get => getColumnWidth(nameof(GridEntry.Account), 100); set => setColumnWidth(nameof(GridEntry.Account), value); }

	private static DataGridLength getColumnWidth(string columnName, double defaultWidth)
		=> Configuration.Instance.GridColumnsWidths.TryGetValue(columnName, out var val)
		? new DataGridLength(val)
		: new DataGridLength(defaultWidth);

	private void setColumnWidth(string columnName, DataGridLength width, [CallerMemberName] string propertyName = "")
	{
		if (DisablePersistColumnWidths) return;
		var dictionary = Configuration.Instance.GridColumnsWidths;

		var newValue = (int)width.DisplayValue;
		var valueSame = dictionary.TryGetValue(columnName, out var val) && val == newValue;
		dictionary[columnName] = newValue;

		if (!valueSame)
		{
			Configuration.Instance.GridColumnsWidths = dictionary;
			this.RaisePropertyChanged(propertyName);
		}
	}

	#endregion
}
