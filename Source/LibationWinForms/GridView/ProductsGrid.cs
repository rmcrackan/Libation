using ApplicationServices;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Collections.Generic;
using Dinah.Core.WindowsDesktop.Forms;
using LibationFileManager;
using LibationUiBase.GridView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.GridView;

public delegate void GridEntryClickedEventHandler(GridEntry liveGridEntry);
public delegate void LibraryBookEntryClickedEventHandler(LibraryBookEntry liveGridEntry);
public delegate void GridEntryRectangleClickedEventHandler(GridEntry liveGridEntry, Rectangle cellRectangle);
public delegate void ProductsGridCellContextMenuStripNeededEventHandler(GridEntry[] liveGridEntry, ContextMenuStrip ctxMenu);

public partial class ProductsGrid : UserControl
{
	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Disable the grid context menu")]
	public bool DisableContextMenu { get; set; }
	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Disable grid column reordering and don't persist width changes")]
	public bool DisableColumnCustomization
	{
		get => field;
		set
		{
			field = value;
			gridEntryDataGridView.AllowUserToOrderColumns = !value;
		}
	}

	/// <summary>Number of visible rows has changed</summary>
	public event EventHandler<int>? VisibleCountChanged;
	public event LibraryBookEntryClickedEventHandler? LiberateClicked;
	public event GridEntryClickedEventHandler? CoverClicked;
	public event LibraryBookEntryClickedEventHandler? DetailsClicked;
	public event GridEntryRectangleClickedEventHandler? DescriptionClicked;
	public new event EventHandler<ScrollEventArgs>? Scroll;
	public event EventHandler? RemovableCountChanged;
	public event ProductsGridCellContextMenuStripNeededEventHandler? LiberateContextMenuStripNeeded;

	private GridEntryBindingList? bindingList;
	internal IEnumerable<LibraryBook> GetVisibleBookEntries()
		=> GetVisibleGridEntries().Select(lbe => lbe.LibraryBook);

	public IEnumerable<LibraryBookEntry> GetVisibleGridEntries()
		=> bindingList?.GetFilteredInItems().OfType<LibraryBookEntry>() ?? [];

	internal IEnumerable<LibraryBookEntry> GetAllBookEntries()
		=> bindingList?.AllItems().BookEntries() ?? Enumerable.Empty<LibraryBookEntry>();

	public ISearchEngine? SearchEngine { get => field; set { field = value; bindingList?.SearchEngine = value; } }

	public ProductsGrid()
	{
		InitializeComponent();
		EnableDoubleBuffering();
		gridEntryDataGridView.Scroll += (_, s) => Scroll?.Invoke(this, s);
		gridEntryDataGridView.CellContextMenuStripNeeded += GridEntryDataGridView_CellContextMenuStripNeeded;
		removeGVColumn.Frozen = false;
		defaultFont = gridEntryDataGridView.DefaultCellStyle.Font ?? gridEntryDataGridView.Font;						
	}

	#region Scaling

	[PropertyChangeFilter(nameof(Configuration.GridFontScaleFactor))]
	private void Configuration_FontScaleChanged(object sender, PropertyChangedEventArgsEx e)
	{
		if (e.NewValue is float v)
			setGridFontScale(v);
	}

	[PropertyChangeFilter(nameof(Configuration.GridScaleFactor))]
	private void Configuration_ScaleChanged(object sender, PropertyChangedEventArgsEx e)
	{
		if (e.NewValue is float v)
			setGridScale(v);
	}

	/// <summary>
	/// Keep track of the original dimensions for rescaling
	/// </summary>
	private static readonly Dictionary<DataGridViewElement, int> originalDims = new();
	private readonly Font defaultFont;
	private void setGridScale(float scale)
	{
		foreach (var col in gridEntryDataGridView.Columns.Cast<DataGridViewColumn>())
		{
			//Only resize fixed-width columns. The rest can be adjusted by users.
			if (col.Resizable is DataGridViewTriState.False)
			{
				if (!originalDims.ContainsKey(col))
					originalDims[col] = col.Width;

				col.Width = this.DpiScale(originalDims[col], scale);
			}

			if (col is IDataGridScaleColumn scCol)
				scCol.ScaleFactor = scale;
		}

		if (!originalDims.ContainsKey(gridEntryDataGridView.RowTemplate))
			originalDims[gridEntryDataGridView.RowTemplate] = gridEntryDataGridView.RowTemplate.Height;

		var height = gridEntryDataGridView.RowTemplate.Height = this.DpiScale(originalDims[gridEntryDataGridView.RowTemplate], scale);

		foreach (var row in gridEntryDataGridView.Rows.Cast<DataGridViewRow>())
			row.Height = height;
	}

