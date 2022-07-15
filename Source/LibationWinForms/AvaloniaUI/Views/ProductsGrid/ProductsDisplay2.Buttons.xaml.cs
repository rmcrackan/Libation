using Avalonia;
using Avalonia.Controls;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2
	{
		private GridView.ImageDisplay imageDisplay;
		private void Configure_Buttons() { }

		public void LiberateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button.DataContext is SeriesEntrys2 sEntry)
			{
				_viewModel.ToggleSeriesExpanded(sEntry);

				//Expanding and collapsing reset the list, which will cause focus to shift
				//to the topright cell. Reset focus onto the clicked button's cell.
				((sender as Control).Parent.Parent as DataGridCell)?.Focus();
			}
			else if (button.DataContext is LibraryBookEntry2 lbEntry)
			{
				LiberateClicked?.Invoke(this, lbEntry.LibraryBook);
			}
		}

		public void Cover_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			if (sender is not Image tblock || tblock.DataContext is not GridEntry2 gEntry)
				return;

			var picDef = new PictureDefinition(gEntry.LibraryBook.Book.PictureLarge ?? gEntry.LibraryBook.Book.PictureId, PictureSize.Native);

			void PictureCached(object sender, PictureCachedEventArgs e)
			{
				if (e.Definition.PictureId == picDef.PictureId)
					imageDisplay.CoverPicture = e.Picture;

				PictureStorage.PictureCached -= PictureCached;
			}

			PictureStorage.PictureCached += PictureCached;
			(bool isDefault, byte[] initialImageBts) = PictureStorage.GetPicture(picDef);

			var windowTitle = $"{gEntry.Title} - Cover";

			if (imageDisplay is null || imageDisplay.IsDisposed || !imageDisplay.Visible)
			{
				imageDisplay = new GridView.ImageDisplay();
				imageDisplay.RestoreSizeAndLocation(Configuration.Instance);
				imageDisplay.FormClosed += (_, _) => imageDisplay.SaveSizeAndLocation(Configuration.Instance);				
			}

			imageDisplay.BookSaveDirectory = AudibleFileStorage.Audio.GetDestinationDirectory(gEntry.LibraryBook);
			imageDisplay.PictureFileName = System.IO.Path.GetFileName(AudibleFileStorage.Audio.GetBooksDirectoryFilename(gEntry.LibraryBook, ".jpg"));
			imageDisplay.Text = windowTitle;
			imageDisplay.CoverPicture = initialImageBts;
			if (!isDefault)
				PictureStorage.PictureCached -= PictureCached;

			if (!imageDisplay.Visible)
				imageDisplay.Show(null);
		}

		public void Description_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			if (sender is TextBlock tblock && tblock.DataContext is GridEntry2 gEntry)
			{
				var pt = tblock.Parent.PointToScreen(tblock.Parent.Bounds.TopRight);
				var displayWindow = new GridView.DescriptionDisplay
				{
					SpawnLocation = new System.Drawing.Point(pt.X, pt.Y),
					DescriptionText = gEntry.LongDescription,
					BorderThickness = 2,
				};

				void CloseWindow(object o, DataGridRowEventArgs e)
				{
					displayWindow.Close();
				}
				productsGrid.LoadingRow += CloseWindow;
				displayWindow.FormClosed += (_, _) =>
				{
					productsGrid.LoadingRow -= CloseWindow;
				};

				displayWindow.Show();
			}
		}

		public void OnTagsButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button.DataContext is LibraryBookEntry2 lbEntry)
			{
				var bookDetailsForm = new LibationWinForms.Dialogs.BookDetailsDialog(lbEntry.LibraryBook);
				if (bookDetailsForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					lbEntry.Commit(bookDetailsForm.NewTags, bookDetailsForm.BookLiberatedStatus, bookDetailsForm.PdfLiberatedStatus);
			}
		}
	}
}
