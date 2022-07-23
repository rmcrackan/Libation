using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DataLayer;
using LibationWinForms.AvaloniaUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views
{
	public partial class ProcessQueueControl : UserControl
    {
		private TrackedQueue<ProcessBookViewModel> Queue => _viewModel.Items;
		private ProcessQueueViewModel _viewModel => DataContext as ProcessQueueViewModel;

		public ProcessQueueControl()
		{
			InitializeComponent();

			ProcessBookControl.PositionButtonClicked += ProcessBookControl2_ButtonClicked;
			ProcessBookControl.CancelButtonClicked += ProcessBookControl2_CancelButtonClicked;

			#region Design Mode Testing
			if (Design.IsDesignMode)
			{
				var vm = new ProcessQueueViewModel();
				var Logger = LogMe.RegisterForm(vm);
				DataContext = vm;
				using var context = DbContexts.GetContext();
				List<ProcessBookViewModel> testList = new()
				{
					new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4IM1G"), Logger)
					{
						Result = ProcessBookResult.FailedAbort,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4IWVG"), Logger)
					{
						Result = ProcessBookResult.FailedSkip,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4JA2Q"), Logger)
					{
						Result = ProcessBookResult.FailedRetry,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4NUPO"), Logger)
					{
						Result = ProcessBookResult.ValidationFail,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4NMX4"), Logger)
					{
						Result = ProcessBookResult.Cancelled,
						Status = ProcessBookStatus.Cancelled,
					},
					new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4NOZ0"), Logger)
					{
						Result = ProcessBookResult.Success,
						Status = ProcessBookStatus.Completed,
					},
					new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017WJ5ZK6"), Logger)
					{
						Result = ProcessBookResult.None,
						Status = ProcessBookStatus.Working,
					},
					new ProcessBookViewModel(context.GetLibraryBook_Flat_NoTracking("B017V4IM1G"), Logger)
					{
						Result = ProcessBookResult.None,
						Status = ProcessBookStatus.Queued,
					},
				};

				vm.Items.Enqueue(testList);
				vm.Items.MoveNext();
				vm.Items.MoveNext();
				vm.Items.MoveNext();
				vm.Items.MoveNext();
				vm.Items.MoveNext();
				vm.Items.MoveNext();
				vm.Items.MoveNext();
				return;
			}
			#endregion
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		#region Control event handlers

		private async void ProcessBookControl2_CancelButtonClicked(ProcessBookViewModel item)
		{
			if (item is not null)
				await item.CancelAsync();
			Queue.RemoveQueued(item);
		}

		private void ProcessBookControl2_ButtonClicked(ProcessBookViewModel item, QueuePosition queueButton)
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
