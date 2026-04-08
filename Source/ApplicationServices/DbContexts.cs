using DataLayer;
using LibationFileManager;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace ApplicationServices;

public static class DbContexts
{
	private static bool _sqliteDbValidated;

	/// <summary>Use for fully functional context, incl. SaveChanges(). For query-only, use the other method</summary>
	public static LibationContext GetContext()
	{
		var context = !string.IsNullOrEmpty(Configuration.Instance.PostgresqlConnectionString)
			? LibationContextFactory.CreatePostgres(Configuration.Instance.PostgresqlConnectionString)
			: LibationContextFactory.CreateSqlite(SqliteStorage.ConnectionString);
		try
		{
			context.Database.Migrate();
		}
		// SQLITE_READONLY == 8 (https://www.sqlite.org/rescode.html)
		catch (SqliteException ex) when (ex.SqliteErrorCode == 8)
		{
			var dbPath = SqliteStorage.DatabasePath;
			throw new InvalidOperationException(
				$"""
				Libation cannot write its SQLite database (migrations need write access).

				Database path:
				{dbPath}

				This usually means the folder or the database file is not writable by your user (wrong owner or permissions), or the location is on a read-only or restricted filesystem.

				On Linux: check ownership and permissions on that folder (for example chmod/chown). Snap installs often store data under ~/snap/libation/<revision>/.local/share/Libation — that entire tree must be writable.

				If the problem continues, try moving the Libation Files location (Settings) to a folder you know is writable, or use the non-Snap build if Snap confinement is blocking writes.
				""",
				ex);
		}

		// Validate SQLite DB file was created and is accessible (once per process; OS may delay availability)
		if (!_sqliteDbValidated && string.IsNullOrEmpty(Configuration.Instance.PostgresqlConnectionString))
		{
			EssentialFileValidator.ValidateCreatedAndReport(SqliteStorage.DatabasePath);
			_sqliteDbValidated = true;
		}

		return context;
	}

	/// <summary>Use for full library querying. No lazy loading</summary>
	public static List<LibraryBook> GetLibrary_Flat_NoTracking(bool includeParents = false)
	{
		using var context = GetContext();
		return context.GetLibrary_Flat_NoTracking(includeParents);
	}

	public static List<LibraryBook> GetUnliberated_Flat_NoTracking()
	{
		using var context = GetContext();
		return context.GetUnLiberated_Flat_NoTracking();
	}

	public static List<LibraryBook> GetDeletedLibraryBooks()
	{
		using var context = GetContext();
		return context.GetDeletedLibraryBooks();
	}

	public static LibraryBook? GetLibraryBook_Flat_NoTracking(string productId, bool caseSensative = true)
	{
		using var context = GetContext();
		return context.GetLibraryBook_Flat_NoTracking(productId, caseSensative);
	}
}
