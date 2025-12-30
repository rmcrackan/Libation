using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Threading;
using DataLayer;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.ProcessQueue;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Views
{
	public partial class ProcessQueueControl : UserControl
	{
		private TrackedQueue<ProcessBookViewModel>? Queue => _viewModel?.Queue;
		private ProcessQueueViewModel? _viewModel => DataContext as ProcessQueueViewModel;

		public ProcessQueueControl()
		{
			InitializeComponent();

			ProcessBookControl.PositionButtonClicked += ProcessBookControl2_ButtonClicked;
			ProcessBookControl.CancelButtonClicked += ProcessBookControl2_CancelButtonClicked;

			#region Design Mode Testing
#if DEBUG
			if (Design.IsDesignMode)
			{
				ViewModels.MainVM.Configure_NonUI();
				Configuration.CreateMockInstance();
				var vm = new ProcessQueueViewModel();
				DataContext = vm;

				var trialBook = MockLibraryBook.CreateBook();
				List<ProcessBookViewModel> testList = new()
				{
					new ProcessBookViewModel(trialBook, Configuration.Instance)
					{
						Result = ProcessBookResult.FailedAbort,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBookViewModel(trialBook, Configuration.Instance)
					{
						Result = ProcessBookResult.FailedSkip,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBookViewModel(trialBook, Configuration.Instance)
					{
						Result = ProcessBookResult.FailedRetry,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBookViewModel(trialBook, Configuration.Instance)
					{
						Result = ProcessBookResult.ValidationFail,
						Status = ProcessBookStatus.Failed,
					},
					new ProcessBookViewModel(trialBook, Configuration.Instance)
					{
						Result = ProcessBookResult.Cancelled,
						Status = ProcessBookStatus.Cancelled,
					},
					new ProcessBookViewModel(trialBook, Configuration.Instance)
					{
						Result = ProcessBookResult.Success,
						Status = ProcessBookStatus.Completed,
					},
					new ProcessBookViewModel(trialBook, Configuration.Instance)
					{
						Result = ProcessBookResult.None,
						Status = ProcessBookStatus.Working,
					},
					new ProcessBookViewModel(trialBook, Configuration.Instance)
					{
						Result = ProcessBookResult.None,
						Status = ProcessBookStatus.Queued,
					},
				};

				vm.Queue.Enqueue(testList);
				vm.Queue.MoveNext();
				vm.Queue.MoveNext();
				vm.Queue.MoveNext();
				vm.Queue.MoveNext();
				vm.Queue.MoveNext();
				vm.Queue.MoveNext();
				vm.Queue.MoveNext();
				return;
			}
#endif
			#endregion
		}

		#region Auto-Scroll Current Item Into View
		protected override void OnDataContextBeginUpdate()
		{
			if (DataContext is ProcessQueueViewModel vm)
			{
				vm.ProcessStart -= Book_ProcessStart;
			}
			base.OnDataContextBeginUpdate();
		}

		protected override void OnDataContextEndUpdate()
		{
			if (DataContext is ProcessQueueViewModel vm)
			{
				vm.ProcessStart += Book_ProcessStart;
			}
			base.OnDataContextEndUpdate();
		}

		private void Book_ProcessStart(object? sender, ProcessBookViewModel e)
		{
			Dispatcher.UIThread.Invoke(() =>
			{
				if (Queue?.IndexOf(e) is int newtBookIndex  && newtBookIndex > 0 && QueueListControl.Presenter?.Panel is VirtualizingStackPanel panel && itemIsVisible(newtBookIndex - 1, panel))
				{
					// Only scroll the new item into view if the previous item is visible.
					// This allows users to scroll through the queue without being interrupted.
					QueueListControl.ScrollIntoView(newtBookIndex);
				}
			});

			static bool itemIsVisible(int newtBookIndex, VirtualizingStackPanel panel)
				=> panel.FirstRealizedIndex <= newtBookIndex && panel.LastRealizedIndex >= newtBookIndex;
		}
		#endregion

		public void NumericUpDown_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
		{
			if (e.Key == Avalonia.Input.Key.Enter && sender is Avalonia.Input.IInputElement input) input.Focus();
		}

		#region Control event handlers

		private async void ProcessBookControl2_CancelButtonClicked(ProcessBookViewModel? item)
		{
			if (item is not null)
			{
				await item.CancelAsync();
				Queue?.RemoveQueued(item);
			}
		}

		private void ProcessBookControl2_ButtonClicked(ProcessBookViewModel? item, QueuePosition queueButton)
		{
			if (item is not null)
				Queue?.MoveQueuePosition(item, queueButton);
		}

		public async void CancelAllBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Queue?.ClearQueue();
			if (Queue?.Current is not null)
				await Queue.Current.CancelAsync();
		}

		public void ClearFinishedBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Queue?.ClearCompleted();

			if (_viewModel?.Running is false)
				_viewModel.RunningTime = string.Empty;
		}

		public void ClearLogBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			_viewModel?.LogEntries.Clear();
		}

		private async void LogCopyBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (_viewModel is ProcessQueueViewModel vm)
			{
				string logText = string.Join("\r\n", vm.LogEntries.Select(r => $"{r.LogDate.ToShortDateString()} {r.LogDate.ToShortTimeString()}\t{r.LogMessage}"));
				if (App.MainWindow?.Clipboard?.SetTextAsync(logText) is Task setter)
					await setter;
			}
		}

		private async void cancelAllBtn_Click(object? sender, EventArgs e)
		{
			Queue?.ClearQueue();
			if (Queue?.Current is not null)
				await Queue.Current.CancelAsync();
		}

		private void btnClearFinished_Click(object? sender, EventArgs e)
		{
			Queue?.ClearCompleted();

			if (_viewModel?.Running is false)
				_viewModel.RunningTime = string.Empty;
		}

		#endregion
	}

	public class DecimalConverter : IValueConverter
	{
		public static readonly DecimalConverter Instance = new();

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is string sourceText && targetType.IsAssignableTo(typeof(decimal?)))
			{
				if (sourceText == "∞") return 0;

				for (int i = sourceText.Length; i > 0; i--)
				{
					if (decimal.TryParse(sourceText[..i], out var val))
						return val;
				}

				return 0;
			}
			return 0;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is decimal val)
			{
				return
					val == 0 ? "∞"
					: (
						val >= 10 ? ((long)val).ToString()
						: val >= 1 ? val.ToString("F1")
						: val.ToString("F2")
					) + " MB/s";
			}
			return value?.ToString();
		}
	}
}
