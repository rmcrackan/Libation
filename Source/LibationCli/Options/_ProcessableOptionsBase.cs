using ApplicationServices;
using AudibleApi;
using CommandLine;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli;

public abstract class ProcessableOptionsBase : OptionsBase
{

	[Value(0, MetaName = "[asins]", HelpText = "Optional product IDs (ASINs) of books to process.")]
	public IEnumerable<string>? Asins { get; set; }

	[Option('i', "id", Required = false, HelpText = "Product ID (ASIN) of a book to process. Repeatable. Same as positional [asins].")]
	public IEnumerable<string>? Ids { get; set; }

	protected IEnumerable<string> GetProductIds()
		=> (Asins ?? []).Concat(Ids ?? []).Select(NormalizeProductId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase);

	private static string NormalizeProductId(string id) => id.Trim().TrimStart('[').TrimEnd(']');

	protected static TProcessable CreateProcessable<TProcessable>(EventHandler<LibraryBook>? completedAction = null)
		where TProcessable : Processable, IProcessable<TProcessable>
	{
		var strProc = TProcessable.Create(Configuration.Instance);
		LibraryBook? currentLibraryBook = null;

		if (Environment.UserInteractive && !Console.IsOutputRedirected && !Console.IsErrorRedirected)
		{
			var progressBar = new ConsoleProgressBar(Console.Out);
			strProc.Completed += (_, e) => progressBar.Clear();
			strProc.StreamingTimeRemaining += (_, e) => progressBar.RemainingTime = e;
			strProc.StreamingProgressChanged += (_, e) => progressBar.Progress = e.ProgressPercentage;
		}

		strProc.Begin += (o, e) =>
		{
			currentLibraryBook = e;
			Console.WriteLine($"{typeof(TProcessable).Name} Begin: {e}");
		};

		strProc.Completed += (o, e) =>
		{
			Console.WriteLine($"{typeof(TProcessable).Name} Completed: {e}");
		};

		strProc.Completed += (s, e) =>
		{
			try
			{
				completedAction?.Invoke(s, e);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine("CLI error. See log for more details.");
				Serilog.Log.Logger.Error(ex, "CLI error");
			}
		};

		if (strProc is AudioDecodable audDec)
		{
			audDec.RequestCoverArt += (_, _) =>
			{
				if (currentLibraryBook is null)
					return null;

				var pictureId = Configuration.Instance.FileDownloadQuality == Configuration.DownloadQuality.High
				? currentLibraryBook.Book.PictureLarge ?? currentLibraryBook.Book.PictureId
				: currentLibraryBook.Book.PictureId;

				return pictureId is null ? null : PictureStorage.GetPictureSynchronously(new PictureDefinition(pictureId, PictureSize.Native));
			};
		}

		return strProc;
	}

	/// <summary>How much this run may download before it stops, or null for a verb without a per-run limit.</summary>
	protected virtual RunDownloadLimit? RunLimit => null;

	/// <summary>
	/// Whether this run should leave alone the titles Audible recently refused. False for a run that names
	/// its titles or passes --force: an explicit request is always attempted.
	/// </summary>
	internal virtual bool HonorsDeferredRetries => false;

	protected async Task RunAsync(Processable Processable, Action<LibraryBook>? config = null, Action<string>? notFound = null)
	{
		var skippedForDailyLimit = 0;
		var deferredThisRun = new List<DeferredDownload>();
		var runLimitReached = false;

		// Needs no guard against pdf or convert runs, unlike the daily limit below: the tracker counts only
		// audiobook downloads this run recorded, and those verbs record none.
		var runLimit = RunLimit is RunDownloadLimit limit ? new RunLimitTracker(limit, DateTimeOffset.Now) : null;

		var productIds = GetProductIds().ToArray();
		if (productIds.Length > 0)
		{
			foreach (var asin in productIds)
			{
				if (DbContexts.GetLibraryBook_Flat_NoTracking(asin, caseSensative: false) is LibraryBook lb)
				{
					if (!await ProcessOrStopAsync(lb, true))
						break;
				}
				else
				{
					var msg = $"Book with ASIN '{asin}' not found in library. Skipping.";
					Console.Error.WriteLine(msg);
					Serilog.Log.Logger.Error(msg);
					notFound?.Invoke(asin);
				}
			}
		}
		else
		{
			// Read once, before the first book: a run that spends hours downloading must not start skipping
			// titles because of failures it recorded itself a moment ago.
			var deferrals = HonorsDeferredRetries ? DownloadDeferrals.Load(DateTimeOffset.Now) : DownloadDeferrals.None;

			var libraryBooks = DbContexts.GetLibrary_Flat_NoTracking();
			foreach (var lb in Processable.GetValidLibraryBooks(libraryBooks))
			{
				if (deferrals.Find(lb) is DeferredDownload deferred)
				{
					deferredThisRun.Add(deferred);
					Serilog.Log.Logger.Information(
						"Not attempting {libraryBook} yet. {@DebugInfo}",
						lb.LogFriendly(),
						new { deferred.Kind, deferred.ConsecutiveFailures, deferred.Reason, RetryAfter = deferred.RetryAfter.ToLocalTime() });
					continue;
				}

				if (!await ProcessOrStopAsync(lb, false))
					break;
			}
		}

		if (deferredThisRun.Count > 0)
		{
			var now = DateTimeOffset.Now;
			foreach (var line in DeferredDownloadUserMessage.BuildCliSkippedLines(deferredThisRun, now))
				Console.WriteLine(line);

			Serilog.Log.Logger.Information(
				"Skipped {deferredCount} titles that recently failed to download. Skipped: {skipped}",
				deferredThisRun.Count,
				DeferredDownloadUserMessage.BuildLogBreakdown(deferredThisRun));
		}

		if (skippedForDailyLimit > 0)
		{
			var summary = DailyDownloadLimitUserMessage.BuildCliSkippedSummary(skippedForDailyLimit);
			Console.WriteLine(summary);
			Serilog.Log.Logger.Information(summary);
		}

		var done = runLimitReached
			? "Done. Stopped early: this run's download limit was reached."
			: "Done. All books have been processed";
		Console.WriteLine(done);
		Serilog.Log.Logger.Information(done);

		// False ends the run. The limit is checked here rather than at the top of the run so that a run whose
		// books happen to end exactly at the limit says nothing: nothing was cut short.
		async Task<bool> ProcessOrStopAsync(LibraryBook libraryBook, bool validate)
		{
			if (runLimit is not null && runLimit.TryStop(out var stopMessage))
			{
				runLimitReached = true;
				Console.WriteLine(stopMessage);
				Serilog.Log.Logger.Information(stopMessage);
				return false;
			}

			config?.Invoke(libraryBook);

			if (IsSkippedByDailyLimit(Processable, libraryBook))
			{
				skippedForDailyLimit++;
				return true;
			}

			runLimit?.Attempting(libraryBook.Book.AudibleProductId);
			await ProcessOneAsync(Processable, libraryBook, validate);
			return true;
		}
	}

