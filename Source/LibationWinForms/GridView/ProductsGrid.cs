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
	public delegate void GridEntryClickedEventHandler(GridEntry liveGridEntry);
	public delegate void LibraryBookEntryClickedEventHandler(LibraryBookEntry liveGridEntry);
	public delegate void GridEntryRectangleClickedEventHandler(GridEntry liveGridEntry, Rectangle cellRectangle);

	public partial class ProductsGrid : UserControl
	{
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;
		public event LibraryBookEntryClickedEventHandler LiberateClicked;
		public event GridEntryClickedEventHandler CoverClicked;
		public event LibraryBookEntryClickedEventHandler DetailsClicked;
		public event GridEntryRectangleClickedEventHandler DescriptionClicked;
		public new event EventHandler<ScrollEventArgs> Scroll;
		public event EventHandler RemovableCountChanged;

		private GridEntryBindingList bindingList;
		internal IEnumerable<LibraryBook> GetVisibleBooks()
			=> bindingList
			.BookEntries()
			.Select(lbe => lbe.LibraryBook);
		internal IEnumerable<LibraryBookEntry> GetAllBookEntries()
			=> bindingList.AllItems().BookEntries();

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
			else if (entry is SeriesEntry sEntry)
			{
				if (e.ColumnIndex == liberateGVColumn.Index)
				{
					if (sEntry.Liberate.Expanded)
						bindingList.CollapseItem(sEntry);
					else
						bindingList.ExpandItem(sEntry);

					sEntry.NotifyPropertyChanged(nameof(sEntry.Liberate));

					VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());
				}
				else if (e.ColumnIndex == descriptionGVColumn.Index)
					DescriptionClicked?.Invoke(sEntry, gridEntryDataGridView.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false));
				else if (e.ColumnIndex == coverGVColumn.Index)
					CoverClicked?.Invoke(sEntry);
			}

			if (e.ColumnIndex == removeGVColumn.Index)
			{
				gridEntryDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
				RemovableCountChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		private GridEntry getGridEntry(int rowIndex) => gridEntryDataGridView.GetBoundItem<GridEntry>(rowIndex);

		#endregion

		#region UI display functions

		internal bool RemoveColumnVisible
		{
			get => removeGVColumn.Visible;
			set
			{
				if (value)
				{
					foreach (var book in bindingList.AllItems())
						book.Remove = RemoveStatus.NotRemoved;
				}
				removeGVColumn.Visible = value;
			}
		}

		internal void BindToGrid(List<LibraryBook> dbBooks)
		{
			var geList = dbBooks.Where(lb => lb.Book.IsProduct()).Select(b => new LibraryBookEntry(b)).Cast<GridEntry>().ToList();

			var episodes = dbBooks.Where(lb => lb.Book.IsEpisodeChild());
						
			foreach (var parent in dbBooks.Where(lb => lb.Book.IsEpisodeParent()))
			{
				var seriesEpisodes = episodes.FindChildren(parent);

				if (!seriesEpisodes.Any()) continue;

				var seriesEntry = new SeriesEntry(parent, seriesEpisodes);

				geList.Add(seriesEntry);
				geList.AddRange(seriesEntry.Children);
			}

			bindingList = new GridEntryBindingList(geList.OrderByDescending(e => e.DateAdded));
			bindingList.CollapseAll();
			syncBindingSource.DataSource = bindingList;
			VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());
		}

		internal void UpdateGrid(List<LibraryBook> dbBooks)
		{
			#region Add new or update existing grid entries

			//Remove filter prior to adding/updating boooks
			string existingFilter = syncBindingSource.Filter;
			Filter(null);

			bindingList.SuspendFilteringOnUpdate = true;

			//Add absent entries to grid, or update existing entry

			var allEntries = bindingList.AllItems().BookEntries();
			var seriesEntries = bindingList.AllItems().SeriesEntries().ToList();

			foreach (var libraryBook in dbBooks.OrderBy(e => e.DateAdded))
			{
				var existingEntry = allEntries.FindByAsin(libraryBook.Book.AudibleProductId);

				if (libraryBook.Book.IsProduct())
					AddOrUpdateBook(libraryBook, existingEntry);
				else if(libraryBook.Book.IsEpisodeChild()) 
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

		public void RemoveBooks(IEnumerable<LibraryBookEntry> removedBooks)
		{
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

			VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());
		}

		private void AddOrUpdateBook(LibraryBook book, LibraryBookEntry existingBookEntry)
		{
			if (existingBookEntry is null)
				// Add the new product to top
				bindingList.Insert(0, new LibraryBookEntry(book));
			else
				// update existing
				existingBookEntry.UpdateLibraryBook(book);
		}

		private void AddOrUpdateEpisode(LibraryBook episodeBook, LibraryBookEntry existingEpisodeEntry, List<SeriesEntry> seriesEntries, IEnumerable<LibraryBook> dbBooks)
		{
			if (existingEpisodeEntry is null)
			{
				LibraryBookEntry episodeEntry;
				var seriesEntry = seriesEntries.FindSeriesParent(episodeBook);

				if (seriesEntry is null)
				{
					//Series doesn't exist yet, so create and add it
					var seriesBook = dbBooks.FindSeriesParent(episodeBook);

					if (seriesBook is null)
					{
						//This should be impossible because the importer ensures every episode has a parent.
						var ex = new ApplicationException($"Episode's series parent not found in database.");
						var seriesLinks = string.Join("\r\n", episodeBook.Book.SeriesLink?.Select(sb => $"{nameof(sb.Series.Name)}={sb.Series.Name}, {nameof(sb.Series.AudibleSeriesId)}={sb.Series.AudibleSeriesId}"));
						Serilog.Log.Logger.Error(ex, "Episode={episodeBook}, Series: {seriesLinks}", episodeBook, seriesLinks);
						throw ex;
					}

					seriesEntry = new SeriesEntry(seriesBook, episodeBook);
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

				seriesEntry.NotifyPropertyChanged();

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
				syncBindingSource.RemoveFilter();
			else
				syncBindingSource.Filter = searchString;

			if (visibleCount != bindingList.Count)
				VisibleCountChanged?.Invoke(this, bindingList.BookEntries().Count());

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

			//Remove column is always first;
			removeGVColumn.DisplayIndex = 0;
			removeGVColumn.Visible = false;
			removeGVColumn.ValueType = typeof(RemoveStatus);
			removeGVColumn.FalseValue = RemoveStatus.NotRemoved;
			removeGVColumn.TrueValue = RemoveStatus.Removed;
			removeGVColumn.IndeterminateValue = RemoveStatus.SomeRemoved;
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
