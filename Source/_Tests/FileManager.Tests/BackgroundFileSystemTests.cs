using FileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
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
