using DataLayer;
using Dinah.Core.Windows.Forms;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.grid
{
	public partial class ProductsGrid : UserControl
	{
		public delegate void LibraryBookEntryClickedEventHandler(LibraryBookEntry liveGridEntry);
		public delegate void LibraryBookEntryRectangleClickedEventHandler(LibraryBookEntry liveGridEntry, Rectangle cellRectangle);

		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;
		public event LibraryBookEntryClickedEventHandler LiberateClicked;
		public event LibraryBookEntryClickedEventHandler CoverClicked;
		public event LibraryBookEntryClickedEventHandler DetailsClicked;
		public event LibraryBookEntryRectangleClickedEventHandler DescriptionClicked;
		public new event EventHandler<ScrollEventArgs> Scroll;

		private FilterableSortableBindingList bindingList;
		private SyncBindingSource gridEntryBindingSource;

		public ProductsGrid()
		{
			InitializeComponent();
			EnableDoubleBuffering();
			//There a bug in designer that causes errors if you add BindingSource to the DataGridView at design time. 
			gridEntryBindingSource = new SyncBindingSource();
			gridEntryDataGridView.DataSource = gridEntryBindingSource;
			gridEntryDataGridView.Scroll += (_, s) => Scroll?.Invoke(this, s);
		}

		private void EnableDoubleBuffering()
		{
			var propertyInfo = gridEntryDataGridView.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			propertyInfo.SetValue(gridEntryDataGridView, true, null);
		}

		#region Button controls
		private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			// handle grid button click: https://stackoverflow.com/a/13687844
			if ( e.RowIndex < 0)
				return;

			var column = gridEntryDataGridView.Columns[e.ColumnIndex];

			var entry = getGridEntry(e.RowIndex);
			if (entry is LibraryBookEntry lbEntry)
			{
				if (gridEntryDataGridView.Columns[e.ColumnIndex].DataPropertyName == liberateGVColumn.DataPropertyName)
					LiberateClicked?.Invoke(lbEntry);
				else if (gridEntryDataGridView.Columns[e.ColumnIndex].DataPropertyName == tagAndDetailsGVColumn.DataPropertyName && entry is LibraryBookEntry)
					DetailsClicked?.Invoke(lbEntry);
				else if (gridEntryDataGridView.Columns[e.ColumnIndex].DataPropertyName == descriptionGVColumn.DataPropertyName)
					DescriptionClicked?.Invoke(lbEntry, gridEntryDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false));
				else if (gridEntryDataGridView.Columns[e.ColumnIndex].DataPropertyName == coverGVColumn.DataPropertyName)
					CoverClicked?.Invoke(lbEntry);
			}
			else if (entry is SeriesEntry sEntry && gridEntryDataGridView.Columns[e.ColumnIndex].DataPropertyName == liberateGVColumn.DataPropertyName)
			{
				if (sEntry.Liberate.Expanded)
					bindingList.CollapseItem(sEntry);
				else
					bindingList.ExpandItem(sEntry);

				sEntry.NotifyPropertyChanged(nameof(sEntry.Liberate));

				VisibleCountChanged?.Invoke(this, bindingList.LibraryBooks().Count());
			}
		}

		private GridEntry getGridEntry(int rowIndex) => gridEntryDataGridView.GetBoundItem<GridEntry>(rowIndex);

		#endregion


		#region UI display functions

		internal void BindToGrid(List<LibraryBook> dbBooks)
		{
			var geList = dbBooks.Where(b => b.Book.ContentType is not ContentType.Episode).Select(b => new LibraryBookEntry(b)).Cast<GridEntry>().ToList();

			var episodes = dbBooks.Where(b => b.Book.ContentType is ContentType.Episode).ToList();

			var series = episodes.Select(lb => lb.Book.SeriesLink.First()).DistinctBy(s => s.Series).ToList();

			foreach (var s in series)
			{
				var seriesEntry = new SeriesEntry();
				seriesEntry.Children = episodes.Where(lb => lb.Book.SeriesLink.First().Series == s.Book.SeriesLink.First().Series).Select(lb => new LibraryBookEntry(lb) { Parent = seriesEntry }).ToList();

				seriesEntry.setSeriesBook(s);

				geList.Add(seriesEntry);
				geList.AddRange(seriesEntry.Children);
			}

			bindingList = new FilterableSortableBindingList(geList.OrderByDescending(e => e.DateAdded));
			bindingList.CollapseAll();
			gridEntryBindingSource.DataSource = bindingList;
			VisibleCountChanged?.Invoke(this, bindingList.LibraryBooks().Count());
		}

		internal void UpdateGrid(List<LibraryBook> dbBooks)
		{
			int visibleCount = bindingList.Count;
			string existingFilter = gridEntryBindingSource.Filter;

			//Add absent books to grid, or update current books

			var allItmes = bindingList.AllItems().LibraryBooks();
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
						//Find the series that libraryBook belongs to, if it exists
						var series = bindingList.AllItems().Series().FirstOrDefault(i => libraryBook.Book.SeriesLink.Any(s => s.Series.Name == i.Series));

						if (series is null)
						{
							//Series doesn't exist yet, so create and add it
							var newSeries = new SeriesEntry { Children = new List<LibraryBookEntry> { lb } };
							newSeries.setSeriesBook(libraryBook.Book.SeriesLink.First());
							lb.Parent = newSeries;
							newSeries.Liberate.Expanded = true;
							bindingList.Insert(0, newSeries);
							series = newSeries;
						}
						else
						{
							lb.Parent = series;
							series.Children.Add(lb);
						}
						//Add episode beneath the parent
						int seriesIndex = bindingList.IndexOf(series);
						bindingList.Insert(seriesIndex + 1, lb);
					}
					else
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
				.LibraryBooks()
				.ExceptBy(dbBooks.Select(lb => lb.Book.AudibleProductId), ge => ge.AudibleProductId);

			foreach (var removed in removedBooks.Where(b => b.Parent is not null))
			{
				((SeriesEntry)removed.Parent).Children.Remove(removed);
			}

			//Remove series that have no children
			var removedSeries =
				bindingList
				.AllItems()
				.Series()
				.Where(i => i.Children.Count == 0);

			foreach (var removed in removedBooks.Cast<GridEntry>().Concat(removedSeries))
				//no need to re-filter for removed books
				bindingList.Remove(removed);

			if (bindingList.Count != visibleCount)
				VisibleCountChanged?.Invoke(this, bindingList.LibraryBooks().Count());
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
				VisibleCountChanged?.Invoke(this, bindingList.LibraryBooks().Count());

		}

		#endregion

		internal IEnumerable<LibraryBookEntry> GetVisible()
			=> bindingList
			.LibraryBooks();

		private void ProductsGrid_Load(object sender, EventArgs e)
		{
			gridEntryDataGridView.ColumnWidthChanged += gridEntryDataGridView_ColumnWidthChanged;

			contextMenuStrip1.Items.Add(new ToolStripLabel("Show / Hide Columns"));
			contextMenuStrip1.Items.Add(new ToolStripSeparator());

			//Restore Grid Display Settings
			var config = Configuration.Instance;
			var gridColumnsVisibilities = config.GridColumnsVisibilities;
			var gridColumnsWidths = config.GridColumnsWidths;
			var displayIndices = config.GridColumnsDisplayIndices;

			var cmsKiller = new ContextMenuStrip();

			foreach (DataGridViewColumn column in gridEntryDataGridView.Columns)
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
				var column = gridEntryDataGridView.Columns
					.Cast<DataGridViewColumn>()
					.Single(c => c.DataPropertyName == itemName);

				column.DisplayIndex = displayIndices.GetValueOrDefault(itemName, column.Index);
			}
		}
		private void HideMenuItem_Click(object sender, EventArgs e)
		{
			var menuItem = sender as ToolStripMenuItem;
			var propertyName = menuItem.Tag as string;

			var column = gridEntryDataGridView.Columns
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

		private void gridEntryDataGridView_ColumnDisplayIndexChanged(object sender, DataGridViewColumnEventArgs e)
		{
			var config = Configuration.Instance;

			var dictionary = config.GridColumnsDisplayIndices;
			dictionary[e.Column.DataPropertyName] = e.Column.DisplayIndex;
			config.GridColumnsDisplayIndices = dictionary;
		}

		private void gridEntryDataGridView_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
		{
			if (e.ColumnIndex == descriptionGVColumn.Index)
				e.ToolTipText = "Click to see full description";
			else if (e.ColumnIndex == coverGVColumn.Index)
				e.ToolTipText = "Click to see full size";
		}

		private void gridEntryDataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
		{
			var config = Configuration.Instance;

			var dictionary = config.GridColumnsWidths;
			dictionary[e.Column.DataPropertyName] = e.Column.Width;
		}
	}
}
