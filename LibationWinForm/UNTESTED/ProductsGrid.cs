using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dinah.Core.DataBinding;
using DataLayer;

namespace LibationWinForm
{
    // INSTRUCTIONS TO UPDATE DATA_GRID_VIEW
    // - delete current DataGridView
    // - view > other windows > data sources
    // - refresh
    // OR
    // - Add New Data Source
    //   Object. Next
    //   LibationWinForm
    //     AudibleDTO
    //       GridEntry
    // - go to Design view
    // - click on Data Sources > ProductItem. drowdown: DataGridView
    // - drag/drop ProductItem on design surface
    public partial class ProductsGrid : UserControl
    {
        private DataGridView dataGridView;

        private Form1 parent;

        // this is a simple ctor for loading library and wish list. can expand later for other options. eg: overload ctor
        public ProductsGrid(Form1 parent) : this() => this.parent = parent;
        public ProductsGrid() => InitializeComponent();

        private bool hasBeenDisplayed = false;
        public void Display()
        {
            if (hasBeenDisplayed)
                return;
            hasBeenDisplayed = true;

            dataGridView = gridEntryDataGridView;

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
            dataGridView.Sorted += (_, __) => Filter();

            { // add tag buttons
                var editUserTagsButton = new DataGridViewButtonColumn { HeaderText = "Edit Tags" };
                dataGridView.Columns.Add(editUserTagsButton);

                // add image and handle click
                dataGridView.CellPainting += paintEditTag_TextAndImage;
                dataGridView.CellContentClick += dataGridView_GridButtonClick;
            }

            for (var i = dataGridView.ColumnCount - 1; i >= 0; i--)
            {
                DataGridViewColumn col = dataGridView.Columns[i];

                // initial HeaderText is the lookup name from GridEntry class. any formatting below won't change this
                col.Name = col.HeaderText;

                if (!(col is DataGridViewImageColumn || col is DataGridViewButtonColumn))
                    col.SortMode = DataGridViewColumnSortMode.Automatic;

                col.HeaderText = col.HeaderText.Replace("_", " ");

                if (col.Name == nameof(GridEntry.Title))
                    col.Width *= 2;

                if (col.Name == nameof(GridEntry.Misc))
                    col.Width = (int)(col.Width * 1.35);
            }


            //
            // transform into sorted GridEntry.s BEFORE binding
            //
            var lib = LibraryQueries.GetLibrary_Flat_NoTracking();
            var orderedGridEntries = lib
                .Select(lb => new GridEntry(lb)).ToList()
                // default load order: sort by author, then series, then title
                .OrderBy(ge => ge.Authors)
                    .ThenBy(ge => ge.Series)
                    .ThenBy(ge => ge.Title)
                .ToList();

            //
            // BIND
            //
            gridEntryBindingSource.DataSource = orderedGridEntries.ToSortableBindingList();

            //
            // AFTER BINDING, BEFORE FILTERING
            //
            // now that we have data, remove/hide text columns with blank data. don't search image and button columns.
            // simplifies the interface in general. also distinuishes library from wish list etc w/o explicit filters.
            // must be AFTER BINDING, BEFORE FILTERING because we don't want to remove rows when valid data is simply not visible due to filtering.
            for (var c = dataGridView.ColumnCount - 1; c >= 0; c--)
            {
                if (!(dataGridView.Columns[c] is DataGridViewTextBoxColumn textCol))
                    continue;

                bool hasData = false;
                for (var r = 0; r < dataGridView.RowCount; r++)
                {
                    var value = dataGridView[c, r].Value;
                    if (value != null && value.ToString() != "")
                    {
                        hasData = true;
                        break;
                    }
                }

                if (!hasData)
                    dataGridView.Columns.Remove(textCol);
            }

            //
            // FILTER
            //
            Filter();
        }

