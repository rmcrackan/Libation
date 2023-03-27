using Avalonia.Controls;

namespace LibationAvalonia.Dialogs
{
	public partial class TagsBatchDialog : DialogWindow
	{
		public string NewTags { get; set; }
		public TagsBatchDialog()
		{
			InitializeComponent();
			ControlToFocusOnShow = this.FindControl<TextBox>(nameof(EditTagsTb));

			DataContext = this;
		}

		public void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> SaveAndClose();
	}
}
