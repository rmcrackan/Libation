using Avalonia.Markup.Xaml;

namespace LibationAvalonia.Dialogs
{
	public partial class LiberatedStatusBatchAutoDialog : DialogWindow
    {
        public bool SetDownloaded { get; set; }
        public bool SetNotDownloaded { get; set; }

        public LiberatedStatusBatchAutoDialog()
		{
			InitializeComponent();
			DataContext = this;
		}
		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> SaveAndClose();
	}
}
