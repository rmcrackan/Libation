using AudibleUtilities;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using DataLayer;
using FileManager;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using LibationUiBase.Forms;
using LibationUiBase.GridView;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Views;

public partial class MainWindow : ReactiveWindow<MainVM>
{
	public MainWindow()
	{
		if (Design.IsDesignMode)
			Configuration.CreateMockInstance();

		ApiExtended.LoginChoiceFactory = account => Dispatcher.UIThread.Invoke(() => new Dialogs.Login.AvaloniaLoginChoiceEager(account));

		AudibleApiStorage.LoadError += AudibleApiStorage_LoadError;
		InitializeComponent();
		Configure_Upgrade();

		Opened += MainWindow_Opened;
		Closing += MainWindow_Closing;

		KeyBindings.Add(new KeyBinding { Command = ReactiveCommand.Create(selectAndFocusSearchBox), Gesture = new KeyGesture(Key.F, KeyGestureHelper.CommandModifier) });

		if (!Configuration.IsMacOs && ViewModel is MainVM vm)
		{
			KeyBindings.Add(new KeyBinding { Command = ReactiveCommand.Create(vm.ShowSettingsAsync), Gesture = new KeyGesture(Key.P, KeyGestureHelper.CommandModifier) });
			KeyBindings.Add(new KeyBinding { Command = ReactiveCommand.Create(vm.ShowAccountsAsync), Gesture = new KeyGesture(Key.A, KeyGestureHelper.CommandModifier | KeyModifiers.Shift) });
			KeyBindings.Add(new KeyBinding { Command = ReactiveCommand.Create(vm.ExportLibraryAsync), Gesture = new KeyGesture(Key.S, KeyGestureHelper.CommandModifier) });
		}

		Configuration.Instance.PropertyChanged += Settings_PropertyChanged;
		Settings_PropertyChanged(this, null);
		DataContext = new MainVM(this);
#if DEBUG
		Configure_DebugMenu();
#endif
	}

#if DEBUG
	private void Configure_DebugMenu()
	{
		var simulateItem = new MenuItem { Header = "Simulate bad book failures (test dialog)..." };
		simulateItem.Click += async (_, _) =>
		{
			if (ViewModel is MainVM vm)
				await vm.SimulateBadBookFailuresAsync();
		};

		// Insert before Tour; the Separator above Tour in axaml already provides the divider.
		var items = settingsToolStripMenuItem.Items;
		var insertIndex = -1;
		for (var i = 0; i < items.Count; i++)
		{
			if (items[i] is MenuItem menuItem
				&& menuItem.Header?.ToString()?.Contains("Tour", StringComparison.OrdinalIgnoreCase) == true)
			{
				insertIndex = i;
				break;
			}
		}

		if (insertIndex < 0)
			insertIndex = items.Count;

		items.Insert(insertIndex, simulateItem);
	}
#endif

	[Dinah.Core.PropertyChangeFilter(nameof(Configuration.Books))]
	private void Settings_PropertyChanged(object? sender, Dinah.Core.PropertyChangedEventArgsEx? e)
	{
		if (!Configuration.IsWindows)
		{
			//The books directory does not support filenames with windows' invalid characters.
			//Tell the ReplacementCharacters configuration to treat those characters as invalid.
			ReplacementCharacters.AdditionalInvalidFilenameCharacters
				= Configuration.Instance.BooksCanWriteWindowsInvalidChars ? []
				: FileSystemTest.AdditionalInvalidWindowsFilenameCharacters.ToArray();
		}
	}

