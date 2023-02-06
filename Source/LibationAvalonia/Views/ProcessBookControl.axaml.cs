using Avalonia;
using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationAvalonia.ViewModels;
using ApplicationServices;
using DataLayer;
using AppScaffolding;

namespace LibationAvalonia.Views
{
	public delegate void QueueItemPositionButtonClicked(ProcessBookViewModel item, QueuePosition queueButton);
	public delegate void QueueItemCancelButtonClicked(ProcessBookViewModel item);
	public partial class ProcessBookControl : UserControl
	{
		public static event QueueItemPositionButtonClicked PositionButtonClicked;
		public static event QueueItemCancelButtonClicked CancelButtonClicked;
		public ProcessBookControl()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
			{
				using var context = DbContexts.GetContext();
				DataContext = new ProcessBookViewModel(
					context.GetLibraryBook_Flat_NoTracking("B017V4IM1G"),
					LogMe.RegisterForm(default(ILogForm))
					);
				return;
			}
		}

		private ProcessBookViewModel DataItem => DataContext is null ? null : DataContext as ProcessBookViewModel;

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
