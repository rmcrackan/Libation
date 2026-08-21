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
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase.ProcessQueue;

public record LogEntry(DateTime LogDate, string LogMessage)
{
	public string LogDateString => LogDate.ToShortTimeString();
}

public class ProcessQueueViewModel : ReactiveObject
{
	/// <summary>
	/// How often a queue paused on the daily download limit re-checks. Short and fixed: never a delay computed
	/// from when capacity is expected, so a change of setting or the window rolling over is picked up promptly.
	/// </summary>
	private static readonly TimeSpan DailyLimitPollInterval = TimeSpan.FromSeconds(15);

	public ObservableCollection<LogEntry> LogEntries { get; } = new();
	public TrackedQueue<ProcessBookViewModel> Queue { get; } = new();
	private readonly BadBookSessionContext _badBookSession = new();
	public Task? QueueRunner { get; private set; }
	public bool Running => !QueueRunner?.IsCompleted ?? false;

	/// <summary>Set by <see cref="CancelAllAsync"/>; watched by the daily download limit wait loop.</summary>
	private volatile bool cancelAllRequested;
	private bool dailyLimitMessageShownThisRun;

	/// <summary>
	/// The single call the dispatch loop makes into a book. Replaced in tests by a fake that finishes
	/// on command, which is what lets the loop itself - the capacity cap, the enqueue signal, the
	/// abort drain - be driven without downloading anything. A seam rather than a refactor: the loop
	/// is unchanged and this is the only line that knows how a book is processed.
	/// </summary>
	internal Func<ProcessBookViewModel, Task<ProcessBookResult>> ProcessBookHandler { get; set; }
		= book => book.ProcessOneAsync();

	public ProcessQueueViewModel()
	{
		// The queue is mutated from book threads and read by index from the UI thread, so its
		// notifications have to arrive on the UI thread in the order the queue actually changed.
		// alwaysInvoke: true is the part that matters - it makes BeginInvoke post unconditionally.
		// A plain invoker runs inline when it is already on the UI thread, which would let a
		// UI-thread mutation deliver ahead of notifications a book thread posted earlier.
		// Null context means nobody is bound to this queue anyway; delivery stays inline.
		if (SynchronizationContext.Current is not null)
			Queue.NotificationInvoker = new Dinah.Core.Threading.SynchronizeInvoker(alwaysInvoke: true);

		Queue.QueuedCountChanged += Queue_QueuedCountChanged;
		Queue.CompletedCountChanged += Queue_CompletedCountChanged;
		SpeedLimit = Configuration.Instance.DownloadSpeedLimit / 1024m / 1024;
		// Assigned to the field, not through the property: constructing the view model must not write
		// the setting back, or opening the queue on a smaller machine would overwrite what the user chose.
		_maxConcurrentDownloads = Configuration.Instance.MaxConcurrentDownloads;
		AutoScrollQueue = Configuration.Instance.AutoScrollQueue;
	}

	public int CompletedCount { get => field; private set { RaiseAndSetIfChanged(ref field, value); RaisePropertyChanged(nameof(AnyCompleted)); } }
	public int QueuedCount { get => field; private set { this.RaiseAndSetIfChanged(ref field, value); RaisePropertyChanged(nameof(AnyQueued)); } }
	public int ErrorCount { get => field; private set { RaiseAndSetIfChanged(ref field, value); RaisePropertyChanged(nameof(AnyErrors)); } }
	/// <summary>
	/// How many books download and decrypt at once. <see cref="Configuration.MinConcurrentDownloads"/>
	/// means one at a time, which is how Libation behaved before parallel downloads existed - so this
	/// single value is both the limit and the off switch, and the two can never disagree.
	/// </summary>
	public int MaxConcurrentDownloads
	{
		get => _maxConcurrentDownloads;
		set
		{
			var clamped = Math.Clamp(value, Configuration.MinConcurrentDownloads, Configuration.ConcurrentDownloadsHardLimit);
			RaiseAndSetIfChanged(ref _maxConcurrentDownloads, clamped);
			Configuration.Instance.MaxConcurrentDownloads = clamped;
			RaisePropertyChanged(nameof(MaxAllowedConcurrentDownloads));
		}
	}
	private int _maxConcurrentDownloads;

