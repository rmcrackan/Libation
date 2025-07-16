using DataLayer;
using LibationFileManager;
using LibationUiBase.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ApplicationServices;
using System.Threading.Tasks;

#nullable enable
namespace LibationUiBase.ProcessQueue;

public abstract class ProcessQueueViewModelBase : ReactiveObject, ILogForm
{
	public abstract void WriteLine(string text);

	protected abstract ProcessBookViewModelBase CreateNewBook(LibraryBook libraryBook);

	public ObservableCollection<LogEntry> LogEntries { get; } = new();
	public TrackedQueue<ProcessBookViewModelBase> Queue { get; }
	public ProcessBookViewModelBase? SelectedItem { get; set; }
	public Task? QueueRunner { get; private set; }
	public bool Running => !QueueRunner?.IsCompleted ?? false;

	protected readonly LogMe Logger;

	public ProcessQueueViewModelBase(ICollection<ProcessBookViewModelBase>? underlyingList)
	{
		Logger = LogMe.RegisterForm(this);
		Queue = new(underlyingList);
		Queue.QueuededCountChanged += Queue_QueuededCountChanged;
		Queue.CompletedCountChanged += Queue_CompletedCountChanged;
		SpeedLimit = Configuration.Instance.DownloadSpeedLimit / 1024m / 1024;
	}

	private int _completedCount;
	private int _errorCount;
	private int _queuedCount;
	private string? _runningTime;
	private bool _progressBarVisible;
	private decimal _speedLimit;

	public int CompletedCount { get => _completedCount; private set { RaiseAndSetIfChanged(ref _completedCount, value); RaisePropertyChanged(nameof(AnyCompleted)); } }
	public int QueuedCount { get => _queuedCount; private set { this.RaiseAndSetIfChanged(ref _queuedCount, value); RaisePropertyChanged(nameof(AnyQueued)); } }
	public int ErrorCount { get => _errorCount; private set { RaiseAndSetIfChanged(ref _errorCount, value); RaisePropertyChanged(nameof(AnyErrors)); } }
	public string? RunningTime { get => _runningTime; set => RaiseAndSetIfChanged(ref _runningTime, value); }
	public bool ProgressBarVisible { get => _progressBarVisible; set => RaiseAndSetIfChanged(ref _progressBarVisible, value); }
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

			RaisePropertyChanged(nameof(SpeedLimitIncrement));
			RaisePropertyChanged(nameof(SpeedLimit));
		}
	}

	public decimal SpeedLimitIncrement { get; private set; }

	private void Queue_CompletedCountChanged(object? sender, int e)
	{
		int errCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.FailedAbort or ProcessBookResult.FailedSkip or ProcessBookResult.FailedRetry or ProcessBookResult.ValidationFail);
		int completeCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.Success);

		ErrorCount = errCount;
		CompletedCount = completeCount;
		RaisePropertyChanged(nameof(Progress));
	}
	private void Queue_QueuededCountChanged(object? sender, int cueCount)
	{
		QueuedCount = cueCount;
		RaisePropertyChanged(nameof(Progress));
	}

	#region Add Books to Queue

	public bool QueueDownloadPdf(IList<LibraryBook> libraryBooks)
	{
		var needsPdf = libraryBooks.Where(lb => !lb.AbsentFromLastScan && lb.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated).ToArray();
		if (needsPdf.Length > 0)
		{
			Serilog.Log.Logger.Information("Begin download {count} pdfs", needsPdf.Length);
			AddDownloadPdf(needsPdf);
			return true;
		}
		return false;
	}

	public bool QueueConvertToMp3(IList<LibraryBook> libraryBooks)
	{
		//Only Queue Liberated books for conversion.  This isn't a perfect filter, but it's better than nothing.
		var preLiberated = libraryBooks.Where(lb => !lb.AbsentFromLastScan && lb.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated && lb.Book.ContentType is DataLayer.ContentType.Product).ToArray();
		if (preLiberated.Length > 0)
		{
			Serilog.Log.Logger.Information("Begin convert {count} books to mp3", preLiberated.Length);
			AddConvertMp3(preLiberated);
			return true;
		}
		return false;
	}

	public bool QueueDownloadDecrypt(IList<LibraryBook> libraryBooks)
	{
		if (libraryBooks.Count == 1)
		{
			var item = libraryBooks[0];

			if (item.AbsentFromLastScan)
				return false;
			else if (item.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload)
			{
				RemoveCompleted(item);
				Serilog.Log.Logger.Information("Begin single library book backup of {libraryBook}", item);
				AddDownloadDecrypt([item]);
				return true;
			}
			else if (item.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated)
			{
				RemoveCompleted(item);
				Serilog.Log.Logger.Information("Begin single pdf backup of {libraryBook}", item);
				AddDownloadPdf([item]);
				return true;
			}
		}
		else
		{
			var toLiberate
				= libraryBooks
				.Where(x => !x.AbsentFromLastScan && x.Book.UserDefinedItem.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload || x.Book.UserDefinedItem.PdfStatus is LiberatedStatus.NotLiberated)
				.ToArray();

			if (toLiberate.Length > 0)
			{
				Serilog.Log.Logger.Information("Begin backup of {count} library books", toLiberate.Length);
				AddDownloadDecrypt(toLiberate);
				return true;
			}
		}
		return false;
	}

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

	private bool RemoveCompleted(LibraryBook libraryBook)
		=> Queue.FirstOrDefault(b => b?.LibraryBook?.Book?.AudibleProductId == libraryBook.Book.AudibleProductId) is ProcessBookViewModelBase entry
		&& entry.Status is ProcessBookStatus.Completed
		&& Queue.RemoveCompleted(entry);

	private void AddDownloadPdf(IEnumerable<LibraryBook> entries)
	{
		List<ProcessBookViewModelBase> procs = new();
		foreach (var entry in entries)
		{
			if (isBookInQueue(entry))
				continue;

			var pbook = CreateNewBook(entry);
			pbook.AddDownloadPdf();
			procs.Add(pbook);
		}

		Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
		AddToQueue(procs);
	}

	private void AddDownloadDecrypt(IEnumerable<LibraryBook> entries)
	{
		List<ProcessBookViewModelBase> procs = new();
		foreach (var entry in entries)
		{
			if (isBookInQueue(entry))
				continue;

			var pbook = CreateNewBook(entry);
			pbook.AddDownloadDecryptBook();
			pbook.AddDownloadPdf();
			procs.Add(pbook);
		}

		Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
		AddToQueue(procs);
	}

	private void AddConvertMp3(IEnumerable<LibraryBook> entries)
	{
		List<ProcessBookViewModelBase> procs = new();
		foreach (var entry in entries)
		{
			if (isBookInQueue(entry))
				continue;

			var pbook = CreateNewBook(entry);
			pbook.AddConvertToMp3();
			procs.Add(pbook);
		}

		Serilog.Log.Logger.Information("Queueing {count} books", procs.Count);
		AddToQueue(procs);
	}

	private void AddToQueue(IEnumerable<ProcessBookViewModelBase> pbook)
	{
		Invoke(() =>
		{
			Queue.Enqueue(pbook);
			if (!Running)
				QueueRunner = QueueLoop();
		});
	}

	#endregion

	private DateTime StartingTime;
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
				if (Queue.Current is not ProcessBookViewModelBase nextBook)
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
					await MessageBoxBase.Show(@$"
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
