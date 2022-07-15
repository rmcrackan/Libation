using ApplicationServices;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DataLayer;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2 : UserControl
	{
		/// <summary>Number of visible rows has changed</summary>
		public event EventHandler<int> VisibleCountChanged;
		public event EventHandler<int> RemovableCountChanged;
		public event EventHandler<LibraryBook> LiberateClicked;
		public event EventHandler InitialLoaded;


		public List<LibraryBook> GetVisibleBookEntries()
			=> bindingList
			.BookEntries()
			.Select(lbe => lbe.LibraryBook)
			.ToList();
		private IEnumerable<LibraryBookEntry2> GetAllBookEntries()
			=> bindingList
			.AllItems()
			.BookEntries();

		private ProductsDisplayViewModel _viewModel;
		private GridEntryBindingList2 bindingList => _viewModel.GridEntries;

		DataGridColumn removeGVColumn;

		public ProductsDisplay2()
		{
			InitializeComponent();

			Configure_Buttons();
			Configure_ColumnCustomization();
			Configure_Display();
			Configure_Filtering();
			Configure_ScanAndRemove();
			Configure_Sorting();

			foreach ( var column in productsGrid.Columns)
			{
				column.CustomSortComparer = new RowComparer(column);
			}

			if (Design.IsDesignMode)
			{
				using var context = DbContexts.GetContext();
				var book = context.GetLibraryBook_Flat_NoTracking("B017V4IM1G");
				productsGrid.DataContext = _viewModel = new ProductsDisplayViewModel(new List<LibraryBook> { book });
				return;
			}

		}
		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);

			productsGrid = this.FindControl<DataGrid>(nameof(productsGrid));
			productsGrid.Sorting += ProductsGrid_Sorting;
			productsGrid.CanUserSortColumns = true;

			removeGVColumn = productsGrid.Columns[0];
		}
	}
}
