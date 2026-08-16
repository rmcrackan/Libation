using DataLayer;
using Dinah.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServices;

/// <summary>
/// Reads and writes the record of refused download attempts that keeps a scheduled run from asking Audible
/// for the same license every time.
/// <para>
/// Every method swallows its own errors. This is bookkeeping that makes downloading quieter; it must never
/// be the reason a download fails, and a broken query must leave downloading exactly as it was before this
/// existed.
/// </para>
/// </summary>
public static class DownloadAttemptFailureStore
{
	/// <summary>
	/// Records a failed attempt, extending the wait before the title is attempted again. A failure of a
	/// different kind than last time restarts the count: Audible changed its mind about why, so the previous
	/// wait no longer describes the situation.
	/// </summary>
	public static void Record(LibraryBook libraryBook, DownloadFailureKind kind, string? reason, DateTimeOffset? failedAt = null)
	{
		ArgumentNullException.ThrowIfNull(libraryBook);

		if (string.IsNullOrWhiteSpace(libraryBook.Account) || string.IsNullOrWhiteSpace(libraryBook.Book.AudibleProductId))
			return;

		try
		{
			var when = failedAt ?? DateTimeOffset.Now;
			var account = libraryBook.Account;
			var productId = libraryBook.Book.AudibleProductId;

			using var context = DbContexts.GetContext();
			var existing = context.DownloadAttemptFailures
				.SingleOrDefault(f => f.Account == account && f.AudibleProductId == productId);

			var consecutiveFailures = existing is null || existing.Kind != kind ? 1 : existing.ConsecutiveFailures + 1;
			var retryAfter = DownloadRetryBackoff.GetRetryAfter(kind, consecutiveFailures, when);

			if (existing is null)
				context.DownloadAttemptFailures.Add(new DownloadAttemptFailure(productId, account, kind, consecutiveFailures, when, retryAfter, Truncate(reason)));
			else
				existing.Record(kind, consecutiveFailures, when, retryAfter, Truncate(reason));

			context.SaveChanges();

			Serilog.Log.Logger.Information(
				"Not attempting {audibleProductId} again until {retryAfter}. {@DebugInfo}",
				productId,
				retryAfter.ToLocalTime(),
				new { Title = libraryBook.Book.TitleWithSubtitle, Account = account.ToMask(), kind, consecutiveFailures, reason });
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(
				ex,
				"Failed to record a refused download attempt. The title will be attempted again on the next run. {@DebugInfo}",
				new { libraryBook.Book.AudibleProductId, Title = libraryBook.Book.TitleWithSubtitle, kind });
		}
	}

	/// <summary>
	/// Forgets any record for this title, so it is attempted again at the next opportunity. Called when a
	/// download succeeds and when the user asks for the title explicitly.
	/// </summary>
	public static void Clear(LibraryBook libraryBook)
	{
		ArgumentNullException.ThrowIfNull(libraryBook);
		Clear(libraryBook.Account, libraryBook.Book.AudibleProductId);
	}

	public static void Clear(string? account, string? audibleProductId)
	{
		if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(audibleProductId))
			return;

		try
		{
			using var context = DbContexts.GetContext();

			// ExecuteDelete rather than a load-then-remove so the common case (nothing recorded) is one
			// statement. Called after every successful download.
			var deleted = context.DownloadAttemptFailures
				.Where(f => f.Account == account && f.AudibleProductId == audibleProductId)
				.ExecuteDelete();

			if (deleted > 0)
				Serilog.Log.Logger.Debug("Cleared the recorded download failure for {audibleProductId}", audibleProductId);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to clear the recorded download failure for {audibleProductId}", audibleProductId);
		}
	}

	/// <summary>Titles whose wait has not elapsed. Empty when the query fails, so downloading carries on.</summary>
	public static IReadOnlyList<DeferredDownload> GetDeferred(DateTimeOffset now)
	{
		try
		{
			var ticks = now.UtcTicks;

			using var context = DbContexts.GetContext();
			return Project(context.DownloadAttemptFailures.AsNoTracking().Where(f => f.RetryAfterUtcTicks > ticks));
		}
		catch (Exception ex)
		{
			// Failing open is the safer default: a broken query must not stop titles from being downloaded.
			Serilog.Log.Logger.Error(ex, "Failed to read recorded download failures. Treating every title as ready to attempt.");
			return [];
		}
	}

	/// <summary>The current wait for one title, or null when it is ready to be attempted. Null when the query fails.</summary>
	public static DeferredDownload? Find(LibraryBook libraryBook, DateTimeOffset now)
	{
		ArgumentNullException.ThrowIfNull(libraryBook);

		var account = libraryBook.Account;
		var productId = libraryBook.Book.AudibleProductId;

		if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(productId))
			return null;

		try
		{
			var ticks = now.UtcTicks;

			using var context = DbContexts.GetContext();
			return Project(
				context.DownloadAttemptFailures
					.AsNoTracking()
					.Where(f => f.Account == account && f.AudibleProductId == productId && f.RetryAfterUtcTicks > ticks))
				.FirstOrDefault();
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to read the recorded download failure for {audibleProductId}", productId);
			return null;
		}
	}

	private static List<DeferredDownload> Project(IQueryable<DownloadAttemptFailure> query)
		=> query
			// Materialise the columns first: the record's constructor is not translatable to SQL.
			.Select(f => new
			{
				f.Account,
				f.AudibleProductId,
				f.Kind,
				f.ConsecutiveFailures,
				f.LastFailedAtUtcTicks,
				f.RetryAfterUtcTicks,
				f.Reason
			})
			.ToList()
			.Select(f => new DeferredDownload(
				f.Account,
				f.AudibleProductId,
				f.Kind,
				f.ConsecutiveFailures,
				new DateTimeOffset(f.LastFailedAtUtcTicks, TimeSpan.Zero),
				new DateTimeOffset(f.RetryAfterUtcTicks, TimeSpan.Zero),
				f.Reason))
			.ToList();

	/// <summary>Audible's messages can run long; the full text is already in the log.</summary>
	private static string? Truncate(string? reason)
		=> reason is null || reason.Length <= 400 ? reason : reason[..400];
}
