using FileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibationFileManager;

/// <summary>
/// Writes a crash record without Serilog, for the startup window where Serilog is not configured yet or
/// cannot be loaded at all.
/// <para/>
/// Shared by both UIs. Chardonnay had a private copy of this and Classic had nothing, so a Classic user
/// whose startup failed before logging was told, correctly, that the error could not be written anywhere.
/// See issue #2001.
/// </summary>
public static class PreLoggingCrashLog
{
	public const string CrashFileName = "LibationCrash.log";

	/// <summary>
	/// Appends a crash record to the newest Libation log file, falling back to <see cref="CrashFileName"/>
	/// in the Libation files folder and then in the user profile.
	/// </summary>
	/// <param name="exception">The failure to record.</param>
	/// <param name="extraFields">
	/// Anything the caller knows that this assembly cannot see, such as the release identifier.
	/// </param>
	/// <returns>
	/// The file written, to be named in the crash dialog, or null when nothing could be written. Callers
	/// used to tell users to attach <c>LibationCrash.log</c> unconditionally, which is the wrong file
	/// whenever a <c>Log*.log</c> already exists, and no file at all when this returns null.
	/// </returns>
	public static string? TryWrite(Exception? exception, IEnumerable<(string Name, string Value)>? extraFields = null)
	{
		string record;
		try
		{
			record = BuildRecord(exception, extraFields);
		}
		catch (Exception ex)
		{
			// Losing the whole record because one field could not be read is what happened before.
			record = $"{DateTime.Now} - Libation Crash{Environment.NewLine} (crash record could not be built: {ex}){Environment.NewLine} === EXCEPTION ==={Environment.NewLine} {exception}";
		}

		foreach (var candidate in ResolveCandidateFiles())
		{
			if (TryAppend(candidate, record))
				return candidate;
		}

		return null;
	}

	/// <summary>
	/// Every value is read through <see cref="Describe"/>, so a property that throws contributes its error
	/// text instead of taking the record down with it. <c>Books</c> genuinely does throw this early, and
	/// <c>InteropFactory.InteropFunctionsType</c> ran a static constructor that itself needed Serilog.
	/// </summary>
	private static string BuildRecord(Exception? exception, IEnumerable<(string Name, string Value)>? extraFields)
	{
		var fields = new List<(string Name, string Value)>
		{
			("OS", Describe(() => Configuration.OS.ToString())),
			("Version", Describe(() => Configuration.LibationVersion?.ToString() ?? "[null]")),
			("InteropFunctionsType", Describe(() => InteropFactory.InteropFunctionsType?.ToString() ?? "[null]")),
			("LibationFiles", Describe(() => Configuration.Instance.LibationFiles.Location.Path)),
			("Books Folder", Describe(() => Configuration.Instance.Books ?? "[null]")),
		};

		if (extraFields is not null)
			fields.AddRange(extraFields);

		var width = fields.Max(f => f.Name.Length) + 2;
		var lines = fields.Select(f => $" {f.Name.PadRight(width)}{f.Value}");

		return $"""
			{DateTime.Now} - Libation Crash
			{string.Join(Environment.NewLine, lines)}
			 === EXCEPTION ===
			 {exception}
			""";
	}

	private static string Describe(Func<string> read)
	{
		try
		{
			return read();
		}
		catch (Exception ex)
		{
			return ex.ToString();
		}
	}

	private static IEnumerable<LongPath> ResolveCandidateFiles()
	{
		var logDirectory = Describe(() => Configuration.Instance.LibationFiles.Location.Path);

		if (Directory.Exists(logDirectory))
		{
			var newestLog = NewestLogFile(logDirectory);
			if (newestLog is not null)
				yield return newestLog;

			yield return Path.Combine(logDirectory, CrashFileName);
		}

		var userProfile = Describe(() => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
		if (Directory.Exists(userProfile))
			yield return Path.Combine(userProfile, CrashFileName);
	}

	private static string? NewestLogFile(string logDirectory)
	{
		try
		{
			return Directory.GetFiles(logDirectory, "Log*.log")
				.Select(f => new FileInfo(f))
				.OrderByDescending(f => f.CreationTimeUtc)
				.FirstOrDefault()
				?.FullName;
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static bool TryAppend(LongPath path, string record)
	{
		try
		{
			// Without this the record ran straight onto the end of the last log line.
			var separator = NeedsLeadingNewLine(path) ? Environment.NewLine : string.Empty;

			using var writer = new StreamWriter(path, append: true);
			writer.WriteLine(separator + record);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static bool NeedsLeadingNewLine(LongPath path)
	{
		try
		{
			var info = new FileInfo(path);
			if (!info.Exists || info.Length == 0)
				return false;

			using var stream = File.OpenRead(path);
			stream.Seek(-1, SeekOrigin.End);
			var last = stream.ReadByte();
			return last is not ('\n' or '\r');
		}
		catch (Exception)
		{
			return false;
		}
	}
}
