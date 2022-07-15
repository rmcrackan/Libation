using ApplicationServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DataLayer;
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

			#region Design Mode Testing
			/*
			if (Design.IsDesignMode)
			{
				using var context = DbContexts.GetContext();
				var book = context.GetLibraryBook_Flat_NoTracking("B017V4IM1G");
				List<ProcessBook2> testList = new()
				{
					new ProcessBook2(book, Logger)
					{
						Result = ProcessBookResult.FailedAbort,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBook2(book, Logger)
					{
						Result = ProcessBookResult.FailedSkip,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBook2(book, Logger)
					{
						Result = ProcessBookResult.FailedRetry,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBook2(book, Logger)
					{
						Result = ProcessBookResult.ValidationFail,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBook2(book, Logger)
					{
						Result = ProcessBookResult.Cancelled,
						Status = ProcessBookStatus.Cancelled,
					},
					new ProcessBook2(book, Logger)
					{
						Result = ProcessBookResult.Success,
						Status = ProcessBookStatus.Completed,
					},
					new ProcessBook2(book, Logger)
					{
						Result = ProcessBookResult.None,
						Status = ProcessBookStatus.Working,
					},
					new ProcessBook2(book, Logger)
					{
						Result = ProcessBookResult.None,
						Status = ProcessBookStatus.Queued,
					},
				};

				_viewModel.Items.Enqueue(testList);
				return;
			}
			*/
			#endregion
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

		private void LogCopyBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			string logText = string.Join("\r\n", _viewModel.LogEntries.Select(r => $"{r.LogDate.ToShortDateString()} {r.LogDate.ToShortTimeString()}\t{r.LogMessage}"));
			System.Windows.Forms.Clipboard.SetDataObject(logText, false, 5, 150);
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