        private void paintEditTag_TextAndImage(object sender, DataGridViewCellPaintingEventArgs e)
        {
            // DataGridView Image for Button Column: https://stackoverflow.com/a/36253883

            if (e.RowIndex < 0 || !(((DataGridView)sender).Columns[e.ColumnIndex] is DataGridViewButtonColumn))
                return;


            var gridEntry = getGridEntry(e.RowIndex);
            var displayTags = gridEntry.TagsEnumerated.ToList();

            if (displayTags.Any())
                dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = string.Join("\r\n", displayTags);
            else // no tags: use image
            {
                // clear tag text
                dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "";

                // images from: icons8.com -- search: tags
                var image = Properties.Resources.edit_tags_25x25;

                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                var w = image.Width;
                var h = image.Height;
                var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

                e.Graphics.DrawImage(image, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }

        private void dataGridView_GridButtonClick(object sender, DataGridViewCellEventArgs e)
        {
            // handle grid button click: https://stackoverflow.com/a/13687844

            if (e.RowIndex < 0)
                return;
            if (sender != dataGridView)
                throw new Exception($"{nameof(dataGridView_GridButtonClick)} has incorrect sender ...somehow");
            if (!(dataGridView.Columns[e.ColumnIndex] is DataGridViewButtonColumn))
                return;

            var liveGridEntry = getGridEntry(e.RowIndex);

            // EditTagsDialog should display better-formatted title
            liveGridEntry.TryGetFormatted(nameof(liveGridEntry.Title), out string value);

            var editTagsForm = new EditTagsDialog(value, liveGridEntry.Tags);
            if (editTagsForm.ShowDialog() != DialogResult.OK)
                return;

            var qtyChanges = saveChangedTags(liveGridEntry.GetBook(), editTagsForm.NewTags);
            if (qtyChanges == 0)
                return;

            // force a re-draw, and re-apply filters

            // needed to update text colors
            dataGridView.InvalidateRow(e.RowIndex);

            Filter();
        }

        private static int saveChangedTags(Book book, string newTags)
        {
            book.UserDefinedItem.Tags = newTags;

            var qtyChanges = ScrapingDomainServices.Indexer.IndexChangedTags(book);
            return qtyChanges;
        }

        #region Cell Formatting
        private void replaceFormatted(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var col = ((DataGridView)sender).Columns[e.ColumnIndex];
            if (col is DataGridViewTextBoxColumn textCol && getGridEntry(e.RowIndex).TryGetFormatted(textCol.Name, out string value))
                e.Value = value;
        }

        private void hiddenFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var isHidden = getGridEntry(e.RowIndex).TagsEnumerated.Contains("hidden");

            dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Style
                = isHidden
                ? new DataGridViewCellStyle { ForeColor = Color.LightGray }
                : dataGridView.DefaultCellStyle;
        }
        #endregion

        public void UpdateRow(string productId)
        {
            for (var r = dataGridView.RowCount - 1; r >= 0; r--)
            {
                var gridEntry = getGridEntry(r);
                if (gridEntry.GetBook().AudibleProductId == productId)
                {
                    var libBook = LibraryQueries.GetLibraryBook_Flat_NoTracking(productId);
                    gridEntry.REPLACE_Library_Book(libBook);
                    dataGridView.InvalidateRow(r);

                    return;
                }
            }
        }

        #region filter
        string _filterSearchString;
        public void Filter() => Filter(_filterSearchString);
        public void Filter(string searchString)
        {
            _filterSearchString = searchString;

            var searchResults = new LibationSearchEngine.SearchEngine().Search(searchString);
            var productIds = searchResults.Docs.Select(d => d.ProductId).ToList();

            // https://stackoverflow.com/a/18942430
            var currencyManager = (CurrencyManager)BindingContext[dataGridView.DataSource];
            currencyManager.SuspendBinding();
            {
                for (var r = dataGridView.RowCount - 1; r >= 0; r--)
                    dataGridView.Rows[r].Visible = productIds.Contains(getGridEntry(r).GetBook().AudibleProductId);
            }
            currencyManager.ResumeBinding();


            // after applying filters, display new visible count
            parent.SetVisibleCount(dataGridView.Rows.Cast<DataGridViewRow>().Count(r => r.Visible), searchResults.SearchString);
        }
        #endregion

        private GridEntry getGridEntry(int rowIndex) => (GridEntry)dataGridView.Rows[rowIndex].DataBoundItem;
    }
}
