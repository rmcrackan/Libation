using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dinah.Core.DataBinding;
using DataLayer;
using Dinah.Core.Collections.Generic;

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
		public event EventHandler<int> VisibleCountChanged;

		private DataGridView dataGridView;

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
            dataGridView.Sorted += (_, __) => filter();

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

				col.Width = col.Name switch
				{
					nameof(GridEntry.Cover) => 80,
					nameof(GridEntry.Title) => col.Width * 2,
					nameof(GridEntry.Misc) => (int)(col.Width * 1.35),
					var n when n.In(nameof(GridEntry.My_Rating), nameof(GridEntry.Product_Rating)) => col.Width + 8,
					_ => col.Width
				};
            }


            //
            // transform into sorted GridEntry.s BEFORE binding
            //
            var lib = LibraryQueries.GetLibrary_Flat_NoTracking();

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

            filter();
        }

        private static int saveChangedTags(Book book, string newTags)
        {
            book.UserDefinedItem.Tags = newTags;

            var qtyChanges = ApplicationServices.TagUpdater.IndexChangedTags(book);
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
        private void filter() => Filter(_filterSearchString);
        public void Filter(string searchString)
        {
            _filterSearchString = searchString;

			if (dataGridView.Rows.Count == 0)
				return;

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
			VisibleCountChanged?.Invoke(this, dataGridView.Rows.Cast<DataGridViewRow>().Count(r => r.Visible));

			var luceneSearchString_debug = searchResults.SearchString;
        }
        #endregion

        private GridEntry getGridEntry(int rowIndex) => (GridEntry)dataGridView.Rows[rowIndex].DataBoundItem;
    }
}
