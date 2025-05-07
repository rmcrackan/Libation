namespace HangoverAvalonia.Views;

public partial class MainWindow
{
	private void deletedTab_VisibleChanged(bool isVisible)
	{
		if (!isVisible)
			return;

		_viewModel.TrashBinViewModel.Reload();
	}
}
