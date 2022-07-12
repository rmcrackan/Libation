using ApplicationServices;
using AudibleUtilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DataLayer;
using Dinah.Core.DataBinding;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class ProductsDisplay2 : UserControl
	{
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;
		public event EventHandler<int> RemovableCountChanged;
		public event EventHandler<LibraryBook> LiberateClicked;
		public event EventHandler InitialLoaded;

		private ProductsDisplayViewModel _viewModel;
		private GridEntryBindingList2 bindingList => _viewModel.GridEntries;
		private IEnumerable<LibraryBookEntry2> GetAllBookEntries()
			=> bindingList.AllItems().BookEntries();

		internal List<LibraryBook> GetVisible()
			=> bindingList
			.BookEntries()
			.Select(lbe => lbe.LibraryBook)
			.ToList();

		DataGridColumn removeGVColumn;
		public ProductsDisplay2()
		{
			InitializeComponent();

			productsGrid = this.FindControl<DataGrid>(nameof(productsGrid));
			productsGrid.Sorting += Dg1_Sorting;
			productsGrid.CanUserSortColumns = true;

			removeGVColumn = productsGrid.Columns[0];

			if (Design.IsDesignMode)
			{
				using var context = DbContexts.GetContext();
				var book = context.GetLibraryBook_Flat_NoTracking("B017V4IM1G");
				productsGrid.DataContext = _viewModel = new ProductsDisplayViewModel(new List<LibraryBook> { book });
			}
		}
		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private class RowComparer : IComparer
		{
			private static readonly System.Reflection.PropertyInfo HeaderCellPi = typeof(DataGridColumn).GetProperty("HeaderCell", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			private static readonly System.Reflection.PropertyInfo CurrentSortingStatePi = typeof(DataGridColumnHeader).GetProperty("CurrentSortingState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			public DataGridColumn Column { get; init; }
			public string PropertyName { get; init; }
			public ListSortDirection? SortDirection { get; set; }

			/// <summary>
			/// This compare method ensures that all top-level grid entries (standalone books or series parents)
			/// are sorted by PropertyName while all episodes remain immediately beneath their parents and remain
			/// sorted by series index, ascending.
			/// </summary>
			public int Compare(object x, object y)
			{
				if (x is null) return -1;
				if (y is null) return 1;
				if (x is null && y is null) return 0;

				var geA = (GridEntry2)x;
				var geB = (GridEntry2)y;

				SortDirection ??= GetSortOrder(Column);

				SeriesEntrys2 parentA = null;
				SeriesEntrys2 parentB = null;

				if (geA is LibraryBookEntry2 lbA && lbA.Parent is SeriesEntrys2 seA)
					parentA = seA;
				if (geB is LibraryBookEntry2 lbB && lbB.Parent is SeriesEntrys2 seB)
					parentB = seB;

				//both a and b are standalone
				if (parentA is null && parentB is null)
					return Compare(geA, geB);

				//a is a standalone, b is a child
				if (parentA is null && parentB is not null)
				{
					// b is a child of a, parent is always first
					if (parentB == geA)
						return SortDirection is ListSortDirection.Ascending ? -1 : 1;
					else
						return Compare(geA, parentB);
				}

				//a is a child, b is a standalone
				if (parentA is not null && parentB is null)
				{
					// a is a child of b, parent is always first
					if (parentA == geB)
						return SortDirection is ListSortDirection.Ascending ? 1 : -1;
					else
						return Compare(parentA, geB);
				}

				//both are children of the same series, always present in order of series index, ascending
				if (parentA == parentB)
					return geA.SeriesIndex.CompareTo(geB.SeriesIndex) * (SortDirection is ListSortDirection.Ascending ? 1 : -1);

				//a and b are children of different series.
				return Compare(parentA, parentB);
			}

			private static ListSortDirection? GetSortOrder(DataGridColumn column)
				=> CurrentSortingStatePi.GetValue(HeaderCellPi.GetValue(column)) as ListSortDirection?;

			private int Compare(GridEntry2 x, GridEntry2 y)
			{
				var val1 = x.GetMemberValue(PropertyName);
				var val2 = y.GetMemberValue(PropertyName);

				return x.GetMemberComparer(val1.GetType()).Compare(val1, val2);
			}
		}

		Dictionary<DataGridColumn, RowComparer> ColumnComparers = new();
		DataGridColumn CurrentSortColumn;

		private void Dg1_Sorting(object sender, DataGridColumnEventArgs e)
		{
			if (!ColumnComparers.ContainsKey(e.Column))
				ColumnComparers[e.Column] = new RowComparer
				{
					Column = e.Column,
					PropertyName = e.Column.SortMemberPath
				};

			//Force the comparer to get the current sort order. We can't
			//retrieve it from inside this event handler because Avalonia
			//doesn't set the property until after this event.
			ColumnComparers[e.Column].SortDirection = null;

			e.Column.CustomSortComparer = ColumnComparers[e.Column];
			CurrentSortColumn = e.Column;
		}

		#region Button controls

		public void LiberateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button.DataContext is SeriesEntrys2 sEntry)
			{
				if (sEntry.Liberate.Expanded)
					bindingList.CollapseItem(sEntry);
				else
					bindingList.ExpandItem(sEntry);

				VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());
			}
			else if (button.DataContext is LibraryBookEntry2 lbEntry)
			{
				LiberateClicked?.Invoke(this, lbEntry.LibraryBook);
			}
		}

		private GridView.ImageDisplay imageDisplay;
		public async void Cover_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			if (sender is not Image tblock || tblock.DataContext is not GridEntry2 gEntry)
				return;

				var picDefinition = new PictureDefinition(gEntry.LibraryBook.Book.PictureLarge ?? gEntry.LibraryBook.Book.PictureId, PictureSize.Native);
			var picDlTask = Task.Run(() => PictureStorage.GetPictureSynchronously(picDefinition));

			(_, byte[] initialImageBts) = PictureStorage.GetPicture(new PictureDefinition(gEntry.LibraryBook.Book.PictureId, PictureSize._80x80));
			var windowTitle = $"{gEntry.Title} - Cover";

			if (imageDisplay is null || imageDisplay.IsDisposed || !imageDisplay.Visible)
			{
				imageDisplay = new GridView.ImageDisplay();
				imageDisplay.RestoreSizeAndLocation(Configuration.Instance);
				imageDisplay.FormClosed += (_, _) => imageDisplay.SaveSizeAndLocation(Configuration.Instance);
				imageDisplay.Show(null);
			}

			imageDisplay.BookSaveDirectory = AudibleFileStorage.Audio.GetDestinationDirectory(gEntry.LibraryBook);
			imageDisplay.PictureFileName = System.IO.Path.GetFileName(AudibleFileStorage.Audio.GetBooksDirectoryFilename(gEntry.LibraryBook, ".jpg"));
			imageDisplay.Text = windowTitle;
			imageDisplay.CoverPicture = initialImageBts;
			imageDisplay.CoverPicture = await picDlTask;
		}

		public void Description_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			if (sender is TextBlock tblock && tblock.DataContext is GridEntry2 gEntry)
			{
				var pt = tblock.Parent.PointToScreen(tblock.Parent.Bounds.TopRight);
				var displayWindow = new GridView.DescriptionDisplay
				{
					SpawnLocation = new System.Drawing.Point(pt.X, pt.Y),
					DescriptionText = gEntry.LongDescription,
					BorderThickness = 2,
				};

				void CloseWindow(object o, DataGridRowEventArgs e)
				{
					displayWindow.Close();
				}
				productsGrid.LoadingRow += CloseWindow;
				displayWindow.FormClosed += (_, _) =>
				{
					productsGrid.LoadingRow -= CloseWindow;
				};

				displayWindow.Show();
			}
		}

		public void OnTagsButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button.DataContext is LibraryBookEntry2 lbEntry)
			{
				var bookDetailsForm = new Dialogs.BookDetailsDialog(lbEntry.LibraryBook);
				if (bookDetailsForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					lbEntry.Commit(bookDetailsForm.NewTags, bookDetailsForm.BookLiberatedStatus, bookDetailsForm.PdfLiberatedStatus);
			}
		}

		#endregion

		#region Scan and Remove Books

		public void CloseRemoveBooksColumn()
			=> removeGVColumn.IsVisible = false;

		public async Task RemoveCheckedBooksAsync()
		{
			var selectedBooks = GetAllBookEntries().Where(lbe => lbe.Remove == true).ToList();

			if (selectedBooks.Count == 0)
				return;

			var libraryBooks = selectedBooks.Select(rge => rge.LibraryBook).ToList();
			var result = MessageBoxLib.ShowConfirmationDialog(
				libraryBooks,
				$"Are you sure you want to remove {selectedBooks.Count} books from Libation's library?",
				"Remove books from Libation?");

			if (result != System.Windows.Forms.DialogResult.Yes)
				return;

			RemoveBooks(selectedBooks);
			var idsToRemove = libraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();
			var removeLibraryBooks = await LibraryCommands.RemoveBooksAsync(idsToRemove);

			RemovableCountChanged?.Invoke(this, GetAllBookEntries().Count(lbe => lbe.Remove is true));
		}
		public async Task ScanAndRemoveBooksAsync(params Account[] accounts)
		{
			RemovableCountChanged?.Invoke(this, 0);
			removeGVColumn.IsVisible = true;

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

				RemovableCountChanged?.Invoke(this, GetAllBookEntries().Count(lbe => lbe.Remove is true));
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


		#endregion

		#region UI display functions

		public void Display()
		{
			try
			{
				// don't return early if lib size == 0. this will not update correctly if all books are removed
				var dbBooks = DbContexts.GetLibrary_Flat_NoTracking(includeParents: true);
				if (productsGrid.DataContext is null)
				{
					productsGrid.DataContext = _viewModel = new ProductsDisplayViewModel(dbBooks);
					InitialLoaded?.Invoke(this, EventArgs.Empty);
					VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());
				}
				else
					UpdateGrid(dbBooks);
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error displaying library in {0}", nameof(ProductsDisplay2));
			}
		}

		private void UpdateGrid(List<LibraryBook> dbBooks)
		{
			#region Add new or update existing grid entries

			//Remove filter prior to adding/updating boooks
			string existingFilter = bindingList.Filter;
			Filter(null);

			bindingList.SuspendFilteringOnUpdate = true;

			//Add absent entries to grid, or update existing entry

			var allEntries = bindingList.AllItems().BookEntries();
			var seriesEntries = bindingList.AllItems().SeriesEntries().ToList();
			var parentedEpisodes = dbBooks.ParentedEpisodes();

			foreach (var libraryBook in dbBooks.OrderBy(e => e.DateAdded))
			{
				var existingEntry = allEntries.FindByAsin(libraryBook.Book.AudibleProductId);

				if (libraryBook.Book.IsProduct())
					AddOrUpdateBook(libraryBook, existingEntry);
				else if (parentedEpisodes.Any(lb => lb == libraryBook))
					//Only try to add or update is this LibraryBook is a know child of a parent
					AddOrUpdateEpisode(libraryBook, existingEntry, seriesEntries, dbBooks);
			}

			bindingList.SuspendFilteringOnUpdate = false;

			//Re-apply filter after adding new/updating existing books to capture any changes
			Filter(existingFilter);

			#endregion

			// remove deleted from grid.
			// note: actual deletion from db must still occur via the RemoveBook feature. deleting from audible will not trigger this
			var removedBooks =
				bindingList
				.AllItems()
				.BookEntries()
				.ExceptBy(dbBooks.Select(lb => lb.Book.AudibleProductId), ge => ge.AudibleProductId);

			RemoveBooks(removedBooks);
		}

		private void RemoveBooks(IEnumerable<LibraryBookEntry2> removedBooks)
		{
			//Remove books in series from their parents' Children list
			foreach (var removed in removedBooks.Where(b => b.Parent is not null))
			{
				removed.Parent.Children.Remove(removed);

				//In Avalonia, if you fire PropertyChanged with an empty or invalid property name, nothing is updated.
				//So we must notify for specific properties that we believed changed.
				removed.Parent.NotifyPropertyChanged(nameof(SeriesEntrys2.Length));
				removed.Parent.NotifyPropertyChanged(nameof(SeriesEntrys2.PurchaseDate));
			}

			//Remove series that have no children
			var removedSeries =
				bindingList
				.AllItems()
				.EmptySeries();

			foreach (var removed in removedBooks.Cast<GridEntry2>().Concat(removedSeries))
				//no need to re-filter for removed books
				bindingList.Remove(removed);

			VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());
		}

		private void AddOrUpdateBook(LibraryBook book, LibraryBookEntry2 existingBookEntry)
		{
			if (existingBookEntry is null)
				// Add the new product to top
				bindingList.Insert(0, new LibraryBookEntry2(book));
			else
				// update existing
				existingBookEntry.UpdateLibraryBook(book);
		}

		private void AddOrUpdateEpisode(LibraryBook episodeBook, LibraryBookEntry2 existingEpisodeEntry, List<SeriesEntrys2> seriesEntries, IEnumerable<LibraryBook> dbBooks)
		{
			if (existingEpisodeEntry is null)
			{
				LibraryBookEntry2 episodeEntry;

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


					seriesEntry = new SeriesEntrys2(seriesBook, episodeBook);
					seriesEntries.Add(seriesEntry);

					episodeEntry = seriesEntry.Children[0];
					seriesEntry.Liberate.Expanded = true;
					bindingList.Insert(0, seriesEntry);
				}
				else
				{
					//Series exists. Create and add episode child then update the SeriesEntry
					episodeEntry = new(episodeBook) { Parent = seriesEntry };
					seriesEntry.Children.Add(episodeEntry);
					var seriesBook = dbBooks.Single(lb => lb.Book.AudibleProductId == seriesEntry.LibraryBook.Book.AudibleProductId);
					seriesEntry.UpdateSeries(seriesBook);
				}

				//Add episode to the grid beneath the parent
				int seriesIndex = bindingList.IndexOf(seriesEntry);
				bindingList.Insert(seriesIndex + 1, episodeEntry);

				if (seriesEntry.Liberate.Expanded)
					bindingList.ExpandItem(seriesEntry);
				else
					bindingList.CollapseItem(seriesEntry);

				seriesEntry.NotifyPropertyChanged(nameof(SeriesEntrys2.Length));
				seriesEntry.NotifyPropertyChanged(nameof(SeriesEntrys2.PurchaseDate));
			}
			else
				existingEpisodeEntry.UpdateLibraryBook(episodeBook);
		}

		#endregion

		#region Filter

		public void Filter(string searchString)
		{
			int visibleCount = bindingList.Count;

			if (string.IsNullOrEmpty(searchString))
				bindingList.RemoveFilter();
			else
				bindingList.Filter = searchString;

			if (visibleCount != bindingList.Count)
				VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());

			//Re-sort after filtering 
			if (CurrentSortColumn is null)
				bindingList.InternalList.Sort((i1, i2) => i2.DateAdded.CompareTo(i1.DateAdded));
			else
				CurrentSortColumn?.Sort(ColumnComparers[CurrentSortColumn].SortDirection.Value);

			bindingList.ResetCollection();
		}

		#endregion

		#region Column Customizations



		#endregion
	}
}
