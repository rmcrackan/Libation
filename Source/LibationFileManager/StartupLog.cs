using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LibationFileManager;

public enum StartupLogLevel
{
	Debug,
	Information,
	Warning,
	Error,
}

public sealed record StartupLogEntry(DateTimeOffset Timestamp, StartupLogLevel Level, string Message, Exception? Exception);

/// <summary>
/// Logging for the startup window that runs before <see cref="Configuration.ConfigureLogging"/>, where
/// <c>Serilog.Log.Logger</c> is still Serilog's silent logger and writes nowhere.
/// <para/>
/// Nothing here names a Serilog type, which is the point. Libation's releases are ReadyToRun, so a
/// Serilog reference resolves lazily when the line holding it runs; if the install folder's
/// <c>Serilog.dll</c> is missing or older than the one this build was compiled against, that line throws.
/// A <c>catch</c> block cannot protect its own logging call, so an install broken badly enough to need
/// the upgrade recovery in <see cref="InstallUpgradeManager"/> was the exact case in which that recovery
/// destroyed the real exception and aborted. See issue #2001.
/// <para/>
/// Entries recorded before <see cref="ReplayTo"/> are buffered and handed over in order once real logging
/// exists, so startup diagnostics reach the log file instead of vanishing. Every method here swallows its
/// own failures: this must never be the reason startup ends.
/// </summary>
public static class StartupLog
{
	/// <summary>
	/// Room for a whole startup and then some. Bounded so a caller in a retry loop cannot grow this
	/// without limit while there is still no sink to drain it.
	/// </summary>
	private const int MaxBufferedEntries = 500;

	private static readonly object Gate = new();
	private static readonly List<StartupLogEntry> Buffered = [];
	private static Action<StartupLogEntry>? Sink;

	public static void Debug(string message) => Record(StartupLogLevel.Debug, message, null);
	public static void Debug(Exception? exception, string message) => Record(StartupLogLevel.Debug, message, exception);
	public static void Information(string message) => Record(StartupLogLevel.Information, message, null);
	public static void Warning(string message) => Record(StartupLogLevel.Warning, message, null);
	public static void Warning(Exception? exception, string message) => Record(StartupLogLevel.Warning, message, exception);
	public static void Error(string message) => Record(StartupLogLevel.Error, message, null);
	public static void Error(Exception? exception, string message) => Record(StartupLogLevel.Error, message, exception);

	/// <summary>
	/// Hands every buffered entry to <paramref name="sink"/> in the order it was recorded, then sends
	/// later entries straight through. Call once, immediately after logging is configured.
	/// </summary>
	public static void ReplayTo(Action<StartupLogEntry> sink)
	{
		ArgumentNullException.ThrowIfNull(sink);

		StartupLogEntry[] pending;
		lock (Gate)
		{
			Sink = sink;
			pending = [.. Buffered];
			Buffered.Clear();
		}

		foreach (var entry in pending)
			Deliver(sink, entry);
	}

	/// <summary>Entries recorded but not yet handed to a sink.</summary>
	public static IReadOnlyList<StartupLogEntry> BufferedEntries
	{
		get
		{
			lock (Gate)
				return [.. Buffered];
		}
	}

	/// <summary>Drops the sink and anything buffered. Test support: this is process-wide state.</summary>
	public static void ResetForTests()
	{
		lock (Gate)
		{
			Sink = null;
			Buffered.Clear();
		}
	}

	private static void Record(StartupLogLevel level, string message, Exception? exception)
	{
		try
		{
			var entry = new StartupLogEntry(DateTimeOffset.Now, level, message, exception);

			Action<StartupLogEntry>? sink;
			lock (Gate)
			{
				sink = Sink;
				if (sink is null)
				{
					if (Buffered.Count < MaxBufferedEntries)
						Buffered.Add(entry);
					return;
				}
			}

			Deliver(sink, entry);
		}
		catch
		{
			// A logger that can end startup is worse than no logger at all.
		}
	}

	/// <summary>
	/// The sink is where Serilog lives, so it gets its own frame that is never inlined into a caller.
	/// A load failure raised inside it is then contained here rather than surfacing in the middle of
	/// whatever startup step asked for the log line.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void Deliver(Action<StartupLogEntry> sink, StartupLogEntry entry)
	{
		try
		{
			sink(entry);
		}
		catch
		{
			// see Record
		}
	}
}
