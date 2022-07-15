using ApplicationServices;
using Dinah.Core;
using LibationFileManager;
using ReactiveUI;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public class MainWindowViewModel : ViewModelBase
	{
		private string _filterString;
		private string _removeBooksButtonText = "Remove # Books from Libation";
		private bool _removeBooksButtonEnabled = true;
		private bool _autoScanChecked = true;
		private bool _firstFilterIsDefault = true;
		private bool _removeButtonsVisible = true;
		private int _numAccountsScanning = 2;
		private int _accountsCount = 0;
		private bool _queueOpen = true;
		private int _visibleCount = 1;
		private LibraryCommands.LibraryStats _libraryStats;
		private int _visibleNotLiberated = 1;

		/// <summary> The Process Queue's viewmodel </summary>
		public ProcessQueueViewModel ProcessQueueViewModel { get; } = new ProcessQueueViewModel();


		/// <summary> Library filterting query </summary>
		public string FilterString { get => _filterString; set => this.RaiseAndSetIfChanged(ref _filterString, value); }


		/// <summary> Display text for the "Remove # Books from Libation" button </summary>
		public string RemoveBooksButtonText { get => _removeBooksButtonText; set => this.RaiseAndSetIfChanged(ref _removeBooksButtonText, value); }


		/// <summary> Indicates if the "Remove # Books from Libation" button is enabled </summary>
		public bool RemoveBooksButtonEnabled { get => _removeBooksButtonEnabled; set { this.RaiseAndSetIfChanged(ref _removeBooksButtonEnabled, value); } }


		/// <summary> Auto scanning accounts is enables </summary>
		public bool AutoScanChecked 
		{ 
			get => _autoScanChecked; 
			set 
			{ 
				if (value != _autoScanChecked)
					Configuration.Instance.AutoScan = value;
				this.RaiseAndSetIfChanged(ref _autoScanChecked, value);
			} 
		}


		/// <summary> Indicates if the first quick filter is the default filter </summary>
		public bool FirstFilterIsDefault
		{ 
			get => _firstFilterIsDefault; 
			set 
			{ 
				if (value != _firstFilterIsDefault)
					QuickFilters.UseDefault = value;
				this.RaiseAndSetIfChanged(ref _firstFilterIsDefault, value);
			}
		}


		/// <summary> Indicates if the "Remove # Books from Libation" and "Done Removing" buttons should be visible </summary>
		public bool RemoveButtonsVisible
		{
			get => _removeButtonsVisible;
			set
			{
				this.RaiseAndSetIfChanged(ref _removeButtonsVisible, value);
				this.RaisePropertyChanged(nameof(RemoveMenuItemsEnabled));
			}
		}




		/// <summary> The number of accounts currently being scanned </summary>
		public int NumAccountsScanning
		{
			get => _numAccountsScanning;
			set
			{
				this.RaiseAndSetIfChanged(ref _numAccountsScanning, value);
				this.RaisePropertyChanged(nameof(ActivelyScanning));
				this.RaisePropertyChanged(nameof(RemoveMenuItemsEnabled));
				this.RaisePropertyChanged(nameof(ScanningText));
			}
		}

		/// <summary> Indicates if Libation is currently scanning account(s) </summary>
		public bool ActivelyScanning => _numAccountsScanning > 0;
		/// <summary> Indicates if the "Remove Books" menu items are enabled</summary>
		public bool RemoveMenuItemsEnabled => !RemoveButtonsVisible && !ActivelyScanning;
		/// <summary> The library scanning status text </summary>
		public string ScanningText => _numAccountsScanning == 1 ? "Scanning..." : $"Scanning {_numAccountsScanning} accounts...";



		/// <summary> The number of accounts added to Libation </summary>
		public int AccountsCount
		{
			get => _accountsCount;
			set
			{
				this.RaiseAndSetIfChanged(ref _accountsCount, value);
				this.RaisePropertyChanged(nameof(ZeroAccounts));
				this.RaisePropertyChanged(nameof(AnyAccounts));
				this.RaisePropertyChanged(nameof(OneAccount));
				this.RaisePropertyChanged(nameof(MultipleAccounts));
			}
		}

		/// <summary> There are no Audible accounts </summary>
		public bool ZeroAccounts => _accountsCount == 0;
		/// <summary> There is at least one Audible account </summary>
		public bool AnyAccounts => _accountsCount > 0;
		/// <summary> There is exactly one Audible account </summary>
		public bool OneAccount => _accountsCount == 1;
		/// <summary> There are more than 1 Audible accounts </summary>
		public bool MultipleAccounts => _accountsCount > 1;



		/// <summary> The Process Queue panel is open </summary>
		public bool QueueOpen
		{
			get => _queueOpen;
			set
			{
				this.RaiseAndSetIfChanged(ref _queueOpen, value);
				QueueHideButtonText = _queueOpen? "❱❱❱" : "❰❰❰";
				this.RaisePropertyChanged(nameof(QueueHideButtonText));
			}
		}

		/// <summary> The Process Queue's Expand/Collapse button display text </summary>
		public string QueueHideButtonText { get; private set; }



		/// <summary> The number of books visible in the Product Display </summary>
		public int VisibleCount
		{
			get => _visibleCount;
			set
			{
				this.RaiseAndSetIfChanged(ref _visibleCount, value);
				this.RaisePropertyChanged(nameof(VisibleCountText));
				this.RaisePropertyChanged(nameof(VisibleCountMenuItemText));
			}
		}

		/// <summary> The Bottom-right visible book count status text </summary>
		public string VisibleCountText => $"Visible: {VisibleCount}";
		/// <summary> The Visible Books menu item header text </summary>
		public string VisibleCountMenuItemText => $"_Visible Books {VisibleCount}";



		/// <summary> The user's library statistics </summary>
		public LibraryCommands.LibraryStats LibraryStats
		{
			get => _libraryStats;
			set
			{
				this.RaiseAndSetIfChanged(ref _libraryStats, value);

				var backupsCountText
					= !LibraryStats.HasBookResults ? "No books. Begin by importing your library"
					: !LibraryStats.HasPendingBooks ? $"All {"book".PluralizeWithCount(LibraryStats.booksFullyBackedUp)} backed up"
					: $"BACKUPS: No progress: {LibraryStats.booksNoProgress}  In process: {LibraryStats.booksDownloadedOnly}  Fully backed up: {LibraryStats.booksFullyBackedUp} {(LibraryStats.booksError > 0 ? $"  Errors : {LibraryStats.booksError}" : "")}";

				var pdfCountText
					= !LibraryStats.HasPdfResults ? ""
					: LibraryStats.pdfsNotDownloaded == 0 ? $"  |  All {LibraryStats.pdfsDownloaded} PDFs downloaded"
					: $"  |  PDFs: NOT d/l'ed: {LibraryStats.pdfsNotDownloaded} Downloaded: {LibraryStats.pdfsDownloaded}";

				StatusCountText = backupsCountText + pdfCountText;

				BookBackupsToolStripText
					= LibraryStats.HasPendingBooks
					? $"Begin _Book and PDF Backups: {LibraryStats.PendingBooks} remaining"
					: "All books have been liberated";

				PdfBackupsToolStripText
					= LibraryStats.pdfsNotDownloaded > 0
					? $"Begin _PDF Only Backups: {LibraryStats.pdfsNotDownloaded} remaining"
					: "All PDFs have been downloaded";

				this.RaisePropertyChanged(nameof(StatusCountText));
				this.RaisePropertyChanged(nameof(BookBackupsToolStripText));
				this.RaisePropertyChanged(nameof(PdfBackupsToolStripText));
			}
		}

		/// <summary> Bottom-left library statistics display text </summary>
		public string StatusCountText { get; private set; } = "[Calculating backed up book quantities]  |  [Calculating backed up PDFs]";
		/// <summary> The "Begin Book and PDF Backup" menu item header text </summary>
		public string BookBackupsToolStripText { get; private set; } = "Begin _Book and PDF Backups: 0";
		/// <summary> The "Begin PDF Only Backup" menu item header text </summary>
		public string PdfBackupsToolStripText { get; private set; } = "Begin _PDF Only Backups: 0";



		/// <summary> The number of books visible in the Products Display that have not yet been liberated </summary>
		public int VisibleNotLiberated
		{
			get => _visibleNotLiberated;
			set
			{
				this.RaiseAndSetIfChanged(ref _visibleNotLiberated, value);

				LiberateVisibleToolStripText
					= AnyVisibleNotLiberated
					? $"Liberate _Visible Books: {VisibleNotLiberated}"
					: "All visible books are liberated";

				LiberateVisibleToolStripText_2
					= AnyVisibleNotLiberated
					? $"_Liberate: {VisibleNotLiberated}"
					: "All visible books are liberated";

				this.RaisePropertyChanged(nameof(AnyVisibleNotLiberated));
				this.RaisePropertyChanged(nameof(LiberateVisibleToolStripText));
				this.RaisePropertyChanged(nameof(LiberateVisibleToolStripText_2));
			}
		}

		/// <summary> Indicates if any of the books visible in the Products Display haven't been liberated </summary>
		public bool AnyVisibleNotLiberated => VisibleNotLiberated > 0;
		/// <summary> The "Liberate Visible Books" menu item header text (submenu item of the "Liberate Menu" menu item) </summary>
		public string LiberateVisibleToolStripText { get; private set; } = "Liberate _Visible Books: 0";
		/// <summary> The "Liberate" menu item header text (submenu item of the "Visible Books" menu item) </summary>
		public string LiberateVisibleToolStripText_2 { get; private set; } = "_Liberate: 0";
	}
}
