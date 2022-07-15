using ApplicationServices;
using Avalonia.Threading;
using DataLayer;
using LibationWinForms.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_VisibleBooks()
		{
			LibraryCommands.BookUserDefinedItemCommitted += setLiberatedVisibleMenuItemAsync;
		}

		private async void setLiberatedVisibleMenuItemAsync(object _, object __)
			=> await Task.Run(setLiberatedVisibleMenuItem);

		public void liberateVisible(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			try
			{
				SetQueueCollapseState(false);

				Serilog.Log.Logger.Information("Begin backing up visible library books");

				_viewModel.ProcessQueueViewModel.AddDownloadDecrypt(
					productsDisplay
					.GetVisibleBookEntries()
					.UnLiberated()
					);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up visible library books");
			}
		}
		public void replaceTagsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var dialog = new TagsBatchDialog();
			var result = dialog.ShowDialog();
			if (result != System.Windows.Forms.DialogResult.OK)
				return;

			var visibleLibraryBooks = productsDisplay.GetVisibleBookEntries();

			var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
				visibleLibraryBooks,
				$"Are you sure you want to replace tags in {0}?",
				"Replace tags?");

			if (confirmationResult != System.Windows.Forms.DialogResult.Yes)
				return;

			foreach (var libraryBook in visibleLibraryBooks)
				libraryBook.Book.UserDefinedItem.Tags = dialog.NewTags;
			LibraryCommands.UpdateUserDefinedItem(visibleLibraryBooks.Select(lb => lb.Book));
		}

		public void setDownloadedToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var dialog = new LiberatedStatusBatchDialog();
			var result = dialog.ShowDialog();
			if (result != System.Windows.Forms.DialogResult.OK)
				return;

			var visibleLibraryBooks = productsDisplay.GetVisibleBookEntries();

			var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
				visibleLibraryBooks,
				$"Are you sure you want to replace downloaded status in {0}?",
				"Replace downloaded status?");

			if (confirmationResult != System.Windows.Forms.DialogResult.Yes)
				return;

			foreach (var libraryBook in visibleLibraryBooks)
				libraryBook.Book.UserDefinedItem.BookStatus = dialog.BookLiberatedStatus;
			LibraryCommands.UpdateUserDefinedItem(visibleLibraryBooks.Select(lb => lb.Book));
		}

		public async void removeToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var visibleLibraryBooks = productsDisplay.GetVisibleBookEntries();

			var confirmationResult = MessageBoxLib.ShowConfirmationDialog(
				visibleLibraryBooks,
				$"Are you sure you want to remove {0} from Libation's library?",
				"Remove books from Libation?");

			if (confirmationResult != System.Windows.Forms.DialogResult.Yes)
				return;

			var visibleIds = visibleLibraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();
			await LibraryCommands.RemoveBooksAsync(visibleIds);
		}
		public async void productsDisplay_VisibleCountChanged(object sender, int qty)
		{
			_viewModel.VisibleCount = qty;

			await Task.Run(setLiberatedVisibleMenuItem);
		}
		void setLiberatedVisibleMenuItem()
			=> _viewModel.VisibleNotLiberated
				= productsDisplay
				.GetVisibleBookEntries()
				.Count(lb => lb.Book.UserDefinedItem.BookStatus == LiberatedStatus.NotLiberated);
	}
}
