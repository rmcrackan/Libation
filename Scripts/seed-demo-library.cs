#:package Microsoft.Data.Sqlite@10.0.7

using Microsoft.Data.Sqlite;
using System.Text.Json;

// Seeds a Libation library with one book per Liberate-column state, plus a few titles that
// subtitle removal changes, for manual UI testing.
//
//   dotnet run Scripts/seed-demo-library.cs [--clean] [path-to-Libation-files-folder]
//
// Run Libation at least once first so the database exists, and close it before seeding.
// Pass --clean to delete previously seeded demo books instead of adding them.
//
// Reproducing every icon by hand is tedious, and two of them are not database state at all:
// the yellow lamp means "a partial download is sitting on disk", so this writes placeholder
// .aaxc files as well.

const int NotLiberated = 0, Liberated = 1, Error = 2;
const int Product = 1, Episode = 2, Parent = 4;
const string AsinPrefix = "DEMO";

var clean = args.Contains("--clean", StringComparer.OrdinalIgnoreCase);
var pathArg = args.FirstOrDefault(a => !a.StartsWith("--"));

if (FindLibationFiles(pathArg) is not string libationFiles)
{
	Console.Error.WriteLine("Could not find a Libation database.");
	Console.Error.WriteLine("Run Libation once to create it, then pass its folder, e.g.:");
	Console.Error.WriteLine(@"  dotnet run Scripts/seed-demo-library.cs C:\Users\me\AppData\Local\Libation");
	return 1;
}

var dbPath = Path.Combine(libationFiles, "LibationContext.db");
Console.WriteLine($"Database: {dbPath}");

using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString());
connection.Open();
using var transaction = connection.BeginTransaction();

if (clean)
{
	// Books cascade to UserDefinedItem, LibraryBooks, Supplement, BookContributor and SeriesBook,
	// but the Series and Contributors rows those pointed at are left behind.
	var removed = Execute($"delete from Books where AudibleProductId like '{AsinPrefix}%'");
	Execute($"delete from Series where AudibleSeriesId like '{AsinPrefix}%' and SeriesId not in (select SeriesId from SeriesBook)");
	Execute("delete from Contributors where AudibleContributorId = 'DEMOAUTH' and ContributorId not in (select ContributorId from BookContributor)");
	transaction.Commit();

	foreach (var partial in Directory.Exists(DownloadsInProgress())
		? Directory.EnumerateFiles(DownloadsInProgress(), $"{AsinPrefix}*.aaxc")
		: [])
		File.Delete(partial);

	Console.WriteLine($"Removed {removed} demo book(s) and their placeholder downloads.");
	return 0;
}

var books = BuildDemoBooks();
var contributorId = GetOrCreateId(
	"select ContributorId from Contributors where AudibleContributorId = $id",
	"insert into Contributors (Name, AudibleContributorId) values ('Demo Author', $id)",
	"DEMOAUTH");

var added = 0;
var partials = new List<string>();

