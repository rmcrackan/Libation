using ApplicationServices;
using AudibleUtilities;
using Avalonia.Collections;
using Avalonia.Threading;
using DataLayer;
using LibationAvalonia.Dialogs.Login;
using LibationUiBase.GridView;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels
{
	public class ProductsDisplayViewModel : ViewModelBase
	{
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;
		public event EventHandler<int> RemovableCountChanged;

		/// <summary>Backing list of all grid entries</summary>
		private readonly AvaloniaList<IGridEntry> SOURCE = new();
		/// <summary>Grid entries included in the filter set. If null, all grid entries are shown</summary>
		private HashSet<IGridEntry> FilteredInGridEntries;
		public string FilterString { get; private set; }
		public DataGridCollectionView GridEntries { get; private set; }

		private bool _removeColumnVisivle;
		public bool RemoveColumnVisivle { get => _removeColumnVisivle; private set => this.RaiseAndSetIfChanged(ref _removeColumnVisivle, value); }

		public List<LibraryBook> GetVisibleBookEntries()
			=> FilteredInGridEntries?
				.OfType<ILibraryBookEntry>()
				.Select(lbe => lbe.LibraryBook)
				.ToList()
			?? SOURCE
				.OfType<ILibraryBookEntry>()
				.Select(lbe => lbe.LibraryBook)
				.ToList();

		private IEnumerable<ILibraryBookEntry> GetAllBookEntries()
			=> SOURCE
			.BookEntries();

		public ProductsDisplayViewModel()
		{
			SearchEngineCommands.SearchEngineUpdated += SearchEngineCommands_SearchEngineUpdated;
			VisibleCountChanged?.Invoke(this, 0);
		}

		private static readonly System.Reflection.MethodInfo SetFlagsMethod;

		/// <summary>
		/// Tells the <see cref="DataGridCollectionView"/> whether it should process changes to the underlying collection
		/// </summary>
		/// <remarks> DataGridCollectionView.CollectionViewFlags.ShouldProcessCollectionChanged = 4</remarks>
		private void SetShouldProcessCollectionChanged(bool flagSet)
			=> SetFlagsMethod.Invoke(GridEntries, new object[] { 4, flagSet });

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

		internal void BindToGrid(List<LibraryBook> dbBooks)
		{
			GridEntries = new(SOURCE) { Filter = CollectionFilter };

			var geList = dbBooks
				.Where(lb => lb.Book.IsProduct())
				.Select(b => new LibraryBookEntry<AvaloniaEntryStatus>(b))
				.ToList<IGridEntry>();

			var episodes = dbBooks.Where(lb => lb.Book.IsEpisodeChild()).ToList();

			var seriesBooks = dbBooks.Where(lb => lb.Book.IsEpisodeParent()).ToList();

			foreach (var parent in seriesBooks)
			{
				var seriesEpisodes = episodes.FindChildren(parent);

				if (!seriesEpisodes.Any()) continue;

				var seriesEntry = new SeriesEntry<AvaloniaEntryStatus>(parent, seriesEpisodes);
				seriesEntry.Liberate.Expanded = false;

				geList.Add(seriesEntry);
			}

			//Create the filtered-in list before adding entries to avoid a refresh
			FilteredInGridEntries = geList.Union(geList.OfType<ISeriesEntry>().SelectMany(s => s.Children)).FilterEntries(FilterString);
			SOURCE.AddRange(geList.OrderByDescending(e => e.DateAdded));

			//Add all children beneath their parent
			foreach (var series in SOURCE.OfType<ISeriesEntry>().ToList())
			{
				var seriesIndex = SOURCE.IndexOf(series);
				foreach (var child in series.Children)
					SOURCE.Insert(++seriesIndex, child);
			}

			GridEntries.CollectionChanged += GridEntries_CollectionChanged;
			GridEntries_CollectionChanged();
		}

		private void GridEntries_CollectionChanged(object sender = null, EventArgs e = null)
		{
			var count
				= FilteredInGridEntries?.OfType<ILibraryBookEntry>().Count()
				?? SOURCE.OfType<ILibraryBookEntry>().Count();

			VisibleCountChanged?.Invoke(this, count);
		}

		/// <summary>
		/// Call when there's been a change to the library
		/// </summary>
		internal async Task UpdateGridAsync(List<LibraryBook> dbBooks)
		{
			GridEntries.CollectionChanged -= GridEntries_CollectionChanged;

			#region Add new or update existing grid entries

			//Add absent entries to grid, or update existing entry
			var allEntries = SOURCE.BookEntries().ToList();
			var seriesEntries = SOURCE.SeriesEntries().ToList();
			var parentedEpisodes = dbBooks.ParentedEpisodes().ToHashSet();

			await Dispatcher.UIThread.InvokeAsync(() =>
			{
				foreach (var libraryBook in dbBooks.OrderBy(e => e.DateAdded))
				{
					var existingEntry = allEntries.FindByAsin(libraryBook.Book.AudibleProductId);

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
			foreach (var removed in removedBooks.Where(b => b.Liberate.IsEpisode))
				removed.Parent.RemoveChild(removed);

			//Remove series that have no children
			var removedSeries = sourceSnapshot.EmptySeries();

			await Dispatcher.UIThread.InvokeAsync(() => RemoveBooks(removedBooks, removedSeries));

			#endregion

			await Filter(FilterString);
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);

			GridEntries.CollectionChanged += GridEntries_CollectionChanged;
			GridEntries_CollectionChanged();
		}

		private void RemoveBooks(IEnumerable<ILibraryBookEntry> removedBooks, IEnumerable<ISeriesEntry> removedSeries)
		{
			foreach (var removed in removedBooks.Cast<IGridEntry>().Concat(removedSeries).Where(b => b is not null).ToList())
			{
				if (GridEntries.PassesFilter(removed))
					GridEntries.Remove(removed);
				else
				{
					SetShouldProcessCollectionChanged(false);
					SOURCE.Remove(removed);
					SetShouldProcessCollectionChanged(true);
				}
			}
		}

		private void UpsertBook(LibraryBook book, ILibraryBookEntry existingBookEntry)
		{
			if (existingBookEntry is null)
				// Add the new product to top
				SOURCE.Insert(0, new LibraryBookEntry<AvaloniaEntryStatus>(book));
			else
				// update existing
				existingBookEntry.UpdateLibraryBook(book);
		}

		private void UpsertEpisode(LibraryBook episodeBook, ILibraryBookEntry existingEpisodeEntry, List<ISeriesEntry> seriesEntries, IEnumerable<LibraryBook> dbBooks)
		{
			if (existingEpisodeEntry is null)
			{
				ILibraryBookEntry episodeEntry;

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

					seriesEntry = new SeriesEntry<AvaloniaEntryStatus>(seriesBook, episodeBook);
					seriesEntries.Add(seriesEntry);

					episodeEntry = seriesEntry.Children[0];
					seriesEntry.Liberate.Expanded = true;
					SOURCE.Insert(0, seriesEntry);
				}
				else
				{
					//Series exists. Create and add episode child then update the SeriesEntry
					episodeEntry = new LibraryBookEntry<AvaloniaEntryStatus>(episodeBook, seriesEntry);
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
			if (GridEntries.IsEditingItem)
				await Dispatcher.UIThread.InvokeAsync(GridEntries.CommitEdit);

			await Dispatcher.UIThread.InvokeAsync(GridEntries.Refresh);
		}

		public async Task ToggleSeriesExpanded(ISeriesEntry seriesEntry)
		{
			seriesEntry.Liberate.Expanded = !seriesEntry.Liberate.Expanded;

			await refreshGrid();
		}

		#endregion

		#region Filtering

		public async Task Filter(string searchString)
		{
			FilterString = searchString;

			if (SOURCE.Count == 0)
				return;

			FilteredInGridEntries = SOURCE.FilterEntries(searchString);

			await refreshGrid();
		}

		private bool CollectionFilter(object item)
		{
			if (item is ILibraryBookEntry lbe
				&& lbe.Liberate.IsEpisode
				&& lbe.Parent?.Liberate?.Expanded != true)
				return false;

			if (FilteredInGridEntries is null) return true;

			return FilteredInGridEntries.Contains(item);
		}

		private async void SearchEngineCommands_SearchEngineUpdated(object sender, EventArgs e)
		{
			var filterResults = SOURCE.FilterEntries(FilterString);

			if (filterResults is not null && FilteredInGridEntries.Intersect(filterResults).Count() != FilteredInGridEntries.Count)
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
			RemoveColumnVisivle = false;
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

		private void GridEntry_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(IGridEntry.Remove) && sender is ILibraryBookEntry)
			{
				int removeCount = GetAllBookEntries().Count(lbe => lbe.Remove is true);
				RemovableCountChanged?.Invoke(this, removeCount);
			}
		}

		#endregion
	}
}