	private void setGridFontScale(float scale)
		=> gridEntryDataGridView.DefaultCellStyle.Font = new Font(defaultFont.FontFamily, defaultFont.Size * scale);

	#endregion

	private static string? RemoveLineBreaks(string? text)
		=> text?.Replace("\r\n", "").Replace('\r', ' ').Replace('\n', ' ');

	private void GridEntryDataGridView_CellContextMenuStripNeeded(object? sender, DataGridViewCellContextMenuStripNeededEventArgs e)
	{
		// header
		if (DisableContextMenu || e.RowIndex < 0 || sender is not DataGridView dgv)
			return;

		e.ContextMenuStrip = new ContextMenuStrip();
		// any column except cover & stop light
		if (e.ColumnIndex != liberateGVColumn.Index && e.ColumnIndex != coverGVColumn.Index)
		{
			e.ContextMenuStrip.Items.Add("Copy Cell Contents", null, (_, __) =>
			{
				try
				{
					string clipboardText;

					if (dgv.SelectedCells.Count <= 1)
					{
						//Copy contents only of cell that was right-clicked on.
						clipboardText = dgv[e.ColumnIndex, e.RowIndex].FormattedValue?.ToString() ?? string.Empty;
					}
					else
					{
						//Copy contents of selected cells. Each row is a new line,
						//and columns are separated with tabs. Similar formatting to Microsoft Excel.
						var selectedCells
						= dgv.SelectedCells
						.OfType<DataGridViewCell>()
						.Where(c => c.OwningColumn is not null && c.OwningRow is not null)
						.OrderBy(c => c.RowIndex)
						.ThenBy(c => c.OwningColumn!.DisplayIndex)
						.ToList();

						var headerText
						= string.Join("\t",
							selectedCells
							.Select(c => c.OwningColumn)
							.Distinct()
							.Select(c => RemoveLineBreaks(c?.HeaderText))
							.OfType<string>());

						List<string> linesOfText = [headerText];
						foreach (var distinctRow in selectedCells.Select(c => c.RowIndex).Distinct())
						{
							linesOfText.Add(string.Join("\t",
								selectedCells
								.Where(c => c.RowIndex == distinctRow)
								.Select(c => RemoveLineBreaks(c.FormattedValue?.ToString()) ?? string.Empty)
								));
						}
						clipboardText = string.Join(Environment.NewLine, linesOfText);
					}
					Clipboard.SetDataObject(clipboardText, false, 5, 150);
				}
				catch(Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error copying text to clipboard");
				}
			});
			e.ContextMenuStrip.Items.Add(new ToolStripSeparator());
		}

		var clickedEntry = getGridEntry(e.RowIndex);

		var allSelected
			= gridEntryDataGridView
			.SelectedCells
			.OfType<DataGridViewCell>()
			.Select(c => c.OwningRow)
			.OfType<DataGridViewRow>()
			.Distinct()
			.OrderBy(r => r.Index)
			.Select(r => r.DataBoundItem)
			.OfType<GridEntry>()
			.ToArray();

		var clickedIndex = Array.IndexOf(allSelected, clickedEntry);
		if (clickedIndex == -1)
		{
			//User didn't right-click on a selected cell
			gridEntryDataGridView.ClearSelection();
			gridEntryDataGridView[e.ColumnIndex, e.RowIndex].Selected = true;
			allSelected = [clickedEntry];
		}
		else if (clickedIndex > 0)
		{
			//Ensure the clicked entry is first in the list
			(allSelected[0], allSelected[clickedIndex]) = (allSelected[clickedIndex], allSelected[0]);
		}
		LiberateContextMenuStripNeeded?.Invoke(allSelected, e.ContextMenuStrip);
	}

	private void EnableDoubleBuffering()
	{
		var propertyInfo = gridEntryDataGridView.GetType().GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

		propertyInfo?.SetValue(gridEntryDataGridView, true, null);
	}

