using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Collections.Generic;
using Dinah.Core.Windows.Forms;
using FileManager;

namespace LibationWinForm
{
    public partial class Form1 : Form
    {
        // initial call here will initiate config loading
        private Configuration config { get; } = Configuration.Instance;

        private string backupsCountsLbl_Format { get; }
        private string pdfsCountsLbl_Format { get; }
		private string visibleCountLbl_Format { get; }

		private string beginBookBackupsToolStripMenuItem_format { get; }
		private string beginPdfBackupsToolStripMenuItem_format { get; }

		public Form1()
        {
            InitializeComponent();

            // back up string formats
            backupsCountsLbl_Format = backupsCountsLbl.Text;
            pdfsCountsLbl_Format = pdfsCountsLbl.Text;
            visibleCountLbl_Format = visibleCountLbl.Text;

            beginBookBackupsToolStripMenuItem_format = beginBookBackupsToolStripMenuItem.Text;
            beginPdfBackupsToolStripMenuItem_format = beginPdfBackupsToolStripMenuItem.Text;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // call static ctor. There are bad race conditions if static ctor is first executed when we're running in parallel in setBackupCountsAsync()
            var foo = FilePathCache.JsonFile;

            reloadGrid();

            // also applies filter. ONLY call AFTER loading grid
            loadInitialQuickFilterState();

            { // init bottom counts
                backupsCountsLbl.Text = "[Calculating backed up book quantities]";
                pdfsCountsLbl.Text = "[Calculating backed up PDFs]";

                await setBackupCountsAsync();
            }
        }

        #region reload grid
        bool isProcessingGridSelect = false;
        private void reloadGrid()
        {
            // suppressed filter while init'ing UI
            var prev_isProcessingGridSelect = isProcessingGridSelect;
            isProcessingGridSelect = true;
            setGrid();
            isProcessingGridSelect = prev_isProcessingGridSelect;

            // UI init complete. now we can apply filter
            doFilter(lastGoodFilter);
        }

        ProductsGrid currProductsGrid;
        private void setGrid()
        {
            SuspendLayout();
            {
                if (currProductsGrid != null)
                {
                    gridPanel.Controls.Remove(currProductsGrid);
                    currProductsGrid.VisibleCountChanged -= setVisibleCount;
                    currProductsGrid.Dispose();
                }

                currProductsGrid = new ProductsGrid { Dock = DockStyle.Fill };
                currProductsGrid.VisibleCountChanged += setVisibleCount;
                gridPanel.Controls.Add(currProductsGrid);
                currProductsGrid.Display();
            }
            ResumeLayout();
        }
		#endregion

        #region bottom: qty books visible
        private void setVisibleCount(object _, int qty) => visibleCountLbl.Text = string.Format(visibleCountLbl_Format, qty);
		#endregion

		#region bottom: backup counts
		private async Task setBackupCountsAsync()
        {
            var books = LibraryQueries.GetLibrary_Flat_NoTracking()
                .Select(sp => sp.Book)
                .ToList();

            await setBookBackupCountsAsync(books).ConfigureAwait(false);
            await setPdfBackupCountsAsync(books).ConfigureAwait(false);
        }
        enum AudioFileState { full, aax, none }
        private async Task setBookBackupCountsAsync(IEnumerable<Book> books)
        {
            var libraryProductIds = books
                .Select(b => b.AudibleProductId)
                .ToList();

            var noProgress = 0;
            var downloadedOnly = 0;
            var fullyBackedUp = 0;


            //// serial
            //foreach (var productId in libraryProductIds)
            //{
            //    if (await AudibleFileStorage.Audio.ExistsAsync(productId))
            //        fullyBackedUp++;
            //    else if (await AudibleFileStorage.AAX.ExistsAsync(productId))
            //        downloadedOnly++;
            //    else
            //        noProgress++;
            //}

            // parallel
            async Task<AudioFileState> getAudioFileStateAsync(string productId)
            {
                if (await AudibleFileStorage.Audio.ExistsAsync(productId))
                    return AudioFileState.full;
                if (await AudibleFileStorage.AAX.ExistsAsync(productId))
                    return AudioFileState.aax;
                return AudioFileState.none;
            }
            var tasks = libraryProductIds.Select(productId => getAudioFileStateAsync(productId));
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            fullyBackedUp = results.Count(r => r == AudioFileState.full);
            downloadedOnly = results.Count(r => r == AudioFileState.aax);
            noProgress = results.Count(r => r == AudioFileState.none);

            // update bottom numbers
            var pending = noProgress + downloadedOnly;
            var text
                = !results.Any() ? "No books. Begin by importing your library"
                : pending > 0 ? string.Format(backupsCountsLbl_Format, noProgress, downloadedOnly, fullyBackedUp)
                : $"All {"book".PluralizeWithCount(fullyBackedUp)} backed up";
            statusStrip1.UIThread(() => backupsCountsLbl.Text = text);

            // update menu item
            var menuItemText
                = pending > 0
                ? $"{pending} remaining"
                : "All books have been liberated";
            menuStrip1.UIThread(() => beginBookBackupsToolStripMenuItem.Enabled = pending > 0);
            menuStrip1.UIThread(() => beginBookBackupsToolStripMenuItem.Text = string.Format(beginBookBackupsToolStripMenuItem_format, menuItemText));
        }
        private async Task setPdfBackupCountsAsync(IEnumerable<Book> books)
        {
            var libraryProductIds = books
                .Where(b => b.Supplements.Any())
                .Select(b => b.AudibleProductId)
                .ToList();

            int notDownloaded;
            int downloaded;

            //// serial
            //notDownloaded = 0;
            //downloaded = 0;
            //foreach (var productId in libraryProductIds)
            //{
            //    if (await AudibleFileStorage.PDF.ExistsAsync(productId))
            //        downloaded++;
            //    else
            //        notDownloaded++;
            //}

            // parallel
            var tasks = libraryProductIds.Select(productId => AudibleFileStorage.PDF.ExistsAsync(productId));
            var boolResults = await Task.WhenAll(tasks).ConfigureAwait(false);
            downloaded = boolResults.Count(r => r);
            notDownloaded = boolResults.Count(r => !r);

            // update bottom numbers
            var text
                = !boolResults.Any() ? ""
                : notDownloaded > 0 ? string.Format(pdfsCountsLbl_Format, notDownloaded, downloaded)
                : $"|  All {downloaded} PDFs downloaded";
            statusStrip1.UIThread(() => pdfsCountsLbl.Text = text);

            // update menu item
            var menuItemText
                = notDownloaded > 0
                ? $"{notDownloaded} remaining"
                : "All PDFs have been downloaded";
            menuStrip1.UIThread(() => beginPdfBackupsToolStripMenuItem.Enabled = notDownloaded > 0);
            menuStrip1.UIThread(() => beginPdfBackupsToolStripMenuItem.Text = string.Format(beginPdfBackupsToolStripMenuItem_format, menuItemText));
        }
        #endregion

