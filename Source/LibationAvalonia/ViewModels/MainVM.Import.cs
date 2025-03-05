using ApplicationServices;
using AudibleUtilities;
using Avalonia.Controls;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	public partial class MainVM
	{
		private bool _autoScanChecked = Configuration.Instance.AutoScan;
		private string _removeBooksButtonText = "Remove # Books from Libation";
		private bool _removeBooksButtonEnabled = Design.IsDesignMode;
		private bool _removeButtonsVisible = Design.IsDesignMode;
		private int _numAccountsScanning = 2;
		private int _accountsCount = 0;

		/// <summary> Auto scanning accounts is enables </summary>
		public bool AutoScanChecked { get => _autoScanChecked; set => Configuration.Instance.AutoScan = this.RaiseAndSetIfChanged(ref _autoScanChecked, value); }
		/// <summary> Display text for the "Remove # Books from Libation" button </summary>
		public string RemoveBooksButtonText { get => _removeBooksButtonText; set => this.RaiseAndSetIfChanged(ref _removeBooksButtonText, value); }
		/// <summary> Indicates if the "Remove # Books from Libation" button is enabled </summary>
		public bool RemoveBooksButtonEnabled { get => _removeBooksButtonEnabled; set { this.RaiseAndSetIfChanged(ref _removeBooksButtonEnabled, value); } }
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
		/// <summary> Indicates if Libation is currently scanning account(s) </summary>
		public bool ActivelyScanning => _numAccountsScanning > 0;
		/// <summary> Indicates if the "Remove Books" menu items are enabled</summary>
		public bool RemoveMenuItemsEnabled => !RemoveButtonsVisible && !ActivelyScanning;
		/// <summary> The library scanning status text </summary>
		public string ScanningText => _numAccountsScanning == 1 ? "Scanning..." : $"Scanning {_numAccountsScanning} accounts...";
		/// <summary> There is at least one Audible account </summary>
		public bool AnyAccounts => AccountsCount > 0;
		/// <summary> There is exactly one Audible account </summary>
		public bool OneAccount => AccountsCount == 1;
		/// <summary> There are more than 1 Audible accounts </summary>
		public bool MultipleAccounts => AccountsCount > 1;
		/// <summary> The number of accounts added to Libation </summary>
		public int AccountsCount
		{
			get => _accountsCount;
			set
			{
				this.RaiseAndSetIfChanged(ref _accountsCount, value);
				this.RaisePropertyChanged(nameof(AnyAccounts));
				this.RaisePropertyChanged(nameof(OneAccount));
				this.RaisePropertyChanged(nameof(MultipleAccounts));
			}
		}


		public void Configure_Import()
		{
			MainWindow.Loaded += (_, _) =>
			{
				refreshImportMenu();
				AccountsSettingsPersister.Saved += refreshImportMenu;
			};

			AutoScanChecked = Configuration.Instance.AutoScan;

			setyNumScanningAccounts(0);
			LibraryCommands.ScanBegin += (_, accountsLength) => setyNumScanningAccounts(accountsLength);
			LibraryCommands.ScanEnd += (_, newCount) => setyNumScanningAccounts(0);

			if (!Design.IsDesignMode)
				RemoveButtonsVisible = false;
		}

		public void ToggleAutoScan() => AutoScanChecked = !AutoScanChecked;

		public async Task AddAccountsAsync()
		{
			await MessageBox.Show("To load your Audible library, come back here to the Import menu after adding your account");
			await new LibationAvalonia.Dialogs.AccountsDialog().ShowDialog(MainWindow);
		}

		public async Task ScanAccountAsync()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var firstAccount = persister.AccountsSettings.GetAll().FirstOrDefault();
			if (firstAccount != null)
				await scanLibrariesAsync(firstAccount);
		}

		public async Task ScanAllAccountsAsync()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			await scanLibrariesAsync(persister.AccountsSettings.GetAll().ToArray());
		}

		public async Task ScanSomeAccountsAsync()
		{
			var scanAccountsDialog = new LibationAvalonia.Dialogs.ScanAccountsDialog();

			if (await scanAccountsDialog.ShowDialog<DialogResult>(MainWindow) != DialogResult.OK)
				return;

			if (!scanAccountsDialog.CheckedAccounts.Any())
				return;

			await scanLibrariesAsync(scanAccountsDialog.CheckedAccounts.ToArray());
		}

		public async Task RemoveBooksAsync()
		{
			// if 0 accounts, this will not be visible
			// if 1 account, run scanLibrariesRemovedBooks() on this account
			// if multiple accounts, another menu set will open. do not run scanLibrariesRemovedBooks()
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var accounts = persister.AccountsSettings.GetAll();

			if (accounts.Count != 1)
				return;

			var firstAccount = accounts.Single();
			await scanLibrariesRemovedBooks(firstAccount);
		}

		// selectively remove books from all accounts
		public async Task RemoveBooksAllAsync()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var allAccounts = persister.AccountsSettings.GetAll();
			await scanLibrariesRemovedBooks(allAccounts.ToArray());
		}

		public async Task RemoveBooksBtn()
		{
			RemoveBooksButtonEnabled = false;
			await ProductsDisplay.RemoveCheckedBooksAsync();
			RemoveBooksButtonEnabled = true;
		}

		public async Task DoneRemovingBtn()
		{
			RemoveButtonsVisible = false;

			ProductsDisplay.DoneRemovingBooks();

			//Restore the filter
			await PerformFilter(lastGoodFilter);
		}

		// selectively remove books from some accounts
		public async Task RemoveBooksSomeAsync()
		{
			var scanAccountsDialog = new LibationAvalonia.Dialogs.ScanAccountsDialog();

			if (await scanAccountsDialog.ShowDialog<DialogResult>(MainWindow) != DialogResult.OK)
				return;

			if (!scanAccountsDialog.CheckedAccounts.Any())
				return;

			await scanLibrariesRemovedBooks(scanAccountsDialog.CheckedAccounts.ToArray());
		}

		public async Task LocateAudiobooksAsync()
		{
			var locateDialog = new LibationAvalonia.Dialogs.LocateAudiobooksDialog();
			await locateDialog.ShowDialog(MainWindow);
		}

		private void setyNumScanningAccounts(int numScanning)
		{
			_numAccountsScanning = numScanning;
			this.RaisePropertyChanged(nameof(ActivelyScanning));
			this.RaisePropertyChanged(nameof(RemoveMenuItemsEnabled));
			this.RaisePropertyChanged(nameof(ScanningText));
		}

		private async Task scanLibrariesRemovedBooks(params Account[] accounts)
		{
			//This action is meant to operate on the entire library.
			//For removing books within a filter set, use
			//Visible Books > Remove from library

			await ProductsDisplay.Filter(null);

			RemoveBooksButtonEnabled = true;
			RemoveButtonsVisible = true;

			await ProductsDisplay.ScanAndRemoveBooksAsync(accounts);
		}

		private async Task scanLibrariesAsync(params Account[]? accounts)
		{
			try
			{
				var (totalProcessed, newAdded) = await LibraryCommands.ImportAccountAsync(LibationAvalonia.Dialogs.Login.AvaloniaLoginChoiceEager.ApiExtendedFunc, accounts);

				// this is here instead of ScanEnd so that the following is only possible when it's user-initiated, not automatic loop
				if (Configuration.Instance.ShowImportedStats && newAdded > 0)
					await MessageBox.Show($"Total processed: {totalProcessed}\r\nNew: {newAdded}");
			}
			catch (OperationCanceledException)
			{
				Serilog.Log.Information("Audible login attempt cancelled by user");
			}
			catch (Exception ex)
			{
				await MessageBox.ShowAdminAlert(
					MainWindow,
					"Error importing library. Please try again. If this still happens after 2 or 3 tries, stop and contact administrator",
					"Error importing library",
					ex);
			}
		}

		private void refreshImportMenu(object? _ = null, EventArgs? __ = null)
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			AccountsCount = persister.AccountsSettings.Accounts.Count;


			if (NativeMenu.GetMenu(MainWindow)?.Items[0] is not NativeMenuItem ss ||
				ss.Menu is not NativeMenu importMenuItem)
			{
				Serilog.Log.Logger.Error($"Unable to find {nameof(importMenuItem)}");
				return;
			}


			for (int i = importMenuItem.Items.Count - 1; i >= 2; i--)
				importMenuItem.Items.RemoveAt(i);

			if (AccountsCount < 1)
			{
				importMenuItem.Items.Add(new NativeMenuItem { Header = "No accounts yet. Add Account...", Command = ReactiveCommand.Create(AddAccountsAsync) });
			}
			else if (AccountsCount == 1)
			{
				importMenuItem.Items.Add(new NativeMenuItem { Header = "Scan Library", Command = ReactiveCommand.Create(ScanAccountAsync), Gesture = new KeyGesture(Key.S, KeyModifiers.Alt | KeyModifiers.Meta) });
				importMenuItem.Items.Add(new NativeMenuItemSeparator());
				importMenuItem.Items.Add(new NativeMenuItem { Header = "Remove Library Books", Command = ReactiveCommand.Create(RemoveBooksAsync), Gesture = new KeyGesture(Key.R, KeyModifiers.Alt | KeyModifiers.Meta) });
			}
			else
			{
				importMenuItem.Items.Add(new NativeMenuItem { Header = "Scan Library of All Accounts", Command = ReactiveCommand.Create(ScanAllAccountsAsync), Gesture = new KeyGesture(Key.S, KeyModifiers.Alt | KeyModifiers.Meta) });
				importMenuItem.Items.Add(new NativeMenuItem { Header = "Scan Library of Some Accounts", Command = ReactiveCommand.Create(ScanSomeAccountsAsync), Gesture = new KeyGesture(Key.S, KeyModifiers.Alt | KeyModifiers.Meta | KeyModifiers.Shift) });
				importMenuItem.Items.Add(new NativeMenuItemSeparator());
				importMenuItem.Items.Add(new NativeMenuItem { Header = "Remove Books from All Accounts", Command = ReactiveCommand.Create(RemoveBooksAllAsync), Gesture = new KeyGesture(Key.R, KeyModifiers.Alt | KeyModifiers.Meta) });
				importMenuItem.Items.Add(new NativeMenuItem { Header = "Remove Books from Some Accounts", Command = ReactiveCommand.Create(RemoveBooksSomeAsync), Gesture = new KeyGesture(Key.R, KeyModifiers.Alt | KeyModifiers.Meta | KeyModifiers.Shift) });
			}

			importMenuItem.Items.Add(new NativeMenuItemSeparator());
			importMenuItem.Items.Add(new NativeMenuItem { Header = "Locate Audiobooks...", Command = ReactiveCommand.Create(LocateAudiobooksAsync) });
		}
	}
}
