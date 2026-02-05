using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core.Threading;
using LibationWinForms.Dialogs;

namespace LibationWinForms;

public partial class Form1
{
	protected void Configure_VisibleBooks()
	{
		// init formattable
		visibleCountLbl.Format(0);
		liberateVisibleToolStripMenuItem_VisibleBooksMenu.Format(0);
		liberateVisibleToolStripMenuItem_LiberateMenu.Format(0);

		// top menu strip
		visibleBooksToolStripMenuItem.Format(0);

		LibraryCommands.BookUserDefinedItemCommitted += setLiberatedVisibleMenuItemAsync;
	}
	private async void setLiberatedVisibleMenuItemAsync(object? _, object __)
		=> await Task.Run(setLiberatedVisibleMenuItem);

	private static DateTime lastVisibleCountUpdated;
	void setLiberatedVisibleMenuItem()
	{
		//Assume that all calls to update arrive in order,
		//Only display results of the latest book count.
		var updaterTime = lastVisibleCountUpdated = DateTime.UtcNow;
		var libraryStats = LibraryCommands.GetCounts(productsDisplay.GetVisible());
		if (updaterTime < lastVisibleCountUpdated)
			return;

		this.UIThreadSync(() =>
		{
			if (libraryStats.HasPendingBooks)
			{
				liberateVisibleToolStripMenuItem_VisibleBooksMenu.Format(libraryStats.PendingBooks);
				liberateVisibleToolStripMenuItem_LiberateMenu.Format(libraryStats.PendingBooks);
			}
			else
			{
				liberateVisibleToolStripMenuItem_VisibleBooksMenu.Text = "All visible books are liberated";
				liberateVisibleToolStripMenuItem_LiberateMenu.Text = "All visible books are liberated";
			}

			liberateVisibleToolStripMenuItem_VisibleBooksMenu.Enabled = libraryStats.HasPendingBooks;
			liberateVisibleToolStripMenuItem_LiberateMenu.Enabled = libraryStats.HasPendingBooks;
		});
	}

	private void liberateVisible(object sender, EventArgs e)
	{
		try
		{
			if (processBookQueue1.ViewModel.QueueDownloadDecrypt(productsDisplay.GetVisible().UnLiberated().ToArray()))
				SetQueueCollapseState(false);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "An error occurred while backing up visible library books");
		}
	}

	private async void replaceTagsToolStripMenuItem_Click(object sender, EventArgs e)
	{
		var dialog = new TagsBatchDialog();
		var result = dialog.ShowDialog();
		if (result != DialogResult.OK)
			return;

		var visibleLibraryBooks = productsDisplay.GetVisible();

		var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
			visibleLibraryBooks,
			// do not use `$` string interpolation. See impl.
			"Are you sure you want to replace tags in {0}?",
			"Replace tags?");

		if (confirmationResult != DialogResult.Yes)
			return;

		await visibleLibraryBooks.UpdateTagsAsync(dialog.NewTags);
	}

	private async void setBookDownloadedManualToolStripMenuItem_Click(object sender, EventArgs e)
	{
		var dialog = new LiberatedStatusBatchManualDialog();
		var result = dialog.ShowDialog();
		if (result != DialogResult.OK)
			return;

		var visibleLibraryBooks = productsDisplay.GetVisible();

		var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
			visibleLibraryBooks,
			// do not use `$` string interpolation. See impl.
			"Are you sure you want to replace book downloaded status in {0}?",
			"Replace downloaded status?");

		if (confirmationResult != DialogResult.Yes)
			return;

		await visibleLibraryBooks.UpdateBookStatusAsync(dialog.BookLiberatedStatus);
	}

	private async void setPdfDownloadedManualToolStripMenuItem_Click(object sender, EventArgs e)
	{
		var dialog = new LiberatedStatusBatchManualDialog(isPdf: true);
		var result = dialog.ShowDialog();
		if (result != DialogResult.OK)
			return;

		var visibleLibraryBooks = productsDisplay.GetVisible();

		var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
			visibleLibraryBooks,
			// do not use `$` string interpolation. See impl.
			"Are you sure you want to replace PDF downloaded status in {0}?",
			"Replace downloaded status?");

		if (confirmationResult != DialogResult.Yes)
			return;

		await visibleLibraryBooks.UpdatePdfStatusAsync(dialog.BookLiberatedStatus);
	}

	private async void setDownloadedAutoToolStripMenuItem_Click(object sender, EventArgs e)
	{
		var dialog = new LiberatedStatusBatchAutoDialog();
		var result = dialog.ShowDialog();
		if (result != DialogResult.OK)
			return;

		var bulkSetStatus = new BulkSetDownloadStatus(productsDisplay.GetVisible(), dialog.SetDownloaded, dialog.SetNotDownloaded);
		var count = await Task.Run(() => bulkSetStatus.Discover());

		if (count == 0)
			return;

		var confirmationResult = MessageBox.Show(
			bulkSetStatus.AggregateMessage,
			"Replace downloaded status?",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question,
			MessageBoxDefaultButton.Button1);

		if (confirmationResult != DialogResult.Yes)
			return;

		await bulkSetStatus.ExecuteAsync();
	}

	private async void removeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		var visibleLibraryBooks = productsDisplay.GetVisible();

		var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
			visibleLibraryBooks,
			// do not use `$` string interpolation. See impl.
			"Are you sure you want to remove {0} from Libation's library?",
			"Remove books from Libation?");

		if (confirmationResult is DialogResult.Yes)
			await visibleLibraryBooks.RemoveBooksAsync();
	}

	private async void productsDisplay_VisibleCountChanged(object sender, int qty)
	{
		// bottom-left visible count
		visibleCountLbl.Format(qty);

		// top menu strip
		visibleBooksToolStripMenuItem.Format(qty);
		visibleBooksToolStripMenuItem.Enabled = qty > 0;

		await Task.Run(setLiberatedVisibleMenuItem);
	}
}