foreach (var book in books)
{
	if (Scalar("select count(*) from Books where AudibleProductId = $asin", ("$asin", book.Asin)) > 0)
	{
		Console.WriteLine($"  {book.Asin} already present, skipping");
		continue;
	}

	Execute(
		"""
		insert into Books
		 (AudibleProductId, ContentType, Description, IsAbridged, IsSpatial, LengthInMinutes,
		  Locale, Rating_OverallRating, Rating_PerformanceRating, Rating_StoryRating, Subtitle, Title)
		values ($asin, $contentType, 'Seeded by seed-demo-library.cs', 0, 0, 600, 'us', 0, 0, 0, $subtitle, $title)
		""",
		("$asin", book.Asin), ("$contentType", book.ContentType), ("$title", book.Title), ("$subtitle", book.Subtitle));

	var bookId = Scalar("select BookId from Books where AudibleProductId = $asin", ("$asin", book.Asin));

	// PartialDownload is never persisted - the BookStatus setter rewrites it to NotLiberated,
	// because it is derived from a file on disk instead.
	Execute(
		"""
		insert into UserDefinedItem
		 (BookId, BookStatus, IsFinished, PdfStatus, Rating_OverallRating,
		  Rating_PerformanceRating, Rating_StoryRating, Tags)
		values ($bookId, $bookStatus, 0, $pdfStatus, 0, 0, 0, '')
		""",
		("$bookId", bookId), ("$bookStatus", book.BookStatus), ("$pdfStatus", (object?)book.PdfStatus ?? DBNull.Value));

	Execute(
		"""
		insert into LibraryBooks (BookId, AbsentFromLastScan, Account, DateAdded, IncludedUntil, IsAudiblePlus, IsDeleted)
		values ($bookId, $isAbsent, 'demo@example.com', '2026-08-13 00:00:00', $includedUntil, $isPlus, 0)
		""",
		("$bookId", bookId),
		("$isAbsent", book.IsAbsent ? 1 : 0),
		("$includedUntil", book.IsPlus ? "2027-01-31 00:00:00" : (object)DBNull.Value),
		("$isPlus", book.IsPlus ? 1 : 0));

	Execute(
		"insert into BookContributor (BookId, ContributorId, Role, [Order]) values ($bookId, $contributorId, 1, 0)",
		("$bookId", bookId), ("$contributorId", contributorId));

	// The PDF glyph itself keys off UserDefinedItem.PdfStatus, but Book.HasPdf - which drives the
	// context menu and the "Has PDF" detail - needs an actual supplement.
	if (book.PdfStatus is not null)
		Execute(
			"insert into Supplement (BookId, Url) values ($bookId, 'https://example.com/demo.pdf')",
			("$bookId", bookId));

	if (book.SeriesAsin is not null)
	{
		var seriesId = GetOrCreateId(
			"select SeriesId from Series where AudibleSeriesId = $id",
			"insert into Series (AudibleSeriesId, Name) values ($id, 'Demo Series')",
			book.SeriesAsin);

		Execute(
			"insert into SeriesBook (SeriesId, BookId, [Order]) values ($seriesId, $bookId, $order)",
			("$seriesId", seriesId), ("$bookId", bookId), ("$order", (object?)book.SeriesOrder ?? DBNull.Value));
	}

	if (book.NeedsPartialDownload)
		partials.Add(book.Asin);

	added++;
}

transaction.Commit();
Console.WriteLine($"Added {added} book(s).");

if (partials.Count > 0)
{
	var inProgress = DownloadsInProgress();
	Directory.CreateDirectory(inProgress);
	foreach (var asin in partials)
		File.WriteAllText(Path.Combine(inProgress, $"{asin}.aaxc"), "not a real download");

	Console.WriteLine($"Wrote {partials.Count} placeholder download(s) to {inProgress}");
}

Console.WriteLine();
Console.WriteLine("Start Libation and sort by Title. Expected Liberate column, top to bottom:");
foreach (var book in books)
	Console.WriteLine($"  {book.Title,-46} {book.Expectation}");
Console.WriteLine();
Console.WriteLine("Do not click the stoplights: these books are not real and cannot be downloaded.");
return 0;

