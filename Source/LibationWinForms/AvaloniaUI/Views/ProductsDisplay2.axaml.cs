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
using ReactiveUI;
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
		DataGridColumn liberateGVColumn;
		DataGridColumn coverGVColumn;
		DataGridColumn titleGVColumn;
		DataGridColumn authorsGVColumn;
		DataGridColumn narratorsGVColumn;
		DataGridColumn lengthGVColumn;
		DataGridColumn seriesGVColumn;
		DataGridColumn descriptionGVColumn;
		DataGridColumn categoryGVColumn;
		DataGridColumn productRatingGVColumn;
		DataGridColumn purchaseDateGVColumn;
		DataGridColumn myRatingGVColumn;
		DataGridColumn miscGVColumn;
		DataGridColumn tagAndDetailsGVColumn;

		#region Init

		public ProductsDisplay2()
		{
			InitializeComponent();
			

			if (Design.IsDesignMode)
			{
				using var context = DbContexts.GetContext();
				var book = context.GetLibraryBook_Flat_NoTracking("B017V4IM1G");
				productsGrid.DataContext = _viewModel = new ProductsDisplayViewModel(new List<LibraryBook> { book });
				return;
			}
		}
		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);

			productsGrid = this.FindControl<DataGrid>(nameof(productsGrid));
			productsGrid.Sorting += ProductsGrid_Sorting;
			productsGrid.CanUserSortColumns = true;
			productsGrid.LoadingRow += ProductsGrid_LoadingRow;

			removeGVColumn = productsGrid.Columns[0];
			liberateGVColumn = productsGrid.Columns[1];
			coverGVColumn = productsGrid.Columns[2];
			titleGVColumn = productsGrid.Columns[3];
			authorsGVColumn = productsGrid.Columns[4];
			narratorsGVColumn = productsGrid.Columns[5];
			lengthGVColumn = productsGrid.Columns[6];
			seriesGVColumn = productsGrid.Columns[7];
			descriptionGVColumn = productsGrid.Columns[8];
			categoryGVColumn = productsGrid.Columns[9];
			productRatingGVColumn = productsGrid.Columns[10];
			purchaseDateGVColumn = productsGrid.Columns[11];
			myRatingGVColumn = productsGrid.Columns[12];
			miscGVColumn = productsGrid.Columns[13];
			tagAndDetailsGVColumn = productsGrid.Columns[14];

			RegisterCustomColumnComparers();
		}

		#endregion

		#region Apply Background Brush Style to Series Books Rows

		private static object tagObj = new();
		private void ProductsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			if (e.Row.Tag == tagObj)
				return;
			e.Row.Tag = tagObj;

			static IBrush GetRowColor(DataGridRow row)
				=> row.DataContext is GridEntry2 gEntry
				&& gEntry is LibraryBookEntry2 lbEntry
				&& lbEntry.Parent is not null
				? App.SeriesEntryGridBackgroundBrush
				: null;

			e.Row.Background = GetRowColor(e.Row);
			e.Row.DataContextChanged += (sender, e) =>
			{
				var row = sender as DataGridRow;
				row.Background = GetRowColor(row);
			};
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
			ReSort();
		}

		#endregion

		#region Sorting

		private void RegisterCustomColumnComparers()
		{

			removeGVColumn.CustomSortComparer = new RowComparer(removeGVColumn);
			liberateGVColumn.CustomSortComparer = new RowComparer(liberateGVColumn);
			titleGVColumn.CustomSortComparer = new RowComparer(titleGVColumn);
			authorsGVColumn.CustomSortComparer = new RowComparer(authorsGVColumn);
			narratorsGVColumn.CustomSortComparer = new RowComparer(narratorsGVColumn);
			lengthGVColumn.CustomSortComparer = new RowComparer(lengthGVColumn);
			seriesGVColumn.CustomSortComparer = new RowComparer(seriesGVColumn);
			descriptionGVColumn.CustomSortComparer = new RowComparer(descriptionGVColumn);
			categoryGVColumn.CustomSortComparer = new RowComparer(categoryGVColumn);
			productRatingGVColumn.CustomSortComparer = new RowComparer(productRatingGVColumn);
			purchaseDateGVColumn.CustomSortComparer = new RowComparer(purchaseDateGVColumn);
			myRatingGVColumn.CustomSortComparer = new RowComparer(myRatingGVColumn);
			miscGVColumn.CustomSortComparer = new RowComparer(miscGVColumn);
			tagAndDetailsGVColumn.CustomSortComparer = new RowComparer(tagAndDetailsGVColumn);
		}

		private void ReSort()
		{
			if (CurrentSortColumn is null)
			{
				bindingList.InternalList.Sort(new RowComparer(ListSortDirection.Descending, nameof(GridEntry2.DateAdded)));
				bindingList.ResetCollection();
			}
			else
				CurrentSortColumn.Sort(((RowComparer)CurrentSortColumn.CustomSortComparer).SortDirection ?? ListSortDirection.Ascending);
		}


		private DataGridColumn CurrentSortColumn;

		private void ProductsGrid_Sorting(object sender, DataGridColumnEventArgs e)
		{
			var comparer = e.Column.CustomSortComparer as RowComparer;
			//Force the comparer to get the current sort order. We can't
			//retrieve it from inside this event handler because Avalonia
			//doesn't set the property until after this event.
			comparer.SortDirection = null;
			CurrentSortColumn = e.Column;
		}


		#endregion

		#region Button controls

		public void LiberateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button.DataContext is SeriesEntrys2 sEntry)
			{
				if (sEntry.Liberate.Expanded)
					bindingList.CollapseItem(sEntry);
				else
				{
					bindingList.ExpandItem(sEntry);
					ReSort();
				}

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
				removed.Parent.RaisePropertyChanged(nameof(SeriesEntrys2.Length));
				removed.Parent.RaisePropertyChanged(nameof(SeriesEntrys2.PurchaseDate));
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

				seriesEntry.RaisePropertyChanged(nameof(SeriesEntrys2.Length));
				seriesEntry.RaisePropertyChanged(nameof(SeriesEntrys2.PurchaseDate));
			}
			else
				existingEpisodeEntry.UpdateLibraryBook(episodeBook);
		}

		#endregion		

		#region Column Customizations



		#endregion
	}
}
