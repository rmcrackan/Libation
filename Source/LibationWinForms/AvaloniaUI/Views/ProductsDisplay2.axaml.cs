using ApplicationServices;
using AudibleUtilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
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

		private ProductsDisplayViewModel _viewModel;
		private GridEntryBindingList2 bindingList => productsGrid.Items as GridEntryBindingList2;
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

			var dbBooks = DbContexts.GetLibrary_Flat_NoTracking(includeParents: true);
			productsGrid.DataContext = _viewModel = new ProductsDisplayViewModel(dbBooks);

			this.AttachedToVisualTree +=(_, _) => VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private void Dg1_Sorting(object sender, DataGridColumnEventArgs e)
		{
			bindingList.DoSortCore(e.Column.SortMemberPath);
			e.Handled = true;
		}

		#region Button controls

		public void Remove_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			productsGrid.CommitEdit(DataGridEditingUnit.Cell, true);
			RemovableCountChanged?.Invoke(this, GetAllBookEntries().Count(lbe => lbe.Remove is true));
		}

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
		}

		#endregion

		#region Column Customizations



		#endregion
	}
}
