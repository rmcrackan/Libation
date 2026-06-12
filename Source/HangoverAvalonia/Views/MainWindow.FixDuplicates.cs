namespace HangoverAvalonia.Views;

public partial class MainWindow
{
	private void fixDuplicatesTab_VisibleChanged(bool isVisible)
	{
		if (!isVisible)
			return;

		_viewModel.RefreshDuplicateAsinStatus();
	}

	public void ScanDuplicateAsins_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> _viewModel.ScanDuplicateAsins();

	public async void RemoveDuplicateAsins_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await _viewModel.RemoveDuplicateAsinsAsync();
}
