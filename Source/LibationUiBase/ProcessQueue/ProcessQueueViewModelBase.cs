using DataLayer;
using LibationUiBase.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationServices;
using System.Threading.Tasks;

#nullable enable
namespace LibationUiBase.ProcessQueue;

public abstract class ProcessQueueViewModelBase : ReactiveObject, ILogForm
{
	public abstract void WriteLine(string text);
	protected abstract ProcessBookViewModelBase CreateNewProcessBook(LibraryBook libraryBook);

	public TrackedQueue<ProcessBookViewModelBase> Queue { get; }
	public Task? QueueRunner { get; private set; }
	public bool Running => !QueueRunner?.IsCompleted ?? false;
	protected LogMe Logger { get; }

	public ProcessQueueViewModelBase(ICollection<ProcessBookViewModelBase>? underlyingList)
	{
		Logger = LogMe.RegisterForm(this);
		Queue = new(underlyingList);
		Queue.QueuedCountChanged += Queue_QueuedCountChanged;
		Queue.CompletedCountChanged += Queue_CompletedCountChanged;
	}

	private int _completedCount;
	private int _errorCount;
	private int _queuedCount;
	private string? _runningTime;
	private bool _progressBarVisible;

	public int CompletedCount { get => _completedCount; private set { RaiseAndSetIfChanged(ref _completedCount, value); RaisePropertyChanged(nameof(AnyCompleted)); } }
	public int QueuedCount { get => _queuedCount; private set { this.RaiseAndSetIfChanged(ref _queuedCount, value); RaisePropertyChanged(nameof(AnyQueued)); } }
	public int ErrorCount { get => _errorCount; private set { RaiseAndSetIfChanged(ref _errorCount, value); RaisePropertyChanged(nameof(AnyErrors)); } }
	public string? RunningTime { get => _runningTime; set => RaiseAndSetIfChanged(ref _runningTime, value); }
	public bool ProgressBarVisible { get => _progressBarVisible; set => RaiseAndSetIfChanged(ref _progressBarVisible, value); }
	public bool AnyCompleted => CompletedCount > 0;
	public bool AnyQueued => QueuedCount > 0;
	public bool AnyErrors => ErrorCount > 0;
	public double Progress => 100d * Queue.Completed.Count / Queue.Count;

