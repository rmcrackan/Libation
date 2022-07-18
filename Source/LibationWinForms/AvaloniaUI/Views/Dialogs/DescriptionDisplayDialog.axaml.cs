using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{
	public partial class DescriptionDisplayDialog : Window
	{
		public Point SpawnLocation { get; set; }
		public string DescriptionText { get; init; }
		public DescriptionDisplayDialog()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
			DescriptionTextBox = this.FindControl<TextBox>(nameof(DescriptionTextBox));
			this.Activated += DescriptionDisplay_Activated;
			Opened += DescriptionDisplay_Opened;
		}

		private void DescriptionDisplay_Opened(object sender, EventArgs e)
		{
			DescriptionTextBox.Focus();
		}

		private void DescriptionDisplay_Activated(object sender, EventArgs e)
		{
			DataContext = this;
			var workingHeight = this.Screens.Primary.WorkingArea.Height;
			DescriptionTextBox.Measure(new Size(DescriptionTextBox.MinWidth, workingHeight * 0.8));

			this.Width = DescriptionTextBox.DesiredSize.Width;
			this.Height = DescriptionTextBox.DesiredSize.Height;
			this.MinWidth = this.Width;
			this.MaxWidth = this.Width;
			this.MinHeight = this.Height;
			this.MaxHeight = this.Height;

			DescriptionTextBox.Width = this.Width;
			DescriptionTextBox.Height = this.Height;
			DescriptionTextBox.MinWidth = this.Width;
			DescriptionTextBox.MaxWidth = this.Width;
			DescriptionTextBox.MinHeight = this.Height;
			DescriptionTextBox.MaxHeight = this.Height;

			this.Position = new PixelPoint((int)SpawnLocation.X, (int)Math.Min(SpawnLocation.Y, (double)workingHeight - DescriptionTextBox.DesiredSize.Height));
		}

		private void DescriptionTextBox_LostFocus(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Close();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

	}
}