static List<DemoBook> BuildDemoBooks()
{
	var books = new List<DemoBook>();
	var n = 0;

	// The whole lamp/PDF/Plus matrix, so every stoplight rendering is on screen at once.
	foreach (var (lamp, bookStatus, needsPartial) in new[]
	{
		("Green", Liberated, false),
		("Yellow", NotLiberated, true),
		("Red", NotLiberated, false),
	})
		foreach (var (pdfLabel, pdfStatus) in new (string, int?)[]
		{
			("no PDF", null),
			("PDF done", Liberated),
			("PDF todo", NotLiberated),
		})
			foreach (var isPlus in new[] { false, true })
			{
				var owner = isPlus ? "PLUS" : "purchased";
				books.Add(new DemoBook(
					Asin: $"{AsinPrefix}{++n:000}",
					Title: $"{n:00} {lamp} | {pdfLabel} | {owner}",
					ContentType: Product,
					BookStatus: bookStatus,
					PdfStatus: pdfStatus,
					IsPlus: isPlus,
					NeedsPartialDownload: needsPartial,
					Expectation: $"{lamp} lamp, {pdfLabel}, {(isPlus ? "orange PLUS badge" : "NO badge")}"));
			}

	// The error icon replaces the stoplight entirely, and ignores both PDF and Plus.
	books.Add(new DemoBook(
		Asin: $"{AsinPrefix}{++n:000}",
		Title: $"{n:00} Error | purchased",
		ContentType: Product,
		BookStatus: Error,
		PdfStatus: null,
		IsPlus: false,
		NeedsPartialDownload: false,
		Expectation: "red error sign, no stoplight"));

	books.Add(new DemoBook(
		Asin: $"{AsinPrefix}{++n:000}",
		Title: $"{n:00} Error | PLUS",
		ContentType: Product,
		BookStatus: Error,
		PdfStatus: null,
		IsPlus: true,
		NeedsPartialDownload: false,
		Expectation: "red error sign, still NO badge"));

	// A podcast: the parent row shows a plus/minus expander rather than a stoplight, and its
	// episodes show ordinary stoplights. Libation identifies a podcast's series by the parent
	// book's own ASIN, and hides any series whose children it cannot match, so the episodes have
	// to point at that same ASIN or the parent row silently disappears.
	var seriesAsin = $"{AsinPrefix}{++n:000}";
	books.Add(new DemoBook(
		Asin: seriesAsin,
		Title: $"{n:00} Demo Series (parent)",
		ContentType: Parent,
		BookStatus: NotLiberated,
		PdfStatus: null,
		IsPlus: false,
		NeedsPartialDownload: false,
		Expectation: "square plus/minus expander, never a badge",
		SeriesAsin: seriesAsin));

	foreach (var (episode, bookStatus, isPlus) in new[]
	{
		(1, Liberated, false),
		(2, NotLiberated, true),
	})
		books.Add(new DemoBook(
			Asin: $"{AsinPrefix}{++n:000}",
			Title: $"{n:00} Demo Series - episode {episode}",
			ContentType: Episode,
			BookStatus: bookStatus,
			PdfStatus: null,
			IsPlus: isPlus,
			NeedsPartialDownload: false,
			Expectation: $"{(bookStatus == Liberated ? "green" : "red")} lamp, {(isPlus ? "orange PLUS badge" : "NO badge")}",
			SeriesAsin: seriesAsin,
			SeriesOrder: episode.ToString()));

	// Books missing from the last scan. These keep their ordinary icon - there is no "unavailable"
	// glyph - and the grid dims and disables the row around it instead, so they are the rows to
	// check when an icon change might read badly under that treatment.
	//
	// Being absent is not enough on its own: a book which is fully liberated with nothing left to
	// fetch stays available, because there is nothing the row would do if you clicked it.
	foreach (var (label, bookStatus, pdfStatus, isPlus, expectation) in new (string, int, int?, bool, string)[]
	{
		("not downloaded | purchased", NotLiberated, null, false, "red lamp, dimmed and unclickable"),
		("not downloaded | PLUS", NotLiberated, null, true, "red lamp with badge, dimmed and unclickable"),
		("downloaded | purchased", Liberated, null, false, "green lamp, NOT dimmed - nothing left to fetch"),
		("downloaded, PDF todo | purchased", Liberated, NotLiberated, false, "green lamp with PDF arrow, dimmed again - the PDF is outstanding"),
	})
		books.Add(new DemoBook(
			Asin: $"{AsinPrefix}{++n:000}",
			Title: $"{n:00} Absent | {label}",
			ContentType: Product,
			BookStatus: bookStatus,
			PdfStatus: pdfStatus,
			IsPlus: isPlus,
			NeedsPartialDownload: false,
			Expectation: expectation,
			IsAbsent: true));

	// Titles that subtitle removal changes. <title short> stops at the first colon, so it cuts a colon in
	// Audible's own title just as readily as it drops Audible's subtitle field, and the grid's Title column
	// shows both the same way. The HasSubtitle and TitleHasColon filters are the only way to tell them apart.
	foreach (var (title, subtitle, match) in new[]
	{
		("A Book Series Omnibus", "Volume One", "HasSubtitle"),
		("A Book Series Omnibus", "Volume Two", "HasSubtitle"),
		("Star Trek: The Next Generation", "", "TitleHasColon"),
		("Dune: Book One", "The Graphic Novel", "both HasSubtitle and TitleHasColon"),
	})
		books.Add(new DemoBook(
			Asin: $"{AsinPrefix}{++n:000}",
			Title: title,
			ContentType: Product,
			BookStatus: NotLiberated,
			PdfStatus: null,
			IsPlus: false,
			NeedsPartialDownload: false,
			Expectation: $"red lamp; filter match: {match}",
			Subtitle: subtitle));

	return books;
}

