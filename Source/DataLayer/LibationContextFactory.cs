using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataLayer;

public class LibationContextFactory
{
	public static void ConfigureOptions(NpgsqlDbContextOptionsBuilder options)
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		options.MigrationsAssembly("DataLayer.Postgres");
		options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
	}

	public static LibationContext CreatePostgres(string connectionString)
	{
		var options = new DbContextOptionsBuilder<LibationContext>();

		options.UseNpgsql(connectionString, ConfigureOptions);

		return new LibationContext(options.Options);
	}

	public static LibationContext CreateSqlite(string connectionString)
	{
		var options = new DbContextOptionsBuilder<LibationContext>();

		options
			.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
			.UseSqlite(connectionString, options =>
			{
				options.MigrationsAssembly("DataLayer.Sqlite");
				options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
			});

		return new LibationContext(options.Options);
	}

	/// <summary>
	/// Runs EF migrations. For SQLite, wraps SQLITE_READONLY (8) with a clearer message including <paramref name="sqliteDatabaseFilePath"/>.
	/// Pass <paramref name="sqliteDatabaseFilePath"/> as null when the context is not SQLite.
	/// </summary>
	public static void ApplyMigrations(LibationContext context, string? sqliteDatabaseFilePath)
	{
		try
		{
			context.Database.Migrate();
		}
		// SQLITE_READONLY == 8 (https://www.sqlite.org/rescode.html)
		catch (SqliteException ex) when (ex.SqliteErrorCode == 8 && sqliteDatabaseFilePath is not null)
		{
			throw CreateSqliteReadonlyException(sqliteDatabaseFilePath, ex);
		}
	}

	/// <inheritdoc cref="ApplyMigrations(LibationContext, string?)"/>
	public static async Task ApplyMigrationsAsync(LibationContext context, string? sqliteDatabaseFilePath, CancellationToken cancellationToken = default)
	{
		try
		{
			await context.Database.MigrateAsync(cancellationToken);
		}
		// SQLITE_READONLY == 8 (https://www.sqlite.org/rescode.html)
		catch (SqliteException ex) when (ex.SqliteErrorCode == 8 && sqliteDatabaseFilePath is not null)
		{
			throw CreateSqliteReadonlyException(sqliteDatabaseFilePath, ex);
		}
	}

	private static InvalidOperationException CreateSqliteReadonlyException(string sqliteDatabaseFilePath, SqliteException ex)
	{
		// Match LibationFileManager.Configuration.IsLinux (OperatingSystem.IsLinux); avoid referencing that project from DataLayer.
		var linuxSection = OperatingSystem.IsLinux()
			? "\n\nOn Linux: check ownership and permissions on that folder (for example chmod/chown). Snap installs often store data under ~/snap/libation/<revision>/.local/share/Libation — that entire tree must be writable."
			: "";
		var snapHint = OperatingSystem.IsLinux()
			? ", or use the non-Snap build if Snap confinement is blocking writes"
			: "";

		return new InvalidOperationException(
			$"""
			Libation cannot write its SQLite database (migrations need write access).

			Database path:
			{sqliteDatabaseFilePath}

			This usually means the folder or the database file is not writable by your user (wrong owner or permissions), or the location is on a read-only or restricted filesystem.{linuxSection}

			If the problem continues, try moving the Libation Files location (Settings) to a folder you know is writable{snapHint}.
			""",
			ex);
	}
}
