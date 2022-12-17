namespace HangoverAvalonia.Views
{
	public partial class MainWindow
	{
		private void databaseTab_VisibleChanged(bool isVisible)
		{
			if (!isVisible)
				return;
		}

		public void Execute_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_viewModel.ExecuteQuery();
		}
	}
}
