using DataLayer;
using Dinah.Core.Windows.Forms;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.GridView
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

		private GridEntryBindingList bindingList;
		internal IEnumerable<LibraryBookEntry> GetVisible()
			=> bindingList
			.LibraryBooks();

		public ProductsGrid()
		{
			InitializeComponent();
			EnableDoubleBuffering();
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
			if (e.RowIndex < 0)
				return;

			var entry = getGridEntry(e.RowIndex);
			if (entry is LibraryBookEntry lbEntry)
			{
				if (e.ColumnIndex == liberateGVColumn.Index)
					LiberateClicked?.Invoke(lbEntry);
				else if (e.ColumnIndex == tagAndDetailsGVColumn.Index)
					DetailsClicked?.Invoke(lbEntry);
				else if (e.ColumnIndex == descriptionGVColumn.Index)
					DescriptionClicked?.Invoke(lbEntry, gridEntryDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false));
				else if (e.ColumnIndex == coverGVColumn.Index)
					CoverClicked?.Invoke(lbEntry);
			}
			else if (entry is SeriesEntry sEntry && e.ColumnIndex == liberateGVColumn.Index)
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

			var episodes = dbBooks.Where(b => b.IsEpisodeChild()).ToList();

			var allSeries = episodes.SelectMany(lb => lb.Book.SeriesLink.Where(s => !s.Series.AudibleSeriesId.StartsWith("SERIES_"))).DistinctBy(s => s.Series).ToList();
			foreach (var series in allSeries)
			{
				var seriesEntry = new SeriesEntry(series, episodes.Where(lb => lb.Book.SeriesLink.Any(s => s.Series == series.Series)));

				geList.Add(seriesEntry);
				geList.AddRange(seriesEntry.Children);
			}

			bindingList = new GridEntryBindingList(geList.OrderByDescending(e => e.DateAdded));
			bindingList.CollapseAll();
			syncBindingSource.DataSource = bindingList;
			VisibleCountChanged?.Invoke(this, bindingList.LibraryBooks().Count());
		}

		internal void UpdateGrid(List<LibraryBook> dbBooks)
		{
			string existingFilter = syncBindingSource.Filter;
			Filter(null);

			bindingList.SuspendFilteringOnUpdate = true;

			//Add absent books to grid, or update current books

			var allItmes = bindingList.AllItems().LibraryBooks();
			foreach (var libraryBook in dbBooks)
			{
				var existingItem = allItmes.FindBookByAsin(libraryBook.Book.AudibleProductId);

				// add new to top
				if (existingItem is null)
				{
					if (libraryBook.IsEpisodeChild())
					{
						LibraryBookEntry lbe;
						//Find the series that libraryBook belongs to, if it exists
						var series = bindingList.AllItems().FindBookSeriesEntry(libraryBook.Book.SeriesLink);

						if (series is null)
						{
							//Series doesn't exist yet, so create and add it
							var newSeries = new SeriesEntry(libraryBook.Book.SeriesLink.First(), libraryBook);
							lbe = newSeries.Children[0];
							newSeries.Liberate.Expanded = true;
							bindingList.Insert(0, newSeries);
							series = newSeries;
						}
						else
						{
							lbe = new(libraryBook) { Parent = series };
							series.Children.Add(lbe);
						}
						//Add episode beneath the parent
						int seriesIndex = bindingList.IndexOf(series);
						bindingList.Insert(seriesIndex + 1, lbe);

						if (series.Liberate.Expanded)
							bindingList.ExpandItem(series);
						else
							bindingList.CollapseItem(series);

						series.NotifyPropertyChanged();
					}
					else if (libraryBook.Book.ContentType is not ContentType.Episode)
						//Add the new product
						bindingList.Insert(0, new LibraryBookEntry(libraryBook));
				}
				// update existing
				else
				{
					existingItem.UpdateLibraryBook(libraryBook);
				}
			}

			bindingList.SuspendFilteringOnUpdate = false;

			//Re-filter after updating existing / adding new books to capture any changes
			Filter(existingFilter);

			// remove deleted from grid.
			// note: actual deletion from db must still occur via the RemoveBook feature. deleting from audible will not trigger this
			var removedBooks =
				bindingList
				.AllItems()
				.LibraryBooks()
				.ExceptBy(dbBooks.Select(lb => lb.Book.AudibleProductId), ge => ge.AudibleProductId);

			//Remove books in series from their parents' Children list
			foreach (var removed in removedBooks.Where(b => b.Parent is not null))
			{
				removed.Parent.Children.Remove(removed);
				removed.Parent.NotifyPropertyChanged();
			}

			//Remove series that have no children
			var removedSeries =
				bindingList
				.AllItems()
				.EmptySeries();

			foreach (var removed in removedBooks.Cast<GridEntry>().Concat(removedSeries))
				//no need to re-filter for removed books
				bindingList.Remove(removed);

			VisibleCountChanged?.Invoke(this, bindingList.LibraryBooks().Count());
		}

		#endregion

		#region Filter

		public void Filter(string searchString)
		{
			int visibleCount = bindingList.Count;

			if (string.IsNullOrEmpty(searchString))
				syncBindingSource.RemoveFilter();
			else
				syncBindingSource.Filter = searchString;

			if (visibleCount != bindingList.Count)
				VisibleCountChanged?.Invoke(this, bindingList.LibraryBooks().Count());

		}

		#endregion

		#region Column Customizations

		private void ProductsGrid_Load(object sender, EventArgs e)
		{
			//https://stackoverflow.com/a/4498512/3335599
			if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime) return;

			gridEntryDataGridView.ColumnWidthChanged += gridEntryDataGridView_ColumnWidthChanged;
			gridEntryDataGridView.ColumnDisplayIndexChanged += gridEntryDataGridView_ColumnDisplayIndexChanged;

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

		#endregion
	}
}
