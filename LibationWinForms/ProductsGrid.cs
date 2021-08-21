using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core;
using Dinah.Core.DataBinding;
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
		public event EventHandler<int> VisibleCountChanged;
		public event EventHandler BackupCountsChanged;

		// alias
		private DataGridView _dataGridView => gridEntryDataGridView;

		public ProductsGrid()
		{
			InitializeComponent();

			// sorting breaks filters. must reapply filters after sorting
			_dataGridView.Sorted += (_, __) => Filter();
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

		private async Task Liberate_Click(GridEntry liveGridEntry)
		{
			var libraryBook = liveGridEntry.LibraryBook;

			// liberated: open explorer to file
			if (libraryBook.Book.Audio_Exists)
			{
				var filePath = FileManager.AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId);
				if (!Go.To.File(filePath))
					MessageBox.Show($"File not found:\r\n{filePath}");
				return;
			}

			// else: liberate
			await BookLiberation.ProcessorAutomationController.BackupSingleBookAsync(libraryBook, (_, __) => RefreshRow(libraryBook.Book.AudibleProductId));
		}

		private void Details_Click(GridEntry liveGridEntry)
		{
			var bookDetailsForm = new BookDetailsDialog(liveGridEntry.LibraryBook);
			if (bookDetailsForm.ShowDialog() != DialogResult.OK)
				return;

			var qtyChanges = LibraryCommands.UpdateUserDefinedItem(liveGridEntry.LibraryBook.Book, bookDetailsForm.NewTags, bookDetailsForm.BookLiberatedStatus, bookDetailsForm.PdfLiberatedStatus);
			if (qtyChanges == 0)
				return;

			//Re-apply filters
			Filter();

			//Update whole GridEntry row
			liveGridEntry.NotifyPropertyChanged();
		}

		#endregion

		#region UI display functions

		private bool hasBeenDisplayed = false;
		public void Display()
		{
			if (hasBeenDisplayed)
				return;
			hasBeenDisplayed = true;

			//
			// transform into sorted GridEntry.s BEFORE binding
			//
			using var context = DbContexts.GetContext();
			var lib = context.GetLibrary_Flat_NoTracking();

			// if no data. hide all columns. return
			if (!lib.Any())
			{
				for (var i = _dataGridView.ColumnCount - 1; i >= 0; i--)
					_dataGridView.Columns.RemoveAt(i);
				return;
			}

			var orderedGridEntries = lib
				.Select(lb => new GridEntry(lb)).ToList()
				// default load order
				.OrderByDescending(ge => (DateTime)ge.GetMemberValue(nameof(ge.PurchaseDate)))
				//// more advanced example: sort by author, then series, then title
				//.OrderBy(ge => ge.Authors)
				//    .ThenBy(ge => ge.Series)
				//    .ThenBy(ge => ge.Title)
				.ToList();

			// BIND
			gridEntryBindingSource.DataSource = new SortableBindingList<GridEntry>(orderedGridEntries);

			// FILTER
			Filter();

			BackupCountsChanged?.Invoke(this, EventArgs.Empty);
		}

		public void RefreshRow(string productId)
		{
			var liveGridEntry = getGridEntry((ge) => ge.AudibleProductId == productId);

			// update GridEntry Liberate cell
			liveGridEntry?.NotifyPropertyChanged(nameof(liveGridEntry.Liberate));

			// needed in case filtering by -IsLiberated and it gets changed to Liberated. want to immediately show the change
			Filter();

			BackupCountsChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Filter

		private string _filterSearchString;
		private void Filter() => Filter(_filterSearchString);
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
				for (var r = _dataGridView.RowCount - 1; r >= 0; r--)
					_dataGridView.Rows[r].Visible = productIds.Contains(getGridEntry(r).AudibleProductId);
			}

			//Causes repainting of the DataGridView
			bindingContext.ResumeBinding();
			VisibleCountChanged?.Invoke(this, _dataGridView.AsEnumerable().Count(r => r.Visible));
		}

		#endregion

		#region DataGridView Macro

		private GridEntry getGridEntry(Func<GridEntry, bool> predicate)
			=> ((SortableBindingList<GridEntry>)gridEntryBindingSource.DataSource).FirstOrDefault(predicate);

		private GridEntry getGridEntry(int rowIndex) => _dataGridView.GetBoundItem<GridEntry>(rowIndex);

		#endregion
	}
}
