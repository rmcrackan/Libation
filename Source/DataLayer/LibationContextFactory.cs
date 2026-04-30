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
			? "\n\nOn Linux: check ownership and permissions on that folder (chmod/chown). Include LibationContext.db-wal and LibationContext.db-shm if they exist. Snap data is often under ~/snap/libation/<revision>/.local/share/Libation — that entire tree must be writable.\n\nIf Libation will not start, set environment variable LIBATION_FILES_DIR to an existing writable directory you own, then launch again. After Libation starts, you can also change the Libation Files folder in Settings.\n\nIf this persists on Snap and permissions look correct, try the non-Snap build to rule out confinement blocking writes."
			: "\n\nAfter Libation starts, you can change the Libation Files folder in Settings. If Libation will not start, set environment variable LIBATION_FILES_DIR to an existing writable directory you own, then launch again.";

		return new InvalidOperationException(
			$"""
			Libation cannot write its SQLite database (migrations need write access).

			Database path:
			{sqliteDatabaseFilePath}

			This path is the library database (metadata and local settings), not your audiobook storage folder.

			This usually means the folder or database file is not writable by your user (wrong owner or permissions), or the location is on a read-only or restricted filesystem.{linuxSection}
			""",
			ex);
	}
}
