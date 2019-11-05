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
using ScrapingDomainServices;

namespace LibationWinForm
{
    public partial class Form1 : Form
    {
        // initial call here will initiate config loading
        private Configuration config { get; } = Configuration.Instance;

        private string backupsCountsLbl_Format { get; }
        private string pdfsCountsLbl_Format { get; }
		private string visibleCountLbl_Format { get; }

		private string reimportMostRecentLibraryScanToolStripMenuItem_format { get; }
		private string beginImportingBookDetailsToolStripMenuItem_format { get; }

		private string beginBookBackupsToolStripMenuItem_format { get; }
		private string beginPdfBackupsToolStripMenuItem_format { get; }

		public Form1()
        {
            InitializeComponent();

            // back up string formats
            backupsCountsLbl_Format = backupsCountsLbl.Text;
            pdfsCountsLbl_Format = pdfsCountsLbl.Text;
            visibleCountLbl_Format = visibleCountLbl.Text;

            reimportMostRecentLibraryScanToolStripMenuItem_format = reimportMostRecentLibraryScanToolStripMenuItem.Text;
            beginImportingBookDetailsToolStripMenuItem_format = beginImportingBookDetailsToolStripMenuItem.Text;

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

        #region bottom: qty books visible
        public void SetVisibleCount(int qty, string str = null)
        {
            visibleCountLbl.Text = string.Format(visibleCountLbl_Format, qty);

            if (!string.IsNullOrWhiteSpace(str))
                visibleCountLbl.Text += " | " + str;
        }
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
                = !results.Any() ? "No books. Begin by indexing your library"
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
                    currProductsGrid.Dispose();
                }

