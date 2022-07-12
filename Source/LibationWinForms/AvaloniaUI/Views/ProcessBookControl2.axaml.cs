using Avalonia;
using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationWinForms.AvaloniaUI.ViewModels;

namespace LibationWinForms.AvaloniaUI.Views
{
	public delegate void QueueItemPositionButtonClicked(ProcessBook2 item, QueuePosition queueButton);
	public delegate void QueueItemCancelButtonClicked(ProcessBook2 item);
	public partial class ProcessBookControl2 : UserControl
	{
		public static event QueueItemPositionButtonClicked PositionButtonClicked;
		public static event QueueItemCancelButtonClicked CancelButtonClicked;
		public ProcessBookControl2()
		{
			InitializeComponent();
		}

		private ProcessBook2 DataItem => DataContext is null ? null : DataContext as ProcessBook2;

		public void Cancel_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> CancelButtonClicked?.Invoke(DataItem);
		public void MoveFirst_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> PositionButtonClicked?.Invoke(DataItem, QueuePosition.Fisrt);
		public void MoveUp_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> PositionButtonClicked?.Invoke(DataItem, QueuePosition.OneUp);
		public void MoveDown_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> PositionButtonClicked?.Invoke(DataItem, QueuePosition.OneDown);
		public void MoveLast_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> PositionButtonClicked?.Invoke(DataItem, QueuePosition.Last);

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
