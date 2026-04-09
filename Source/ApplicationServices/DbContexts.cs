using DataLayer;
using LibationFileManager;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ApplicationServices;

public static class DbContexts
{
	private static bool _sqliteDbValidated;

	private static readonly object _initialDatabaseStatisticsCaptureLock = new();

	/// <summary>
	/// True after initial DB statistics were read and either written to Serilog or stored for <see cref="TryEmitPendingInitialDatabaseStatistics"/>.
	/// False if capture has not run yet or the last attempt threw (a later <see cref="GetContext"/> may retry).
	/// </summary>
	private static bool _initialDatabaseStatisticsCaptured;

	/// <summary>Shape of the initial DB statistics log event; edit here only when changing what is logged.</summary>
	private sealed class InitialDatabaseStatistics
	{
		public required int LibraryBooksNotInTrash { get; init; }
		public required int LibraryBooksInTrash { get; init; }
		public required int BookRecords { get; init; }
	}

	private static InitialDatabaseStatistics? _pendingInitialDbStats;

	/// <summary>Use for fully functional context, incl. SaveChanges(). For query-only, use the other method</summary>
	public static LibationContext GetContext()
	{
		var context = !string.IsNullOrEmpty(Configuration.Instance.PostgresqlConnectionString)
			? LibationContextFactory.CreatePostgres(Configuration.Instance.PostgresqlConnectionString)
			: LibationContextFactory.CreateSqlite(SqliteStorage.ConnectionString);
		LibationContextFactory.ApplyMigrations(
			context,
			string.IsNullOrEmpty(Configuration.Instance.PostgresqlConnectionString) ? SqliteStorage.DatabasePath : null);

		// Validate SQLite DB file was created and is accessible (once per process; OS may delay availability)
		if (!_sqliteDbValidated && string.IsNullOrEmpty(Configuration.Instance.PostgresqlConnectionString))
		{
			EssentialFileValidator.ValidateCreatedAndReport(SqliteStorage.DatabasePath);
			_sqliteDbValidated = true;
		}

		TryCaptureInitialDatabaseStatistics(context);

		return context;
	}

	private static void TryCaptureInitialDatabaseStatistics(LibationContext context)
	{
		lock (_initialDatabaseStatisticsCaptureLock)
		{
			if (_initialDatabaseStatisticsCaptured)
				return;

			try
			{
				var (notInTrash, inTrash) = context.GetLibraryBookCountsByTrashFlag();
				var bookRecords = context.GetBookCount();

				var stats = new InitialDatabaseStatistics
				{
					LibraryBooksNotInTrash = notInTrash,
					LibraryBooksInTrash = inTrash,
					BookRecords = bookRecords,
				};

				if (Configuration.Instance.SerilogInitialized)
					LogInitialDatabaseStatistics(stats);
				else
					_pendingInitialDbStats = stats;

				_initialDatabaseStatisticsCaptured = true;
			}
			catch (Exception ex)
			{
				if (Configuration.Instance.SerilogInitialized)
					Log.Warning(ex, "Could not capture initial database statistics");
			}
		}
	}

	/// <summary>
	/// Writes initial DB statistics that were captured before Serilog was configured (e.g. WinForms early library load).
	/// Call once after <see cref="Configuration.ConfigureLogging"/>.
	/// </summary>
	public static void TryEmitPendingInitialDatabaseStatistics()
	{
		var pending = Interlocked.Exchange(ref _pendingInitialDbStats, null);
		if (pending is not null)
			LogInitialDatabaseStatistics(pending);
	}

	private static void LogInitialDatabaseStatistics(InitialDatabaseStatistics stats) =>
		Log.Logger.Information("Initial database statistics. {@DbStats}", stats);

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
