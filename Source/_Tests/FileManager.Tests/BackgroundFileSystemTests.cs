using FileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;

namespace BackgroundFileSystemTests;

/// <summary>
/// The first tests for this class, prompted by a CI run where every test passed and the run still failed:
/// FileLiberator.Tests exited with 0xE0434352 and
/// <c>InvalidOperationException: The collection has been marked as complete with regards to additions</c>,
/// thrown from FileSystemWatcher_Changed. Disposing while the OS still had events buffered took the process
/// down, because on Windows those arrive on a native completion callback where nothing catches anything.
/// </summary>
[TestClass]
[DoNotParallelize]
public class DisposeWhileEventsAreArriving
{
	private string tempDir = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempDir = Path.Combine(Path.GetTempPath(), $"libation-bfs-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			Directory.Delete(tempDir, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	/// <summary>
	/// Churns the directory hard enough that events are in flight, then disposes underneath them. The assertion
	/// that matters is that the process is still alive afterwards: before the fix this crashed the test host
	/// rather than failing anything.
	/// </summary>
	[TestMethod]
	public void disposing_under_a_flood_of_events_does_not_crash()
	{
		for (var attempt = 0; attempt < 20; attempt++)
		{
			var sut = new BackgroundFileSystem(tempDir, "*.*", SearchOption.AllDirectories);

			for (var i = 0; i < 50; i++)
				File.WriteAllText(Path.Combine(tempDir, $"file-{attempt}-{i}.txt"), "x");

			// no wait: the point is to dispose while the watcher still has events to deliver
			sut.Dispose();

			foreach (var file in Directory.GetFiles(tempDir))
				File.Delete(file);
		}

		// give any late callback the chance to arrive and take the process with it
		Thread.Sleep(250);
	}

	[TestMethod]
	public void disposing_twice_is_harmless()
	{
		var sut = new BackgroundFileSystem(tempDir, "*.*", SearchOption.AllDirectories);

		sut.Dispose();
		sut.Dispose();
	}

	[TestMethod]
	public void files_present_before_and_after_construction_are_both_found()
	{
		File.WriteAllText(Path.Combine(tempDir, "before.txt"), "x");

		using var sut = new BackgroundFileSystem(tempDir, "*.*", SearchOption.AllDirectories);

		Assert.IsNotNull(sut.FindFile(new Regex(@"before\.txt$")));

		File.WriteAllText(Path.Combine(tempDir, "after.txt"), "x");

		// the watcher feeds a background scanner, so the new file appears when it gets there
		var found = WaitFor(() => sut.FindFile(new Regex(@"after\.txt$")) is not null);
		Assert.IsTrue(found, "a file created after construction never reached the cache");
	}

	private static bool WaitFor(Func<bool> condition, int timeoutMs = 5000)
	{
		for (var waited = 0; waited < timeoutMs; waited += 50)
		{
			if (condition())
				return true;
			Thread.Sleep(50);
		}

		return false;
	}
}

/// <summary>
/// From issue #1984, where a Books folder on a failing USB drive closed Libation on every launch. This type is
/// constructed from the static initializer of <c>AudibleFileStorage</c>, and the runtime caches a failed static
/// initializer for the life of the process: anything that escapes here is rethrown at every later caller that
/// so much as asks where the Books folder is, including the startup logging that runs before the window opens.
/// So construction has to survive a root directory it cannot read, however it cannot read it.
/// </summary>
[TestClass]
[DoNotParallelize]
public class WhenTheRootDirectoryCannotBeRead
{
	private string tempDir = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempDir = Path.Combine(Path.GetTempPath(), $"libation-bfs-unreadable-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			if (!OperatingSystem.IsWindows() && Directory.Exists(tempDir))
				File.SetUnixFileMode(tempDir, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
			Directory.Delete(tempDir, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	[TestMethod]
	public void a_root_that_is_not_there_is_reported_rather_than_thrown()
	{
		using var sut = new BackgroundFileSystem(Path.Combine(tempDir, "no-such-folder"), "*.*", SearchOption.AllDirectories);

		Assert.IsNull(sut.RootDirectory, "a root that cannot be used is dropped, so the owner rebuilds this when it can");
		Assert.IsNull(sut.FindFile(new Regex(".*")));
	}

	[TestMethod]
	public void a_root_that_refuses_to_be_read_costs_the_cache_and_nothing_else()
	{
		// Assert.Inconclusive is not [DoesNotReturn], so return explicitly or the body below still
		// looks reachable on Windows to the platform compatibility analyzer
		if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
		{
			Assert.Inconclusive("Skipped because revoking directory read permission needs unix file modes.");
			return;
		}
		if (Environment.IsPrivilegedProcess)
		{
			Assert.Inconclusive("Skipped because root may read a directory with no permissions, so there is nothing to refuse.");
			return;
		}

		aRootThatRefusesToBeRead(tempDir);
	}

	[SupportedOSPlatform("linux")]
	[SupportedOSPlatform("macos")]
	private static void aRootThatRefusesToBeRead(string directory)
	{
		File.WriteAllText(Path.Combine(directory, "book.m4b"), "audio");
		File.SetUnixFileMode(directory, UnixFileMode.None);

		using var sut = new BackgroundFileSystem(directory, "*.*", SearchOption.AllDirectories);

		Assert.IsNull(sut.FindFile(new Regex(@"book\.m4b$")), "nothing can be read, so nothing is found");
	}
}

/// <summary>
/// A second failure mode in the same class, from a CI run where all three Windows legs failed and the other six
/// passed: every test in FileLiberator.Tests' PDF path suite failed in TestInitialize with an AggregateException
/// wrapping <c>FileNotFoundException: Could not find file ...</c>, naming a path none of those tests had
/// anything to do with. The watcher had raised Created for a folder an earlier test's cleanup then deleted; the
/// scanner asked whether it existed, was told yes, asked what it was, and got an exception. That killed the
/// scanner and was stored on its task, and the next Stop() - reached from AudibleFileStorage.Audio.Refresh() by
/// way of Dispose() - rethrew it at that caller.
/// <para>
/// The trigger is a disagreement between Exists and GetAttributes over a long <c>\\?\</c> path, which cannot be
/// staged on the platform these tests usually run on. So the guard itself is asserted directly, and the rest
/// covers what the death cost: a cache that stops tracking, and an exception handed to an unrelated caller.
/// </para>
/// </summary>
[TestClass]
[DoNotParallelize]
public class PathsThatVanishBeforeTheyAreRead
{
	private string tempDir = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempDir = Path.Combine(Path.GetTempPath(), $"libation-bfs-vanishing-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempDir);
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			Directory.Delete(tempDir, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	/// <summary>Creates a batch of files and deletes the tree while their events are still queued.</summary>
	private void ChurnVanishingFiles()
	{
		for (var batch = 0; batch < 5; batch++)
		{
			var directory = Path.Combine(tempDir, $"vanishing-{batch}");
			Directory.CreateDirectory(directory);

			for (var i = 0; i < 40; i++)
				File.WriteAllText(Path.Combine(directory, $"book-{i}.m4b"), "audio");

			// No wait: the scanner should reach these paths after they have gone.
			Directory.Delete(directory, recursive: true);
		}
	}

	[TestMethod]
	public void a_path_that_is_not_there_reads_as_nothing_rather_than_throwing()
	{
		// The fix, on its own terms. Asking what a path is used to be allowed to throw, on the strength of having
		// asked a moment earlier whether it was there.
		Assert.IsNull(BackgroundFileSystem.TryGetAttributes(Path.Combine(tempDir, "never-existed.m4b")));
		Assert.IsNull(BackgroundFileSystem.TryGetAttributes(Path.Combine(tempDir, "no", "such", "folder", "book.m4b")));
	}

	[TestMethod]
	public void a_path_that_is_there_still_reads_as_what_it_is()
	{
		var file = Path.Combine(tempDir, "real.m4b");
		File.WriteAllText(file, "audio");

		Assert.IsFalse(BackgroundFileSystem.TryGetAttributes(file)!.Value.HasFlag(FileAttributes.Directory));
		Assert.IsTrue(BackgroundFileSystem.TryGetAttributes(tempDir)!.Value.HasFlag(FileAttributes.Directory));
	}

	[TestMethod]
	public void the_scanner_keeps_going_after_a_path_it_cannot_read()
	{
		using var sut = new BackgroundFileSystem(tempDir, "*.*", SearchOption.AllDirectories);

		ChurnVanishingFiles();

		// The scanner is still doing its job, which is what dying used to cost: the first unreadable path ended
		// the loop, and every later change to the Books directory went unnoticed for the rest of the session.
		File.WriteAllText(Path.Combine(tempDir, "survivor.m4b"), "audio");

		var found = WaitFor(() => sut.FindFile(new Regex(@"survivor\.m4b$")) is not null);
		Assert.IsTrue(found, "the scanner stopped tracking changes after a path it could not read");
	}

	[TestMethod]
	public void disposing_afterwards_does_not_hand_the_failure_to_the_caller()
	{
		// This is the CI stack: Refresh() found the Books directory changed, disposed the old file system, and
		// Stop() waited on a scanner that had already faulted.
		var sut = new BackgroundFileSystem(tempDir, "*.*", SearchOption.AllDirectories);

		ChurnVanishingFiles();

		sut.Dispose();
	}

	[TestMethod]
	public void a_file_that_outlives_its_event_is_still_tracked()
	{
		// The guard must not have been widened into ignoring everything: what is really there still lands.
		using var sut = new BackgroundFileSystem(tempDir, "*.*", SearchOption.AllDirectories);

		var directory = Path.Combine(tempDir, "kept");
		Directory.CreateDirectory(directory);
		File.WriteAllText(Path.Combine(directory, "kept.m4b"), "audio");

		var found = WaitFor(() => sut.FindFile(new Regex(@"kept\.m4b$")) is not null);
		Assert.IsTrue(found, "a file that was never deleted did not reach the cache");
	}

	private static bool WaitFor(Func<bool> condition, int timeoutMs = 5000)
	{
		for (var waited = 0; waited < timeoutMs; waited += 50)
		{
			if (condition())
				return true;
			Thread.Sleep(50);
		}

		return false;
	}
}