	/// <summary>
	/// How many books actually run at once: what the user asked for, held down to what this machine
	/// can usefully manage. Applied here, at the point of use, so the stored setting is left alone.
	/// </summary>
	private int EffectiveConcurrentDownloads
		=> Math.Clamp(MaxConcurrentDownloads, Configuration.MinConcurrentDownloads, Configuration.MaxAllowedConcurrentDownloads);
	public bool AutoScrollQueue { get => field; set { RaiseAndSetIfChanged(ref field, value); Configuration.Instance.AutoScrollQueue = value; } }

	/// <summary>Exposed so UI controls can bind their spinner bounds rather than hardcoding them.</summary>
	public int MinConcurrentDownloads => Configuration.MinConcurrentDownloads;

	/// <summary>
	/// The spinner's upper bound: what this machine can usefully manage, but never below what is
	/// already stored. A spinner whose maximum sits under the stored value coerces its displayed
	/// value down to the maximum and, being two-way, writes that back - which would overwrite an 8
	/// chosen on a larger machine with a 2 just for opening the panel on a smaller one. Raising the
	/// bound to meet the stored value leaves the setting alone; lowering it is still the user's to do.
	/// </summary>
	public int MaxAllowedConcurrentDownloads
		=> Math.Max(Configuration.MaxAllowedConcurrentDownloads, MaxConcurrentDownloads);
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
			// Apply to all currently active books. Over a copy: the speed limit is changed from the UI
			// thread while book tasks start and finish, and Active is the live list.
			foreach (var activeBook in Queue.GetActive().OfType<ProcessBookViewModel>())
				activeBook.Configuration.DownloadSpeedLimit = config.DownloadSpeedLimit;

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
			or ProcessBookResult.WidevineRecommended
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

	private void ProcessBook_LogWritten(object? sender, string logMessage) => AddQueueLogEntry(logMessage);

	private void AddQueueLogEntry(string logMessage)
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

	/// <summary>
	/// Queues visible books with an instant simulated failure for testing the bad-book error dialog.
	/// Does not download or modify files.
	/// </summary>
	public void QueueSimulatedBadBookFailures(IList<LibraryBook> libraryBooks, Configuration? config = null, int maxBooks = 5)
	{
		config ??= Configuration.Instance;
		if (libraryBooks.Count == 0)
			return;

		RunOnQueueUiThread(() => addSimulatedBadBookFailuresCore(libraryBooks, config, maxBooks));
	}

	private void addSimulatedBadBookFailuresCore(IList<LibraryBook> libraryBooks, Configuration config, int maxBooks)
	{
		var procs = libraryBooks
			.Where(e => !IsBookInQueue(e))
			.Take(maxBooks)
			.Select(entry => new ProcessBookViewModel(entry, config, _badBookSession).AddSimulateBadBookFailure())
			.ToArray();

		if (procs.Length == 0)
			return;

		Serilog.Log.Logger.Information("Queueing {count} books for simulated bad-book failure testing", procs.Length);
		AddToQueue(procs);
	}

	/// <param name="notifyIfNothingQueued">
	/// Whether to tell the user when a multi-book request queued nothing. Always logged either way. Automated
	/// callers (auto-download after a scan) pass false so a routine no-op cannot put a dialog on screen.
	/// </param>
	public async Task<bool> QueueDownloadDecryptAsync(IList<LibraryBook> libraryBooks, Configuration? config = null, bool notifyIfNothingQueued = true)
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
			// Titles Audible recently refused are left out of a multi-book request but never out of a
			// single-title one: picking one title is the user overriding the wait.
			var request = BackupRequest.Create(libraryBooks, DownloadDeferrals.Load(DateTimeOffset.Now));

