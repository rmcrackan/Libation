using ApplicationServices;
using System;
using System.Threading.Tasks;
using DataLayer;
using Avalonia.Threading;
using LibationAvalonia.Dialogs;
using ReactiveUI;
using LibationUiBase.Forms;
using System.Linq;
using LibationUiBase;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		private int _visibleNotLiberated = 0;
		private int _visibleCount = 0;

		/// <summary> The Bottom-right visible book count status text </summary>
		public string VisibleCountText => $"Visible: {_visibleCount}";
		/// <summary> The Visible Books menu item header text </summary>
		public string VisibleCountMenuItemText => menufyText($"Visible Books {_visibleCount}");
		/// <summary> Indicates if any of the books visible in the Products Display haven't been liberated </summary>
		public bool AnyVisibleNotLiberated => _visibleNotLiberated > 0;
		/// <summary> The "Liberate Visible Books" menu item header text (submenu item of the "Liberate Menu" menu item) </summary>
		public string LiberateVisibleToolStripText { get; private set; } = "Liberate _Visible Books: 0";
		/// <summary> The "Liberate" menu item header text (submenu item of the "Visible Books" menu item) </summary>
		public string LiberateVisibleToolStripText_2 { get; private set; } = menufyText("Liberate: 0");

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
		}

		private void setVisibleNotLiberatedCount(int visibleNotLiberated)
		{
			_visibleNotLiberated = visibleNotLiberated;

			LiberateVisibleToolStripText
				= AnyVisibleNotLiberated
				? "Liberate " + menufyText($"Visible Books: {visibleNotLiberated}")
				: "All visible books are liberated";

			LiberateVisibleToolStripText_2
				= AnyVisibleNotLiberated
				? menufyText($"Liberate: {visibleNotLiberated}")
				: "All visible books are liberated";

			this.RaisePropertyChanged(nameof(AnyVisibleNotLiberated));
			this.RaisePropertyChanged(nameof(LiberateVisibleToolStripText));
			this.RaisePropertyChanged(nameof(LiberateVisibleToolStripText_2));
		}

		public async void ProductsDisplay_VisibleCountChanged(object? sender, int qty)
		{
			setVisibleCount(qty);
			await Dispatcher.UIThread.InvokeAsync(setLiberatedVisibleMenuItem);
		}

		private async void setLiberatedVisibleMenuItemAsync(object? _, object __)
			=> await Dispatcher.UIThread.InvokeAsync(setLiberatedVisibleMenuItem);


		public void LiberateVisible()
		{
			try
			{
				if (ProcessQueue.QueueDownloadDecrypt(ProductsDisplay.GetVisibleBookEntries().UnLiberated().ToArray()))
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
				"Are you sure you want to replace tags in {0}?",
				"Replace tags?");

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
				"Are you sure you want to replace book downloaded status in {0}?",
				"Replace downloaded status?");

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
				"Are you sure you want to replace PDF downloaded status in {0}?",
				"Replace downloaded status?");

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
				"Replace downloaded status?",
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
				"Are you sure you want to remove {0} from Libation's library?",
				"Remove books from Libation?",
				MessageBoxDefaultButton.Button2);

			if (confirmationResult is DialogResult.Yes)
				await visibleLibraryBooks.RemoveBooksAsync();
		}

		private void setLiberatedVisibleMenuItem()
		{
			var libraryStats = LibraryCommands.GetCounts(ProductsDisplay.GetVisibleBookEntries());
			setVisibleNotLiberatedCount(libraryStats.PendingBooks);
		}
	}
}
