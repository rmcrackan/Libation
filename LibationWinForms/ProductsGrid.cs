using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core.Collections.Generic;
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
    // - click on Data Sources > ProductItem. drowdown: DataGridView
    // - drag/drop ProductItem on design surface
    public partial class ProductsGrid : UserControl
    {
        public event EventHandler<int> VisibleCountChanged;
        public event EventHandler BackupCountsChanged;

        private const string EDIT_TAGS = "Edit Tags";
        private const string LIBERATE = "Liberate";

        // alias
        private DataGridView dataGridView => gridEntryDataGridView;

		private LibationContext context;

		public ProductsGrid()
		{
			InitializeComponent();
            formatDataGridView();
            addLiberateButtons();
            addEditTagsButtons();
            formatColumns();
			Disposed += (_, __) => context?.Dispose();

			manageLiveImageUpdateSubscriptions();
		}

        private void formatDataGridView()
        {
            dataGridView.Dock = DockStyle.Fill;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.AutoGenerateColumns = false;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView.ReadOnly = true;
            dataGridView.RowHeadersVisible = false;

            // adjust height for 80x80 pictures.
            // this must be done before databinding. or can alter later by iterating through rows
            dataGridView.RowTemplate.Height = 82;
            dataGridView.CellFormatting += replaceFormatted;
            dataGridView.CellFormatting += hiddenFormatting;

            // sorting breaks filters. must reapply filters after sorting
            dataGridView.Sorted += (_, __) => filter();
        }

        #region format text cells. ie: not buttons
        private void replaceFormatted(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var col = ((DataGridView)sender).Columns[e.ColumnIndex];
            if (col is DataGridViewTextBoxColumn textCol && getGridEntry(e.RowIndex).TryDisplayValue(textCol.Name, out string value))
            {
                // DO NOT DO THIS: getCell(e).Value = value;
                // it's the wrong way and will infinitely call CellFormatting on each assign

                // this is the correct way. will actually set FormattedValue (and EditedFormattedValue) while leaving Value as-is for sorting
                e.Value = value;

                getCell(e).ToolTipText = value;
            }
        }

        private void hiddenFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var dgv = (DataGridView)sender;
            // no action needed for buttons
            if (e.RowIndex < 0 || dgv.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
                return;

            var isHidden = getGridEntry(e.RowIndex).TagsEnumerated.Contains("hidden");

            getCell(e).Style
                = isHidden
                ? new DataGridViewCellStyle { ForeColor = Color.LightGray }
                : dgv.DefaultCellStyle;
        }
        #endregion

        #region liberation buttons
        private void addLiberateButtons()
        {
            dataGridView.Columns.Insert(0, new DataGridViewButtonColumn { HeaderText = LIBERATE });

            dataGridView.CellPainting += liberate_Paint;
            dataGridView.CellContentClick += liberate_Click;
        }

        private void liberate_Paint(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (!isColumnValid(e, LIBERATE))
                return;

            var cell = getCell(e);
            var gridEntry = getGridEntry(e.RowIndex);
            var liberatedStatus = gridEntry.Liberated_Status;
            var pdfStatus = gridEntry.Pdf_Status;

            // mouseover text
            {
                var libState = liberatedStatus switch
                {
                    GridEntry.LiberatedState.Liberated => "Liberated",
                    GridEntry.LiberatedState.DRM => "Downloaded but needs DRM removed",
                    GridEntry.LiberatedState.NotDownloaded => "Book NOT downloaded",
                    _ => throw new Exception("Unexpected liberation state")
                };

                var pdfState = pdfStatus switch
                {
                    GridEntry.PdfState.Downloaded => "\r\nPDF downloaded",
                    GridEntry.PdfState.NotDownloaded => "\r\nPDF NOT downloaded",
                    GridEntry.PdfState.NoPdf => "",
                    _ => throw new Exception("Unexpected PDF state")
                };

                var text = libState + pdfState;

                if (liberatedStatus == GridEntry.LiberatedState.NotDownloaded ||
                    liberatedStatus == GridEntry.LiberatedState.DRM ||
                    pdfStatus == GridEntry.PdfState.NotDownloaded)
                    text += "\r\nClick to complete";

                //DEBUG//cell.Value = text;
                cell.ToolTipText = text;
            }

            // draw img
            {
                var image_lib
                    = liberatedStatus == GridEntry.LiberatedState.NotDownloaded ? "red"
                    : liberatedStatus == GridEntry.LiberatedState.DRM ? "yellow"
                    : liberatedStatus == GridEntry.LiberatedState.Liberated ? "green"
                    : throw new Exception("Unexpected liberation state");
                var image_pdf
                    = pdfStatus == GridEntry.PdfState.NoPdf ? ""
                    : pdfStatus == GridEntry.PdfState.NotDownloaded ? "_pdf_no"
                    : pdfStatus == GridEntry.PdfState.Downloaded ? "_pdf_yes"
                    : throw new Exception("Unexpected PDF state");
                var image = (Bitmap)Properties.Resources.ResourceManager.GetObject($"liberate_{image_lib}{image_pdf}");
                drawImage(e, image);
            }
        }

        private async void liberate_Click(object sender, DataGridViewCellEventArgs e)
        {
            if (!isColumnValid(e, LIBERATE))
                return;
            
            var productId = getGridEntry(e.RowIndex).GetBook().AudibleProductId;

            // liberated: open explorer to file
            if (FileManager.AudibleFileStorage.Audio.Exists(productId))
            {
                var filePath = FileManager.AudibleFileStorage.Audio.GetPath(productId);
                System.Diagnostics.Process.Start("explorer.exe", $"/select, \"{filePath}\"");
                return;
            }

            // not liberated: liberate
            var msg
                = "Liberate entire library instead?"
                + "\r\n\r\nClick Yes to begin liberating your entire library"
                + "\r\n\r\nClick No to liberate this book only";
            if (MessageBox.Show(msg, "Liberate entire library?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                await BookLiberation.ProcessorAutomationController.BackupAllBooksAsync((_, libraryBook) => RefreshRow(libraryBook.Book.AudibleProductId));
            else
                await BookLiberation.ProcessorAutomationController.BackupSingleBookAsync(productId, (_, __) => RefreshRow(productId));
        }
        #endregion

        public void RefreshRow(string productId)
        {
            var rowId = getRowId((ge) => ge.GetBook().AudibleProductId == productId);

            // update cells incl Liberate button text
            dataGridView.InvalidateRow(rowId);

            // needed in case filtering by -IsLiberated and it gets changed to Liberated. want to immediately show the change
            filter();

            BackupCountsChanged?.Invoke(this, EventArgs.Empty);
        }

        #region tag buttons
        private void addEditTagsButtons()
        {
            dataGridView.Columns.Add(new DataGridViewButtonColumn { HeaderText = EDIT_TAGS });

            dataGridView.CellPainting += editTags_Paint;
            dataGridView.CellContentClick += editTags_Click;
        }

        private void editTags_Paint(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // DataGridView Image for Button Column: https://stackoverflow.com/a/36253883

            if (!isColumnValid(e, EDIT_TAGS))
                return;

            var cell = getCell(e);
            var gridEntry = getGridEntry(e.RowIndex);

            var displayTags = gridEntry.TagsEnumerated.ToList();

            if (displayTags.Any())
                cell.Value = string.Join("\r\n", displayTags);
            else
            {
                // if removing all tags: clear previous tag text
                cell.Value = "";
                drawImage(e, Properties.Resources.edit_tags_25x25);
            }
        }

        private void editTags_Click(object sender, DataGridViewCellEventArgs e)
        {
            // handle grid button click: https://stackoverflow.com/a/13687844

            var dgv = (DataGridView)sender;

            if (!isColumnValid(e, EDIT_TAGS))
                return;

            var liveGridEntry = getGridEntry(e.RowIndex);

            // EditTagsDialog should display better-formatted title
            liveGridEntry.TryDisplayValue(nameof(liveGridEntry.Title), out string value);

            var editTagsForm = new EditTagsDialog(value, liveGridEntry.Tags);
            if (editTagsForm.ShowDialog() != DialogResult.OK)
                return;

			var qtyChanges = context.UpdateTags(liveGridEntry.GetBook(), editTagsForm.NewTags);
			if (qtyChanges == 0)
                return;

            // force a re-draw, and re-apply filters

            // needed to update text colors
            dgv.InvalidateRow(e.RowIndex);

            filter();
        }
        #endregion

        private static void drawImage(DataGridViewCellPaintingEventArgs e, Bitmap image)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            var w = image.Width;
            var h = image.Height;
            var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
            var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

            e.Graphics.DrawImage(image, new Rectangle(x, y, w, h));
            e.Handled = true;
        }

        private bool isColumnValid(DataGridViewCellEventArgs e, string colName) => isColumnValid(e.RowIndex, e.ColumnIndex, colName);
        private bool isColumnValid(DataGridViewCellPaintingEventArgs e, string colName) => isColumnValid(e.RowIndex, e.ColumnIndex, colName);
        private bool isColumnValid(int rowIndex, int colIndex, string colName)
        {
            var col = dataGridView.Columns[colIndex];
            return rowIndex >= 0 && col.Name == colName && col is DataGridViewButtonColumn;
        }

        private void formatColumns()
        {
            for (var i = dataGridView.ColumnCount - 1; i >= 0; i--)
            {
                var col = dataGridView.Columns[i];

                // initial HeaderText is the lookup name from GridEntry class. any formatting below won't change this
                col.Name = col.HeaderText;

                if (!(col is DataGridViewImageColumn || col is DataGridViewButtonColumn))
                    col.SortMode = DataGridViewColumnSortMode.Automatic;

                col.HeaderText = col.HeaderText.Replace("_", " ");

                col.Width = col.Name switch
                {
                    LIBERATE => 70,
                    nameof(GridEntry.Cover) => 80,
                    nameof(GridEntry.Title) => col.Width * 2,
                    nameof(GridEntry.Misc) => (int)(col.Width * 1.35),
                    var n when n.In(nameof(GridEntry.My_Rating), nameof(GridEntry.Product_Rating)) => col.Width + 8,
                    _ => col.Width
                };
            }
        }

		#region live update newly downloaded and cached images
		private void manageLiveImageUpdateSubscriptions()
		{
			FileManager.PictureStorage.PictureCached += crossThreadImageUpdate;
			Disposed += (_, __) => FileManager.PictureStorage.PictureCached -= crossThreadImageUpdate;
		}

		private void crossThreadImageUpdate(object _, string pictureId)
			=> dataGridView.UIThread(() => updateRowImage(pictureId));
		private void updateRowImage(string pictureId)
		{
			var rowId = getRowId((ge) => ge.GetBook().PictureId == pictureId);
			if (rowId > -1)
				dataGridView.InvalidateRow(rowId);
		}
		#endregion

		private bool hasBeenDisplayed = false;
        public void Display()
        {
            if (hasBeenDisplayed)
                return;
            hasBeenDisplayed = true;

            //
            // transform into sorted GridEntry.s BEFORE binding
            //
            context = DbContexts.GetContext();
            var lib = context.GetLibrary_Flat_WithTracking();

            // if no data. hide all columns. return
            if (!lib.Any())
            {
                for (var i = dataGridView.ColumnCount - 1; i >= 0; i--)
                    dataGridView.Columns.RemoveAt(i);
                return;
            }

            var orderedGridEntries = lib
                .Select(lb => new GridEntry(lb)).ToList()
                // default load order
                .OrderByDescending(ge => ge.Purchase_Date)
                //// more advanced example: sort by author, then series, then title
                //.OrderBy(ge => ge.Authors)
                //    .ThenBy(ge => ge.Series)
                //    .ThenBy(ge => ge.Title)
                .ToList();

            //
            // BIND
            //
            gridEntryBindingSource.DataSource = orderedGridEntries.ToSortableBindingList();

            //
            // FILTER
            //
            filter();

            BackupCountsChanged?.Invoke(this, EventArgs.Empty);
        }

        #region filter
        string _filterSearchString;
        private void filter() => Filter(_filterSearchString);
        public void Filter(string searchString)
        {
            _filterSearchString = searchString;

			if (dataGridView.Rows.Count == 0)
				return;

            var searchResults = SearchEngineCommands.Search(searchString);
            var productIds = searchResults.Docs.Select(d => d.ProductId).ToList();

            // https://stackoverflow.com/a/18942430
            var currencyManager = (CurrencyManager)BindingContext[dataGridView.DataSource];
            currencyManager.SuspendBinding();
            {
                for (var r = dataGridView.RowCount - 1; r >= 0; r--)
                    dataGridView.Rows[r].Visible = productIds.Contains(getGridEntry(r).GetBook().AudibleProductId);
            }
            currencyManager.ResumeBinding();
			VisibleCountChanged?.Invoke(this, dataGridView.AsEnumerable().Count(r => r.Visible));
        }
        #endregion

        private int getRowId(Func<GridEntry, bool> func) => dataGridView.GetRowIdOfBoundItem(func);

        private GridEntry getGridEntry(int rowIndex) => dataGridView.GetBoundItem<GridEntry>(rowIndex);

        private DataGridViewCell getCell(DataGridViewCellFormattingEventArgs e) => getCell(e.RowIndex, e.ColumnIndex);

        private DataGridViewCell getCell(DataGridViewCellPaintingEventArgs e) => getCell(e.RowIndex, e.ColumnIndex);

        private DataGridViewCell getCell(int rowIndex, int columnIndex) => dataGridView.Rows[rowIndex].Cells[columnIndex];
    }
}
