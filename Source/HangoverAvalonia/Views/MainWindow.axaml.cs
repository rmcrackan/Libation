using AppScaffolding;
using Avalonia.Controls;
using HangoverAvalonia.ViewModels;

namespace HangoverAvalonia.Views;

public partial class MainWindow : Window
{
	MainVM _viewModel => DataContext as MainVM;
	public MainWindow()
	{
		InitializeComponent();

		var config = LibationScaffolding.RunPreConfigMigrations();
		LibationScaffolding.RunPostConfigMigrations(config);
		LibationScaffolding.RunPostMigrationScaffolding(Variety.Chardonnay, config);
	}

	public void OnLoad()
	{
		_viewModel.ConfirmDbMutationAsync = action => HangoverMutationConfirm.ConfirmAsync(this, action);

		fixDuplicatesTab.PropertyChanged += (_, e) => { if (e.Property.Name == nameof(TabItem.IsSelected)) fixDuplicatesTab_VisibleChanged(fixDuplicatesTab.IsSelected); };
		deletedTab.PropertyChanged += (_, e) => { if (e.Property.Name == nameof(TabItem.IsSelected)) deletedTab_VisibleChanged(deletedTab.IsSelected); };
	}
}
