using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core.Threading;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
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
		private async void setLiberatedVisibleMenuItemAsync(object _, object __)
			=> await Task.Run(setLiberatedVisibleMenuItem);
		void setLiberatedVisibleMenuItem()
		{
			var notLiberated = productsDisplay.GetVisible().Count(lb => lb.Book.UserDefinedItem.BookStatus == DataLayer.LiberatedStatus.NotLiberated);
			this.UIThreadSync(() =>
			{
				if (notLiberated > 0)
				{
					liberateVisibleToolStripMenuItem_VisibleBooksMenu.Format(notLiberated);
					liberateVisibleToolStripMenuItem_VisibleBooksMenu.Enabled = true;

					liberateVisibleToolStripMenuItem_LiberateMenu.Format(notLiberated);
					liberateVisibleToolStripMenuItem_LiberateMenu.Enabled = true;
				}
				else
				{
					liberateVisibleToolStripMenuItem_VisibleBooksMenu.Text = "All visible books are liberated";
					liberateVisibleToolStripMenuItem_VisibleBooksMenu.Enabled = false;

					liberateVisibleToolStripMenuItem_LiberateMenu.Text = "All visible books are liberated";
					liberateVisibleToolStripMenuItem_LiberateMenu.Enabled = false;
				}
			});
		}

		private void liberateVisible(object sender, EventArgs e)
		{
			try
			{
				SetQueueCollapseState(false);

				Serilog.Log.Logger.Information("Begin backing up visible library books");

				processBookQueue1.AddDownloadDecrypt(
					productsDisplay
					.GetVisible()
					.UnLiberated()
					);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error occurred while backing up visible library books");
			}
		}

		private void replaceTagsToolStripMenuItem_Click(object sender, EventArgs e)
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

            visibleLibraryBooks.UpdateTags(dialog.NewTags);
        }

		private void setBookDownloadedManualToolStripMenuItem_Click(object sender, EventArgs e)
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

			visibleLibraryBooks.UpdateBookStatus(dialog.BookLiberatedStatus);
        }

        private void setPdfDownloadedManualToolStripMenuItem_Click(object sender, EventArgs e)
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

            visibleLibraryBooks.UpdatePdfStatus(dialog.BookLiberatedStatus);
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

            bulkSetStatus.Execute();
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

			//Not used for anything?
			var notLiberatedCount = productsDisplay.GetVisible().Count(lb => lb.Book.UserDefinedItem.BookStatus == DataLayer.LiberatedStatus.NotLiberated);
			
			await Task.Run(setLiberatedVisibleMenuItem);
		}
	}
}
