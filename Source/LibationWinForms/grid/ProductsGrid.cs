using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Windows.Forms;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{

	#region // legacy instructions to update data_grid_view
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
	//
	// as of august 2021 this does not work in vs2019 with .net5 projects
	// VS has improved since then with .net6+ but I haven't checked again
	#endregion


	public partial class ProductsGrid : UserControl
	{
		public event EventHandler<LibraryBook> LiberateClicked;
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;

		// alias
		private DataGridView _dataGridView => gridEntryDataGridView;

		public ProductsGrid()
		{
			InitializeComponent();

			if (this.DesignMode)
				return;

			EnableDoubleBuffering();

			_dataGridView.CellContentClick += DataGridView_CellContentClick;

			this.Load += ProductsGrid_Load;
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
			if (e.RowIndex < 0)
				return;

			if (e.ColumnIndex == liberateGVColumn.Index)
				Liberate_Click(getGridEntry(e.RowIndex));
			else if (e.ColumnIndex == tagAndDetailsGVColumn.Index)
				Details_Click(getGridEntry(e.RowIndex));
			else if (e.ColumnIndex == descriptionGVColumn.Index)
				Description_Click(getGridEntry(e.RowIndex), _dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false));
			else if (e.ColumnIndex == coverGVColumn.Index)
				await Cover_Click(getGridEntry(e.RowIndex));
		}

		private ImageDisplay imageDisplay;
		private async Task Cover_Click(GridEntry liveGridEntry)
		{
			var picDefinition = new PictureDefinition(liveGridEntry.LibraryBook.Book.PictureLarge ?? liveGridEntry.LibraryBook.Book.PictureId, PictureSize.Native);
			var picDlTask = Task.Run(() => PictureStorage.GetPictureSynchronously(picDefinition));

			(_, byte[] initialImageBts) = PictureStorage.GetPicture(new PictureDefinition(liveGridEntry.LibraryBook.Book.PictureId, PictureSize._80x80));
			var windowTitle = $"{liveGridEntry.Title} - Cover";

			if (imageDisplay is null || imageDisplay.IsDisposed || !imageDisplay.Visible)
			{
				imageDisplay = new ImageDisplay();
				imageDisplay.RestoreSizeAndLocation(Configuration.Instance);
				imageDisplay.FormClosed += (_, _) => imageDisplay.SaveSizeAndLocation(Configuration.Instance);
				imageDisplay.Show(this);
			}

			imageDisplay.BookSaveDirectory = AudibleFileStorage.Audio.GetDestinationDirectory(liveGridEntry.LibraryBook);
			imageDisplay.PictureFileName = System.IO.Path.GetFileName(AudibleFileStorage.Audio.GetBooksDirectoryFilename(liveGridEntry.LibraryBook, ".jpg"));
			imageDisplay.Text = windowTitle;
			imageDisplay.CoverPicture = initialImageBts;
			imageDisplay.CoverPicture = await picDlTask;
		}

		private void Description_Click(GridEntry liveGridEntry, Rectangle cellDisplay)
		{
			var displayWindow = new DescriptionDisplay
			{
				SpawnLocation = PointToScreen(cellDisplay.Location + new Size(cellDisplay.Width, 0)),
				DescriptionText = liveGridEntry.LongDescription,
				BorderThickness = 2,
			};

			void CloseWindow(object o, EventArgs e)
			{
				displayWindow.Close();
			}

			_dataGridView.Scroll += CloseWindow;
			displayWindow.FormClosed += (_, _) => _dataGridView.Scroll -= CloseWindow;
			displayWindow.Show(this);
		}

		private void Liberate_Click(GridEntry liveGridEntry)
		{
			var libraryBook = liveGridEntry.LibraryBook;

			// liberated: open explorer to file
			if (libraryBook.Book.Audio_Exists())
			{
				var filePath = AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId);
				if (!Go.To.File(filePath))
				{
					var suffix = string.IsNullOrWhiteSpace(filePath) ? "" : $":\r\n{filePath}";
					MessageBox.Show($"File not found" + suffix);
				}
				return;
			}

			LiberateClicked?.Invoke(this, liveGridEntry.LibraryBook);
		}

		private static void Details_Click(GridEntry liveGridEntry)
		{
			var bookDetailsForm = new BookDetailsDialog(liveGridEntry.LibraryBook);
			if (bookDetailsForm.ShowDialog() == DialogResult.OK)
				liveGridEntry.Commit(bookDetailsForm.NewTags, bookDetailsForm.BookLiberatedStatus, bookDetailsForm.PdfLiberatedStatus);
		}

		#endregion

		#region UI display functions

		private SortableFilterableBindingList bindingList;

		private bool hasBeenDisplayed;
		public event EventHandler InitialLoaded;
		public void Display()
		{
			// don't return early if lib size == 0. this will not update correctly if all books are removed
			var lib = DbContexts.GetLibrary_Flat_NoTracking();

			if (!hasBeenDisplayed)
			{
				// bind
				bindToGrid(lib);
				hasBeenDisplayed = true;
				InitialLoaded?.Invoke(this, new());
				VisibleCountChanged?.Invoke(this, bindingList.Count);
			}
			else
				updateGrid(lib);

		}

		private void bindToGrid(List<LibraryBook> dbBooks)
		{
			bindingList = new SortableFilterableBindingList(dbBooks.OrderByDescending(lb => lb.DateAdded).Select(lb => new GridEntry(lb)));
			gridEntryBindingSource.DataSource = bindingList;
		}

		private void updateGrid(List<LibraryBook> dbBooks)
		{
			int visibleCount = bindingList.Count;
			string existingFilter = gridEntryBindingSource.Filter;

			//Add absent books to grid, or update current books

			var allItmes = bindingList.AllItems();
			for (var i = dbBooks.Count - 1; i >= 0; i--)
			{
				var libraryBook = dbBooks[i];
				var existingItem = allItmes.FirstOrDefault(i => i.AudibleProductId == libraryBook.Book.AudibleProductId);

				// add new to top
				if (existingItem is null)
					bindingList.Insert(0, new GridEntry(libraryBook));
				// update existing
				else
					existingItem.UpdateLibraryBook(libraryBook);
			}

			if (bindingList.Count != visibleCount)
			{
				//refilter for newly added items
				Filter(null);
				Filter(existingFilter);
			}

			// remove deleted from grid.
			// note: actual deletion from db must still occur via the RemoveBook feature. deleting from audible will not trigger this
			var removedBooks = 
				bindingList
				.AllItems()
				.ExceptBy(dbBooks.Select(lb => lb.Book.AudibleProductId), ge => ge.AudibleProductId)
				.ToList();

			foreach (var removed in removedBooks)
				//no need to re-filter for removed books
				bindingList.Remove(removed);

			if (bindingList.Count != visibleCount)
				VisibleCountChanged?.Invoke(this, bindingList.Count);
		}

		#endregion

		#region Filter

		public void Filter(string searchString)
		{
			int visibleCount = bindingList.Count;

			if (string.IsNullOrEmpty(searchString))
				gridEntryBindingSource.RemoveFilter();
			else
				gridEntryBindingSource.Filter = searchString;

			if (visibleCount != bindingList.Count)
				VisibleCountChanged?.Invoke(this, bindingList.Count);
		}

		#endregion

		internal List<LibraryBook> GetVisible()
			=> bindingList
			.Select(row => row.LibraryBook)
			.ToList();

		private GridEntry getGridEntry(int rowIndex) => _dataGridView.GetBoundItem<GridEntry>(rowIndex);

		#region Column Customizations

		// to ensure this is only ever called once: Load instead of 'override OnVisibleChanged'
		private void ProductsGrid_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			contextMenuStrip1.Items.Add(new ToolStripLabel("Show / Hide Columns"));
			contextMenuStrip1.Items.Add(new ToolStripSeparator());

			//Restore Grid Display Settings
			var config = Configuration.Instance;
			var gridColumnsVisibilities = config.GridColumnsVisibilities;
			var gridColumnsWidths = config.GridColumnsWidths;
			var displayIndices = config.GridColumnsDisplayIndices;

			var cmsKiller = new ContextMenuStrip();

			foreach (DataGridViewColumn column in _dataGridView.Columns)
			{
				var itemName = column.DataPropertyName;
				var visible = gridColumnsVisibilities.GetValueOrDefault(itemName, true);

				var menuItem = new ToolStripMenuItem()
				{
					Text = column.HeaderText,
					Checked = visible,
					Tag = itemName
				};
				menuItem.Click += HideMenuItem_Click;
				contextMenuStrip1.Items.Add(menuItem);

				column.Width = gridColumnsWidths.GetValueOrDefault(itemName, column.Width);
				column.MinimumWidth = 10;
				column.HeaderCell.ContextMenuStrip = contextMenuStrip1;
				column.Visible = visible;

				//Setting a default ContextMenuStrip will allow the columns to handle the
				//Show() event so it is not passed up to the _dataGridView.ContextMenuStrip.
				//This allows the ContextMenuStrip to be shown if right-clicking in the gray
				//background of _dataGridView but not shown if right-clicking inside cells.
				column.ContextMenuStrip = cmsKiller;
			}

			//We must set DisplayIndex properties in ascending order
			foreach (var itemName in displayIndices.OrderBy(i => i.Value).Select(i => i.Key))
			{
				var column = _dataGridView.Columns
					.Cast<DataGridViewColumn>()
					.Single(c => c.DataPropertyName == itemName);

				column.DisplayIndex = displayIndices.GetValueOrDefault(itemName, column.Index);
			}

			base.OnVisibleChanged(e);
		}

		private void gridEntryDataGridView_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
		{
			var config = Configuration.Instance;

			var dictionary = config.GridColumnsDisplayIndices;
			dictionary[e.Column.DataPropertyName] = e.Column.DisplayIndex;
			config.GridColumnsDisplayIndices = dictionary;
		}

		private void gridEntryDataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
		{
			var config = Configuration.Instance;

			var dictionary = config.GridColumnsWidths;
			dictionary[e.Column.DataPropertyName] = e.Column.Width;
			config.GridColumnsWidths = dictionary;
		}

		private void HideMenuItem_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripMenuItem;
			var propertyName = menuItem.Tag as string;

			var column = _dataGridView.Columns
				.Cast<DataGridViewColumn>()
				.FirstOrDefault(c => c.DataPropertyName == propertyName);

			if (column != null)
			{
				var visible = menuItem.Checked;
				menuItem.Checked = !visible;
				column.Visible = !visible;

				var config = Configuration.Instance;

				var dictionary = config.GridColumnsVisibilities;
				dictionary[propertyName] = column.Visible;
				config.GridColumnsVisibilities = dictionary;
			}
		}

		private void gridEntryDataGridView_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
		{
			if (e.ColumnIndex == descriptionGVColumn.Index)
				e.ToolTipText = "Click to see full description";
			else if (e.ColumnIndex == coverGVColumn.Index)
				e.ToolTipText = "Click to see full size";
		}

		#endregion
	}
}