	private void Queue_CompletedCountChanged(object? sender, int e)
	{
		int errCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.FailedAbort or ProcessBookResult.FailedSkip or ProcessBookResult.FailedRetry or ProcessBookResult.ValidationFail);
		int completeCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.Success);

		ErrorCount = errCount;
		CompletedCount = completeCount;
		RaisePropertyChanged(nameof(Progress));
	}

	private void Queue_QueuedCountChanged(object? sender, int cueCount)
	{
		QueuedCount = cueCount;
		RaisePropertyChanged(nameof(Progress));
	}

	#region Add Books to Queue

	public bool QueueDownloadPdf(IList<LibraryBook> libraryBooks)
	{
		var needsPdf = libraryBooks.Where(lb => lb.NeedsPdfDownload()).ToArray();
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
			else if (item.NeedsBookDownload())
			{
				RemoveCompleted(item);
				Serilog.Log.Logger.Information("Begin single library book backup of {libraryBook}", item);
				AddDownloadDecrypt([item]);
				return true;
			}
			else if (item.NeedsPdfDownload())
			{
				RemoveCompleted(item);
				Serilog.Log.Logger.Information("Begin single pdf backup of {libraryBook}", item);
				AddDownloadPdf([item]);
				return true;
			}
		}
		else
		{
			var toLiberate = libraryBooks.UnLiberated().ToArray();

			if (toLiberate.Length > 0)
			{
				Serilog.Log.Logger.Information("Begin backup of {count} library books", toLiberate.Length);
				AddDownloadDecrypt(toLiberate);
				return true;
			}
		}
		return false;
	}

	private bool IsBookInQueue(LibraryBook libraryBook)
		=> Queue.FirstOrDefault(b => b?.LibraryBook?.Book?.AudibleProductId == libraryBook.Book.AudibleProductId) is not ProcessBookViewModelBase entry ? false
		: entry.Status is ProcessBookStatus.Cancelled or ProcessBookStatus.Failed ? !Queue.RemoveCompleted(entry)
		: true;

	private bool RemoveCompleted(LibraryBook libraryBook)
		=> Queue.FirstOrDefault(b => b?.LibraryBook?.Book?.AudibleProductId == libraryBook.Book.AudibleProductId) is ProcessBookViewModelBase entry
		&& entry.Status is ProcessBookStatus.Completed
		&& Queue.RemoveCompleted(entry);

	private void AddDownloadPdf(IEnumerable<LibraryBook> entries)
	{
		var procs = entries.Where(e => !IsBookInQueue(e)).Select(Create).ToArray();
		Serilog.Log.Logger.Information("Queueing {count} books for PDF-only download", procs.Length);
		AddToQueue(procs);

		ProcessBookViewModelBase Create(LibraryBook entry)
			=> CreateNewProcessBook(entry).AddDownloadPdf();
	}

	private void AddDownloadDecrypt(IEnumerable<LibraryBook> entries)
	{
		var procs = entries.Where(e => !IsBookInQueue(e)).Select(Create).ToArray();
		Serilog.Log.Logger.Information("Queueing {count} books ofr download/decrypt", procs.Length);
		AddToQueue(procs);
		
		ProcessBookViewModelBase Create(LibraryBook entry)
			=> CreateNewProcessBook(entry).AddDownloadDecryptBook().AddDownloadPdf();
	}

	private void AddConvertMp3(IEnumerable<LibraryBook> entries)
	{
		var procs = entries.Where(e => !IsBookInQueue(e)).Select(Create).ToArray();
		Serilog.Log.Logger.Information("Queueing {count} books for mp3 conversion", procs.Length);
		AddToQueue(procs);

		ProcessBookViewModelBase Create(LibraryBook entry)
			=> CreateNewProcessBook(entry).AddConvertToMp3();
	}

	private void AddToQueue(IEnumerable<ProcessBookViewModelBase> pbook)
	{
		Queue.Enqueue(pbook);
		if (!Running)
			QueueRunner = Task.Run(QueueLoop);
	}

	#endregion

	private async Task QueueLoop()
	{
		try
		{
			Serilog.Log.Logger.Information("Begin processing queue");

			RunningTime = string.Empty;
			ProgressBarVisible = true;
			var startingTime = DateTime.Now;
			bool shownServiceOutageMessage = false;

			using var counterTimer = new System.Threading.Timer(_ => RunningTime = timeToStr(DateTime.Now - startingTime), null, 0, 500);

			while (Queue.MoveNext())
			{
				if (Queue.Current is not ProcessBookViewModelBase nextBook)
				{
					Serilog.Log.Logger.Information("Current queue item is empty.");
					continue;
				}

				Serilog.Log.Logger.Information("Begin processing queued item: '{item_LibraryBook}'", nextBook.LibraryBook);

				var result = await nextBook.ProcessOneAsync();

				Serilog.Log.Logger.Information("Completed processing queued item: '{item_LibraryBook}' with result: {result}", nextBook.LibraryBook, result);

				if (result == ProcessBookResult.ValidationFail)
					Queue.ClearCurrent();
				else if (result == ProcessBookResult.FailedAbort)
					Queue.ClearQueue();
				else if (result == ProcessBookResult.FailedSkip)
					nextBook.LibraryBook.UpdateBookStatus(LiberatedStatus.Error);
				else if (result == ProcessBookResult.LicenseDeniedPossibleOutage && !shownServiceOutageMessage)
				{
					await MessageBoxBase.Show($"""
					You were denied a content license for {nextBook.LibraryBook.Book.TitleWithSubtitle}

					This error appears to be caused by a temporary interruption of service that sometimes affects Libation's users. This type of error usually resolves itself in 1 to 2 days, and in the meantime you should still be able to access your books through Audible's website or app.
					""",
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

		string timeToStr(TimeSpan time)
			=> time.TotalHours < 1 ? $"{time:mm\\:ss}"
			: $"{time.TotalHours:F0}:{time:mm\\:ss}";
	}
}
