using ApplicationServices;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using LibationUiBase.Forms;
using LibationUiBase;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LibationUiBase.ProcessQueue;

public record LogEntry(DateTime LogDate, string LogMessage)
{
	public string LogDateString => LogDate.ToShortTimeString();
}

public class ProcessQueueViewModel : ReactiveObject
{
	public ObservableCollection<LogEntry> LogEntries { get; } = new();
	public TrackedQueue<ProcessBookViewModel> Queue { get; } = new();
	public Task? QueueRunner { get; private set; }
	public bool Running => !QueueRunner?.IsCompleted ?? false;

	public ProcessQueueViewModel()
	{
		Queue.QueuedCountChanged += Queue_QueuedCountChanged;
		Queue.CompletedCountChanged += Queue_CompletedCountChanged;
		SpeedLimit = Configuration.Instance.DownloadSpeedLimit / 1024m / 1024;
	}

	public int CompletedCount { get => field; private set { RaiseAndSetIfChanged(ref field, value); RaisePropertyChanged(nameof(AnyCompleted)); } }
	public int QueuedCount { get => field; private set { this.RaiseAndSetIfChanged(ref field, value); RaisePropertyChanged(nameof(AnyQueued)); } }
	public int ErrorCount { get => field; private set { RaiseAndSetIfChanged(ref field, value); RaisePropertyChanged(nameof(AnyErrors)); } }
	public string? RunningTime { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	public bool ProgressBarVisible { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	public bool AnyCompleted => CompletedCount > 0;
	public bool AnyQueued => QueuedCount > 0;
	public bool AnyErrors => ErrorCount > 0;
	public double Progress => 100d * Queue.Completed.Count / Queue.Count;
	public decimal SpeedLimitIncrement { get; private set; }

	private decimal _speedLimit;
	public decimal SpeedLimit
	{
		get => _speedLimit;
		set
		{
			var newValue = Math.Min(999 * 1024 * 1024, (long)Math.Ceiling(value * 1024 * 1024));
			var config = Configuration.Instance;
			config.DownloadSpeedLimit = newValue;

			_speedLimit
				= config.DownloadSpeedLimit <= newValue ? value
				: value == 0.01m ? config.DownloadSpeedLimit / 1024m / 1024
				: 0;

			config.DownloadSpeedLimit = (long)(_speedLimit * 1024 * 1024);
			if (Queue.Current is ProcessBookViewModel currentBook)
				currentBook.Configuration.DownloadSpeedLimit = config.DownloadSpeedLimit;

			SpeedLimitIncrement = _speedLimit > 100 ? 10
				: _speedLimit > 10 ? 1
				: _speedLimit > 1 ? 0.1m
				: 0.01m;

			RaisePropertyChanged(nameof(SpeedLimitIncrement));
			RaisePropertyChanged(nameof(SpeedLimit));
		}
	}

	private void Queue_CompletedCountChanged(object? sender, int e)
	{
		var errCount = Queue.Completed.Count(p => p.Result
			is ProcessBookResult.FailedAbort
			or ProcessBookResult.FailedSkip
			or ProcessBookResult.FailedRetry
			or ProcessBookResult.ValidationFail
			or ProcessBookResult.DiskFull);
		var completeCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.Success);

		ErrorCount = errCount;
		CompletedCount = completeCount;
		RaisePropertyChanged(nameof(Progress));
	}

	private void Queue_QueuedCountChanged(object? sender, int cueCount)
	{
		QueuedCount = cueCount;
		RaisePropertyChanged(nameof(Progress));
	}

	private void ProcessBook_LogWritten(object? sender, string logMessage)
		=> Invoke(() => LogEntries.Add(new(DateTime.Now, logMessage.Trim())));

	#region Add Books to Queue

	public async Task<bool> QueueDownloadPdfAsync(IList<LibraryBook> libraryBooks, Configuration? config = null)
	{
		config ??= Configuration.Instance;
		if (!await IsBooksDirectoryValidAsync(config))
			return false;

		var needsPdf = libraryBooks.Where(lb => lb.NeedsPdfDownload).ToArray();
		if (needsPdf.Length > 0)
		{
			Serilog.Log.Logger.Information("Begin download {count} pdfs", needsPdf.Length);
			AddDownloadPdf(needsPdf, config);
			return true;
		}
		return false;
	}

	public async Task<bool> QueueConvertToMp3Async(IList<LibraryBook> libraryBooks, Configuration? config = null)
	{
		config ??= Configuration.Instance;
		if (!await IsBooksDirectoryValidAsync(config))
			return false;

		//Only Queue Liberated books for conversion.  This isn't a perfect filter, but it's better than nothing.
		var preLiberated = libraryBooks.Where(lb => !lb.AbsentFromLastScan && lb.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated && lb.Book.ContentType is DataLayer.ContentType.Product).ToArray();
		if (preLiberated.Length > 0)
		{
			if (preLiberated.Length == 1)
				RemoveCompleted(preLiberated[0]);
			Serilog.Log.Logger.Information("Begin convert {count} books to mp3", preLiberated.Length);
			AddConvertMp3(preLiberated, config);
			return true;
		}
		return false;
	}

	public async Task<bool> QueueDownloadDecryptAsync(IList<LibraryBook> libraryBooks, Configuration? config = null)
	{
		config ??= Configuration.Instance;
		if (!await IsBooksDirectoryValidAsync(config))
			return false;

		if (libraryBooks.Count == 1)
		{
			var item = libraryBooks[0];

			if (item.AbsentFromLastScan)
			{
				Serilog.Log.Logger.Warning("Download not queued: {libraryBook} is absent from the last library scan.", item.LogFriendly());
				await MessageBoxBase.Show(
					"This title is marked absent from your last library scan.\n\nRun Scan (or `libationcli scan`) so Libation can refresh your library, then try again.",
					"Library scan required",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return false;
			}
			if (item.NeedsBookDownload)
			{
				RemoveCompleted(item);
				Serilog.Log.Logger.Information("Begin single library book backup of {libraryBook}", item);
				AddDownloadDecrypt([item], config);
				return true;
			}
			if (item.NeedsPdfDownload)
			{
				RemoveCompleted(item);
				Serilog.Log.Logger.Information("Begin single pdf backup of {libraryBook}", item);
				AddDownloadPdf([item], config);
				return true;
			}

			Serilog.Log.Logger.Warning(
				"Download not queued: single-item backup not applicable for {libraryBook} (book status or type does not request download).",
				item.LogFriendly());
			if (!item.Book.AudioExists)
			{
				await MessageBoxBase.Show(
					"Libation could not queue a download for this title.\n\n"
					+ "If it should be downloadable: confirm it is not already liberated, try \"Set download status\" to Not downloaded, or check whether a library scan is required.",
					"Download not queued",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
			return false;
		}
		else
		{
			var toLiberate = libraryBooks.UnLiberated().ToArray();

			if (toLiberate.Length > 0)
			{
				// May no-op when free space is unknown (common on UNC); see DiskSpaceBackupPreflight.
				if (!await DiskSpaceBackupPreflight.ConfirmBulkBackupAsync(toLiberate.Length, config, backupQueueAlreadyRunning: Running))
					return false;

				Serilog.Log.Logger.Information("Begin backup of {count} library books", toLiberate.Length);
				AddDownloadDecrypt(toLiberate, config);
				return true;
			}
		}
		return false;
	}

	private async Task<bool> IsBooksDirectoryValidAsync(Configuration config)
	{
		if (string.IsNullOrWhiteSpace(config.Books?.Path))
		{
			Serilog.Log.Logger.Error("Books location is not set in configuration.");
			await MessageBoxBase.Show(
				"Please choose a \"Books location\" folder in the Settings menu.",
				"Books Directory Not Set",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return false;
		}
		else if (AudibleFileStorage.BooksDirectory is null)
		{
			Serilog.Log.Logger.Error("Failed to create books directory: {booksDir}", config.Books?.Path);
			await MessageBoxBase.Show(
				$"Libation was unable to create the \"Books location\" folder at:\n{config.Books}\n\nPlease change the Books location in the settings menu.",
				"Failed to Create Books Directory",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return false;
		}
		else if (AudibleFileStorage.DownloadsInProgressDirectory is null)
		{
			Serilog.Log.Logger.Error("Failed to create DownloadsInProgressDirectory in {InProgress}", config.InProgress);
			await MessageBoxBase.Show(
				$"Libation was unable to create the \"Downloads In Progress\" folder in:\n{config.InProgress}\n\nPlease change the In Progress location in the settings menu.",
				"Failed to Create Downloads In Progress Directory",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return false;
		}
		else if (AudibleFileStorage.DecryptInProgressDirectory is null)
		{
			Serilog.Log.Logger.Error("Failed to create DecryptInProgressDirectory in {InProgress}", config.InProgress);
			await MessageBoxBase.Show(
				$"Libation was unable to create the \"Decrypt In Progress\" folder in:\n{config.InProgress}\n\nPlease change the In Progress location in the settings menu.",
				"Failed to Create Decrypt In Progress Directory",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return false;
		}

		return true;
	}

	private bool IsBookInQueue(LibraryBook libraryBook)
		=> Queue.FirstOrDefault(b => b?.LibraryBook?.Book?.AudibleProductId == libraryBook.Book.AudibleProductId) is not ProcessBookViewModel entry ? false
		: entry.Status is ProcessBookStatus.Cancelled or ProcessBookStatus.Failed ? !Queue.RemoveCompleted(entry)
		: true;

	private bool RemoveCompleted(LibraryBook libraryBook)
		=> Queue.FirstOrDefault(b => b?.LibraryBook?.Book?.AudibleProductId == libraryBook.Book.AudibleProductId) is ProcessBookViewModel entry
		&& entry.Status is ProcessBookStatus.Completed
		&& Queue.RemoveCompleted(entry);

	/// <summary>
	/// ProcessBookViewModel requires a captured UI SynchronizationContext. Callers may resume on a
	/// thread-pool thread after await (e.g. auto-download after BackgroundWorker).
	/// </summary>
	private void RunOnQueueUiThread(Action action) => Invoke(action);

	private void AddDownloadPdf(IList<LibraryBook> entries, Configuration config)
		=> RunOnQueueUiThread(() => addDownloadPdfCore(entries, config));

	private void addDownloadPdfCore(IList<LibraryBook> entries, Configuration config)
	{
		var procs = entries.Where(e => !IsBookInQueue(e)).Select(Create).ToArray();
		Serilog.Log.Logger.Information("Queueing {count} books for PDF-only download", procs.Length);
		AddToQueue(procs);

		ProcessBookViewModel Create(LibraryBook entry)
			=> new ProcessBookViewModel(entry, config).AddDownloadPdf();
	}

	private void AddDownloadDecrypt(IList<LibraryBook> entries, Configuration config)
		=> RunOnQueueUiThread(() => addDownloadDecryptCore(entries, config));

	private void addDownloadDecryptCore(IList<LibraryBook> entries, Configuration config)
	{
		var procs = entries.Where(e => !IsBookInQueue(e)).Select(Create).ToArray();
		Serilog.Log.Logger.Information("Queueing {count} books ofr download/decrypt", procs.Length);
		AddToQueue(procs);

		ProcessBookViewModel Create(LibraryBook entry)
			=> new ProcessBookViewModel(entry, config).AddDownloadDecryptBook().AddDownloadPdf();
	}

	private void AddConvertMp3(IList<LibraryBook> entries, Configuration config)
		=> RunOnQueueUiThread(() => addConvertMp3Core(entries, config));

	private void addConvertMp3Core(IList<LibraryBook> entries, Configuration config)
	{
		var procs = entries.Where(e => !IsBookInQueue(e)).Select(Create).ToArray();
		Serilog.Log.Logger.Information("Queueing {count} books for mp3 conversion", procs.Length);
		AddToQueue(procs);

		ProcessBookViewModel Create(LibraryBook entry)
			=> new ProcessBookViewModel(entry, config).AddConvertToMp3();
	}

	private void AddToQueue(IList<ProcessBookViewModel> pbook)
	{
		foreach (var book in pbook)
			book.LogWritten += ProcessBook_LogWritten;

		Queue.Enqueue(pbook);
		if (!Running)
			QueueRunner = Task.Run(QueueLoop);
	}

	#endregion
	public event EventHandler<ProcessBookViewModel>? ProcessStart;
	public event EventHandler<ProcessBookViewModel>? ProcessEnd;
	private async Task QueueLoop()
	{
		try
		{
			Serilog.Log.Logger.Information("Begin processing queue");

			RunningTime = string.Empty;
			ProgressBarVisible = true;
			var startingTime = DateTime.Now;
			bool shownLicenseGuidanceMessage = false;
			bool shownDiskFullMessage = false;

			using var counterTimer = new System.Threading.Timer(_ => RunningTime = timeToStr(DateTime.Now - startingTime), null, 0, 500);

			while (Queue.MoveNext())
			{
				if (Queue.Current is not ProcessBookViewModel nextBook)
				{
					Serilog.Log.Logger.Information("Current queue item is empty.");
					continue;
				}

				Serilog.Log.Logger.Information("Begin processing queued item: '{item_LibraryBook}'", nextBook.LibraryBook);
				SpeedLimit = nextBook.Configuration.DownloadSpeedLimit / 1024m / 1024;
				ProcessStart?.Invoke(this, nextBook);
				var result = await nextBook.ProcessOneAsync();

				Serilog.Log.Logger.Information("Completed processing queued item: '{item_LibraryBook}' with result: {result}", nextBook.LibraryBook, result);

				if (result == ProcessBookResult.ValidationFail)
					Queue.ClearCurrent();
				else if (result == ProcessBookResult.FailedAbort)
					Queue.ClearQueue();
				// Stop the whole queue on first real disk-full write (local or network); do not retry hundreds of titles.
				else if (result == ProcessBookResult.DiskFull)
				{
					if (!shownDiskFullMessage)
					{
						await MessageBoxBase.Show(
							DiskFullUserMessage.BuildQueueStoppedBody(),
							DiskFullUserMessage.DialogCaption,
							MessageBoxButtons.OK,
							MessageBoxIcon.Warning);
						shownDiskFullMessage = true;
					}
					Queue.ClearQueue();
				}
				else if (result == ProcessBookResult.FailedSkip)
					await nextBook.LibraryBook.UpdateBookStatusAsync(LiberatedStatus.Error);
				else if (!shownLicenseGuidanceMessage
					&& (result == ProcessBookResult.LicenseDeniedPossibleOutage
						|| (result == ProcessBookResult.LicenseDenied && nextBook.LibraryBook.IsAudiblePlus)))
				{
					var body = result == ProcessBookResult.LicenseDeniedPossibleOutage
						? ContentLicenseDeniedUserMessage.BuildDialogBodyForPossibleOutage(nextBook.LibraryBook.Book.TitleWithSubtitle)
						: ContentLicenseDeniedUserMessage.BuildDialogBodyForPlusCatalog(nextBook.LibraryBook.Book.TitleWithSubtitle);
					await MessageBoxBase.Show(
						body,
						ContentLicenseDeniedUserMessage.DialogCaption,
						MessageBoxButtons.OK,
						MessageBoxIcon.Asterisk);
					shownLicenseGuidanceMessage = true;
				}
				ProcessEnd?.Invoke(this, nextBook);
			}
			Serilog.Log.Logger.Information("Completed processing queue");

			Queue_CompletedCountChanged(this, 0);
			ProgressBarVisible = false;
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "An error was encountered while processing queued items");
		}
		finally
		{
			DiskSpaceBackupPreflight.ResetBulkPreflightForQueueRun();
		}

		string timeToStr(TimeSpan time)
			=> time.TotalHours < 1 ? $"{time:mm\\:ss}"
			: $"{time.TotalHours:F0}:{time:mm\\:ss}";
	}
}
