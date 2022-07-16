using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class ProcessQueueControl2 : UserControl
    {
		private TrackedQueue2<ProcessBook2> Queue => _viewModel.Items;
		private ProcessQueueViewModel _viewModel => DataContext as ProcessQueueViewModel;

		public ProcessQueueControl2()
		{
			InitializeComponent();

			ProcessBookControl2.PositionButtonClicked += ProcessBookControl2_ButtonClicked;
			ProcessBookControl2.CancelButtonClicked += ProcessBookControl2_CancelButtonClicked;			
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		#region Control event handlers

		private async void ProcessBookControl2_CancelButtonClicked(ProcessBook2 item)
		{
			if (item is not null)
				await item.CancelAsync();
			Queue.RemoveQueued(item);
		}

		private void ProcessBookControl2_ButtonClicked(ProcessBook2 item, QueuePosition queueButton)
		{
			Queue.MoveQueuePosition(item, queueButton);
		}

		public async void CancelAllBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Queue.ClearQueue();
			if (Queue.Current is not null)
				await Queue.Current.CancelAsync();
		}

		public void ClearFinishedBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Queue.ClearCompleted();

			if (!_viewModel.Running)
				_viewModel.RunningTime = string.Empty;
		}

		public void ClearLogBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_viewModel.LogEntries.Clear();
		}

		private async void LogCopyBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			string logText = string.Join("\r\n", _viewModel.LogEntries.Select(r => $"{r.LogDate.ToShortDateString()} {r.LogDate.ToShortTimeString()}\t{r.LogMessage}"));
			await Application.Current.Clipboard.SetTextAsync(logText);
		}

		private async void cancelAllBtn_Click(object sender, EventArgs e)
		{
			Queue.ClearQueue();
			if (Queue.Current is not null)
				await Queue.Current.CancelAsync();
		}

		private void btnClearFinished_Click(object sender, EventArgs e)
		{
			Queue.ClearCompleted();

			if (!_viewModel.Running)
				_viewModel.RunningTime = string.Empty;
		}

		#endregion
	}
}
