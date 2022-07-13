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
		DataGridColumn liberateGVColumn;
		DataGridColumn coverGVColumn;
		DataGridColumn titleGVColumn;
		DataGridColumn authorsGVColumn;
		DataGridColumn narratorsGVColumn;
		DataGridColumn lengthGVColumn;
		DataGridColumn seriesGVColumn;
		DataGridColumn descriptionGVColumn;
		DataGridColumn categoryGVColumn;
		DataGridColumn productRatingGVColumn;
		DataGridColumn purchaseDateGVColumn;
		DataGridColumn myRatingGVColumn;
		DataGridColumn miscGVColumn;
		DataGridColumn tagAndDetailsGVColumn;		 

		public ProductsDisplay2()
		{
			InitializeComponent();

			Configure_Buttons();
			Configure_ColumnCustomization();
			Configure_Display();
			Configure_Filtering();
			Configure_ScanAndRemove();
			Configure_Sorting();

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
			productsGrid.LoadingRow += ProductsGrid_LoadingRow;

			removeGVColumn = productsGrid.Columns[0];
			liberateGVColumn = productsGrid.Columns[1];
			coverGVColumn = productsGrid.Columns[2];
			titleGVColumn = productsGrid.Columns[3];
			authorsGVColumn = productsGrid.Columns[4];
			narratorsGVColumn = productsGrid.Columns[5];
			lengthGVColumn = productsGrid.Columns[6];
			seriesGVColumn = productsGrid.Columns[7];
			descriptionGVColumn = productsGrid.Columns[8];
			categoryGVColumn = productsGrid.Columns[9];
			productRatingGVColumn = productsGrid.Columns[10];
			purchaseDateGVColumn = productsGrid.Columns[11];
			myRatingGVColumn = productsGrid.Columns[12];
			miscGVColumn = productsGrid.Columns[13];
			tagAndDetailsGVColumn = productsGrid.Columns[14];

			RegisterCustomColumnComparers();
		}

		private static object tagObj = new();
		private void ProductsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
		{
			if (e.Row.Tag == tagObj)
				return;
			e.Row.Tag = tagObj;

			static IBrush GetRowColor(DataGridRow row)
				=> row.DataContext is GridEntry2 gEntry
				&& gEntry is LibraryBookEntry2 lbEntry
				&& lbEntry.Parent is not null
				? App.SeriesEntryGridBackgroundBrush
				: null;

			e.Row.Background = GetRowColor(e.Row);
			e.Row.DataContextChanged += (sender, e) =>
			{
				var row = sender as DataGridRow;
				row.Background = GetRowColor(row);
			};
		}
	}
}