/// <summary>Locate the Libation files folder the same way Libation itself does.</summary>
static string? FindLibationFiles(string? explicitPath)
{
	var candidates = new List<string>();

	if (explicitPath is not null)
		candidates.Add(explicitPath);

	foreach (var folder in KnownLibationFolders())
	{
		candidates.Add(folder);

		// appsettings.json may redirect the files folder somewhere else entirely.
		var appSettings = Path.Combine(folder, "appsettings.json");
		if (!File.Exists(appSettings))
			continue;

		try
		{
			if (JsonDocument.Parse(File.ReadAllText(appSettings)).RootElement
				.TryGetProperty("LibationFiles", out var redirect) && redirect.GetString() is string path)
				candidates.Add(path);
		}
		catch (JsonException) { }
	}

	return candidates.FirstOrDefault(c => File.Exists(Path.Combine(c, "LibationContext.db")));
}

static IEnumerable<string> KnownLibationFolders()
{
	yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Libation");
	yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Libation");
	yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Libation");
	yield return Path.Combine(Path.GetTempPath(), $"Libation-{Environment.UserName}");
	yield return AppContext.BaseDirectory;
}

/// <summary>Where Libation looks for resumable downloads, honouring the InProgress setting.</summary>
string DownloadsInProgress()
{
	var inProgress = Path.Combine(Path.GetTempPath(), $"Libation-{Environment.UserName}");

	var settings = Path.Combine(libationFiles, "Settings.json");
	if (File.Exists(settings))
	{
		try
		{
			if (JsonDocument.Parse(File.ReadAllText(settings)).RootElement
				.TryGetProperty("InProgress", out var configured)
				&& configured.GetString() is string path && !string.IsNullOrWhiteSpace(path))
				inProgress = path;
		}
		catch (JsonException) { }
	}

	return Path.Combine(inProgress, "DownloadsInProgress");
}

/// <summary>Look up a row's id by its Audible id, inserting it first if it isn't there yet.</summary>
/// <remarks>
/// Neither Contributors.AudibleContributorId nor Series.AudibleSeriesId is unique, so an upsert
/// would happily insert a duplicate every time this script runs.
/// </remarks>
int GetOrCreateId(string selectSql, string insertSql, string audibleId)
{
	using (var select = Command(selectSql, [("$id", audibleId)]))
		if (select.ExecuteScalar() is object existing and not DBNull)
			return Convert.ToInt32(existing);

	Execute(insertSql, ("$id", audibleId));
	return Scalar("select last_insert_rowid()");
}

int Scalar(string sql, params (string Name, object Value)[] parameters)
{
	using var command = Command(sql, parameters);
	return Convert.ToInt32(command.ExecuteScalar());
}

int Execute(string sql, params (string Name, object Value)[] parameters)
{
	using var command = Command(sql, parameters);
	return command.ExecuteNonQuery();
}

SqliteCommand Command(string sql, (string Name, object Value)[] parameters)
{
	var command = connection.CreateCommand();
	command.Transaction = transaction;
	command.CommandText = sql;
	foreach (var (name, value) in parameters)
		command.Parameters.AddWithValue(name, value);
	return command;
}

record DemoBook(
	string Asin,
	string Title,
	int ContentType,
	int BookStatus,
	int? PdfStatus,
	bool IsPlus,
	bool NeedsPartialDownload,
	string Expectation,
	string? SeriesAsin = null,
	string? SeriesOrder = null,
	bool IsAbsent = false,
	string Subtitle = "");
