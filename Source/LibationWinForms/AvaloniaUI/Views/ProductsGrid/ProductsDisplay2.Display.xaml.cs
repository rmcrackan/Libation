using ApplicationServices;
using Avalonia.Controls;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2
	{
		private void Configure_Display() { }

		public async Task Display()
		{
			try
			{ 
				var dbBooks = DbContexts.GetLibrary_Flat_NoTracking(includeParents: true);

				if (_viewModel is null)
				{
					_viewModel = new ProductsDisplayViewModel(dbBooks);
					await Dispatcher.UIThread.InvokeAsync(() => InitialLoaded?.Invoke(this, EventArgs.Empty));

					int bookEntryCount = bindingList.BookEntries().Count();
					await Dispatcher.UIThread.InvokeAsync(() => VisibleCountChanged?.Invoke(this, bookEntryCount));

					//Avalonia displays items in the DataConncetion from an internal copy of
					//the bound list, not the actual bound list. So we need to reflect to get
					//the current display order and set each GridEntry.ListIndex correctly.
					var DataConnection_PI = typeof(DataGrid).GetProperty("DataConnection", BindingFlags.NonPublic | BindingFlags.Instance);
					var DataSource_PI = DataConnection_PI.PropertyType.GetProperty("DataSource", BindingFlags.Public | BindingFlags.Instance);

					bindingList.CollectionChanged += (s, e) =>
					{
						var displayListGE = ((IEnumerable)DataSource_PI.GetValue(DataConnection_PI.GetValue(productsGrid))).Cast<GridEntry2>();
						int index = 0;
						foreach (var di in displayListGE)
						{
							di.ListIndex = index++;
						}
					};

					//Assign the viewmodel after we subscribe to CollectionChanged
					//so that out handler executes first.
					productsGrid.DataContext = _viewModel;
				}
				else
				{
					//List is already displayed. Replace all items with new ones, refilter, and re-sort
					string existingFilter = _viewModel?.GridEntries?.Filter;
					var newEntries = ProductsDisplayViewModel.CreateGridEntries(dbBooks);
					await Dispatcher.UIThread.InvokeAsync(() =>
					{
						bindingList.ReplaceList(newEntries);
						bindingList.Filter = existingFilter;
						ReSort();
					});
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error displaying library in {0}", nameof(ProductsDisplay2));
			}
		}
	}
}
