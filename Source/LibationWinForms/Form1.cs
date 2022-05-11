using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using AudibleUtilities;
using Dinah.Core;
using Dinah.Core.Drawing;
using Dinah.Core.Threading;
using LibationFileManager;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
	public partial class Form1 : Form
	{
		private string beginBookBackupsToolStripMenuItem_format { get; }
		private string beginPdfBackupsToolStripMenuItem_format { get; }

		private ProductsGrid productsGrid { get; }

		public Form1()
		{
			InitializeComponent();

			productsGrid = new ProductsGrid { Dock = DockStyle.Fill };
			gridPanel.Controls.Add(productsGrid);
			productsGrid.VisibleCountChanged += setVisibleCount;

			// back up string formats
			beginBookBackupsToolStripMenuItem_format = beginBookBackupsToolStripMenuItem.Text;
			beginPdfBackupsToolStripMenuItem_format = beginPdfBackupsToolStripMenuItem.Text;

			if (this.DesignMode)
				return;

			// independent UI updates
			this.Load += (_, _) => this.RestoreSizeAndLocation(Configuration.Instance);
			this.FormClosing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);
			LibraryCommands.LibrarySizeChanged += reloadGridAndUpdateBottomNumbers;
			LibraryCommands.BookUserDefinedItemCommitted += setBackupCounts;
            QuickFilters.Updated += updateFiltersMenu;
            LibraryCommands.ScanBegin += LibraryCommands_ScanBegin;
            LibraryCommands.ScanEnd += LibraryCommands_ScanEnd;

			// accounts updated
			this.Load += refreshImportMenu;
			AccountsSettingsPersister.Saved += refreshImportMenu;

			configAndInitAutoScan();

			configVisibleBooksMenu();

			// init default/placeholder cover art
			var format = System.Drawing.Imaging.ImageFormat.Jpeg;
			PictureStorage.SetDefaultImage(PictureSize._80x80, Properties.Resources.default_cover_80x80.ToBytes(format));
			PictureStorage.SetDefaultImage(PictureSize._300x300, Properties.Resources.default_cover_300x300.ToBytes(format));
			PictureStorage.SetDefaultImage(PictureSize._500x500, Properties.Resources.default_cover_500x500.ToBytes(format));
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			// can't refactor into "this.Load => reloadGridAndUpdateBottomNumbers"
			// because loadInitialQuickFilterState must follow it
			reloadGridAndUpdateBottomNumbers();

			// also applies filter. ONLY call AFTER loading grid
			loadInitialQuickFilterState();
		}

		private void reloadGridAndUpdateBottomNumbers(object _ = null, object __ = null)
		{
			// suppressed filter while init'ing UI
			var prev_isProcessingGridSelect = isProcessingGridSelect;
			isProcessingGridSelect = true;
			this.UIThreadSync(() => productsGrid.Display());
			isProcessingGridSelect = prev_isProcessingGridSelect;

			// UI init complete. now we can apply filter
			this.UIThreadAsync(() => doFilter(lastGoodFilter));

			setBackupCounts();
        }

		#region bottom: qty books visible
		private void setVisibleCount(object _, int qty) => visibleCountLbl.Text = string.Format("Visible: {0}", qty);
		#endregion

		#region bottom: backup counts
		private System.ComponentModel.BackgroundWorker updateCountsBw;
		private bool runBackupCountsAgain;

		private void setBackupCounts(object _ = null, object __ = null)
		{
			runBackupCountsAgain = true;

			if (updateCountsBw is not null)
				return;

			updateCountsBw = new System.ComponentModel.BackgroundWorker();
			updateCountsBw.DoWork += UpdateCountsBw_DoWork;
			updateCountsBw.RunWorkerCompleted += UpdateCountsBw_RunWorkerCompleted;
			updateCountsBw.RunWorkerAsync();
		}

		private void UpdateCountsBw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			while (runBackupCountsAgain)
			{
				runBackupCountsAgain = false;

				var libraryStats = LibraryCommands.GetCounts();
				e.Result = libraryStats;
			}
			updateCountsBw = null;
		}

		private void UpdateCountsBw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			var libraryStats = e.Result as LibraryCommands.LibraryStats;

			setBookBackupCounts(libraryStats);
			setPdfBackupCounts(libraryStats);
		}

		private void setBookBackupCounts(LibraryCommands.LibraryStats libraryStats)
		{
			var backupsCountsLbl_Format = "BACKUPS: No progress: {0}  In process: {1}  Fully backed up: {2}";

			// enable/disable export
			var hasResults = 0 < (libraryStats.booksFullyBackedUp + libraryStats.booksDownloadedOnly + libraryStats.booksNoProgress + libraryStats.booksError);
			exportLibraryToolStripMenuItem.Enabled = hasResults;

			// update bottom numbers
			var pending = libraryStats.booksNoProgress + libraryStats.booksDownloadedOnly;
			var statusStripText
				= !hasResults ? "No books. Begin by importing your library"
				: libraryStats.booksError > 0 ? string.Format(backupsCountsLbl_Format + "  Errors: {3}", libraryStats.booksNoProgress, libraryStats.booksDownloadedOnly, libraryStats.booksFullyBackedUp, libraryStats.booksError)
				: pending > 0 ? string.Format(backupsCountsLbl_Format, libraryStats.booksNoProgress, libraryStats.booksDownloadedOnly, libraryStats.booksFullyBackedUp)
				: $"All {"book".PluralizeWithCount(libraryStats.booksFullyBackedUp)} backed up";

			// update menu item
			var menuItemText
				= pending > 0
				? $"{pending} remaining"
				: "All books have been liberated";

			// update UI
			statusStrip1.UIThreadAsync(() => backupsCountsLbl.Text = statusStripText);
			menuStrip1.UIThreadAsync(() => beginBookBackupsToolStripMenuItem.Enabled = pending > 0);
			menuStrip1.UIThreadAsync(() => beginBookBackupsToolStripMenuItem.Text = string.Format(beginBookBackupsToolStripMenuItem_format, menuItemText));
		}
		private void setPdfBackupCounts(LibraryCommands.LibraryStats libraryStats)
		{
			var pdfsCountsLbl_Format = "|  PDFs: NOT d/l\'ed: {0}  Downloaded: {1}";

			// update bottom numbers
			var hasResults = 0 < (libraryStats.pdfsNotDownloaded + libraryStats.pdfsDownloaded);
			var statusStripText
				= !hasResults ? ""
				: libraryStats.pdfsNotDownloaded > 0 ? string.Format(pdfsCountsLbl_Format, libraryStats.pdfsNotDownloaded, libraryStats.pdfsDownloaded)
				: $"|  All {libraryStats.pdfsDownloaded} PDFs downloaded";

			// update menu item
			var menuItemText
				= libraryStats.pdfsNotDownloaded > 0
				? $"{libraryStats.pdfsNotDownloaded} remaining"
				: "All PDFs have been downloaded";

			// update UI
			statusStrip1.UIThreadAsync(() => pdfsCountsLbl.Text = statusStripText);
			menuStrip1.UIThreadAsync(() => beginPdfBackupsToolStripMenuItem.Enabled = libraryStats.pdfsNotDownloaded > 0);
			menuStrip1.UIThreadAsync(() => beginPdfBackupsToolStripMenuItem.Text = string.Format(beginPdfBackupsToolStripMenuItem_format, menuItemText));
		}
		#endregion

		#region filter
		private void filterHelpBtn_Click(object sender, EventArgs e) => new SearchSyntaxDialog().ShowDialog();

		private void AddFilterBtn_Click(object sender, EventArgs e) => QuickFilters.Add(this.filterSearchTb.Text);

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

		private bool isProcessingGridSelect = false;
		private string lastGoodFilter = "";
		private void doFilter(string filterString)
		{
			this.filterSearchTb.Text = filterString;
			doFilter();
		}
		private void doFilter()
		{
			if (isProcessingGridSelect || productsGrid is null)
				return;

			try
			{
				productsGrid.Filter(filterSearchTb.Text);
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

		#region Auto-scanner
		private InterruptableTimer autoScanTimer;

		private void configAndInitAutoScan()
		{
			var hours = 0;
			var minutes = 5;
			var seconds = 0;
			var _5_minutes = new TimeSpan(hours, minutes, seconds);
			autoScanTimer = new InterruptableTimer(_5_minutes);

			// subscribe as async/non-blocking. I'd actually rather prefer blocking but real-world testing found that caused a deadlock in the AudibleAPI
			autoScanTimer.Elapsed += async (_, __) =>
			{
				using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
				var accounts = persister.AccountsSettings
					.GetAll()
					.Where(a => a.LibraryScan)
					.ToArray();

				// in autoScan, new books SHALL NOT show dialog
				await Invoke(async () => await LibraryCommands.ImportAccountAsync(Login.WinformLoginChoiceEager.ApiExtendedFunc, accounts));
			};

			// load init state to menu checkbox
			this.Load += updateAutoScanLibraryToolStripMenuItem;
			// if enabled: begin on load
			this.Load += startAutoScan;

			// if new 'default' account is added, run autoscan
			AccountsSettingsPersister.Saving += accountsPreSave;
			AccountsSettingsPersister.Saved += accountsPostSave;

			// when autoscan setting is changed, update menu checkbox and run autoscan
			Configuration.Instance.AutoScanChanged += updateAutoScanLibraryToolStripMenuItem;
			Configuration.Instance.AutoScanChanged += startAutoScan;
		}

        private List<(string AccountId, string LocaleName)> preSaveDefaultAccounts;
		private List<(string AccountId, string LocaleName)> getDefaultAccounts()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			return persister.AccountsSettings
				.GetAll()
				.Where(a => a.LibraryScan)
				.Select(a => (a.AccountId, a.Locale.Name))
				.ToList();
		}
		private void accountsPreSave(object sender = null, EventArgs e = null)
			=> preSaveDefaultAccounts = getDefaultAccounts();
		private void accountsPostSave(object sender = null, EventArgs e = null)
		{
			var postSaveDefaultAccounts = getDefaultAccounts();
			var newDefaultAccounts = postSaveDefaultAccounts.Except(preSaveDefaultAccounts).ToList();

			if (newDefaultAccounts.Any())
				startAutoScan();
		}

		private void startAutoScan(object sender = null, EventArgs e = null)
		{
			if (Configuration.Instance.AutoScan)
				autoScanTimer.PerformNow();
			else
				autoScanTimer.Stop();
		}

		private void updateAutoScanLibraryToolStripMenuItem(object sender, EventArgs e) => autoScanLibraryToolStripMenuItem.Checked = Configuration.Instance.AutoScan;
		#endregion

		#region Import menu
		private void refreshImportMenu(object _ = null, EventArgs __ = null)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var count = persister.AccountsSettings.Accounts.Count;

			autoScanLibraryToolStripMenuItem.Visible = count > 0;

			noAccountsYetAddAccountToolStripMenuItem.Visible = count == 0;
			scanLibraryToolStripMenuItem.Visible = count == 1;
			scanLibraryOfAllAccountsToolStripMenuItem.Visible = count > 1;
			scanLibraryOfSomeAccountsToolStripMenuItem.Visible = count > 1;

			removeLibraryBooksToolStripMenuItem.Visible = count > 0;
			removeSomeAccountsToolStripMenuItem.Visible = count > 1;
			removeAllAccountsToolStripMenuItem.Visible = count > 1;
		}

		private void autoScanLibraryToolStripMenuItem_Click(object sender, EventArgs e) => Configuration.Instance.AutoScan = !autoScanLibraryToolStripMenuItem.Checked;

		private void noAccountsYetAddAccountToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("To load your Audible library, come back here to the Import menu after adding your account");
			new AccountsDialog(this).ShowDialog();
		}

		private async void scanLibraryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
            var firstAccount = persister.AccountsSettings.GetAll().FirstOrDefault();
            await scanLibrariesAsync(firstAccount);
        }

		private async void scanLibraryOfAllAccountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var allAccounts = persister.AccountsSettings.GetAll();
			await scanLibrariesAsync(allAccounts);
		}

		private async void scanLibraryOfSomeAccountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var scanAccountsDialog = new ScanAccountsDialog(this);

			if (scanAccountsDialog.ShowDialog() != DialogResult.OK)
				return;

			if (!scanAccountsDialog.CheckedAccounts.Any())
				return;

			await scanLibrariesAsync(scanAccountsDialog.CheckedAccounts);
		}

		private void removeLibraryBooksToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// if 0 accounts, this will not be visible
			// if 1 account, run scanLibrariesRemovedBooks() on this account
			// if multiple accounts, another menu set will open. do not run scanLibrariesRemovedBooks()
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var accounts = persister.AccountsSettings.GetAll();

			if (accounts.Count != 1)
				return;

			var firstAccount = accounts.Single();
			scanLibrariesRemovedBooks(firstAccount);
		}

		// selectively remove books from all accounts
		private void removeAllAccountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var allAccounts = persister.AccountsSettings.GetAll();
			scanLibrariesRemovedBooks(allAccounts.ToArray());
		}

		// selectively remove books from some accounts
		private void removeSomeAccountsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using var scanAccountsDialog = new ScanAccountsDialog(this);

			if (scanAccountsDialog.ShowDialog() != DialogResult.OK)
				return;

			if (!scanAccountsDialog.CheckedAccounts.Any())
				return;

			scanLibrariesRemovedBooks(scanAccountsDialog.CheckedAccounts.ToArray());
		}

		private void scanLibrariesRemovedBooks(params Account[] accounts)
		{
			using var dialog = new RemoveBooksDialog(accounts);
			dialog.ShowDialog();
		}

		private async Task scanLibrariesAsync(IEnumerable<Account> accounts) => await scanLibrariesAsync(accounts.ToArray());
		private async Task scanLibrariesAsync(params Account[] accounts)
		{
			try
			{
				var (totalProcessed, newAdded) = await LibraryCommands.ImportAccountAsync(Login.WinformLoginChoiceEager.ApiExtendedFunc, accounts);

				// this is here instead of ScanEnd so that the following is only possible when it's user-initiated, not automatic loop
				if (Configuration.Instance.ShowImportedStats && newAdded > 0)
					MessageBox.Show($"Total processed: {totalProcessed}\r\nNew: {newAdded}");
			}
			catch (Exception ex)
			{
				MessageBoxAlertAdmin.Show(
					"Error importing library. Please try again. If this still happens after 2 or 3 tries, stop and contact administrator",
					"Error importing library",
					ex);
			}
		}
		#endregion

		#region Liberate menu
		private async void beginBookBackupsToolStripMenuItem_Click(object sender, EventArgs e)
			=> await BookLiberation.ProcessorAutomationController.BackupAllBooksAsync();

		private async void beginPdfBackupsToolStripMenuItem_Click(object sender, EventArgs e)
			=> await BookLiberation.ProcessorAutomationController.BackupAllPdfsAsync();

		private async void convertAllM4bToMp3ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var result = MessageBox.Show(
				"This converts all m4b titles in your library to mp3 files. Original files are not deleted."
				+ "\r\nFor large libraries this will take a long time and will take up more disk space."
				+ "\r\n\r\nContinue?"
				+ "\r\n\r\n(To always download titles as mp3 instead of m4b, go to Settings: Download my books as .MP3 files)",
				"Convert all M4b => Mp3?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning);
			if (result == DialogResult.Yes)
				await BookLiberation.ProcessorAutomationController.ConvertAllBooksAsync();
		}

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
				MessageBoxAlertAdmin.Show("Error attempting to export your library.", "Error exporting", ex);
			}
		}
		#endregion

		#region Quick Filters menu
		private void FirstFilterIsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
		{
			firstFilterIsDefaultToolStripMenuItem.Checked = !firstFilterIsDefaultToolStripMenuItem.Checked;
			QuickFilters.UseDefault = firstFilterIsDefaultToolStripMenuItem.Checked;
		}

		private void loadInitialQuickFilterState()
		{
			// set inital state. do once only
			firstFilterIsDefaultToolStripMenuItem.Checked = QuickFilters.UseDefault;

			// load default filter. do once only
			if (QuickFilters.UseDefault)
				doFilter(QuickFilters.Filters.FirstOrDefault());

			updateFiltersMenu();
		}

		private object quickFilterTag { get; } = new object();
		private void updateFiltersMenu(object _ = null, object __ = null)
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

		#region Visible Books menu
		private void configVisibleBooksMenu()
        {

        }

		private async void liberateToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			var visibleBooks = productsGrid.GetVisible().ToList();
			await BookLiberation.ProcessorAutomationController.BackupAllBooksAsync(visibleBooks);
		}

		private void replaceTagsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var visibleLibraryBooks = productsGrid.GetVisible().ToList();
			foreach (var libraryBook in visibleLibraryBooks)
				libraryBook.Book.UserDefinedItem.Tags = "ggggg";
			LibraryCommands.UpdateUserDefinedItem(visibleLibraryBooks.Select(lb => lb.Book));
		}

		private void setDownloadedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var visibleLibraryBooks = productsGrid.GetVisible().ToList();
			foreach (var libraryBook in visibleLibraryBooks)
				libraryBook.Book.UserDefinedItem.BookStatus = DataLayer.LiberatedStatus.NotLiberated;
			LibraryCommands.UpdateUserDefinedItem(visibleLibraryBooks.Select(lb => lb.Book));
		}

		private async void removeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var visibleIds = productsGrid.GetVisible().Select(lb => lb.Book.AudibleProductId).ToList();
			await LibraryCommands.RemoveBooksAsync(visibleIds);
		}
		#endregion

		#region Settings menu
		private void accountsToolStripMenuItem_Click(object sender, EventArgs e) => new AccountsDialog(this).ShowDialog();

		private void basicSettingsToolStripMenuItem_Click(object sender, EventArgs e) => new SettingsDialog().ShowDialog();

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
			=> MessageBox.Show($"Running Libation version {AppScaffolding.LibationScaffolding.BuildVersion}", $"Libation v{AppScaffolding.LibationScaffolding.BuildVersion}");
        #endregion

        #region Scanning label
		private void LibraryCommands_ScanBegin(object sender, int accountsLength)
		{
			scanLibraryToolStripMenuItem.Enabled = false;
			scanLibraryOfAllAccountsToolStripMenuItem.Enabled = false;
			scanLibraryOfSomeAccountsToolStripMenuItem.Enabled = false;

			this.scanningToolStripMenuItem.Visible = true;
			this.scanningToolStripMenuItem.Text
				= (accountsLength == 1)
				? "Scanning..."
				: $"Scanning {accountsLength} accounts...";
		}

		private void LibraryCommands_ScanEnd(object sender, EventArgs e)
		{
			scanLibraryToolStripMenuItem.Enabled = true;
			scanLibraryOfAllAccountsToolStripMenuItem.Enabled = true;
			scanLibraryOfSomeAccountsToolStripMenuItem.Enabled = true;

			this.scanningToolStripMenuItem.Visible = false;
		}
        #endregion
    }
}
