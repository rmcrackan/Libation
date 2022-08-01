using Avalonia.Controls;
using HangoverAvalonia.ViewModels;

namespace HangoverAvalonia.Views
{
	public partial class MainWindow : Window
	{
		MainWindowViewModel _viewModel => DataContext as MainWindowViewModel;
		public MainWindow()
		{
			InitializeComponent();
		}

		public void Execute_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_viewModel.ExecuteQuery();
		}
	}
}
