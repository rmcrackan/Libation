using ApplicationServices;
using AppScaffolding;
using DataLayer;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace HangoverBase;

/// <summary>
/// One-time utility to find and remove duplicate <see cref="Book"/> + <see cref="LibraryBook"/> rows
/// that share the same <see cref="Book.AudibleProductId"/>.
/// </summary>
public static class DuplicateAsinCleanup
{
	public const string SettingsKey = "HangoverDuplicateAsinCleanupCompleted";

	public static bool IsCompleted()
		=> UNSAFE_MigrationHelper.Settings_TryGet(SettingsKey, out var value) && value == "true";

	public static DuplicateAsinScanResult Scan()
	{
		using var context = DbContexts.GetContext();
		var groups = GetDuplicateGroups(context);
		return new DuplicateAsinScanResult(groups.Count, BuildReport(groups, previewOnly: true));
	}

	public static DuplicateAsinCleanupResult Execute()
	{
		if (IsCompleted())
			return new DuplicateAsinCleanupResult(0, 0, "Duplicate ASIN cleanup has already been completed for this library.");

		using var context = DbContexts.GetContext();
		var groups = GetDuplicateGroups(context);

		if (groups.Count == 0)
		{
			MarkCompleted();
			return new DuplicateAsinCleanupResult(0, 0, "No duplicate ASIN rows found. Nothing to do.");
		}

		var removedBooks = 0;
		var removedLibraryBooks = 0;

		foreach (var group in groups)
		{
			var keeper = SelectKeeper(group);
			var duplicates = group.Where(lb => !ReferenceEquals(lb, keeper)).ToList();

			foreach (var duplicate in duplicates)
			{
				MergeIntoKeeper(context, keeper, duplicate);

				context.LibraryBooks.Remove(duplicate);
				context.Books.Remove(duplicate.Book);

				removedLibraryBooks++;
				removedBooks++;
			}
		}

		var changes = context.SaveChanges();
		MarkCompleted();

		var summary = new StringBuilder();
		summary.AppendLine($"Removed {removedBooks} duplicate Book row(s) and {removedLibraryBooks} LibraryBook row(s).");
		summary.AppendLine($"SaveChanges reported {changes} database change(s).");
		summary.AppendLine();
		summary.Append(BuildReport(groups, previewOnly: false));

		return new DuplicateAsinCleanupResult(groups.Count, removedBooks, summary.ToString());
	}

	private static List<List<LibraryBook>> GetDuplicateGroups(LibationContext context)
	{
		var libraryBooks = context.LibraryBooks
			.Include(lb => lb.Book).ThenInclude(b => b.UserDefinedItem)
			.Include(lb => lb.Book).ThenInclude(b => b.SeriesLink).ThenInclude(sb => sb.Series)
			.ToList();

		return libraryBooks
			.GroupBy(lb => lb.Book.AudibleProductId, StringComparer.OrdinalIgnoreCase)
			.Where(g => g.Count() > 1)
			.Select(g => g.OrderByDescending(Score).ThenBy(lb => lb.DateAdded).ToList())
			.ToList();
	}

	private static LibraryBook SelectKeeper(List<LibraryBook> group)
		=> group.OrderByDescending(Score).ThenBy(lb => lb.DateAdded).First();

	private static int Score(LibraryBook libraryBook)
	{
		var book = libraryBook.Book;
		var udi = book.UserDefinedItem;

		var score = 0;
		if (!libraryBook.IsDeleted)
			score += 1000;

		score += bookStatusRank(udi.BookStatus) * 10;
		score += bookStatusRank(udi.PdfStatus ?? LiberatedStatus.NotLiberated);

		score += book.ContentType switch
		{
			ContentType.Parent => 15,
			ContentType.Episode => 15,
			ContentType.Product => 10,
			_ => 0,
		};

		if (book.SeriesLink.Any())
			score += 5;
		if (!string.IsNullOrWhiteSpace(book.PictureId))
			score += 2;

		return score;
	}

	private static int bookStatusRank(LiberatedStatus status) => status switch
	{
		LiberatedStatus.Liberated => 10,
		LiberatedStatus.PartialDownload => 8,
		LiberatedStatus.NotLiberated => 3,
		LiberatedStatus.Error => 1,
		_ => 0,
	};

