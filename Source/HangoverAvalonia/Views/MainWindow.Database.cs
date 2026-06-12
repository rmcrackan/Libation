namespace HangoverAvalonia.Views;

public partial class MainWindow
{
	public async void Execute_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await _viewModel.ExecuteQueryAsync();
}
