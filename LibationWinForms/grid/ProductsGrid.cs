using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using Dinah.Core;
using Dinah.Core.DataBinding;
using Dinah.Core.Threading;
using Dinah.Core.Windows.Forms;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
	// INSTRUCTIONS TO UPDATE DATA_GRID_VIEW
	// - delete current DataGridView
	// - view > other windows > data sources
	// - refresh
	// OR
	// - Add New Data Source
	//   Object. Next
	//   LibationWinForms
	//     AudibleDTO
	//       GridEntry
	// - go to Design view
	// - click on Data Sources > ProductItem. dropdown: DataGridView
	// - drag/drop ProductItem on design surface
	// AS OF AUGUST 2021 THIS DOES NOT WORK IN VS2019 WITH .NET-5 PROJECTS 

	public partial class ProductsGrid : UserControl
	{
		private static List<string> bookConversionInProgress = new();
		public event EventHandler<int> VisibleCountChanged;

		// alias
		private DataGridView _dataGridView => gridEntryDataGridView;

		public ProductsGrid()
		{
			InitializeComponent();

			// sorting breaks filters. must reapply filters after sorting
			_dataGridView.Sorted += Filter;
			_dataGridView.CellContentClick += DataGridView_CellContentClick;

			EnableDoubleBuffering();
		}
		private void EnableDoubleBuffering()
		{
			var propertyInfo = _dataGridView.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			propertyInfo.SetValue(_dataGridView, true, null);
		}

		#region Button controls

		private async void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			// handle grid button click: https://stackoverflow.com/a/13687844
			if (e.RowIndex < 0 || _dataGridView.Columns[e.ColumnIndex] is not DataGridViewButtonColumn)
				return;

			var liveGridEntry = getGridEntry(e.RowIndex);

			switch (_dataGridView.Columns[e.ColumnIndex].DataPropertyName)
			{
				case nameof(liveGridEntry.Liberate):
					await Liberate_Click(liveGridEntry);
					break;
				case nameof(liveGridEntry.DisplayTags):
					Details_Click(liveGridEntry);
					break;
			}
		}

		private static async Task Liberate_Click(GridEntry liveGridEntry)
		{
			var libraryBook = liveGridEntry.LibraryBook;

			// liberated: open explorer to file
			if (libraryBook.Book.Audio_Exists)
			{
				var filePath = LibationFileManager.AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId);
				if (!Go.To.File(filePath))
				{
					var suffix = string.IsNullOrWhiteSpace(filePath) ? "" : $":\r\n{filePath}";
					MessageBox.Show($"File not found" + suffix);
				}
				return;
			}

			//don't try to decrypt the same book at the same time.
			if (bookConversionInProgress.Contains(libraryBook.Book.AudibleProductId))
				return;

			bookConversionInProgress.Add(libraryBook.Book.AudibleProductId);

			// else: liberate
			await BookLiberation.ProcessorAutomationController.BackupSingleBookAsync(libraryBook, b => bookConversionInProgress.Remove(b.Book.AudibleProductId));
		}

		private static void Details_Click(GridEntry liveGridEntry)
		{
			var bookDetailsForm = new BookDetailsDialog(liveGridEntry.LibraryBook);
			if (bookDetailsForm.ShowDialog() != DialogResult.OK)
				return;

			liveGridEntry.BeginEdit();

			liveGridEntry.DisplayTags = bookDetailsForm.NewTags;
			liveGridEntry.Liberate = (bookDetailsForm.BookLiberatedStatus, bookDetailsForm.PdfLiberatedStatus);

			liveGridEntry.EndEdit();
		}

		#endregion

		#region UI display functions

		public int Count { get; private set; }

		private SortableBindingList<GridEntry> bindingList;

		public void Display()
		{
			var lib = DbContexts.GetLibrary_Flat_NoTracking();

			Count = lib.Count;

			// if no data. hide all columns. return
			if (!lib.Any())
			{
				for (var i = _dataGridView.ColumnCount - 1; i >= 0; i--)
					_dataGridView.Columns.RemoveAt(i);
				return;
			}

			var orderedBooks = lib
				// default load order
				.OrderByDescending(lb => lb.DateAdded)
				//// more advanced example: sort by author, then series, then title
				//.OrderBy(lb => lb.Book.AuthorNames)
				//    .ThenBy(lb => lb.Book.SeriesSortable)
				//    .ThenBy(lb => lb.Book.TitleSortable)
				.ToList();

			// BIND
			if (bindingList is null)
				bindToGrid(orderedBooks);
			else
				updateGrid(orderedBooks);

			// FILTER
			Filter();
		}

        private void bindToGrid(List<DataLayer.LibraryBook> orderedBooks)
        {
            bindingList = new SortableBindingList<GridEntry>(orderedBooks.Select(lb => toGridEntry(lb)));
            gridEntryBindingSource.DataSource = bindingList;
        }

        private void updateGrid(List<DataLayer.LibraryBook> orderedBooks)
        {
            for (var i = orderedBooks.Count - 1; i >= 0; i--)
            {
                var libraryBook = orderedBooks[i];
                var existingItem = bindingList.FirstOrDefault(i => i.AudibleProductId == libraryBook.Book.AudibleProductId);

                // add new to top
                if (existingItem is null)
                    bindingList.Insert(0, toGridEntry(libraryBook));
                // update existing
                else
                    existingItem.UpdateLibraryBook(libraryBook);
            }

            // remove deleted from grid. note: actual deletion from db must still occur via the RemoveBook feature. deleting from audible will not trigger this
            var oldIds = bindingList.Select(ge => ge.AudibleProductId).ToList();
            var newIds = orderedBooks.Select(lb => lb.Book.AudibleProductId).ToList();
            var remove = oldIds.Except(newIds).ToList();
            foreach (var id in remove)
            {
                var oldItem = bindingList.FirstOrDefault(ge => ge.AudibleProductId == id);
                if (oldItem is not null)
                    bindingList.Remove(oldItem);
            }
        }

        private GridEntry toGridEntry(DataLayer.LibraryBook libraryBook)
		{
			var entry = new GridEntry(libraryBook);
			entry.Committed += Filter;
			entry.LibraryBookUpdated += (sender, productId) => _dataGridView.InvalidateRow(_dataGridView.GetRowIdOfBoundItem((GridEntry)sender));
			return entry;
		}

        #endregion

        #region Filter

        private string _filterSearchString;
		private void Filter(object _ = null, EventArgs __ = null) => Filter(_filterSearchString);
		public void Filter(string searchString)
		{
			_filterSearchString = searchString;

			if (_dataGridView.Rows.Count == 0)
				return;

			var searchResults = SearchEngineCommands.Search(searchString);
			var productIds = searchResults.Docs.Select(d => d.ProductId).ToList();

			// https://stackoverflow.com/a/18942430
			var bindingContext = BindingContext[_dataGridView.DataSource];
			bindingContext.SuspendBinding();
			{
				this.UIThreadSync(() =>
				{
					for (var r = _dataGridView.RowCount - 1; r >= 0; r--)
						_dataGridView.Rows[r].Visible = productIds.Contains(getGridEntry(r).AudibleProductId);
				});
			}

			// Causes repainting of the DataGridView
			bindingContext.ResumeBinding();
			VisibleCountChanged?.Invoke(this, _dataGridView.AsEnumerable().Count(r => r.Visible));
		}

		#endregion

		#region DataGridView Macro
		private GridEntry getGridEntry(int rowIndex) => _dataGridView.GetBoundItem<GridEntry>(rowIndex);
		#endregion
	}
}