		#region filter
		private void filterHelpBtn_Click(object sender, EventArgs e) => new Dialogs.SearchSyntaxDialog().ShowDialog();

        private void AddFilterBtn_Click(object sender, EventArgs e)
        {
            QuickFilters.Add(this.filterSearchTb.Text);
            UpdateFilterDropDown();
        }

        private void filterSearchTb_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                doFilter();

                // silence the 'ding'
                e.Handled = true;
            }
        }
        private void filterBtn_Click(object sender, EventArgs e) => doFilter();

        string lastGoodFilter = "";
        private void doFilter(string filterString)
        {
            this.filterSearchTb.Text = filterString;
            doFilter();
        }
        private void doFilter()
        {
            if (isProcessingGridSelect || currProductsGrid == null)
                return;

            try
            {
                currProductsGrid.Filter(filterSearchTb.Text);
                lastGoodFilter = filterSearchTb.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bad filter string:\r\n\r\n{ex.Message}", "Bad filter string", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // re-apply last good filter
                doFilter(lastGoodFilter);
            }
        }
		#endregion

		#region index menu
		private async void scanLibraryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var dialog = new IndexLibraryDialog();
			dialog.ShowDialog();

			var totalProcessed = dialog.TotalBooksProcessed;
			var newAdded = dialog.NewBooksAdded;

			MessageBox.Show($"Total processed: {totalProcessed}\r\nNew: {newAdded}");

			// update backup counts if we have new library items
			if (newAdded > 0)
				await setBackupCountsAsync();

			if (totalProcessed > 0)
				reloadGrid();
		}
		#endregion

		#region liberate menu
		private async void setBackupCountsAsync(object _, string __) => await setBackupCountsAsync();

        private async void beginBookBackupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var backupBook = BookLiberation.ProcessorAutomationController.GetWiredUpBackupBook();
            backupBook.Download.Completed += setBackupCountsAsync;
            backupBook.Decrypt.Completed += setBackupCountsAsync;
            await BookLiberation.ProcessorAutomationController.RunAutomaticBackup(backupBook);
        }

        private async void beginPdfBackupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var downloadPdf = BookLiberation.ProcessorAutomationController.GetWiredUpDownloadPdf();
            downloadPdf.Completed += setBackupCountsAsync;
            await BookLiberation.ProcessorAutomationController.RunAutomaticDownload(downloadPdf);
        }
        #endregion

        #region quick filters menu
        private void loadInitialQuickFilterState()
        {
            // set inital state. do once only
            firstFilterIsDefaultToolStripMenuItem.Checked = QuickFilters.UseDefault;

            // load default filter. do once only
            if (QuickFilters.UseDefault)
                doFilter(QuickFilters.Filters.FirstOrDefault());

            // do after every save
            UpdateFilterDropDown();
        }

        private void FirstFilterIsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            firstFilterIsDefaultToolStripMenuItem.Checked = !firstFilterIsDefaultToolStripMenuItem.Checked;
            QuickFilters.UseDefault = firstFilterIsDefaultToolStripMenuItem.Checked;
        }

        object quickFilterTag { get; } = new object();
        public void UpdateFilterDropDown()
        {
            // remove old
            for (var i = quickFiltersToolStripMenuItem.DropDownItems.Count - 1; i >= 0; i--)
            {
                var menuItem = quickFiltersToolStripMenuItem.DropDownItems[i];
                if (menuItem.Tag == quickFilterTag)
                    quickFiltersToolStripMenuItem.DropDownItems.Remove(menuItem);
            }

            // re-populate
            var index = 0;
            foreach (var filter in QuickFilters.Filters)
            {
                var menuItem = new ToolStripMenuItem
                {
                    Tag = quickFilterTag,
                    Text = $"&{++index}: {filter}"
                };
                menuItem.Click += (_, __) => doFilter(filter);
                quickFiltersToolStripMenuItem.DropDownItems.Add(menuItem);
            }
        }

        private void EditQuickFiltersToolStripMenuItem_Click(object sender, EventArgs e) => new Dialogs.EditQuickFilters(this).ShowDialog();
        #endregion

        #region settings menu item
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e) => new SettingsDialog().ShowDialog();
        #endregion
    }
}
