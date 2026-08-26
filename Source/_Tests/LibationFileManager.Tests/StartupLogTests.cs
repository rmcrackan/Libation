using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibationFileManager.Tests;

/// <summary>
/// Issue #2001: startup ran long before Serilog was configured, so its log calls wrote nowhere, and on a
/// broken install they threw and ended startup instead.
/// </summary>
[TestClass]
[DoNotParallelize]
public class StartupLogTests
{
	[TestInitialize]
	public void Initialize() => StartupLog.ResetForTests();

	[TestCleanup]
	public void Cleanup() => StartupLog.ResetForTests();

	[TestMethod]
	public void Entries_recorded_before_a_sink_exists_are_kept_in_order()
	{
		StartupLog.Information("first");
		StartupLog.Warning("second");
		StartupLog.Error(new InvalidOperationException("boom"), "third");

		var buffered = StartupLog.BufferedEntries;

		Assert.AreEqual(3, buffered.Count);
		CollectionAssert.AreEqual(new[] { "first", "second", "third" }, buffered.Select(e => e.Message).ToArray());
		CollectionAssert.AreEqual(
			new[] { StartupLogLevel.Information, StartupLogLevel.Warning, StartupLogLevel.Error },
			buffered.Select(e => e.Level).ToArray());
		Assert.AreEqual("boom", buffered[2].Exception?.Message);
	}

	// The point of buffering: these messages used to be dropped even on a healthy install, because
	// Serilog.Log.Logger is still Serilog's silent logger until logging is configured.
	[TestMethod]
	public void Everything_buffered_reaches_the_sink_once_logging_exists()
	{
		StartupLog.Information("before");

		var delivered = new List<StartupLogEntry>();
		StartupLog.ReplayTo(delivered.Add);

		StartupLog.Information("after");

		CollectionAssert.AreEqual(new[] { "before", "after" }, delivered.Select(e => e.Message).ToArray());
		Assert.AreEqual(0, StartupLog.BufferedEntries.Count, "a replayed entry should not also stay buffered");
	}

	[TestMethod]
	public void A_sink_that_throws_is_contained()
	{
		StartupLog.ReplayTo(_ => throw new FileNotFoundException(
			"Could not load file or assembly 'Serilog, Version=4.3.0.0, Culture=neutral, PublicKeyToken=24c2f752a8e58a10'."));

		// The caller is mid-recovery on a broken install. Logging must not be what ends it.
		StartupLog.Error("this must not throw");
		StartupLog.Warning(new Exception("nor this"), "still fine");
	}

	[TestMethod]
	public void A_sink_that_throws_while_draining_the_buffer_is_contained_too()
	{
		StartupLog.Information("buffered before the bad sink arrived");

		StartupLog.ReplayTo(_ => throw new InvalidOperationException("sink is broken"));
	}

	[TestMethod]
	public void The_buffer_is_bounded_so_it_cannot_grow_without_a_sink()
	{
		for (var i = 0; i < 5_000; i++)
			StartupLog.Information($"message {i}");

		Assert.IsTrue(StartupLog.BufferedEntries.Count < 5_000, "the buffer should stop growing");
		Assert.IsTrue(StartupLog.BufferedEntries.Count > 0);
	}
}
