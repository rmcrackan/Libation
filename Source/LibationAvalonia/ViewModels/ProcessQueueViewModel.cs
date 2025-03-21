using ApplicationServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using DataLayer;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia.ViewModels
{

	public class ProcessQueueViewModel : ViewModelBase, ILogForm
	{
		public ObservableCollection<LogEntry> LogEntries { get; } = new();
		public AvaloniaList<ProcessBookViewModel> Items { get; } = new();
		public TrackedQueue<ProcessBookViewModel> Queue { get; }
		public ProcessBookViewModel? SelectedItem { get; set; }
		public Task? QueueRunner { get; private set; }
		public bool Running => !QueueRunner?.IsCompleted ?? false;

		private readonly LogMe Logger;

		public ProcessQueueViewModel()
		{
			Logger = LogMe.RegisterForm(this);
			Queue = new(Items);
			Queue.QueuededCountChanged += Queue_QueuededCountChanged;
			Queue.CompletedCountChanged += Queue_CompletedCountChanged;

			if (Design.IsDesignMode)
				_ = Configuration.Instance.LibationFiles;

			SpeedLimit = Configuration.Instance.DownloadSpeedLimit / 1024m / 1024;
		}

		private int _completedCount;
		private int _errorCount;
		private int _queuedCount;
		private string? _runningTime;
		private bool _progressBarVisible;
		private decimal _speedLimit;

		public int CompletedCount { get => _completedCount; private set => Dispatcher.UIThread.Invoke(() => { this.RaiseAndSetIfChanged(ref _completedCount, value); this.RaisePropertyChanged(nameof(AnyCompleted)); }); }
		public int QueuedCount { get => _queuedCount; private set => Dispatcher.UIThread.Invoke(() => { this.RaiseAndSetIfChanged(ref _queuedCount, value); this.RaisePropertyChanged(nameof(AnyQueued)); }); }
		public int ErrorCount { get => _errorCount; private set => Dispatcher.UIThread.Invoke(() => { this.RaiseAndSetIfChanged(ref _errorCount, value); this.RaisePropertyChanged(nameof(AnyErrors)); }); }
		public string? RunningTime { get => _runningTime; set => Dispatcher.UIThread.Invoke(() => { this.RaiseAndSetIfChanged(ref _runningTime, value); }); }
		public bool ProgressBarVisible { get => _progressBarVisible; set => Dispatcher.UIThread.Invoke(() => { this.RaiseAndSetIfChanged(ref _progressBarVisible, value); }); }
		public bool AnyCompleted => CompletedCount > 0;
		public bool AnyQueued => QueuedCount > 0;
		public bool AnyErrors => ErrorCount > 0;
		public double Progress => 100d * Queue.Completed.Count / Queue.Count;

		public decimal SpeedLimit
		{
			get
			{
				return _speedLimit;
			}
			set
			{
				var newValue = Math.Min(999 * 1024 * 1024, (long)(value * 1024 * 1024));
				var config = Configuration.Instance;
				config.DownloadSpeedLimit = newValue;

				_speedLimit
					= config.DownloadSpeedLimit <= newValue ? value
					: value == 0.01m ? config.DownloadSpeedLimit / 1024m / 1024
					: 0;

				config.DownloadSpeedLimit = (long)(_speedLimit * 1024 * 1024);

				SpeedLimitIncrement = _speedLimit > 100 ? 10
					: _speedLimit > 10 ? 1
					: _speedLimit > 1 ? 0.1m
					: 0.01m;

				Dispatcher.UIThread.Invoke(() =>
				   {
					   this.RaisePropertyChanged(nameof(SpeedLimitIncrement));
					   this.RaisePropertyChanged();
				   });
			}
		}

		public decimal SpeedLimitIncrement { get; private set; }

		private void Queue_CompletedCountChanged(object? sender, int e)
		{
			int errCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.FailedAbort or ProcessBookResult.FailedSkip or ProcessBookResult.FailedRetry or ProcessBookResult.ValidationFail);
			int completeCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.Success);

			ErrorCount = errCount;
			CompletedCount = completeCount;
			Dispatcher.UIThread.Invoke(() => this.RaisePropertyChanged(nameof(Progress)));
		}
		private void Queue_QueuededCountChanged(object? sender, int cueCount)
		{
			QueuedCount = cueCount;
			Dispatcher.UIThread.Invoke(() => this.RaisePropertyChanged(nameof(Progress)));
		}

		public void WriteLine(string text)
		{
			Dispatcher.UIThread.Invoke(() =>
				LogEntries.Add(new()
				{
					LogDate = DateTime.Now,
					LogMessage = text.Trim()
				}));
		}


		#region Add Books to Queue

		private bool isBookInQueue(LibraryBook libraryBook)
		{
			var entry = Queue.FirstOrDefault(b => b?.LibraryBook?.Book?.AudibleProductId == libraryBook.Book.AudibleProductId);
			if (entry == null)
				return false;
			else if (entry.Status is ProcessBookStatus.Cancelled or ProcessBookStatus.Failed)
				return !Queue.RemoveCompleted(entry);
			else
				return true;
		}

		public void AddDownloadPdf(LibraryBook libraryBook)
			=> AddDownloadPdf(new List<LibraryBook>() { libraryBook });

		public void AddDownloadDecrypt(LibraryBook libraryBook)
			=> AddDownloadDecrypt(new List<LibraryBook>() { libraryBook });

		public void AddConvertMp3(LibraryBook libraryBook)
			=> AddConvertMp3(new List<LibraryBook>() { libraryBook });

		public void AddDownloadPdf(IEnumerable<LibraryBook> entries)
		{
			List<ProcessBookViewModel> procs = new();
			foreach (var entry in entries)
			{
				if (isBookInQueue(entry))
					continue;

				ProcessBookViewModel pbook = new(entry, Logger);
				pbook.AddDownloadPdf();
				procs.Add(pbook);
			}

			Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
			AddToQueue(procs);
		}

		public void AddDownloadDecrypt(IEnumerable<LibraryBook> entries)
		{
			List<ProcessBookViewModel> procs = new();
			foreach (var entry in entries)
			{
				if (isBookInQueue(entry))
					continue;

				ProcessBookViewModel pbook = new(entry, Logger);
				pbook.AddDownloadDecryptBook();
				pbook.AddDownloadPdf();
				procs.Add(pbook);
			}

			Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
			AddToQueue(procs);
		}

		public void AddConvertMp3(IEnumerable<LibraryBook> entries)
		{
			List<ProcessBookViewModel> procs = new();
			foreach (var entry in entries)
			{
				if (isBookInQueue(entry))
					continue;

				ProcessBookViewModel pbook = new(entry, Logger);
				pbook.AddConvertToMp3();
				procs.Add(pbook);
			}

			Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
			AddToQueue(procs);
		}

		public void AddToQueue(IEnumerable<ProcessBookViewModel> pbook)
		{
			Dispatcher.UIThread.Invoke(() =>
			{
				Queue.Enqueue(pbook);
				if (!Running)
					QueueRunner = QueueLoop();
			});
		}

		#endregion

		DateTime StartingTime;
		private async Task QueueLoop()
		{
			try
			{
				Serilog.Log.Logger.Information("Begin processing queue");

				RunningTime = string.Empty;
				ProgressBarVisible = true;
				StartingTime = DateTime.Now;

				using var counterTimer = new System.Threading.Timer(CounterTimer_Tick, null, 0, 500);

				bool shownServiceOutageMessage = false;

				while (Queue.MoveNext())
				{
					if (Queue.Current is not ProcessBookViewModel nextBook)
					{
						Serilog.Log.Logger.Information("Current queue item is empty.");
						continue;
					}

					Serilog.Log.Logger.Information("Begin processing queued item. {item_LibraryBook}", nextBook.LibraryBook);

					var result = await nextBook.ProcessOneAsync();

					Serilog.Log.Logger.Information("Completed processing queued item: {item_LibraryBook}\r\nResult: {result}", nextBook.LibraryBook, result);

					if (result == ProcessBookResult.ValidationFail)
						Queue.ClearCurrent();
					else if (result == ProcessBookResult.FailedAbort)
						Queue.ClearQueue();
					else if (result == ProcessBookResult.FailedSkip)
						nextBook.LibraryBook.UpdateBookStatus(LiberatedStatus.Error);
					else if (result == ProcessBookResult.LicenseDeniedPossibleOutage && !shownServiceOutageMessage)
					{
						await MessageBox.Show(@$"
You were denied a content license for {nextBook.LibraryBook.Book.TitleWithSubtitle}

This error appears to be caused by a temporary interruption of service that sometimes affects Libation's users. This type of error usually resolves itself in 1 to 2 days, and in the meantime you should still be able to access your books through Audible's website or app.
",
						"Possible Interruption of Service",
						MessageBoxButtons.OK,
						MessageBoxIcon.Asterisk);
						shownServiceOutageMessage = true;
					}
				}
				Serilog.Log.Logger.Information("Completed processing queue");

				Queue_CompletedCountChanged(this, 0);
				ProgressBarVisible = false;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "An error was encountered while processing queued items");
			}
		}

		private void CounterTimer_Tick(object? state)
		{
			string timeToStr(TimeSpan time)
			{
				string minsSecs = $"{time:mm\\:ss}";
				if (time.TotalHours >= 1)
					return $"{time.TotalHours:F0}:{minsSecs}";
				return minsSecs;
			}
			RunningTime = timeToStr(DateTime.Now - StartingTime);
		}
	}

	public class LogEntry
	{
		public DateTime LogDate { get; init; }
		public string LogDateString => LogDate.ToShortTimeString();
		public string? LogMessage { get; init; }
	}
}
