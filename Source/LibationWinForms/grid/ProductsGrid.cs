using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core.Windows.Forms;
using LibationFileManager;

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

		internal delegate void LibraryBookEntryClickedEventHandler(DataGridViewCellEventArgs e, LibraryBookEntry liveGridEntry);
		internal delegate void LibraryBookEntryRectangleClickedEventHandler(DataGridViewCellEventArgs e, LibraryBookEntry liveGridEntry, Rectangle cellRectangle);
		internal event LibraryBookEntryClickedEventHandler LiberateClicked;
		internal event LibraryBookEntryClickedEventHandler CoverClicked;
		internal event LibraryBookEntryClickedEventHandler DetailsClicked;
		internal event LibraryBookEntryRectangleClickedEventHandler DescriptionClicked;
		public new event EventHandler<ScrollEventArgs> Scroll;

		private FilterableSortableBindingList bindingList;

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
			_dataGridView.Scroll += (_, s) => Scroll?.Invoke(this, s);

			Load += ProductsGrid_Load;
		}

		private void ProductsGrid_Scroll(object sender, ScrollEventArgs e)
		{
			throw new NotImplementedException();
		}

		private void EnableDoubleBuffering()
		{
			var propertyInfo = _dataGridView.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			propertyInfo.SetValue(_dataGridView, true, null);
		}

		#region Button controls

		private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			// handle grid button click: https://stackoverflow.com/a/13687844
			if (e.RowIndex < 0)
				return;

			var entry = getGridEntry(e.RowIndex);
			if (entry is LibraryBookEntry lbEntry)
			{
				if (e.ColumnIndex == liberateGVColumn.Index)
					LiberateClicked?.Invoke(e, lbEntry);
				else if (e.ColumnIndex == tagAndDetailsGVColumn.Index && entry is LibraryBookEntry)
					DetailsClicked?.Invoke(e, lbEntry);
				else if (e.ColumnIndex == descriptionGVColumn.Index)
					DescriptionClicked?.Invoke(e, lbEntry, _dataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false));
				else if (e.ColumnIndex == coverGVColumn.Index)
					CoverClicked?.Invoke(e, lbEntry);
			}
			else if (entry is SeriesEntry sEntry && e.ColumnIndex == liberateGVColumn.Index)
			{
				if (sEntry.Liberate.Expanded)
					bindingList.CollapseItem(sEntry);
				else
					bindingList.ExpandItem(sEntry);

				sEntry.NotifyPropertyChanged(nameof(sEntry.Liberate));
			}
		}

		private GridEntry getGridEntry(int rowIndex) => _dataGridView.GetBoundItem<GridEntry>(rowIndex);

		#endregion

		#region UI display functions

		internal void bindToGrid(List<LibraryBook> dbBooks)
		{
			var geList = dbBooks.Where(b => b.Book.ContentType is not ContentType.Episode).Select(b => new LibraryBookEntry(b)).Cast<GridEntry>().ToList();

			var episodes = dbBooks.Where(b => b.Book.ContentType is ContentType.Episode).ToList();

			var series = episodes.Select(lb => lb.Book.SeriesLink.First()).DistinctBy(s => s.Series).ToList();

			foreach (var s in series)
			{
				var seriesEntry = new SeriesEntry();
				seriesEntry.Children = episodes.Where(lb => lb.Book.SeriesLink.First().Series == s.Book.SeriesLink.First().Series).Select(lb => new LibraryBookEntry(lb) { Parent = seriesEntry }).Cast<GridEntry>().ToList();

				seriesEntry.setSeriesBook(s);
				geList.Add(seriesEntry);
			}

			bindingList = new FilterableSortableBindingList(geList.OrderByDescending(ge => ge.DateAdded));
			gridEntryBindingSource.DataSource = bindingList;
		}

		internal void updateGrid(List<LibraryBook> dbBooks)
		{
			int visibleCount = bindingList.Count;
			string existingFilter = gridEntryBindingSource.Filter;

			//Add absent books to grid, or update current books

			var allItmes = bindingList.AllItems().Where(i => i is LibraryBookEntry).Cast<LibraryBookEntry>();
			for (var i = dbBooks.Count - 1; i >= 0; i--)
			{
				var libraryBook = dbBooks[i];
				var existingItem = allItmes.FirstOrDefault(i => i.AudibleProductId == libraryBook.Book.AudibleProductId);

				// add new to top
				if (existingItem is null)
				{
					var lb = new LibraryBookEntry(libraryBook);

					if (libraryBook.Book.ContentType is ContentType.Episode)
					{
						//Find the series that libraryBook, if it exists
						var series = bindingList.AllItems().Where(i => i is SeriesEntry).Cast<SeriesEntry>().FirstOrDefault(i => libraryBook.Book.SeriesLink.Any(s => s.Series.Name == i.Series));

						if (series is null)
						{
							//Series doesn't exist yet, so create and add it
							var newSeries = new SeriesEntry { Children = new List<GridEntry> { lb } };
							newSeries.setSeriesBook(libraryBook.Book.SeriesLink.First());
							lb.Parent = newSeries;
							newSeries.Liberate.Expanded = true;
							bindingList.Insert(0, newSeries);
						}
						else
						{
							lb.Parent = series;
							series.Children.Add(lb);
						}
					}
					//Add the new product
					bindingList.Insert(0, lb);
				}
				// update existing
				else
				{
					existingItem.UpdateLibraryBook(libraryBook);
				}
			}

			if (bindingList.Count != visibleCount)
			{
				//re-filter for newly added items
				Filter(null);
				Filter(existingFilter);
			}

			// remove deleted from grid.
			// note: actual deletion from db must still occur via the RemoveBook feature. deleting from audible will not trigger this
			var removedBooks =
				bindingList
				.AllItems()
				.Where(i => i is LibraryBookEntry)
				.Cast<LibraryBookEntry>()
				.ExceptBy(dbBooks.Select(lb => lb.Book.AudibleProductId), ge => ge.AudibleProductId);

			//Remove series that have no children
			var removedSeries =
				bindingList
				.AllItems()
				.Where(i => i is SeriesEntry)
				.Cast<SeriesEntry>()
				.Where(i => removedBooks.Count(r => r.Series == i.Series) == i.Children.Count);

			foreach (var removed in removedBooks.Cast<GridEntry>().Concat(removedSeries))
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

		internal IEnumerable<LibraryBook> GetVisible()
			=> bindingList
			.Where(row => row is LibraryBookEntry)
			.Cast<LibraryBookEntry>()
			.Select(row => row.LibraryBook);

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