			if (request.Queueable.Length == 0)
			{
				// This branch used to return with no log entry and no message, so a request Libation had
				// understood and declined was indistinguishable from a dead button.
				Serilog.Log.Logger.Information(
					"Download not queued: none of the {requested} requested titles need downloading. Skipped: {skipped}",
					request.RequestedCount,
					request.BuildSkippedLogSummary());

				if (notifyIfNothingQueued)
					await MessageBoxBase.Show(
						request.BuildNothingQueuedBody(),
						BackupRequest.NothingQueuedCaption,
						MessageBoxButtons.OK,
						MessageBoxIcon.Information);

				return false;
			}

			if (request.SkippedCount > 0)
				Serilog.Log.Logger.Information(
					"Skipping {skippedCount} of {requested} requested titles. Skipped: {skipped}",
					request.SkippedCount,
					request.RequestedCount,
					request.BuildSkippedLogSummary());

			if (request.Deferred.Count > 0)
				AddQueueLogEntry(request.BuildDeferredDetail(DateTimeOffset.Now));

			// May no-op when free space is unknown (common on UNC); see DiskSpaceBackupPreflight.
			if (!await DiskSpaceBackupPreflight.ConfirmBulkBackupAsync(request.Queueable.Length, config, backupQueueAlreadyRunning: Running))
				return false;

