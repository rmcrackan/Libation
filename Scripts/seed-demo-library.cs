#:package Microsoft.Data.Sqlite@10.0.7

using Microsoft.Data.Sqlite;

// Seeds a Libation library with one book per Liberate-column icon state, for manual UI testing.
// Usage: dotnet run seed-demo-library.cs [path-to-LibationContext.db]
// Run Libation at least once first so the database and its schema exist, and close it before seeding.

const int NotLiberated = 0, Liberated = 1, Error = 2;
const int Product = 1, Episode = 2, Parent = 4;

var dbPath = args.Length > 0
	? args[0]
	: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Libation", "LibationContext.db");

if (!File.Exists(dbPath))
{
	Console.Error.WriteLine($"No database at {dbPath}");
	Console.Error.WriteLine("Run Libation once to create it, then pass its path as an argument.");
	return 1;
}

Console.WriteLine($"Seeding {dbPath}");

using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString());
connection.Open();
using var transaction = connection.BeginTransaction();

var contributorId = Scalar(
	"""
	insert into Contributors (Name, AudibleContributorId) values ('Demo Author', 'DEMOAUTH')
	on conflict do nothing;
	select ContributorId from Contributors where AudibleContributorId = 'DEMOAUTH';
	""");

// asin, title, contentType, bookStatus, pdfStatus, seriesAsin, seriesOrder
(string, string, int, int, int?, string?, string?)[] books =
[
	("GREEN0", "Green - no PDF",       Product, Liberated,   null,          null, null),
	("GREEN1", "Green - PDF yes",      Product, Liberated,   Liberated,     null, null),
	("GREEN2", "Green - PDF no",       Product, Liberated,   NotLiberated,  null, null),
	("YELLW0", "Yellow - no PDF",      Product, NotLiberated, null,         null, null),
	("YELLW1", "Yellow - PDF yes",     Product, NotLiberated, Liberated,    null, null),
	("YELLW2", "Yellow - PDF no",      Product, NotLiberated, NotLiberated, null, null),
	("RED000", "Red - no PDF",         Product, NotLiberated, null,         null, null),
	("RED001", "Red - PDF yes",        Product, NotLiberated, Liberated,    null, null),
	("RED002", "Red - PDF no",         Product, NotLiberated, NotLiberated, null, null),
	("ERROR0", "Error",                Product, Error,        null,         null, null),
	("SERIES1", "Demo Series (parent)", Parent, NotLiberated, null,         "SERIES1", null),
	("EPISOD1", "Episode 1",           Episode, Liberated,   null,          "SERIES1", "1"),
	("EPISOD2", "Episode 2",           Episode, NotLiberated, Liberated,    "SERIES1", "2"),
];

var added = 0;
foreach (var (asin, title, contentType, bookStatus, pdfStatus, seriesAsin, seriesOrder) in books)
{
	if (Scalar("select count(*) from Books where AudibleProductId = $asin", ("$asin", asin)) > 0)
	{
		Console.WriteLine($"  {asin} already present, skipping");
		continue;
	}

	Execute(
		"""
		insert into Books
		 (AudibleProductId, ContentType, Description, IsAbridged, IsSpatial, LengthInMinutes,
		  Locale, Rating_OverallRating, Rating_PerformanceRating, Rating_StoryRating, Subtitle, Title)
		values ($asin, $contentType, '', 0, 0, 600, 'us', 0, 0, 0, '', $title)
		""",
		("$asin", asin), ("$contentType", contentType), ("$title", title));

	var bookId = Scalar("select BookId from Books where AudibleProductId = $asin", ("$asin", asin));

	Execute(
		"""
		insert into UserDefinedItem
		 (BookId, BookStatus, IsFinished, PdfStatus, Rating_OverallRating,
		  Rating_PerformanceRating, Rating_StoryRating, Tags)
		values ($bookId, $bookStatus, 0, $pdfStatus, 0, 0, 0, '')
		""",
		("$bookId", bookId), ("$bookStatus", bookStatus), ("$pdfStatus", (object?)pdfStatus ?? DBNull.Value));

	Execute(
		"""
		insert into LibraryBooks (BookId, AbsentFromLastScan, Account, DateAdded, IsAudiblePlus, IsDeleted)
		values ($bookId, 0, 'demo@example.com', '2026-08-13 00:00:00', 0, 0)
		""",
		("$bookId", bookId));

	Execute(
		"insert into BookContributor (BookId, ContributorId, Role, [Order]) values ($bookId, $contributorId, 1, 0)",
		("$bookId", bookId), ("$contributorId", contributorId));

	if (seriesAsin is not null)
	{
		var seriesId = Scalar(
			"""
			insert into Series (AudibleSeriesId, Name) values ($seriesAsin, 'Demo Series')
			on conflict do nothing;
			select SeriesId from Series where AudibleSeriesId = $seriesAsin;
			""",
			("$seriesAsin", seriesAsin));

		Execute(
			"insert into SeriesBook (SeriesId, BookId, [Order]) values ($seriesId, $bookId, $order)",
			("$seriesId", seriesId), ("$bookId", bookId), ("$order", (object?)seriesOrder ?? DBNull.Value));
	}

	added++;
}

transaction.Commit();
Console.WriteLine($"Added {added} book(s).");

// The yellow lamp means "a partial download exists on disk", which is not stored in the database.
var inProgress = Path.Combine(Path.GetTempPath(), $"Libation-{Environment.UserName}", "DownloadsInProgress");
Directory.CreateDirectory(inProgress);
foreach (var asin in new[] { "YELLW0", "YELLW1", "YELLW2" })
{
	var partial = Path.Combine(inProgress, $"{asin}_partial.aaxc");
	if (!File.Exists(partial))
		File.WriteAllText(partial, "partial");
}
Console.WriteLine($"Wrote partial-download placeholders to {inProgress}");
Console.WriteLine("Start Libation to see the demo library.");
return 0;

int Scalar(string sql, params (string Name, object Value)[] parameters)
{
	using var command = Command(sql, parameters);
	return Convert.ToInt32(command.ExecuteScalar());
}

void Execute(string sql, params (string Name, object Value)[] parameters)
{
	using var command = Command(sql, parameters);
	command.ExecuteNonQuery();
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