	private static void MergeIntoKeeper(LibationContext context, LibraryBook keeper, LibraryBook duplicate)
	{
		var keeperBook = keeper.Book;
		var duplicateBook = duplicate.Book;
		var keeperUdi = keeperBook.UserDefinedItem;
		var duplicateUdi = duplicateBook.UserDefinedItem;

		if (bookStatusRank(duplicateUdi.BookStatus) > bookStatusRank(keeperUdi.BookStatus))
			keeperUdi.BookStatus = duplicateUdi.BookStatus;

		if (duplicateUdi.PdfStatus is LiberatedStatus duplicatePdf
			&& bookStatusRank(duplicatePdf) > bookStatusRank(keeperUdi.PdfStatus ?? LiberatedStatus.NotLiberated))
			keeperUdi.SetPdfStatus(duplicatePdf);

		if (!string.IsNullOrWhiteSpace(duplicateUdi.Tags))
		{
			var mergedTags = keeperUdi.TagsEnumerated
				.Concat(duplicateUdi.TagsEnumerated)
				.Distinct(StringComparer.OrdinalIgnoreCase);
			keeperUdi.Tags = string.Join(" ", mergedTags);
		}

		if (duplicateUdi.LastDownloaded is DateTime duplicateLast
			&& (keeperUdi.LastDownloaded is null || duplicateLast > keeperUdi.LastDownloaded))
			keeperUdi.SetLastDownloaded(duplicateUdi.LastDownloadedVersion, duplicateUdi.LastDownloadedFormat, duplicateUdi.LastDownloadedFileVersion);

		if (duplicateUdi.Rating.OverallRating > keeperUdi.Rating.OverallRating)
			keeperUdi.UpdateRating(duplicateUdi.Rating.OverallRating, duplicateUdi.Rating.PerformanceRating, duplicateUdi.Rating.StoryRating);

		foreach (var seriesBook in duplicateBook.SeriesLink)
		{
			if (keeperBook.SeriesLink.Any(sb => sb.Series.AudibleSeriesId == seriesBook.Series.AudibleSeriesId))
				continue;
			keeperBook.UpsertSeries(seriesBook.Series, seriesBook.Order, context);
		}
	}

	private static string BuildReport(List<List<LibraryBook>> groups, bool previewOnly)
	{
		var builder = new StringBuilder();

		if (groups.Count == 0)
		{
			builder.Append("No duplicate ASIN rows found.");
			return builder.ToString();
		}

		var duplicateRowCount = groups.Sum(g => g.Count - 1);
		builder.Append(previewOnly
			? $"Found {groups.Count} duplicate ASIN group(s) ({duplicateRowCount} extra row(s) would be removed)."
			: $"Processed {groups.Count} duplicate ASIN group(s).");
		builder.AppendLine();
		builder.AppendLine();

		foreach (var group in groups.OrderBy(g => g[0].Book.AudibleProductId, StringComparer.OrdinalIgnoreCase))
		{
			var keeper = SelectKeeper(group);
			var asin = keeper.Book.AudibleProductId;
			var title = keeper.Book.TitleWithSubtitle;
			var removing = group.Where(lb => !ReferenceEquals(lb, keeper))
				.Select(lb => $"{lb.Book.AudibleProductId} ({lb.DateAdded:d}, {lb.Account})")
				.ToList();

			builder.Append(asin);
			builder.Append(" | ");
			builder.Append(title);
			builder.AppendLine();
			builder.Append("  Keep ");
			builder.Append($"{keeper.DateAdded:d}, {keeper.Account}");
			builder.Append(previewOnly ? "; would remove " : "; removed ");
			builder.AppendLine(string.Join("; ", removing));
		}

		return builder.ToString().TrimEnd();
	}

	private static void MarkCompleted()
	{
		if (UNSAFE_MigrationHelper.Settings_TryGet(SettingsKey, out _))
			UNSAFE_MigrationHelper.Settings_Update(SettingsKey, "true");
		else
			UNSAFE_MigrationHelper.Settings_Insert(SettingsKey, "true");
	}
}

public readonly record struct DuplicateAsinScanResult(int DuplicateGroupCount, string Report);

public readonly record struct DuplicateAsinCleanupResult(int DuplicateGroupCount, int RemovedBookCount, string Report);