			Serilog.Log.Logger.Information("Begin backup of {count} library books", request.Queueable.Length);
			AddDownloadDecrypt(request.Queueable, config);
			return true;
		}
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
			=> new ProcessBookViewModel(entry, config, _badBookSession).AddDownloadPdf();
	}

	private void AddDownloadDecrypt(IList<LibraryBook> entries, Configuration config)
		=> RunOnQueueUiThread(() => addDownloadDecryptCore(entries, config));

	private void addDownloadDecryptCore(IList<LibraryBook> entries, Configuration config)
	{
		var procs = entries.Where(e => !IsBookInQueue(e)).Select(Create).ToArray();
		Serilog.Log.Logger.Information("Queueing {count} books for download/decrypt", procs.Length);
		if (procs.Length < entries.Count)
			Serilog.Log.Logger.Information("{count} of the requested books are already in the queue and were not added again", entries.Count - procs.Length);
		AddToQueue(procs);

		ProcessBookViewModel Create(LibraryBook entry)
			=> new ProcessBookViewModel(entry, config, _badBookSession).AddDownloadDecryptBook().AddDownloadPdf().AddUploadToAudiobookshelf();
	}

	private void AddConvertMp3(IList<LibraryBook> entries, Configuration config)
		=> RunOnQueueUiThread(() => addConvertMp3Core(entries, config));

	private void addConvertMp3Core(IList<LibraryBook> entries, Configuration config)
	{
		var procs = entries.Where(e => !IsBookInQueue(e)).Select(Create).ToArray();
		Serilog.Log.Logger.Information("Queueing {count} books for mp3 conversion", procs.Length);
		AddToQueue(procs);

		ProcessBookViewModel Create(LibraryBook entry)
			=> new ProcessBookViewModel(entry, config, _badBookSession).AddConvertToMp3();
	}

	/// <summary>
	/// Internal rather than private so the dispatch loop can be started from a test with books it
	/// controls, without going through the queueing dialogs on the way in.
	/// </summary>
	internal void AddToQueue(IList<ProcessBookViewModel> pbook)
	{
		// Queueing more work withdraws an earlier Cancel All, which may still be settling on the book it
		// cancelled. Otherwise these new books would inherit that cancellation at the daily-limit gate.
		cancelAllRequested = false;

		foreach (var book in pbook)
			book.LogWritten += ProcessBook_LogWritten;

		Queue.Enqueue(pbook);
		SignalEnqueued();
		if (!Running)
			QueueRunner = Task.Run(QueueLoop);
	}

	/// <summary>
	/// Completes when books are added to the queue, so <see cref="QueueLoop"/> can wake on a new
	/// arrival instead of only on a book finishing.
	/// </summary>
	private TaskCompletionSource _enqueueSignal = new(TaskCreationOptions.RunContinuationsAsynchronously);

	private Task WaitForEnqueueAsync() => Volatile.Read(ref _enqueueSignal).Task;

	/// <summary>
	/// Swaps in a fresh signal for the next wait, then completes the old one to release anyone
	/// already waiting on it.
	/// </summary>
	private void SignalEnqueued()
		=> Interlocked
			.Exchange(ref _enqueueSignal, new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously))
			.TrySetResult();

	#endregion

	#region Daily download limit

	private enum DailyLimitGate
	{
		/// <summary>The limit does not stop this book right now.</summary>
		Proceed,
		/// <summary>This book is limited but something else in the queue is not; try it later.</summary>
		Defer,
		/// <summary>The user cancelled the queue while it was waiting.</summary>
		Cancelled
	}

	/// <summary>
	/// Runs immediately before a book downloads, never at queueing time, so the queue keeps its contents and a
	/// user can raise or turn off the limit mid-run. Every iteration re-reads the setting, re-queries the
	/// history and re-reads the clock: a queue left alone for days must resume by itself as its oldest
	/// downloads age out of the rolling window.
	/// </summary>
	/// <param name="deferralsSoFar">
	/// How many books this queue run has already moved to the back for the limit. Compared against the live
	/// queued count so the rotation cannot continue indefinitely.
	/// </param>
	private async Task<DailyLimitGate> WaitForDailyLimitAsync(ProcessBookViewModel nextBook, int deferralsSoFar)
	{
		if (!nextBook.IncludesBookDownload)
			return DailyLimitGate.Proceed;

		var paused = false;

		while (true)
		{
			if (cancelAllRequested)
			{
				nextBook.StatusOverride = null;
				return DailyLimitGate.Cancelled;
			}

			var now = DateTimeOffset.Now;
			var allowance = DailyDownloadLimit.Evaluate(Configuration.Instance, DownloadHistoryStore.GetCurrentWindow(now), now);

			if (!allowance.Blocks(nextBook.LibraryBook.IsAudiblePlus))
			{
				nextBook.StatusOverride = null;
				if (paused)
				{
					var resumed = $"Daily download limit: capacity is available again. Resuming with {nextBook.LibraryBook.Book.TitleWithSubtitle}.";
					Serilog.Log.Logger.Information("Daily download limit no longer blocks {libraryBook}. Resuming the queue.", nextBook.LibraryBook.LogFriendly());
					AddQueueLogEntry(resumed);
				}
				return DailyLimitGate.Proceed;
			}

			// Under "Plus titles only" a mixed queue can keep going; do not stall owned titles behind a Plus title.
			if (deferralsSoFar < QueuedCount && AnyOtherQueuedBookAllowed(nextBook, allowance))
			{
				nextBook.StatusOverride = null;
				Serilog.Log.Logger.Information(
					"Daily download limit blocks {libraryBook}. Moving it to the end of the queue and continuing with titles the limit does not cover.",
					nextBook.LibraryBook.LogFriendly());
				AddQueueLogEntry(DailyDownloadLimitUserMessage.BuildDeferredLogEntry(allowance, nextBook.LibraryBook.Book.TitleWithSubtitle));
				return DailyLimitGate.Defer;
			}

			if (!paused)
			{
				paused = true;
				Serilog.Log.Logger.Information(
					"Daily download limit reached; pausing the queue before {libraryBook}. {@DebugInfo}",
					nextBook.LibraryBook.LogFriendly(),
					new { allowance.Scope, allowance.Unit, allowance.Quantity, allowance.UsedBooks, allowance.UsedBytes, allowance.NextCapacityAt });
				AddQueueLogEntry(DailyDownloadLimitUserMessage.BuildQueueLogEntry(allowance, nextBook.LibraryBook.Book.TitleWithSubtitle));
				ShowDailyLimitMessageOncePerRun(allowance, nextBook.LibraryBook.Book.TitleWithSubtitle);
			}

			nextBook.StatusOverride = DailyDownloadLimitUserMessage.BuildWaitingStatus(allowance);

			await Task.Delay(DailyLimitPollInterval);
		}
	}

	/// <summary>Moves the book being held back to the end of the queue without counting it as completed.</summary>
	private void RequeueLast(ProcessBookViewModel book)
	{
		// Removes this book, not the first active one. ClearCurrent() drops Active[0], which with
		// several books in flight is some other book's download: deferring the second of three
		// active books would silently evict the first instead.
		Queue.RemoveActive(book);
		Queue.Enqueue([book]);
	}

	private bool AnyOtherQueuedBookAllowed(ProcessBookViewModel nextBook, DailyDownloadLimit.Allowance allowance)
		=> Queue.Any(b =>
			b is not null
			&& !ReferenceEquals(b, nextBook)
			&& b.Status is ProcessBookStatus.Queued
			&& (!b.IncludesBookDownload || !allowance.Blocks(b.LibraryBook.IsAudiblePlus)));

	/// <summary>
	/// Deliberately not awaited. This dialog only completes when the user dismisses it, and a queue that is
	/// waiting must be free to resume by itself hours later with nobody at the keyboard. Shown once per queue
	/// run so a multi-day drip-feed does not stack up a dialog per day; later pauses use the log and status.
	/// </summary>
	private void ShowDailyLimitMessageOncePerRun(DailyDownloadLimit.Allowance allowance, string bookTitleWithSubtitle)
	{
		if (dailyLimitMessageShownThisRun)
			return;

		dailyLimitMessageShownThisRun = true;

		_ = MessageBoxBase.Show(
			DailyDownloadLimitUserMessage.BuildQueuePausedBody(allowance, bookTitleWithSubtitle),
			DailyDownloadLimitUserMessage.DialogCaption,
			MessageBoxButtons.OK,
			MessageBoxIcon.Information)
			.ContinueWith(
				t => Serilog.Log.Logger.Error(t.Exception, "Failed to show the daily download limit message"),
				TaskContinuationOptions.OnlyOnFaulted);
	}

	#endregion

	public event EventHandler<ProcessBookViewModel>? ProcessStart;
	public event EventHandler<ProcessBookViewModel>? ProcessEnd;

	/// <summary>
	/// Clears the queue and cancels every book currently downloading. Also ends a pause on the daily
	/// download limit, which is why both UIs call this instead of manipulating the queue directly.
	/// </summary>
	/// <remarks>
	/// <see cref="TrackedQueue{T}.ClearQueue"/> only prevents new work from starting. With parallel
	/// downloads there may be several books already running, and those keep going until they are
	/// cancelled individually.
	/// <para>
	/// Each cancellation is isolated. This runs from the queue loop's abort and disk full paths, so
	/// a single book throwing here would surface through <see cref="Task.WhenAll(Task[])"/>, take
	/// <c>QueueLoop</c> out through its outer catch, and leave the remaining books running
	/// unsupervised with the progress bar still on screen.
	/// </para>
	/// </remarks>
	/// <param name="except">
	/// A book calling this from its own completion path (abort, disk full). It has already finished
	/// and must not be asked to cancel itself.
	/// </param>
	public async Task CancelAllAsync(ProcessBookViewModel? except = null)
	{
		// Still set here, not only in the sequential path this replaced: a queue paused on the daily
		// download limit is waiting inside WaitForDailyLimitAsync and this flag is how it learns to stop.
		cancelAllRequested = true;
		Queue.ClearQueue();

		// Snapshot before cancelling: Active is mutated as each book unwinds.
		var inFlight = Queue.GetActive().Where(b => b != except).ToArray();

		await Task.WhenAll(inFlight.Select(CancelOneAsync));

		static async Task CancelOneAsync(ProcessBookViewModel book)
		{
			try
			{
				await book.CancelAsync();
			}
			catch (Exception ex)
			{
				// One book failing to cancel must not abandon the rest of the list.
				Serilog.Log.Logger.Error(ex, "Error while cancelling {Book}", book.LibraryBook.LogFriendly());
			}
		}
	}

	private async Task QueueLoop()
	{
		try
		{
			Serilog.Log.Logger.Information("Begin processing queue");

			_badBookSession.Reset();
			dailyLimitMessageShownThisRun = false;
			RunningTime = string.Empty;
			ProgressBarVisible = true;
			var startingTime = DateTime.Now;

			// Shared state written from parallel book tasks — protected by resultLock
			bool shownLicenseGuidanceMessage = false;
			bool shownWidevineGuidanceMessage = false;
			bool shownDiskFullMessage = false;
			var resultLock = new object();
			// A plain flag, not a CancellationTokenSource: nothing here is cancellable by token. The
			// book tasks are stopped by CancelAllAsync; this only tells the dispatch loop to stop
			// starting new ones. Written from book tasks and read by the loop, hence Volatile — a
			// captured local lives on the closure, so it can be passed by ref but not marked volatile.
			bool aborted = false;
			var activeTasks = new HashSet<Task>();
			// Bounds the daily-limit deferral rotation, so a book can never be shuffled to the back
			// forever. Counted since the last book that actually started, not since the run began.
			int consecutiveDeferrals = 0;

			using var counterTimer = new Timer(_ => RunningTime = timeToStr(DateTime.Now - startingTime), null, 0, 500);

			async Task ProcessBookAsync(ProcessBookViewModel book)
			{
				Serilog.Log.Logger.Information("Begin processing queued item: '{item_LibraryBook}'", book.LibraryBook);
				ProcessStart?.Invoke(this, book);

				var result = await ProcessBookHandler(book);

				// Claimed before the queue is touched, and before the logging. Only the book that
				// actually answered Abort tears the queue down: Abort is a session-wide override now, so
				// every book in flight arrives here - directly or by inheriting that answer - and each one
				// re-entering CancelAllAsync would have every book asking every other book to cancel.
				// Claiming this early also keeps the window small in which the loop can start another
				// book, which would then outlive the abort by starting after CancelAllAsync snapshots
				// what to cancel.
				bool tearsDownTheQueue
					= (result is ProcessBookResult.FailedAbort or ProcessBookResult.DiskFull)
					&& ClaimAbort();

				Serilog.Log.Logger.Information("Completed processing: '{item_LibraryBook}' result: {result}", book.LibraryBook, result);

				if (result == ProcessBookResult.ValidationFail)
				{
					Queue.RemoveActive(book);
				}
				else
				{
					Queue.MarkCompleted(book);

					if (result == ProcessBookResult.FailedAbort)
					{
						// Same reasoning: several books can run out of disk at once.
						if (tearsDownTheQueue)
							await CancelAllAsync(book);
			// True for the one book that gets to tear the queue down, false for every book that arrives
			// after it. Written under the lock and read by the dispatch loop without it, hence the
			// volatile write.
			bool ClaimAbort()
			{
				lock (resultLock)
				{
					if (aborted)
						return false;
					Volatile.Write(ref aborted, true);
					return true;
				}
			}

					}
					else if (result == ProcessBookResult.DiskFull)
					{
						if (tearsDownTheQueue)
							await CancelAllAsync(book);
						else
						{
							// Inherited the abort rather than answering it. It was cancelled by the book
							// that did, and that is what it should report.
							book.Result = ProcessBookResult.Cancelled;
							book.Status = ProcessBookStatus.Cancelled;
						}
						bool show;
						lock (resultLock) { show = !shownDiskFullMessage; shownDiskFullMessage = true; }
						if (show)
							await MessageBoxBase.Show(
								DiskFullUserMessage.BuildQueueStoppedBody(),
								DiskFullUserMessage.DialogCaption,
								MessageBoxButtons.OK,
								MessageBoxIcon.Warning);
					}
					else if (result == ProcessBookResult.FailedSkip)
					{
						await book.LibraryBook.UpdateBookStatusAsync(LiberatedStatus.Error);
					}
					else if (result == ProcessBookResult.LicenseDeniedPossibleOutage
						|| (result == ProcessBookResult.LicenseDenied && book.LibraryBook.IsAudiblePlus))
					{
						bool show;
						lock (resultLock) { show = !shownLicenseGuidanceMessage; shownLicenseGuidanceMessage = true; }
						if (show)
						{
							var body = result == ProcessBookResult.LicenseDeniedPossibleOutage
								? ContentLicenseDeniedUserMessage.BuildDialogBodyForPossibleOutage(book.LibraryBook.Book.TitleWithSubtitle)
								: ContentLicenseDeniedUserMessage.BuildDialogBodyForPlusCatalog(book.LibraryBook.Book.TitleWithSubtitle);
							await MessageBoxBase.Show(
								body,
								ContentLicenseDeniedUserMessage.DialogCaption,
								MessageBoxButtons.OK,
								MessageBoxIcon.Asterisk);
						}
					}
					else if (result == ProcessBookResult.WidevineRecommended)
					{
						bool show;
						lock (resultLock) { show = !shownWidevineGuidanceMessage; shownWidevineGuidanceMessage = true; }
						if (show)
							await MessageBoxBase.Show(
								WidevineRecommendationUserMessage.BuildDialogBody(book.LibraryBook.Book.TitleWithSubtitle),
								WidevineRecommendationUserMessage.DialogCaption,
								MessageBoxButtons.OK,
								MessageBoxIcon.Asterisk);
					}
				}

				ProcessEnd?.Invoke(this, book);
			}

			while (true)
			{
				// A faulted book task is dropped here, before the closing WhenAll could rethrow it, so
				// its exception has to be observed on the way out or it is lost. ProcessOneAsync can
				// throw out of its finally via GetFailureActionAsync; in the sequential loop that
				// reached the outer catch and was logged, and it still should be.
				activeTasks.RemoveWhere(t =>
				{
					if (!t.IsCompleted)
						return false;
					if (t.IsFaulted)
						Serilog.Log.Logger.Error(t.Exception, "A book failed to process and did not report a result");
					return true;
				});

				// Captured before the queue is inspected. If a book is enqueued between the
				// TryDequeueNext below and the wait at the bottom of the loop, this task is
				// already completed and the wait returns immediately rather than missing it.
				var enqueued = WaitForEnqueueAsync();

				if (Volatile.Read(ref aborted))
				{
					await Task.WhenAll(activeTasks);
					break;
				}

				// If at capacity, wait for a slot to open before trying to dequeue more
				if (activeTasks.Count >= EffectiveConcurrentDownloads)
				{
					await Task.WhenAny(activeTasks);
					continue;
				}

				if (Queue.TryDequeueNext(out var nextBook))
				{
					// Checked as a book is about to start rather than at queueing time, so the queue keeps
					// its contents and the user can raise or turn off the limit mid-run. It belongs in this
					// single dispatch loop and not inside the book task: the gate decides whether a book may
					// start at all, and a per-book wait would have every blocked book polling at once.
					// Books already in flight keep running while the loop is held here.
					var gate = await WaitForDailyLimitAsync(nextBook, consecutiveDeferrals);

					if (gate is DailyLimitGate.Defer)
					{
						consecutiveDeferrals++;
						RequeueLast(nextBook);
						continue;
					}

					if (gate is DailyLimitGate.Cancelled)
					{
						Serilog.Log.Logger.Information("Queue was cancelled while waiting on the daily download limit.");
						nextBook.Result = ProcessBookResult.Cancelled;
						nextBook.Status = ProcessBookStatus.Cancelled;
						// The sequential loop left this to the next MoveNext(). Nothing moves this book
						// off the active list now, so it is retired here.
						Queue.MarkCompleted(nextBook);
						continue;
					}

					consecutiveDeferrals = 0;
					activeTasks.Add(ProcessBookAsync(nextBook));
					continue;
				}

				// Queue is empty; if no tasks are running we are done - unless a book was queued
				// while we were looking, in which case go round again and pick it up.
				if (activeTasks.Count == 0)
				{
					if (enqueued.IsCompleted)
						continue;
					break;
				}

				// Items are still in flight but nothing is queued. Wake on whichever comes first:
				// a book finishing, or a new book being queued. Waiting only on the active tasks
				// would leave newly queued books sitting until an in-flight one happened to
				// finish, so a batch queued a moment after the loop started would trickle in one
				// at a time instead of filling the available slots.
				await Task.WhenAny(activeTasks.Append(enqueued));
			}

			await Task.WhenAll(activeTasks);

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
			// Scoped to the run, not to the drain. A queue parked in WaitForDailyLimitAsync only
			// re-reads this every DailyLimitPollInterval, so clearing it when the last cancellation
			// settles would let the gate wake up, see false, and resume the book just cancelled.
			cancelAllRequested = false;
