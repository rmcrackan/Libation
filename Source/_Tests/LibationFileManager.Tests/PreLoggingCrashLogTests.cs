using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationFileManager.Tests;

/// <summary>
/// Issue #2001: the crash dialog told reporters to attach LibationCrash.log, which is not where the record
/// goes when a Log*.log already exists, so they went looking for a file that was not there. On one run the
/// record was lost entirely, because a field getter threw and the caller's bare catch swallowed everything.
/// </summary>
[TestClass]
[DoNotParallelize]
public class PreLoggingCrashLogTests
{
	private string tempLibationFiles = null!;

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-crash-log-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempLibationFiles);

		// A fresh Configuration resolves LibationFiles from this variable, so the crash record lands here.
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempLibationFiles);
		Configuration.CreateMockInstance();
	}

	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);

		try { Directory.Delete(tempLibationFiles, recursive: true); } catch { }
	}

	[TestMethod]
	public void With_no_log_file_yet_the_record_goes_to_LibationCrash_log()
	{
		var written = PreLoggingCrashLog.TryWrite(new InvalidOperationException("something went wrong"));

		Assert.AreEqual(Path.Combine(tempLibationFiles, PreLoggingCrashLog.CrashFileName), written);
		StringAssert.Contains(File.ReadAllText(written!), "something went wrong");
	}

	[TestMethod]
	public void An_existing_log_file_gets_the_record_appended_and_is_the_path_reported()
	{
		var existingLog = Path.Combine(tempLibationFiles, "Log202608.log");
		File.WriteAllText(existingLog, "an earlier line with no trailing newline");

		var written = PreLoggingCrashLog.TryWrite(new InvalidOperationException("something went wrong"));

		Assert.AreEqual(existingLog, written);

		var contents = File.ReadAllText(existingLog);
		StringAssert.Contains(contents, "an earlier line with no trailing newline");
		StringAssert.Contains(contents, "Libation Crash");
		Assert.IsFalse(
			contents.Contains("newline" + DateTime.Now.Year),
			"the record ran onto the end of the previous log line");
	}

	[TestMethod]
	public void The_newest_log_file_is_chosen()
	{
		var older = Path.Combine(tempLibationFiles, "Log202607.log");
		var newer = Path.Combine(tempLibationFiles, "Log202608.log");
		File.WriteAllText(older, "older\n");
		File.SetCreationTimeUtc(older, DateTime.UtcNow.AddDays(-30));
		File.WriteAllText(newer, "newer\n");
		File.SetCreationTimeUtc(newer, DateTime.UtcNow);

		Assert.AreEqual(newer, PreLoggingCrashLog.TryWrite(new Exception("boom")));
	}

	[TestMethod]
	public void Caller_supplied_fields_are_recorded()
	{
		var written = PreLoggingCrashLog.TryWrite(
			new Exception("boom"),
			[("ReleaseIdentifier", "WindowsAvalonia")]);

		var contents = File.ReadAllText(written!);
		StringAssert.Contains(contents, "ReleaseIdentifier");
		StringAssert.Contains(contents, "WindowsAvalonia");
	}

	// The record has to survive a field that cannot be read. Books throws this early in startup, and
	// InteropFactory.InteropFunctionsType ran a static constructor that itself needed Serilog.
	[TestMethod]
	public void A_field_that_cannot_be_read_does_not_cost_us_the_record()
	{
		var written = PreLoggingCrashLog.TryWrite(new InvalidOperationException("the failure that matters"));

		Assert.IsNotNull(written);
		var contents = File.ReadAllText(written!);
		StringAssert.Contains(contents, "the failure that matters");
		StringAssert.Contains(contents, "LibationFiles");
	}

	// The path goes straight into the crash dialog for someone to read and paste into a file browser, so it
	// must not carry the Windows extended-length prefix. Returning the LongPath as-is put "\\?\" in front
	// of it on Windows only, which Linux and macOS runs cannot notice.
	[TestMethod]
	public void The_reported_path_is_the_one_a_person_can_use()
	{
		var written = PreLoggingCrashLog.TryWrite(new Exception("boom"));

		Assert.IsNotNull(written);
		Assert.IsFalse(written!.StartsWith(@"\\?\"), $"reported path should not carry the extended-length prefix: {written}");
		Assert.IsTrue(File.Exists(written), "the reported path should be usable as-is");
	}

	[TestMethod]
	public void A_second_crash_appends_rather_than_replacing_the_first()
	{
		PreLoggingCrashLog.TryWrite(new Exception("first crash"));
		var written = PreLoggingCrashLog.TryWrite(new Exception("second crash"));

		var contents = File.ReadAllText(written!);
		StringAssert.Contains(contents, "first crash");
		StringAssert.Contains(contents, "second crash");
	}
}
