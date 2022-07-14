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
			// init formattable
			visibleCountLbl.Format(0);
			liberateVisibleToolStripMenuItem_VisibleBooksMenu.Format(0);
			liberateVisibleToolStripMenuItem_LiberateMenu.Format(0);

			// top menu strip
			visibleBooksToolStripMenuItem.Format(0);

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

				processBookQueue1.AddDownloadDecrypt(
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
			Dispatcher.UIThread.Post(() =>
			{
				// bottom-left visible count
				visibleCountLbl.Format(qty);

				// top menu strip
				visibleBooksToolStripMenuItem.Format(qty);
				visibleBooksToolStripMenuItem.IsEnabled = qty > 0;
			});

			//Not used for anything?
			var notLiberatedCount = productsDisplay.GetVisibleBookEntries().Count(lb => lb.Book.UserDefinedItem.BookStatus == DataLayer.LiberatedStatus.NotLiberated);

			await Task.Run(setLiberatedVisibleMenuItem);
		}
		void setLiberatedVisibleMenuItem()
		{
			var notLiberated = productsDisplay.GetVisibleBookEntries().Count(lb => lb.Book.UserDefinedItem.BookStatus == DataLayer.LiberatedStatus.NotLiberated);

			Dispatcher.UIThread.Post(() =>
			{
				if (notLiberated > 0)
				{
					liberateVisibleToolStripMenuItem_VisibleBooksMenu.Format(notLiberated);
					liberateVisibleToolStripMenuItem_VisibleBooksMenu.IsEnabled = true;

					liberateVisibleToolStripMenuItem_LiberateMenu.Format(notLiberated);
					liberateVisibleToolStripMenuItem_LiberateMenu.IsEnabled = true;
				}
				else
				{
					liberateVisibleToolStripMenuItem_VisibleBooksMenu.Header = "All visible books are liberated";
					liberateVisibleToolStripMenuItem_VisibleBooksMenu.IsEnabled = false;

					liberateVisibleToolStripMenuItem_LiberateMenu.Header = "All visible books are liberated";
					liberateVisibleToolStripMenuItem_LiberateMenu.IsEnabled = false;
				}
			});
		}
	}
}
