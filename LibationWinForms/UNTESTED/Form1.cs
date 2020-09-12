using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Drawing;
using Dinah.Core.Windows.Forms;
using FileManager;
using InternalUtilities;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
    public partial class Form1 : Form
    {
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

        private void Form1_Load(object sender, EventArgs e)
		{
			// load default/missing cover images. this will also initiate the background image downloader
			var format = System.Drawing.Imaging.ImageFormat.Jpeg;
			PictureStorage.SetDefaultImage(PictureSize._80x80, Properties.Resources.default_cover_80x80.ToBytes(format));
			PictureStorage.SetDefaultImage(PictureSize._300x300, Properties.Resources.default_cover_300x300.ToBytes(format));
			PictureStorage.SetDefaultImage(PictureSize._500x500, Properties.Resources.default_cover_500x500.ToBytes(format));

            RefreshImportMenu();

			setVisibleCount(null, 0);

			reloadGrid();

            // also applies filter. ONLY call AFTER loading grid
            loadInitialQuickFilterState();

            // init bottom counts
            backupsCountsLbl.Text = "[Calculating backed up book quantities]";
            pdfsCountsLbl.Text = "[Calculating backed up PDFs]";
            setBackupCounts(null, null);
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
                    currProductsGrid.BackupCountsChanged -= setBackupCounts;
                    currProductsGrid.Dispose();
                }

                currProductsGrid = new ProductsGrid { Dock = DockStyle.Fill };
                currProductsGrid.VisibleCountChanged += setVisibleCount;
                currProductsGrid.BackupCountsChanged += setBackupCounts;
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
        private void setBackupCounts(object _, object __)
        {
            var books = DbContexts.GetContext()
                .GetLibrary_Flat_NoTracking()
                .Select(sp => sp.Book)
                .ToList();

			setBookBackupCounts(books);
			setPdfBackupCounts(books);
		}
        enum AudioFileState { full, aax, none }
        private void setBookBackupCounts(IEnumerable<Book> books)
        {
            static AudioFileState getAudioFileState(string productId)
            {
                if (AudibleFileStorage.Audio.Exists(productId))
                    return AudioFileState.full;
                if (AudibleFileStorage.AAX.Exists(productId))
                    return AudioFileState.aax;
                return AudioFileState.none;
			}

			var results = books
				.AsParallel()
				.Select(b => getAudioFileState(b.AudibleProductId))
				.ToList();
			var fullyBackedUp = results.Count(r => r == AudioFileState.full);
			var downloadedOnly = results.Count(r => r == AudioFileState.aax);
			var noProgress = results.Count(r => r == AudioFileState.none);

            // enable/disable export
            exportLibraryToolStripMenuItem.Enabled = results.Any();

            // update bottom numbers
            var pending = noProgress + downloadedOnly;
            var statusStripText
                = !results.Any() ? "No books. Begin by importing your library"
                : pending > 0 ? string.Format(backupsCountsLbl_Format, noProgress, downloadedOnly, fullyBackedUp)
                : $"All {"book".PluralizeWithCount(fullyBackedUp)} backed up";

            // update menu item
            var menuItemText
                = pending > 0
                ? $"{pending} remaining"
                : "All books have been liberated";

            Serilog.Log.Logger.Information("Book counts. {@DebugInfo}", new { fullyBackedUp, downloadedOnly, noProgress, pending, statusStripText, menuItemText });

            // update UI
            statusStrip1.UIThread(() => backupsCountsLbl.Text = statusStripText);
            menuStrip1.UIThread(() => beginBookBackupsToolStripMenuItem.Enabled = pending > 0);
            menuStrip1.UIThread(() => beginBookBackupsToolStripMenuItem.Text = string.Format(beginBookBackupsToolStripMenuItem_format, menuItemText));
        }
        private void setPdfBackupCounts(IEnumerable<Book> books)
        {
            var boolResults = books
				.AsParallel()
				.Where(b => b.Supplements.Any())
				.Select(b => AudibleFileStorage.PDF.Exists(b.AudibleProductId))
				.ToList();
            var downloaded = boolResults.Count(r => r);
            var notDownloaded = boolResults.Count(r => !r);

            // update bottom numbers
            var statusStripText
                = !boolResults.Any() ? ""
                : notDownloaded > 0 ? string.Format(pdfsCountsLbl_Format, notDownloaded, downloaded)
                : $"|  All {downloaded} PDFs downloaded";

            // update menu item
            var menuItemText
                = notDownloaded > 0
                ? $"{notDownloaded} remaining"
                : "All PDFs have been downloaded";

            Serilog.Log.Logger.Information("PDF counts. {@DebugInfo}", new { downloaded, notDownloaded, statusStripText, menuItemText });

            // update UI
            statusStrip1.UIThread(() => pdfsCountsLbl.Text = statusStripText);
            menuStrip1.UIThread(() => beginPdfBackupsToolStripMenuItem.Enabled = notDownloaded > 0);
            menuStrip1.UIThread(() => beginPdfBackupsToolStripMenuItem.Text = string.Format(beginPdfBackupsToolStripMenuItem_format, menuItemText));
        }
        #endregion

        #region filter
        private void filterHelpBtn_Click(object sender, EventArgs e) => new SearchSyntaxDialog().ShowDialog();

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

        #region Import menu
        public void RefreshImportMenu()
        {
            using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
            var count = persister.AccountsSettings.Accounts.Count;

            noAccountsYetAddAccountToolStripMenuItem.Visible = count == 0;
            scanLibraryToolStripMenuItem.Visible = count == 1;
            scanLibraryOfAllAccountsToolStripMenuItem.Visible = count > 1;
            scanLibraryOfSomeAccountsToolStripMenuItem.Visible = count > 1;
        }

        private void noAccountsYetAddAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("To load your Audible library, come back here to the Import menu after adding your account");
            new AccountsDialog(this).ShowDialog();
        }

        private void scanLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
            var firstAccount = persister.AccountsSettings.GetAll().FirstOrDefault();
            scanLibraries(firstAccount);
        }

        private void scanLibraryOfAllAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
            var allAccounts = persister.AccountsSettings.GetAll();
            scanLibraries(allAccounts);
        }

        private void scanLibraryOfSomeAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var scanAccountsDialog = new ScanAccountsDialog(this);

            if (scanAccountsDialog.ShowDialog() != DialogResult.OK)
                return;

            if (!scanAccountsDialog.CheckedAccounts.Any())
                return;

            scanLibraries(scanAccountsDialog.CheckedAccounts);
        }

        private void scanLibraries(IEnumerable<Account> accounts) => scanLibraries(accounts.ToArray());
        private void scanLibraries(params Account[] accounts)
		{
			using var dialog = new IndexLibraryDialog(accounts);
			dialog.ShowDialog();

			var totalProcessed = dialog.TotalBooksProcessed;
			var newAdded = dialog.NewBooksAdded;

			MessageBox.Show($"Total processed: {totalProcessed}\r\nNew: {newAdded}");

			if (totalProcessed > 0)
				reloadGrid();
        }
        #endregion

        #region liberate menu
        private async void beginBookBackupsToolStripMenuItem_Click(object sender, EventArgs e)
            => await BookLiberation.ProcessorAutomationController.BackupAllBooksAsync(updateGridRow);

        private async void beginPdfBackupsToolStripMenuItem_Click(object sender, EventArgs e)
            => await BookLiberation.ProcessorAutomationController.BackupAllPdfsAsync(updateGridRow);

        private void updateGridRow(object _, LibraryBook libraryBook) => currProductsGrid.RefreshRow(libraryBook.Book.AudibleProductId);
        #endregion

        #region Export menu
        private void exportLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Where to export Library",
                    Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV files (*.csv)|*.csv|JSON files (*.json)|*.json" // + "|All files (*.*)|*.*"
                };

                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                // FilterIndex is 1-based, NOT 0-based
                switch (saveFileDialog.FilterIndex)
                {
                    case 1: // xlsx
                    default:
                        LibraryExporter.ToXlsx(saveFileDialog.FileName);
                        break;
                    case 2: // csv
                        LibraryExporter.ToCsv(saveFileDialog.FileName);
                        break;
                    case 3: // json
                        LibraryExporter.ToJson(saveFileDialog.FileName);
                        break;
                }

                MessageBox.Show("Library exported to:\r\n" + saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error attempting to export library");
                MessageBox.Show("Error attempting to export your library. Error message:\r\n\r\n" + ex.Message, "Error exporting", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private void EditQuickFiltersToolStripMenuItem_Click(object sender, EventArgs e) => new EditQuickFilters(this).ShowDialog();
        #endregion

        #region settings menu
        private void accountsToolStripMenuItem_Click(object sender, EventArgs e) => new AccountsDialog(this).ShowDialog();

        private void basicSettingsToolStripMenuItem_Click(object sender, EventArgs e) => new SettingsDialog().ShowDialog();

        private void advancedSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var oldLocation = Configuration.Instance.LibationFiles;
            new LibationFilesDialog().ShowDialog();

            // no change
            if (System.IO.Path.GetFullPath(oldLocation).EqualsInsensitive(System.IO.Path.GetFullPath(Configuration.Instance.LibationFiles)))
                return;

            MessageBox.Show(
                "You have changed a file path important for this program. All files will remain in their original location; nothing will be moved. Libation must be restarted so these changes are handled correctly.",
                "Closing Libation",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
            Application.Exit();
            Environment.Exit(0);
        }
        #endregion
    }
}