	private void AudibleApiStorage_LoadError(object? sender, AccountSettingsLoadErrorEventArgs e)
	{
		try
		{
			//Backup AccountSettings.json and create a new, empty file.
			var backupFile =
				FileUtility.SaferMoveToValidPath(
					e.SettingsFilePath,
					e.SettingsFilePath,
					Configuration.Instance.ReplacementCharacters,
					"bak");
			AudibleApiStorage.EnsureAccountsSettingsFileExists();
			e.Handled = true;

			showAccountSettingsRecoveredMessage(backupFile);
		}
		catch
		{
			showAccountSettingsUnrecoveredMessage();
		}

		async void showAccountSettingsRecoveredMessage(LongPath backupFile)
		{
			var ex = e.GetException();
			var body = AccountSettingsDecryptFailure.TryFindInTree(ex, out _)
				? AccountSettingsDecryptFailure.GetRecoveredDialogBody(ex, backupFile.PathWithoutPrefix)
				: $"""
					Libation could not load your account settings, so it had created a new, empty account settings file.

					You will need to re-add you Audible account(s) before scanning or downloading.

					The old account settings file has been archived at '{backupFile.PathWithoutPrefix}'

					{ex}
					""";

			await MessageBox.Show(
				this,
				body,
				AccountSettingsDecryptFailure.LoadErrorCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}

		void showAccountSettingsUnrecoveredMessage()
		{
			var messageBoxWindow = MessageBox.Show(this, $"""
			Libation could not load your account settings. The file may be corrupted, but Libation is unable to delete it.

			Please move or delete the account settings file '{e.SettingsFilePath}'

			{e.GetException().ToString()}
			""",
			"Error Loading Account Settings",
			MessageBoxButtons.OK);

			//Force the message box to show synchronously because we're not handling the exception
			//and libation will crash after the event handler returns
			var frame = new DispatcherFrame();
			_ = messageBoxWindow.ContinueWith(static (_, s) => (s as DispatcherFrame)?.Continue = false, frame);
			Dispatcher.UIThread.PushFrame(frame);
			messageBoxWindow.GetAwaiter().GetResult();
		}
	}

	private async void MainWindow_Opened(object? sender, EventArgs e)
	{
		await MessageBox.VerboseLoggingWarning_ShowIfTrue();

		if (AudibleFileStorage.BooksDirectory is null)
		{
			var result = await MessageBox.Show(
				this,
				"Please set a valid Books location in the settings dialog.",
				"Books Directory Not Set",
				MessageBoxButtons.OKCancel,
				MessageBoxIcon.Warning,
				MessageBoxDefaultButton.Button1);

			if (result is DialogResult.OK)
				await new SettingsDialog().ShowDialog(this);
		}

		if (Configuration.Instance.FirstLaunch)
		{
			var result = await MessageBox.Show(this, "Would you like a guided tour to get started?", "Libation Walkthrough", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

			if (result is DialogResult.Yes)
			{
				await new Walkthrough(this).RunAsync();
			}

			Configuration.Instance.FirstLaunch = false;
		}
	}

	private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
	{
		productsDisplay?.CloseImageDisplay();
		this.SaveSizeAndLocation(Configuration.Instance);
		//This is double firing with 11.3.9
		Closing -= MainWindow_Closing;
	}

	private void selectAndFocusSearchBox()
	{
		filterSearchTb.SelectAll();
		filterSearchTb.Focus();
	}

	public async Task OnLibraryLoadedAsync(List<LibraryBook> initialLibrary)
	{
		//Get the ViewModel before crossing the await boundary
		if (ViewModel is not MainVM vm)
			return;

		if (QuickFilters.UseDefault)
			await vm.PerformFilter(QuickFilters.Filters.FirstOrDefault());

		vm.BindToGridTask = Task.WhenAll(
			vm.SetBackupCountsAsync(initialLibrary),
			vm.RefreshBooksInTrashAsync(),
			Task.Run(() => vm.ProductsDisplay.BindToGridAsync(initialLibrary)));

		await vm.BindToGridTask;
	}

	private void PanelGrip_DragStarted(object? sender, Avalonia.Input.VectorEventArgs e)
	{
		if (ViewModel is not null) ViewModel.IsResizingPanel = true;
	}

	private void PanelGrip_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
	{
		if (ViewModel is null) return;
		ViewModel.IsResizingPanel = false;
		ViewModel.SavePanelWidths();
	}

	private void SidebarGrip_DragDelta(object? sender, Avalonia.Input.VectorEventArgs e)
	{
		if (ViewModel is not MainVM vm) return;
		var other = vm.InspectorOpen ? vm.InspectorWidth : 0;
		var max = Math.Min(MainVM.SidebarMaxWidth, Bounds.Width - other - MainVM.GridMinWidth);
		vm.SidebarWidth = Math.Clamp(vm.SidebarWidth + e.Vector.X, MainVM.SidebarMinWidth, Math.Max(MainVM.SidebarMinWidth, max));
	}

	private void InspectorGrip_DragDelta(object? sender, Avalonia.Input.VectorEventArgs e)
	{
		if (ViewModel is not MainVM vm) return;
		var other = vm.SidebarOpen ? vm.SidebarWidth : 0;
		var max = Math.Min(MainVM.InspectorMaxWidth, Bounds.Width - other - MainVM.GridMinWidth);
		vm.InspectorWidth = Math.Clamp(vm.InspectorWidth - e.Vector.X, MainVM.InspectorMinWidth, Math.Max(MainVM.InspectorMinWidth, max));
	}

	public void ProductsDisplay_LiberateClicked(object _, IList<LibraryBook> libraryBook, Configuration config) => ViewModel?.LiberateClicked(libraryBook, config);
	public void ProductsDisplay_LiberateSeriesClicked(object _, SeriesEntry series) => ViewModel?.LiberateSeriesClicked(series);
	public void ProductsDisplay_ConvertToMp3Clicked(object _, LibraryBook[] libraryBook) => ViewModel?.ConvertToMp3Clicked(libraryBook);

	public void ProductsDisplay_DescriptionClicked(object? _, GridEntry entry)
	{
		if (ViewModel is null)
			return;
		ViewModel.InspectorOpen = true;
		ViewModel.ShowDetailsTab();
		// After the tap's row selection has reached the inspector, which collapses on a new entry.
		Dispatcher.UIThread.Post(bookInspector.ExpandDescription, DispatcherPriority.Background);
	}

	BookDetailsDialog? bookDetailsForm;
	private async void BookInspector_EditSelectionTagsClicked(object? sender, EventArgs e)
		=> await (ViewModel?.ReplaceSelectionTagsAsync() ?? Task.CompletedTask);

	public void ProductsDisplay_TagsButtonClicked(object _, LibraryBook libraryBook)
	{
		if (bookDetailsForm is null || !bookDetailsForm.IsVisible)
		{
			bookDetailsForm = new BookDetailsDialog(libraryBook);
			bookDetailsForm.Show(this);
		}
		else
			bookDetailsForm.LibraryBook = libraryBook;
	}

	public async void filterSearchTb_KeyPress(object _, KeyEventArgs e)
	{
		if (e.Key == Key.Return && ViewModel is not null)
		{
			await ViewModel.FilterBtn(filterSearchTb.Text ?? string.Empty);

			// silence the 'ding'
			e.Handled = true;
		}
	}

	private async void ClearFilterButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (ViewModel is null)
			return;
		await ViewModel.FilterBtn(string.Empty);
		// Typed text lives only in the TextBox (OneWay binding). If the VM filter was already empty,
		// PerformFilter does not refresh the binding, so clear the control explicitly (WinForms sets Text in performFilter).
		filterSearchTb.Text = string.Empty;
	}

	internal Button addQuickFilterBtn => librarySidebar.addQuickFilterBtn;

	private async void ClearAllFilters_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (ViewModel is null)
			return;
		await ViewModel.SelectScopeAsync(LibraryScope.All);
		ClearFilterButton_Click(sender, e);
	}

