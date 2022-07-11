using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationWinForms.AvaloniaUI.ViewModels;

namespace LibationWinForms.AvaloniaUI.Views
{
	public enum QueueButton
	{
		Cancel,
		MoveFirst,
		MoveUp,
		MoveDown,
		MoveLast
	}
	public delegate void QueueItemButtonClicked(ProcessBook2 item, QueueButton queueButton);
	public partial class ProcessBookControl2 : UserControl
	{
		public static event QueueItemButtonClicked ButtonClicked;
		public ProcessBookControl2()
		{
			InitializeComponent();
		}

		private ProcessBook2 DataItem => DataContext is null ? null : DataContext as ProcessBook2;

		public void Cancel_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> ButtonClicked?.Invoke(DataItem, QueueButton.Cancel);
		public void MoveFirst_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> ButtonClicked?.Invoke(DataItem, QueueButton.MoveFirst);
		public void MoveUp_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> ButtonClicked?.Invoke(DataItem, QueueButton.MoveUp);
		public void MoveDown_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> ButtonClicked?.Invoke(DataItem, QueueButton.MoveDown);
		public void MoveLast_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> ButtonClicked?.Invoke(DataItem, QueueButton.MoveLast);

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
