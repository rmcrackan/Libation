using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DataLayer;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2 : UserControl
	{
		public event EventHandler<LibraryBook> LiberateClicked;

		private ProductsDisplayViewModel _viewModel => DataContext as ProductsDisplayViewModel;

		public ProductsDisplay2()
		{
			InitializeComponent();

			Configure_Buttons();
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

		private void RemoveColumn_PropertyChanged(object sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
		{
			if (sender is DataGridColumn col && e.Property.Name == nameof(DataGridColumn.IsVisible))
			{
				col.DisplayIndex = 0;
				col.CanUserReorder = false;
			}
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);

			productsGrid = this.FindControl<DataGrid>(nameof(productsGrid));
		}
	}
}
