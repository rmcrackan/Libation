using ApplicationServices;
using Avalonia.Threading;
using DataLayer;
using LibationAvalonia.Dialogs;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;
using LibationUiBase.Resources;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	private int _visibleNotLiberated = 0;
	private int _visibleCount = 0;

	/// <summary> The Bottom-right visible book count status text </summary>
	public string VisibleCountText => string.Format(Strings.MainVM_Visible, _visibleCount);
	/// <summary> The Visible Books menu item header text </summary>
	public string VisibleCountMenuItemText => menufyText(string.Format(Strings.MainVM_VisibleBooks, _visibleCount));
	/// <summary> Indicates if any of the books visible in the Products Display haven't been liberated </summary>
	public bool AnyVisibleNotLiberated => _visibleNotLiberated > 0;
	/// <summary> The "Liberate Visible Books" menu item header text (submenu item of the "Liberate Menu" menu item) </summary>
	public string LiberateVisibleToolStripText { get; private set; } = Strings.MainVM_LiberateVisibleBooks0;
	/// <summary> The "Liberate" menu item header text (submenu item of the "Visible Books" menu item) </summary>
	public string LiberateVisibleToolStripText_2 { get; private set; } = menufyText(Strings.MainVM_Liberate0);

	private void Configure_VisibleBooks()
	{
		LibraryCommands.BookUserDefinedItemCommitted += setLiberatedVisibleMenuItemAsync;
		ProductsDisplay.VisibleCountChanged += ProductsDisplay_VisibleCountChanged;
	}

	private void setVisibleCount(int visibleCount)
	{
		_visibleCount = visibleCount;
		this.RaisePropertyChanged(nameof(VisibleCountText));
		this.RaisePropertyChanged(nameof(VisibleCountMenuItemText));
		updateNoMatchesVisible();
	}

	/// <summary>
	/// An empty grid only needs explaining when a filter emptied it. Mutually exclusive with the
	/// getting-started panel, which stands down while a filter is applied.
	/// </summary>
	private void updateNoMatchesVisible()
		=> NoMatchesVisible = _visibleCount == 0 && HasActiveFilter;

	private void setVisibleNotLiberatedCount(int visibleNotLiberated)
	{
		_visibleNotLiberated = visibleNotLiberated;

		LiberateVisibleToolStripText
			= AnyVisibleNotLiberated
			? string.Format(Strings.MainVM_LiberateMenu, menufyText(string.Format(Strings.MainVM_VisibleBooksCount, visibleNotLiberated)))
			: Strings.MainVM_AllVisibleBooksAreLiberated;

		LiberateVisibleToolStripText_2
			= AnyVisibleNotLiberated
			? menufyText(string.Format(Strings.MainVM_Liberate, visibleNotLiberated))
			: Strings.MainVM_AllVisibleBooksAreLiberated;

		this.RaisePropertyChanged(nameof(AnyVisibleNotLiberated));
		this.RaisePropertyChanged(nameof(LiberateVisibleToolStripText));
		this.RaisePropertyChanged(nameof(LiberateVisibleToolStripText_2));
	}

	public async void ProductsDisplay_VisibleCountChanged(object? sender, int qty)
	{
		setVisibleCount(qty);
		await setLiberatedVisibleMenuItemAsync();
	}

	private async void setLiberatedVisibleMenuItemAsync(object? _, object __)
		=> await setLiberatedVisibleMenuItemAsync();


	public async void LiberateVisible()
	{
		try
		{
			if (await ProcessQueue.QueueDownloadDecryptAsync(ProductsDisplay.GetVisibleBookEntries().UnLiberated().ToArray()))
				setQueueCollapseState(false);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "An error occurred while backing up visible library books");
		}
	}

	public async Task ReplaceTagsAsync()
	{
		var dialog = new TagsBatchDialog();
		var result = await dialog.ShowDialog<DialogResult>(MainWindow);
		if (result != DialogResult.OK)
			return;

		var visibleLibraryBooks = ProductsDisplay.GetVisibleBookEntries();

		var confirmationResult = await MessageBox.ShowConfirmationDialog(
			MainWindow,
			visibleLibraryBooks,
			// do not use `$` string interpolation. See impl.
			Strings.MainVM_AreYouSureYouWantTo,
			Strings.MainVM_ReplaceTags);

		if (confirmationResult != DialogResult.Yes)
			return;

		await visibleLibraryBooks.UpdateTagsAsync(dialog.NewTags);
	}

	public async Task SetBookDownloadedAsync()
	{
		var dialog = new LiberatedStatusBatchManualDialog();
		var result = await dialog.ShowDialog<DialogResult>(MainWindow);
		if (result != DialogResult.OK)
			return;

		var visibleLibraryBooks = ProductsDisplay.GetVisibleBookEntries();

		var confirmationResult = await MessageBox.ShowConfirmationDialog(
			MainWindow,
			visibleLibraryBooks,
			// do not use `$` string interpolation. See impl.
			Strings.MainVM_AreYouSureYouWantTo2,
			Strings.MainVM_ReplaceDownloadedStatus);

		if (confirmationResult != DialogResult.Yes)
			return;

		await visibleLibraryBooks.UpdateBookStatusAsync(dialog.BookLiberatedStatus);
	}

	public async Task SetPdfDownloadedAsync()
	{
		var dialog = new LiberatedStatusBatchManualDialog(isPdf: true);
		var result = await dialog.ShowDialog<DialogResult>(MainWindow);
		if (result != DialogResult.OK)
			return;

		var visibleLibraryBooks = ProductsDisplay.GetVisibleBookEntries();

		var confirmationResult = await MessageBox.ShowConfirmationDialog(
			MainWindow,
			visibleLibraryBooks,
			// do not use `$` string interpolation. See impl.
			Strings.MainVM_AreYouSureYouWantTo3,
			Strings.MainVM_ReplaceDownloadedStatus);

		if (confirmationResult != DialogResult.Yes)
			return;

		await visibleLibraryBooks.UpdatePdfStatusAsync(dialog.BookLiberatedStatus);
	}

	public async Task SetDownloadedAutoAsync()
	{
		var dialog = new LiberatedStatusBatchAutoDialog();
		var result = await dialog.ShowDialog<DialogResult>(MainWindow);
		if (result != DialogResult.OK)
			return;

		var bulkSetStatus = new BulkSetDownloadStatus(ProductsDisplay.GetVisibleBookEntries(), dialog.SetDownloaded, dialog.SetNotDownloaded);
		var count = await Task.Run(bulkSetStatus.Discover);

		if (count == 0)
			return;

		var confirmationResult = await MessageBox.Show(
			bulkSetStatus.AggregateMessage,
			Strings.MainVM_ReplaceDownloadedStatus,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question,
			MessageBoxDefaultButton.Button1);

		if (confirmationResult != DialogResult.Yes)
			return;

		await bulkSetStatus.ExecuteAsync();
	}

	public async Task RemoveVisibleAsync()
	{
		var visibleLibraryBooks = ProductsDisplay.GetVisibleBookEntries();

		var confirmationResult = await MessageBox.ShowConfirmationDialog(
			MainWindow,
			visibleLibraryBooks,
			// do not use `$` string interpolation. See impl.
			Strings.MainVM_AreYouSureYouWantTo4,
			Strings.MainVM_RemoveBooksFromLibation2,
			MessageBoxDefaultButton.Button2);

		if (confirmationResult is DialogResult.Yes)
			await visibleLibraryBooks.RemoveBooksAsync();
	}

	private async Task setLiberatedVisibleMenuItemAsync()
	{
		try
		{
			var visible = ProductsDisplay.GetVisibleBookEntries();
			var libraryStats = await Task.Run(() => LibraryCommands.GetCounts(visible));
			await Dispatcher.UIThread.InvokeAsync(() => setVisibleNotLiberatedCount(libraryStats.PendingBooks));
		}
		// Every caller is an async void event handler, where a failure is not a faulted task anyone awaits but
		// an unhandled exception that closes Libation. Counting these books reads the file system, which can
		// fail at any moment for reasons that have nothing to do with the app - a Books folder on a drive that
		// was just unplugged is enough. A stale number above a menu item is not worth the session.
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error counting the visible books which are not yet liberated");
		}
	}
}