                currProductsGrid = new ProductsGrid(this) { Dock = DockStyle.Fill };
                gridPanel.Controls.Add(currProductsGrid);
                currProductsGrid.Display();
            }
            ResumeLayout();
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
                filterSearchTb.Text = lastGoodFilter;
                doFilter();
            }
        }
        #endregion

        #region index menu
        //
        // IMPORTANT
        //
        // IRunnableDialog.Run() extension method contains work flow
        //
        #region // example code: chaining multiple dialogs
        public class MyDialog1 : IRunnableDialog
        {
            public IEnumerable<string> Files;

            public IButtonControl AcceptButton { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Control.ControlCollection Controls => throw new NotImplementedException();
            public string SuccessMessage => throw new NotImplementedException();
            public DialogResult DialogResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public void Close() => throw new NotImplementedException();
            public Task DoMainWorkAsync() => throw new NotImplementedException();
            public DialogResult ShowDialog() => throw new NotImplementedException();
            public string StringBasedValidate() => throw new NotImplementedException();
        }
        public class MyDialog2 : Form, IIndexLibraryDialog
        {
            public MyDialog2(IEnumerable<string> files) { }
            Button BeginFileImportBtn = new Button();

            public void Begin() => BeginFileImportBtn.PerformClick();

            public int TotalBooksProcessed => throw new NotImplementedException();
            public int NewBooksAdded => throw new NotImplementedException();
            public string SuccessMessage => throw new NotImplementedException();
            public Task DoMainWorkAsync() => throw new NotImplementedException();
            public string StringBasedValidate() => throw new NotImplementedException();
        }
        private async void downloadPagesToFile(object sender, EventArgs e)
        {
            var dialog1 = new MyDialog1();
            if (dialog1.RunDialog() != DialogResult.OK || !dialog1.Files.Any())
                return;

            if (MessageBox.Show("Index from these files?", "Index?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var dialog2  = new MyDialog2(dialog1.Files);
                dialog2.Shown += (_, __) => dialog2.Begin();
                await indexDialog(dialog2);
            }
        }
        #endregion

        private void indexToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            #region label: Re-import most recent library scan
            {
                var libDir = WebpageStorage.GetMostRecentLibraryDir();
                if (libDir == null)
                {
                    reimportMostRecentLibraryScanToolStripMenuItem.Enabled = false;
                    reimportMostRecentLibraryScanToolStripMenuItem.Text = string.Format(reimportMostRecentLibraryScanToolStripMenuItem_format, "No previous scans");
                }
                else
                {
                    reimportMostRecentLibraryScanToolStripMenuItem.Enabled = true;

                    var now = DateTime.Now;
                    var span = now - libDir.CreationTime;
                    var ago
                        // less than 1 min
                        = (int)span.TotalSeconds < 60 ? $"{(int)span.TotalSeconds} sec ago"
                        // less than 1 hr
                        : (int)span.TotalMinutes < 60 ? $"{(int)span.TotalMinutes} min ago"
                        // today. eg: 4:25 PM
                        : now.Date == libDir.CreationTime.Date ? libDir.CreationTime.ToString("h:mm tt")
                        // else date and time
                        : libDir.CreationTime.ToString("MM/dd/yyyy h:mm tt");
                    reimportMostRecentLibraryScanToolStripMenuItem.Text = string.Format(reimportMostRecentLibraryScanToolStripMenuItem_format, ago);
                }
            }
            #endregion

            #region label: Begin importing book details
            {
                var noDetails = BookQueries.BooksWithoutDetailsCount();
                if (noDetails == 0)
                {
                    beginImportingBookDetailsToolStripMenuItem.Enabled = false;
                    beginImportingBookDetailsToolStripMenuItem.Text = string.Format(beginImportingBookDetailsToolStripMenuItem_format, "No books without details");
                }
                else
                {
                    beginImportingBookDetailsToolStripMenuItem.Enabled = true;
                    beginImportingBookDetailsToolStripMenuItem.Text = string.Format(beginImportingBookDetailsToolStripMenuItem_format, $"{noDetails} remaining");
                }
            }
            #endregion
        }

		private async void scanLibraryToolStripMenuItem_Click(object sender, EventArgs e)
		{
// legacy/scraping method
			//await indexDialog(new ScanLibraryDialog());
// new/api method
await indexDialog(new IndexLibraryDialog());
		}

		private async void reimportMostRecentLibraryScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // DO NOT ConfigureAwait(false)
            // this would result in index() => reloadGrid() => setGrid() => "gridPanel.Controls.Remove(currProductsGrid);"
            // throwing 'Cross-thread operation not valid: Control 'ProductsGrid' accessed from a thread other than the thread it was created on.'
            var (TotalBooksProcessed, NewBooksAdded) = await Indexer.IndexLibraryAsync(WebpageStorage.GetMostRecentLibraryDir());

            MessageBox.Show($"Total processed: {TotalBooksProcessed}\r\nNew: {NewBooksAdded}");

            await indexComplete(TotalBooksProcessed, NewBooksAdded);
        }

        private async Task indexDialog(IIndexLibraryDialog dialog)
        {
            if (!dialog.RunDialog().In(DialogResult.Abort, DialogResult.Cancel, DialogResult.None))
                await indexComplete(dialog.TotalBooksProcessed, dialog.NewBooksAdded);
        }
        private async Task indexComplete(int totalBooksProcessed, int newBooksAdded)
        {
            // update backup counts if we have new library items
            if (newBooksAdded > 0)
                await setBackupCountsAsync();

            // skip reload if:
            // - no grid is loaded
            // - none indexed
            if (currProductsGrid == null || totalBooksProcessed == 0)
                return;

            reloadGrid();
        }

        private void updateGridRow(object _, string productId) => currProductsGrid?.UpdateRow(productId);

        private async void beginImportingBookDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var scrapeBookDetails = BookLiberation.ProcessorAutomationController.GetWiredUpScrapeBookDetails();
            scrapeBookDetails.BookSuccessfullyImported += updateGridRow;
            await BookLiberation.ProcessorAutomationController.RunAutomaticDownload(scrapeBookDetails);
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
