using ApplicationServices;
using Avalonia.Threading;
using DataLayer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
	public partial class MainWindow
	{
		private void Configure_VisibleBooks()
		{
			LibraryCommands.BookUserDefinedItemCommitted += setLiberatedVisibleMenuItemAsync;
		}

		private async void setLiberatedVisibleMenuItemAsync(object _, object __)
			=> await Dispatcher.UIThread.InvokeAsync(setLiberatedVisibleMenuItem);

		public void liberateVisible(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			try
			{
				SetQueueCollapseState(false);

				Serilog.Log.Logger.Information("Begin backing up visible library books");

				_viewModel.ProcessQueue.AddDownloadDecrypt(
					_viewModel
					.ProductsDisplay
					.GetVisibleBookEntries()
					.UnLiberated()
					);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up visible library books");
			}
		}
		public async void replaceTagsToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var dialog = new Dialogs.TagsBatchDialog();
			var result = await dialog.ShowDialog<DialogResult>(this);
			if (result != DialogResult.OK)
				return;

			var visibleLibraryBooks = _viewModel.ProductsDisplay.GetVisibleBookEntries();

			var confirmationResult = await MessageBox.ShowConfirmationDialog(
				this,
				visibleLibraryBooks,
                // do not use `$` string interpolation. See impl.
                "Are you sure you want to replace tags in {0}?",
				"Replace tags?");

			if (confirmationResult != DialogResult.Yes)
				return;

			visibleLibraryBooks.UpdateTags(dialog.NewTags);
        }

		public async void setBookDownloadedManualToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var dialog = new Dialogs.LiberatedStatusBatchManualDialog();
			var result = await dialog.ShowDialog<DialogResult>(this);
			if (result != DialogResult.OK)
				return;

			var visibleLibraryBooks = _viewModel.ProductsDisplay.GetVisibleBookEntries();

			var confirmationResult = await MessageBox.ShowConfirmationDialog(
				this,
				visibleLibraryBooks,
                // do not use `$` string interpolation. See impl.
                "Are you sure you want to replace book downloaded status in {0}?",
				"Replace downloaded status?");

			if (confirmationResult != DialogResult.Yes)
				return;

            visibleLibraryBooks.UpdateBookStatus(dialog.BookLiberatedStatus);
        }

        public async void setPdfDownloadedManualToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
        {
            var dialog = new Dialogs.LiberatedStatusBatchManualDialog(isPdf: true);
            var result = await dialog.ShowDialog<DialogResult>(this);
            if (result != DialogResult.OK)
                return;

            var visibleLibraryBooks = _viewModel.ProductsDisplay.GetVisibleBookEntries();

            var confirmationResult = await MessageBox.ShowConfirmationDialog(
                this,
                visibleLibraryBooks,
                // do not use `$` string interpolation. See impl.
                "Are you sure you want to replace PDF downloaded status in {0}?",
                "Replace downloaded status?");

            if (confirmationResult != DialogResult.Yes)
                return;

            visibleLibraryBooks.UpdatePdfStatus(dialog.BookLiberatedStatus);
        }

        public async void setDownloadedAutoToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
        {
            var dialog = new Dialogs.LiberatedStatusBatchAutoDialog();
            var result = await dialog.ShowDialog<DialogResult>(this);
            if (result != DialogResult.OK)
                return;

            var bulkSetStatus = new BulkSetDownloadStatus(_viewModel.ProductsDisplay.GetVisibleBookEntries(), dialog.SetDownloaded, dialog.SetNotDownloaded);
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

            bulkSetStatus.Execute();
        }

        public async void removeToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var visibleLibraryBooks = _viewModel.ProductsDisplay.GetVisibleBookEntries();

			var confirmationResult = await MessageBox.ShowConfirmationDialog(
				this,
				visibleLibraryBooks,
                // do not use `$` string interpolation. See impl.
                "Are you sure you want to remove {0} from Libation's library?",
				"Remove books from Libation?",
				MessageBoxDefaultButton.Button2);

			if (confirmationResult != DialogResult.Yes)
				return;

			var visibleIds = visibleLibraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();
			await LibraryCommands.RemoveBooksAsync(visibleIds);
		}
		public async void ProductsDisplay_VisibleCountChanged(object sender, int qty)
		{
			_viewModel.VisibleCount = qty;

			await Dispatcher.UIThread.InvokeAsync(setLiberatedVisibleMenuItem);
		}
		void setLiberatedVisibleMenuItem()
			=> _viewModel.VisibleNotLiberated
				= _viewModel.ProductsDisplay
				.GetVisibleBookEntries()
				.Count(lb => lb.Book.UserDefinedItem.BookStatus == LiberatedStatus.NotLiberated);
	}
}
