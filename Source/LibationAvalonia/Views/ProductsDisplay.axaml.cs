using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using LibationAvalonia.ViewModels;
using LibationAvalonia.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Views
{
	public partial class ProductsDisplay : UserControl
	{
		public event EventHandler<LibraryBook> LiberateClicked;

		private ProductsDisplayViewModel _viewModel => DataContext as ProductsDisplayViewModel;
		ImageDisplayDialog imageDisplayDialog;

		public ProductsDisplay()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
			{
				using var context = DbContexts.GetContext();
				List<LibraryBook> sampleEntries = new()
				{
					//context.GetLibraryBook_Flat_NoTracking("B00DCD0OXU"),
					context.GetLibraryBook_Flat_NoTracking("B017V4IM1G"),
					context.GetLibraryBook_Flat_NoTracking("B017V4IWVG"),
					context.GetLibraryBook_Flat_NoTracking("B017V4JA2Q"),
					context.GetLibraryBook_Flat_NoTracking("B017V4NUPO"),
					context.GetLibraryBook_Flat_NoTracking("B017V4NMX4"),
					context.GetLibraryBook_Flat_NoTracking("B017V4NOZ0"),
					context.GetLibraryBook_Flat_NoTracking("B017WJ5ZK6")
				};

				var pdvm = new ProductsDisplayViewModel();
				pdvm.DisplayBooks(sampleEntries);
				DataContext = pdvm;

				return;
			}

			Configure_ColumnCustomization();

			foreach (var column in productsGrid.Columns)
			{
				column.CustomSortComparer = new RowComparer(column);
			}
		}

		private void ProductsGrid_Sorting(object sender, DataGridColumnEventArgs e)
		{

		}

		private void RemoveColumn_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (sender is DataGridColumn col && e.Property.Name == nameof(DataGridColumn.IsVisible))
			{
				col.DisplayIndex = 0;
				col.CanUserReorder = false;
			}
		}

		public void DataGrid_CopyToClipboard(object sender, DataGridRowClipboardEventArgs  e)
		{
		
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);

			productsGrid = this.FindControl<DataGrid>(nameof(productsGrid));
		}

		#region Column Customizations

		private void Configure_ColumnCustomization()
		{
			if (Design.IsDesignMode) return;

			productsGrid.ColumnDisplayIndexChanged += ProductsGrid_ColumnDisplayIndexChanged;

			var config = Configuration.Instance;
			var gridColumnsVisibilities = config.GridColumnsVisibilities;
			var displayIndices = config.GridColumnsDisplayIndices;

			var contextMenu = new ContextMenu();
			contextMenu.MenuClosed += ContextMenu_MenuClosed;
			contextMenu.ContextMenuOpening += ContextMenu_ContextMenuOpening;
			List<Control> menuItems = new();
			contextMenu.Items = menuItems;

			menuItems.Add(new MenuItem { Header = "Show / Hide Columns" });
			menuItems.Add(new MenuItem { Header = "-" });

			var HeaderCell_PI = typeof(DataGridColumn).GetProperty("HeaderCell", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

			foreach (var column in productsGrid.Columns)
			{
				var itemName = column.SortMemberPath;

				if (itemName == nameof(GridEntry.Remove))
					continue;

				menuItems.Add
					(
						new MenuItem
						{
							Header = ((string)column.Header).Replace((char)0xa, ' '),
							Tag = column,
							Margin = new Thickness(6, 0),
							Icon = new CheckBox
							{
								Width = 50,
							}
						}
					);

				var headercell = HeaderCell_PI.GetValue(column) as DataGridColumnHeader;
				headercell.ContextMenu = contextMenu;

				column.IsVisible = gridColumnsVisibilities.GetValueOrDefault(itemName, true);
			}

			//We must set DisplayIndex properties in ascending order
			foreach (var itemName in displayIndices.OrderBy(i => i.Value).Select(i => i.Key))
			{
				if (!productsGrid.Columns.Any(c => c.SortMemberPath == itemName))
					continue;

				var column = productsGrid.Columns
					.Single(c => c.SortMemberPath == itemName);

				column.DisplayIndex = displayIndices.GetValueOrDefault(itemName, productsGrid.Columns.IndexOf(column));
			}
		}

		private void ContextMenu_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			var contextMenu = sender as ContextMenu;
			foreach (var mi in contextMenu.Items.OfType<MenuItem>())
			{
				if (mi.Tag is DataGridColumn column)
				{
					var cbox = mi.Icon as CheckBox;
					cbox.IsChecked = column.IsVisible;
				}
			}
		}

		private void ContextMenu_MenuClosed(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var contextMenu = sender as ContextMenu;
			var config = Configuration.Instance;
			var dictionary = config.GridColumnsVisibilities;

			foreach (var mi in contextMenu.Items.OfType<MenuItem>())
			{
				if (mi.Tag is DataGridColumn column)
				{
					var cbox = mi.Icon as CheckBox;
					column.IsVisible = cbox.IsChecked == true;
					dictionary[column.SortMemberPath] = cbox.IsChecked == true;
				}
			}

			//If all columns are hidden, register the context menu on the grid so users can unhide.
			if (!productsGrid.Columns.Any(c => c.IsVisible))
				productsGrid.ContextMenu = contextMenu;
			else
				productsGrid.ContextMenu = null;

			config.GridColumnsVisibilities = dictionary;
		}

		private void ProductsGrid_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
		{
			var config = Configuration.Instance;

			var dictionary = config.GridColumnsDisplayIndices;
			dictionary[e.Column.SortMemberPath] = e.Column.DisplayIndex;
			config.GridColumnsDisplayIndices = dictionary;
		}

		#endregion

		#region Button Click Handlers

		public void LiberateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button.DataContext is SeriesEntry sEntry)
			{
				_viewModel.ToggleSeriesExpanded(sEntry);

				//Expanding and collapsing reset the list, which will cause focus to shift
				//to the topright cell. Reset focus onto the clicked button's cell.
				(sender as Button).Parent?.Focus();
			}
			else if (button.DataContext is LibraryBookEntry lbEntry)
			{
				LiberateClicked?.Invoke(this, lbEntry.LibraryBook);
			}
		}

		public void CloseImageDisplay()
		{
			if (imageDisplayDialog is not null && imageDisplayDialog.IsVisible)
				imageDisplayDialog.Close();
		}

		public void Cover_Click(object sender, Avalonia.Input.TappedEventArgs args)
		{
			if (sender is not Image tblock || tblock.DataContext is not GridEntry gEntry)
				return;


			if (imageDisplayDialog is null || !imageDisplayDialog.IsVisible)
			{
				imageDisplayDialog = new ImageDisplayDialog();
			}

			var picDef = new PictureDefinition(gEntry.LibraryBook.Book.PictureLarge ?? gEntry.LibraryBook.Book.PictureId, PictureSize.Native);

			void PictureCached(object sender, PictureCachedEventArgs e)
			{
				if (e.Definition.PictureId == picDef.PictureId)
					imageDisplayDialog.CoverBytes = e.Picture;

				PictureStorage.PictureCached -= PictureCached;
			}

			PictureStorage.PictureCached += PictureCached;
			(bool isDefault, byte[] initialImageBts) = PictureStorage.GetPicture(picDef);


			var windowTitle = $"{gEntry.Title} - Cover";


			imageDisplayDialog.BookSaveDirectory = AudibleFileStorage.Audio.GetDestinationDirectory(gEntry.LibraryBook);
			imageDisplayDialog.PictureFileName = System.IO.Path.GetFileName(AudibleFileStorage.Audio.GetBooksDirectoryFilename(gEntry.LibraryBook, ".jpg"));
			imageDisplayDialog.Title = windowTitle;
			imageDisplayDialog.CoverBytes = initialImageBts;

			if (!isDefault)
				PictureStorage.PictureCached -= PictureCached;

			if (!imageDisplayDialog.IsVisible)
				imageDisplayDialog.Show();
		}

		public void Description_Click(object sender, Avalonia.Input.TappedEventArgs args)
		{
			if (sender is TextBlock tblock && tblock.DataContext is GridEntry gEntry)
			{
				var pt = tblock.Parent.PointToScreen(tblock.Parent.Bounds.TopRight);
				var displayWindow = new DescriptionDisplayDialog
				{
					SpawnLocation = new Point(pt.X, pt.Y),
					DescriptionText = gEntry.LongDescription,
				};

				void CloseWindow(object o, DataGridRowEventArgs e)
				{
					displayWindow.Close();
				}
				productsGrid.LoadingRow += CloseWindow;
				displayWindow.Closing += (_, _) =>
				{
					productsGrid.LoadingRow -= CloseWindow;
				};

				displayWindow.Show();
			}
		}

		BookDetailsDialog bookDetailsForm;

		public void OnTagsButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button.DataContext is LibraryBookEntry lbEntry && VisualRoot is Window window)
			{
				if (bookDetailsForm is null || !bookDetailsForm.IsVisible)
				{
					bookDetailsForm = new BookDetailsDialog(lbEntry.LibraryBook);
					bookDetailsForm.Show(window);
				}
				else
					bookDetailsForm.LibraryBook = lbEntry.LibraryBook;
			}
		}

		#endregion
	}
}
