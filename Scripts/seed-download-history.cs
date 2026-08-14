#:package Microsoft.Data.Sqlite@10.0.7

using Microsoft.Data.Sqlite;

// Seeds Libation's DownloadHistory table with fake completed downloads, for manual testing of the
// daily download limit without downloading anything.
//
//   dotnet run Scripts/seed-download-history.cs [--count 50] [--age-seconds 7200] [--owned]
//                                               [--mb 300] [--clean] [path-to-Libation-files-folder]
//
// The limit uses a rolling 24 hour window, so --age-seconds controls how soon the rows fall out of it:
//
//   --age-seconds 7200    two hours ago: the limit stays reached for the next 22 hours
//   --age-seconds 86325   just under 24 hours ago: a paused queue resumes itself about a minute from now
//
// Pass --clean to delete all rows. Libation can be running; each row is just a row.

const string FakeAsinPrefix = "FAKE";

var clean = args.Contains("--clean", StringComparer.OrdinalIgnoreCase);
var owned = args.Contains("--owned", StringComparer.OrdinalIgnoreCase);
var count = IntArg("--count") ?? 50;
var ageSeconds = IntArg("--age-seconds") ?? 7200;
var megabytes = IntArg("--mb") ?? 300;
var pathArg = args.FirstOrDefault(a => !a.StartsWith("--") && !int.TryParse(a, out _));

if (FindLibationFiles(pathArg) is not string libationFiles)
{
	Console.Error.WriteLine("Could not find a Libation database.");
	Console.Error.WriteLine("Run Libation once to create it, then pass its folder, e.g.:");
	Console.Error.WriteLine(@"  dotnet run Scripts/seed-download-history.cs C:\Users\me\AppData\Local\Libation");
	return 1;
}

var dbPath = Path.Combine(libationFiles, "LibationContext.db");
Console.WriteLine($"Database: {dbPath}");

using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString());
connection.Open();

if (!TableExists())
{
	Console.Error.WriteLine("This database has no DownloadHistory table yet. Start Libation once so it applies migrations, then retry.");
	return 1;
}

if (clean)
{
	Console.WriteLine($"Removed {Execute("delete from DownloadHistory")} download history row(s).");
	return 0;
}

// Stored as UTC ticks so the rolling window is exact on every provider and across DST changes.
var completedAt = DateTimeOffset.Now.AddSeconds(-ageSeconds);
var bytes = megabytes * 1024L * 1024;

using var transaction = connection.BeginTransaction();
for (var i = 0; i < count; i++)
{
	Execute(
		"""
		insert into DownloadHistory (CompletedAtUtcTicks, AudibleProductId, IsAudiblePlus, Bytes)
		values ($ticks, $asin, $isPlus, $bytes)
		""",
		("$ticks", completedAt.AddSeconds(i).UtcTicks),
		("$asin", $"{FakeAsinPrefix}{i:0000}"),
		("$isPlus", owned ? 0 : 1),
		("$bytes", bytes));
}
transaction.Commit();

Console.WriteLine(
	$"Inserted {count} {(owned ? "purchased" : "Audible Plus")} download(s) of {megabytes} MB each, " +
	$"completed {TimeSpan.FromSeconds(ageSeconds):g} ago.");
Console.WriteLine($"Total rows: {Scalar("select count(*) from DownloadHistory")}");
Console.WriteLine($"The oldest leaves the 24 hour window at {completedAt.AddHours(24):yyyy-MM-dd HH:mm:ss}");
return 0;

int? IntArg(string name)
{
	var index = Array.FindIndex(args, a => a.Equals(name, StringComparison.OrdinalIgnoreCase));
	return index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out var value) ? value : null;
}

bool TableExists()
	=> Scalar("select count(*) from sqlite_master where type = 'table' and name = 'DownloadHistory'") > 0;

int Execute(string sql, params (string Name, object Value)[] parameters)
{
	using var command = connection.CreateCommand();
	command.CommandText = sql;
	foreach (var (name, value) in parameters)
		command.Parameters.AddWithValue(name, value);
	return command.ExecuteNonQuery();
}

long Scalar(string sql)
{
	using var command = connection.CreateCommand();
	command.CommandText = sql;
	return Convert.ToInt64(command.ExecuteScalar());
}

static string? FindLibationFiles(string? explicitPath)
{
	if (explicitPath is not null)
		return File.Exists(Path.Combine(explicitPath, "LibationContext.db")) ? explicitPath : null;

	string[] candidates =
	[
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Libation"),
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Libation"),
	];

	return candidates.FirstOrDefault(c => File.Exists(Path.Combine(c, "LibationContext.db")));
}