	protected bool announcedDailyLimit;

	/// <summary>
	/// True when the user's daily download limit covers this title and has been reached. Unlike the GUI queue the
	/// CLI never waits: a command-line run must not sit idle for hours, and in Docker the entrypoint has to return
	/// so its own sleep loop keeps working. Skipped titles stay un-liberated and are retried on the next run.
	/// </summary>
	protected bool IsSkippedByDailyLimit(Processable processable, LibraryBook libraryBook)
	{
		// Only audiobook downloads are limited, and only titles the configured scope covers, so the common
		// case of no limit (or a Plus-only limit against an owned title) costs nothing.
		if (processable is not DownloadDecryptBook
			|| !DailyDownloadLimit.AppliesTo(libraryBook.IsAudiblePlus, Configuration.Instance))
			return false;

		var now = DateTimeOffset.Now;
		var allowance = DailyDownloadLimit.Evaluate(Configuration.Instance, DownloadHistoryStore.GetCurrentWindow(now), now);

		if (!allowance.Blocks(libraryBook.IsAudiblePlus))
			return false;

		if (!announcedDailyLimit)
		{
			announcedDailyLimit = true;
			foreach (var line in DailyDownloadLimitUserMessage.BuildCliSkippedLines(allowance))
			{
				Console.Error.WriteLine(line);
				Serilog.Log.Logger.Information(line);
			}
		}

		Serilog.Log.Logger.Information(
			"Daily download limit reached; skipping {libraryBook}. {@DebugInfo}",
			libraryBook.LogFriendly(),
			new { allowance.Scope, allowance.Unit, allowance.Quantity, allowance.UsedBooks, allowance.UsedBytes, allowance.NextCapacityAt });

		return true;
	}

	protected async Task ProcessOneAsync(Processable Processable, LibraryBook libraryBook, bool validate)
	{
		try
		{
			var statusHandler = await Processable.ProcessSingleAsync(libraryBook, validate);

			if (statusHandler.IsSuccess)
				return;

			foreach (var errorMessage in statusHandler.Errors)
			{
				Console.Error.WriteLine(errorMessage);
				Serilog.Log.Logger.Error(errorMessage);
			}
		}
		catch (ApiErrorException ex) when (WidevineRecommendation.ShouldRecommendWidevine(ex, Configuration.Instance))
		{
			Console.Error.WriteLine(WidevineRecommendation.BuildLogSummary(libraryBook.Book.TitleWithSubtitle));
			Serilog.Log.Logger.Error(ex, "ADRM license unavailable (Sable acr:null) {@DebugInfo}", new { Book = libraryBook.LogFriendly() });
			ReportNextAttempt(libraryBook);
		}
		catch (ContentLicenseDeniedException clEx)
		{
			foreach (var line in ContentLicenseDeniedCliSummary.Lines(clEx))
				Console.Error.WriteLine(line);
			Serilog.Log.Logger.Error(clEx, "Content license denied {@DebugInfo}", new { Book = libraryBook.LogFriendly() });
			ReportNextAttempt(libraryBook);
		}
		catch (Exception ex)
		{
			var msg = "Error processing book. Skipping. For options of skipping or marking as error, retry with main Libation app.";
			Console.Error.WriteLine(msg + ". See log for more details.");
			Serilog.Log.Logger.Error(ex, $"{msg} {{@DebugInfo}}", new { Book = libraryBook.LogFriendly() });

			if (!ReportNextAttempt(libraryBook))
				Console.Error.WriteLine("This book will be tried again on next attempt.");
		}
	}

	/// <summary>
	/// Says when a title Libation has decided to wait on will be attempted again, so a scheduled run explains
	/// its own silence on the next several runs rather than appearing to have forgotten the title.
	/// </summary>
	/// <returns>True when the title is being waited on.</returns>
	private static bool ReportNextAttempt(LibraryBook libraryBook)
	{
		var now = DateTimeOffset.Now;
		if (DownloadAttemptFailureStore.Find(libraryBook, now) is not DeferredDownload deferred)
			return false;

		Console.Error.WriteLine(
			$"Not attempting this title again {DeferredDownloadUserMessage.DescribeWhen(deferred.RetryAfter, now)}. "
			+ "To try it sooner, name it: libationcli liberate " + libraryBook.Book.AudibleProductId);
		return true;
	}
}