	private void Configure_Upgrade()
	{
		setProgressVisible(false);
#pragma warning disable CS8321 // Local function is declared but never used
		async Task upgradeAvailable(LibationUiBase.UpgradeEventArgs e)
		{
			var notificationResult = await new UpgradeNotificationDialog(e.UpgradeProperties, e.CapUpgrade, e.UpgradeUnavailableReason).ShowDialogAsync(this);

			e.Ignore = notificationResult == DialogResult.Ignore;
			e.InstallUpgrade = notificationResult == DialogResult.OK;
		}
#pragma warning restore CS8321 // Local function is declared but never used

		var upgrader = new LibationUiBase.Upgrader();
		upgrader.DownloadProgress += async (_, e) => await Dispatcher.UIThread.InvokeAsync(() => ViewModel?.DownloadProgress = e.ProgressPercentage);
		upgrader.DownloadBegin += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(true));
		upgrader.DownloadCompleted += async (_, _) => await Dispatcher.UIThread.InvokeAsync(() => setProgressVisible(false));
		upgrader.UpgradeFailed += async (_, message) => await Dispatcher.UIThread.InvokeAsync(() => { setProgressVisible(false); MessageBox.Show(this, message, "Upgrade Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); });

#if !DEBUG
		Opened += async (_, _) => await upgrader.CheckForUpgradeAtStartupAsync(upgradeAvailable);
#endif
	}

	private void setProgressVisible(bool visible) => ViewModel?.DownloadProgress = visible ? 0 : null;

	public SearchSyntaxDialog ShowSearchSyntaxDialog()
	{
		var dialog = new SearchSyntaxDialog();
		dialog.InsertRequested += Dialog_InsertRequested;
		dialog.ReplaceRequested += Dialog_ReplaceRequested;
		dialog.Closed += Dialog_Closed;
		filterHelpBtn.IsEnabled = false;
		dialog.Show(this);
		return dialog;

		void Dialog_Closed(object? sender, EventArgs e)
		{
			dialog.InsertRequested -= Dialog_InsertRequested;
			dialog.ReplaceRequested -= Dialog_ReplaceRequested;
			filterHelpBtn.IsEnabled = true;
		}
		void Dialog_InsertRequested(object? sender, string tag)
		{
			var text = filterSearchTb.Text ?? "";
			var caret = Math.Clamp(filterSearchTb.CaretIndex, 0, text.Length);
			if (caret > 0 && !char.IsWhiteSpace(text[caret - 1]) && text[caret - 1] != '(')
				tag = " " + tag;
			filterSearchTb.Text = text.Insert(caret, tag);
			filterSearchTb.CaretIndex = caret + tag.Length;
			filterSearchTb.Focus();
		}
		async void Dialog_ReplaceRequested(object? sender, string query)
		{
			filterSearchTb.Text = query;
			filterSearchTb.CaretIndex = query.Length;
			filterSearchTb.Focus();
			if (ViewModel is not null)
				await ViewModel.FilterBtn(query);
		}
	}
}
