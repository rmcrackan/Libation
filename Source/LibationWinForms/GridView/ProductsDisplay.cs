using ApplicationServices;
using AudibleUtilities;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	public partial class ProductsDisplay : UserControl
	{
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;
		public event EventHandler<int> RemovableCountChanged;
		public event EventHandler<LibraryBook> LiberateClicked;
		public event EventHandler InitialLoaded;

		private bool hasBeenDisplayed;

		public ProductsDisplay()
		{
			InitializeComponent();
		}

		#region Button controls		

		private ImageDisplay imageDisplay;
		private async void productsGrid_CoverClicked(GridEntry liveGridEntry)
		{
			var picDefinition = new PictureDefinition(liveGridEntry.LibraryBook.Book.PictureLarge ?? liveGridEntry.LibraryBook.Book.PictureId, PictureSize.Native);
			var picDlTask = Task.Run(() => PictureStorage.GetPictureSynchronously(picDefinition));

			(_, byte[] initialImageBts) = PictureStorage.GetPicture(new PictureDefinition(liveGridEntry.LibraryBook.Book.PictureId, PictureSize._80x80));
			var windowTitle = $"{liveGridEntry.Title} - Cover";

			if (imageDisplay is null || imageDisplay.IsDisposed || !imageDisplay.Visible)
			{
				imageDisplay = new ImageDisplay();
				imageDisplay.RestoreSizeAndLocation(Configuration.Instance);
				imageDisplay.FormClosed += (_, _) => imageDisplay.SaveSizeAndLocation(Configuration.Instance);
				imageDisplay.Show(this);
			}

			imageDisplay.BookSaveDirectory = AudibleFileStorage.Audio.GetDestinationDirectory(liveGridEntry.LibraryBook);
			imageDisplay.PictureFileName = System.IO.Path.GetFileName(AudibleFileStorage.Audio.GetBooksDirectoryFilename(liveGridEntry.LibraryBook, ".jpg"));
			imageDisplay.Text = windowTitle;
			imageDisplay.CoverPicture = initialImageBts;
			imageDisplay.CoverPicture = await picDlTask;
		}

		private void productsGrid_DescriptionClicked(GridEntry liveGridEntry, Rectangle cellRectangle)
		{
			var displayWindow = new DescriptionDisplay
			{
				SpawnLocation = PointToScreen(cellRectangle.Location + new Size(cellRectangle.Width, 0)),
				DescriptionText = liveGridEntry.LongDescription,
				BorderThickness = 2,
			};

			void CloseWindow(object o, EventArgs e)
			{
				displayWindow.Close();
			}

			productsGrid.Scroll += CloseWindow;
			displayWindow.FormClosed += (_, _) => productsGrid.Scroll -= CloseWindow;
			displayWindow.Show(this);
		}

		private void productsGrid_DetailsClicked(LibraryBookEntry liveGridEntry)
		{
			var bookDetailsForm = new BookDetailsDialog(liveGridEntry.LibraryBook);
			if (bookDetailsForm.ShowDialog() == DialogResult.OK)
				liveGridEntry.Commit(bookDetailsForm.NewTags, bookDetailsForm.BookLiberatedStatus, bookDetailsForm.PdfLiberatedStatus);
		}

		#endregion

		#region Scan and Remove Books

		public void CloseRemoveBooksColumn()
			=> productsGrid.RemoveColumnVisible = false;

		public async void RemoveCheckedBooksAsync()
		{
			var selectedBooks = productsGrid.GetAllBookEntries().Where(lbe => lbe.Remove is RemoveStatus.Removed).ToList();

			if (selectedBooks.Count == 0)
				return;

			var libraryBooks = selectedBooks.Select(rge => rge.LibraryBook).ToList();
			var result = MessageBoxLib.ShowConfirmationDialog(
				libraryBooks,
				$"Are you sure you want to remove {selectedBooks.Count} books from Libation's library?",
				"Remove books from Libation?");

			if (result != DialogResult.Yes)
				return;

			productsGrid.RemoveBooks(selectedBooks);
			var idsToRemove = libraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();
			var removeLibraryBooks = await LibraryCommands.RemoveBooksAsync(idsToRemove);
		}

		public async Task ScanAndRemoveBooksAsync(params Account[] accounts)
		{
			RemovableCountChanged?.Invoke(this, 0);
			productsGrid.RemoveColumnVisible = true;

			try
			{
				if (accounts is null || accounts.Length == 0)
					return;

				var allBooks = productsGrid.GetAllBookEntries();
				var lib = allBooks
					.Select(lbe => lbe.LibraryBook)
					.Where(lb => !lb.Book.HasLiberated());

				var removedBooks = await LibraryCommands.FindInactiveBooks(Login.WinformLoginChoiceEager.ApiExtendedFunc, lib, accounts);

				var removable = allBooks.Where(lbe => removedBooks.Any(rb => rb.Book.AudibleProductId == lbe.AudibleProductId)).ToList();

				foreach (var r in removable)
					r.Remove = RemoveStatus.Removed;

				productsGrid_RemovableCountChanged(this, null);
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(
					this,
					"Error scanning library. You may still manually select books to remove from Libation's library.",
					"Error scanning library",
					ex);
			}
		}

		#endregion

		#region UI display functions		

		public void Display()
		{
			try
			{
				// don't return early if lib size == 0. this will not update correctly if all books are removed
				var lib = DbContexts.GetLibrary_Flat_NoTracking(includeParents: true);

				if (!hasBeenDisplayed)
				{
					// bind
					productsGrid.BindToGrid(lib);
					hasBeenDisplayed = true;
					InitialLoaded?.Invoke(this, new());
				}
				else
					productsGrid.UpdateGrid(lib);
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error displaying library in {0}", nameof(ProductsDisplay));
			}
		}

		#endregion

		#region Filter

		public void Filter(string searchString)
			=> productsGrid.Filter(searchString);

		#endregion

		internal List<LibraryBook> GetVisible() => productsGrid.GetVisibleBooks().ToList();

		private void productsGrid_VisibleCountChanged(object sender, int count)
		{
			VisibleCountChanged?.Invoke(this, count);
		}

		private void productsGrid_LiberateClicked(LibraryBookEntry liveGridEntry)
		{
			if (liveGridEntry.LibraryBook.Book.UserDefinedItem.BookStatus is not LiberatedStatus.Error)
				LiberateClicked?.Invoke(this, liveGridEntry.LibraryBook);
		}

		private void productsGrid_RemovableCountChanged(object sender, EventArgs e)
		{
			RemovableCountChanged?.Invoke(sender, productsGrid.GetAllBookEntries().Count(lbe => lbe.Remove is RemoveStatus.Removed));
		}
	}
}