	#region Button controls
	private void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
	{
		try
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
				if (e.ColumnIndex == liberateGVColumn.Index && sEntry.Liberate is not null)
				{
					if (sEntry.Liberate.Expanded)
						bindingList?.CollapseItem(sEntry);
					else
						bindingList?.ExpandItem(sEntry);

					VisibleCountChanged?.Invoke(this, bindingList?.GetFilteredInItems().Count() ?? 0);
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
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, $"An error was encountered while processing a user click in the {nameof(ProductsGrid)}");
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
			if (value && bindingList is not null)
			{
				foreach (var book in bindingList.AllItems())
					book.Remove = false;
			}

			removeGVColumn.DisplayIndex = 0;
			removeGVColumn.Frozen = value;
			removeGVColumn.Visible = value;
		}
	}

	internal async Task BindToGridAsync(List<LibraryBook> dbBooks)
	{
		//Get the UI thread's synchronization context and set it on the current thread to ensure
		//it's available for GetAllProductsAsync and GetAllSeriesEntriesAsync
		var sc = Invoke(() => System.Threading.SynchronizationContext.Current);
		System.Threading.SynchronizationContext.SetSynchronizationContext(sc);

		var geList = await LibraryBookEntry.GetAllProductsAsync(dbBooks);
		var seriesEntries = await SeriesEntry.GetAllSeriesEntriesAsync(dbBooks);

		geList.AddRange(seriesEntries);
		//Sort descending by date (default sort property)
		var comparer = new RowComparer();
		geList.Sort((a, b) => comparer.Compare(b, a));

		//Add all children beneath their parent
		foreach (var series in seriesEntries)
		{
			var seriesIndex = geList.IndexOf(series);
			foreach (var child in series.Children)
				geList.Insert(++seriesIndex, child);
		}
		System.Threading.SynchronizationContext.SetSynchronizationContext(null);
					
		bindingList = new GridEntryBindingList(geList) { SearchEngine = SearchEngine };
		bindingList.CollapseAll();

		//The syncBindingSource ensures that the IGridEntry list is added on the UI thread 
		syncBindingSource.DataSource = bindingList;
		VisibleCountChanged?.Invoke(this, bindingList.GetFilteredInItems().Count());
	}

	internal void UpdateGrid(List<LibraryBook> dbBooks)
	{
		if (bindingList == null)
			throw new InvalidOperationException($"Must call {nameof(BindToGridAsync)} before calling {nameof(UpdateGrid)}");

		//First row that is in view in the DataGridView
		var topRow = gridEntryDataGridView.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Displayed)?.Index ?? 0;

		#region Add new or update existing grid entries

		//Remove filter prior to adding/updating books
		string? existingFilter = syncBindingSource.Filter;
		Filter(null);

		//Add absent entries to grid, or update existing entry

		var allEntries = bindingList.AllItems().BookEntries().ToDictionarySafe(b => b.AudibleProductId);
		var seriesEntries = bindingList.AllItems().SeriesEntries().ToList();
		var parentedEpisodes = dbBooks.ParentedEpisodes().ToHashSet();

		//Get the UI thread's synchronization context and set it on the current thread to ensure
		//it's available for creation of new IGridEntry items during upsert
		var sc = Invoke(() => System.Threading.SynchronizationContext.Current);
		System.Threading.SynchronizationContext.SetSynchronizationContext(sc);

		bindingList.RaiseListChangedEvents = false;
		foreach (var libraryBook in dbBooks.OrderBy(e => e.DateAdded))
		{
			var existingEntry = allEntries.TryGetValue(libraryBook.Book.AudibleProductId, out var e) ? e : null;

			if (libraryBook.Book.IsProduct())
			{
				AddOrUpdateBook(libraryBook, existingEntry);
				continue;
			}
			if (parentedEpisodes.Contains(libraryBook))
			{
				//Only try to add or update is this LibraryBook is a know child of a parent
				AddOrUpdateEpisode(libraryBook, existingEntry, seriesEntries, dbBooks);
			}
		}
		bindingList.RaiseListChangedEvents = true;

		//Re-apply filter after adding new/updating existing books to capture any changes
		//The Filter call also ensures that the binding list is reset so the DataGridView
		//is made aware of all changes that were made while RaiseListChangedEvents was false
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

		if (topRow >= 0 && topRow < gridEntryDataGridView.RowCount)
			gridEntryDataGridView.FirstDisplayedScrollingRowIndex = topRow;
	}

	public void RemoveBooks(IEnumerable<LibraryBookEntry> removedBooks)
	{
		if (bindingList == null)
			throw new InvalidOperationException($"Must call {nameof(BindToGridAsync)} before calling {nameof(RemoveBooks)}");

		//Remove books in series from their parents' Children list
		foreach (var removed in removedBooks.Where(b => b.Liberate?.IsEpisode is true))
			removed.Parent?.RemoveChild(removed);

		//Remove series that have no children
		var removedSeries =
			bindingList
			.AllItems()
			.EmptySeries();

		foreach (var removed in removedBooks.Cast<GridEntry>().Concat(removedSeries))
			//no need to re-filter for removed books
			bindingList.Remove(removed);

		VisibleCountChanged?.Invoke(this, bindingList.GetFilteredInItems().Count());
	}

	private void AddOrUpdateBook(LibraryBook book, LibraryBookEntry? existingBookEntry)
	{
		if (bindingList == null)
			throw new InvalidOperationException($"Must call {nameof(BindToGridAsync)} before calling {nameof(AddOrUpdateBook)}");

		if (existingBookEntry is null)
			// Add the new product to top
			bindingList.Insert(0, new LibraryBookEntry(book));
		else
			// update existing
			existingBookEntry.UpdateLibraryBook(book);
	}

	private void AddOrUpdateEpisode(LibraryBook episodeBook, LibraryBookEntry? existingEpisodeEntry, List<SeriesEntry> seriesEntries, IEnumerable<LibraryBook> dbBooks)
	{
		if (bindingList == null)
			throw new InvalidOperationException($"Must call {nameof(BindToGridAsync)} before calling {nameof(AddOrUpdateEpisode)}");

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
					//This is only possible if the user's db  has some malformed
					//entries from earlier Libation releases that could not be
					//automatically fixed. Log, but don't throw.
					Serilog.Log.Logger.Error("Episode={0}, Episode Series: {1}", episodeBook, episodeBook.Book.SeriesNames());
					return;
				}

				seriesEntry = new SeriesEntry(seriesBook, episodeBook);
				seriesEntries.Add(seriesEntry);

				episodeEntry = seriesEntry.Children[0];
				seriesEntry.Liberate?.Expanded = true;
				bindingList.Insert(0, seriesEntry);
			}
			else
			{
				//Series exists. Create and add episode child then update the SeriesEntry
				episodeEntry = new LibraryBookEntry(episodeBook, seriesEntry);
				seriesEntry.Children.Add(episodeEntry);
				seriesEntry.Children.Sort((c1, c2) => c1.SeriesIndex.CompareTo(c2.SeriesIndex));
				var seriesBook = dbBooks.Single(lb => lb.Book.AudibleProductId == seriesEntry.LibraryBook.Book.AudibleProductId);
				seriesEntry.UpdateLibraryBook(seriesBook);
			}

			//Series entry must be expanded so its child can
			//be placed in the correct position beneath it.
			var isExpanded = seriesEntry.Liberate?.Expanded;
			bindingList.ExpandItem(seriesEntry);

			//Add episode to the grid beneath the parent
			int seriesIndex = bindingList.IndexOf(seriesEntry);
			int episodeIndex = seriesEntry.Children.IndexOf(episodeEntry);
			bindingList.Insert(seriesIndex + 1 + episodeIndex, episodeEntry);

			if (isExpanded.HasValue && isExpanded.Value)
				bindingList.ExpandItem(seriesEntry);
			else if (isExpanded.HasValue)
				bindingList.CollapseItem(seriesEntry);
		}
		else
			existingEpisodeEntry.UpdateLibraryBook(episodeBook);
	}

	#endregion

	#region Filter

	public void Filter(string? searchString)
	{
		if (bindingList is null) return;

		int visibleCount = bindingList.Count;

		if (string.IsNullOrEmpty(searchString))
			syncBindingSource.RemoveFilter();
		else
			syncBindingSource.Filter = searchString;

		if (visibleCount != bindingList.Count)
			VisibleCountChanged?.Invoke(this, bindingList.GetFilteredInItems().Count());
	}

	#endregion

	#region Column Customizations

	private void ProductsGrid_Load(object sender, EventArgs e)
	{
		//DesignMode is not set in constructor
		if (DesignMode)
			return;

		setGridFontScale(Configuration.Instance.GridFontScaleFactor);
		setGridScale(Configuration.Instance.GridScaleFactor);
		Configuration.Instance.PropertyChanged += Configuration_ScaleChanged;
		Configuration.Instance.PropertyChanged += Configuration_FontScaleChanged;
		gridEntryDataGridView.EnableHeadersVisualStyles = !Application.IsDarkModeEnabled;

		gridEntryDataGridView.Disposed += (_, _) =>
		{
			Configuration.Instance.PropertyChanged -= Configuration_ScaleChanged;
			Configuration.Instance.PropertyChanged -= Configuration_FontScaleChanged;
		};

		gridEntryDataGridView.ColumnWidthChanged += gridEntryDataGridView_ColumnWidthChanged;
		gridEntryDataGridView.ColumnDisplayIndexChanged += gridEntryDataGridView_ColumnDisplayIndexChanged;

		showHideColumnsContextMenuStrip.Items.Add(new ToolStripLabel("Show / Hide Columns"));
		showHideColumnsContextMenuStrip.Items.Add(new ToolStripSeparator());

		//Restore Grid Display Settings
		var config = Configuration.Instance;
		var gridColumnsWidths = config.GridColumnsWidths;
		var displayIndices = config.GridColumnsDisplayIndices;

		var cmsKiller = new ContextMenuStrip();

		foreach (DataGridViewColumn column in gridEntryDataGridView.Columns)
		{
			if (column == removeGVColumn)
				continue;
			var itemName = column.DataPropertyName;
			var visible = config.GetColumnVisibility(itemName);

			var menuItem = new ToolStripMenuItem(column.HeaderText)
			{
				Checked = visible,
				Tag = itemName
			};
			menuItem.Click += HideMenuItem_Click;
			showHideColumnsContextMenuStrip.Items.Add(menuItem);

			//Only set column widths for user resizable columns.
			//Fixed column widths are set by setGridScale()
			if (column.Resizable is not DataGridViewTriState.False)
				column.Width = gridColumnsWidths.GetValueOrDefault(itemName, this.DpiScale(column.Width));

			column.MinimumWidth = 10;
			column.HeaderCell.ContextMenuStrip = showHideColumnsContextMenuStrip;
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
				.SingleOrDefault(c => c.DataPropertyName == itemName);

			if (column is null) continue;

			column.DisplayIndex = displayIndices.GetValueOrDefault(itemName, column.Index);
		}

		//Remove column is always first;
		removeGVColumn.DisplayIndex = 0;
		removeGVColumn.Visible = false;
		removeGVColumn.ValueType = typeof(bool?);
		removeGVColumn.FalseValue = false;
		removeGVColumn.TrueValue = true;
		removeGVColumn.IndeterminateValue = null;
	}

	private void HideMenuItem_Click(object? sender, EventArgs e)
	{
		var menuItem = sender as ToolStripMenuItem;
		var propertyName = menuItem?.Tag as string;

		var column = gridEntryDataGridView.Columns
			.Cast<DataGridViewColumn>()
			.FirstOrDefault(c => c.DataPropertyName == propertyName);

		if (column != null && menuItem != null && propertyName != null)
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

	private void gridEntryDataGridView_ColumnDisplayIndexChanged(object? sender, DataGridViewColumnEventArgs e)
	{
		if (DisableColumnCustomization) return;
		var config = Configuration.Instance;

		var dictionary = config.GridColumnsDisplayIndices;
		dictionary[e.Column.DataPropertyName] = e.Column.DisplayIndex;
		config.GridColumnsDisplayIndices = dictionary;
	}

	private void gridEntryDataGridView_CellToolTipTextNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
	{
		if (e.ColumnIndex == descriptionGVColumn.Index)
			e.ToolTipText = "Click to see full description";
		else if (e.ColumnIndex == coverGVColumn.Index)
			e.ToolTipText = "Click to see full size";
	}

	private void gridEntryDataGridView_ColumnWidthChanged(object? sender, DataGridViewColumnEventArgs e)
	{
		if (DisableColumnCustomization) return;
		var config = Configuration.Instance;

		var dictionary = config.GridColumnsWidths;
		dictionary[e.Column.DataPropertyName] = e.Column.Width;
		config.GridColumnsWidths = dictionary;
	}

	#endregion
}
