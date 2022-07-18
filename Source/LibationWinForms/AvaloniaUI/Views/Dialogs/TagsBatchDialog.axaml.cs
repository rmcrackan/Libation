using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
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

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> SaveAndClose();
	}
}
