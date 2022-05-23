using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using DataLayer;
using Dinah.Core.Windows.Forms;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{

	#region // legacy instructions to update data_grid_view
	// INSTRUCTIONS TO UPDATE DATA_GRID_VIEW
	// - delete current DataGridView
	// - view > other windows > data sources
	// - refresh
	// OR
	// - Add New Data Source
	//   Object. Next
	//   LibationWinForms
	//     AudibleDTO
	//       GridEntry
	// - go to Design view
	// - click on Data Sources > ProductItem. dropdown: DataGridView
	// - drag/drop ProductItem on design surface
	//
	// as of august 2021 this does not work in vs2019 with .net5 projects
	// VS has improved since then with .net6+ but I haven't checked again
	#endregion


	public partial class ProductsDisplay : UserControl
	{
		public event EventHandler<LibraryBook> LiberateClicked;
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;

		// alias

		private ProductsGrid grid;

		public ProductsDisplay()
		{
			InitializeComponent();

			grid = new ProductsGrid();
			grid.Dock = DockStyle.Fill;
			Controls.Add(grid);

			if (this.DesignMode)
				return;

			grid.LiberateClicked += (_, book) => LiberateClicked?.Invoke(this, book.LibraryBook);
			grid.DetailsClicked += Grid_DetailsClicked;
			grid.CoverClicked += Grid_CoverClicked;
			grid.DescriptionClicked += Grid_DescriptionClicked1;
		}

		#region Button controls		

		private ImageDisplay imageDisplay;
		private async void Grid_CoverClicked(DataGridViewCellEventArgs e, LibraryBookEntry liveGridEntry)
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

		private void Grid_DescriptionClicked1(DataGridViewCellEventArgs e, LibraryBookEntry liveGridEntry, Rectangle cellRectangle)
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

			grid.Scroll += CloseWindow;
			displayWindow.FormClosed += (_, _) => grid.Scroll -= CloseWindow;
			displayWindow.Show(this);
		}


		private void Grid_DetailsClicked(DataGridViewCellEventArgs e, LibraryBookEntry liveGridEntry)
		{
			var bookDetailsForm = new BookDetailsDialog(liveGridEntry.LibraryBook);
			if (bookDetailsForm.ShowDialog() == DialogResult.OK)
				liveGridEntry.Commit(bookDetailsForm.NewTags, bookDetailsForm.BookLiberatedStatus, bookDetailsForm.PdfLiberatedStatus);
		}

		#endregion

		#region UI display functions

		private bool hasBeenDisplayed;
		public event EventHandler InitialLoaded;
		public void Display()
		{
			// don't return early if lib size == 0. this will not update correctly if all books are removed
			var lib = DbContexts.GetLibrary_Flat_NoTracking();

			if (!hasBeenDisplayed)
			{
				// bind
				grid.bindToGrid(lib);
				hasBeenDisplayed = true;
				InitialLoaded?.Invoke(this, new());
				VisibleCountChanged?.Invoke(this, grid.GetVisible().Count());
			}
			else
				grid.updateGrid(lib);

		}

		#endregion

		#region Filter

		public void Filter(string searchString)
			=> grid.Filter(searchString);

		#endregion

		internal List<LibraryBook> GetVisible() => grid.GetVisible().ToList();
	}
}
