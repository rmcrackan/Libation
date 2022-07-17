using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.AvaloniaUI.ViewModels;
using LibationWinForms.AvaloniaUI.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class ProductsDisplay2 : UserControl
	{
		public event EventHandler<LibraryBook> LiberateClicked;

		private ProductsDisplayViewModel _viewModel => DataContext as ProductsDisplayViewModel;
		private GridView.ImageDisplay imageDisplay;

		public ProductsDisplay2()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
			{
				using var context = DbContexts.GetContext();
				List<GridEntry2> sampleEntries = new()
				{
					new LibraryBookEntry2(context.GetLibraryBook_Flat_NoTracking("B017V4IM1G")),
					new LibraryBookEntry2(context.GetLibraryBook_Flat_NoTracking("B017V4IWVG")),
					new LibraryBookEntry2(context.GetLibraryBook_Flat_NoTracking("B017V4JA2Q")),
					new LibraryBookEntry2(context.GetLibraryBook_Flat_NoTracking("B017V4NUPO")),
					new LibraryBookEntry2(context.GetLibraryBook_Flat_NoTracking("B017V4NMX4")),
					new LibraryBookEntry2(context.GetLibraryBook_Flat_NoTracking("B017V4NOZ0")),
					new LibraryBookEntry2(context.GetLibraryBook_Flat_NoTracking("B017WJ5ZK6")),
				};
				DataContext = new ProductsDisplayViewModel(sampleEntries);
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
			_viewModel.Sort(e.Column);
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

				if (itemName == nameof(GridEntry2.Remove))
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

		BookDetailsDialog bookDetailsForm;

		public void OnTagsButtonClick(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var button = args.Source as Button;

			if (button.DataContext is LibraryBookEntry2 lbEntry && VisualRoot is Window window)
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
