using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;

namespace LibationFileManager.Tests;

[TestClass]
public class SingleInstanceTests
{
	private static string UniqueLocation() => Path.Combine(Path.GetTempPath(), "Libation-SI-" + Guid.NewGuid().ToString("N"));

	/// <summary>
	/// Acquire and hold the lock on a dedicated background thread. A named mutex is owned per-thread and
	/// is reentrant on its owning thread, so a "different instance" must be simulated from another thread
	/// (which mirrors the real cross-process case). The returned action releases the lock and joins.
	/// </summary>
	private static (bool isFirstInstance, Action release) HoldOnBackgroundThread(string location)
	{
		var acquired = new ManualResetEventSlim();
		var release = new ManualResetEventSlim();
		var isFirst = false;

		var thread = new Thread(() =>
		{
			using var instance = SingleInstance.TryAcquire(location);
			isFirst = instance.IsFirstInstance;
			acquired.Set();
			release.Wait();
		})
		{ IsBackground = true };

		thread.Start();
		acquired.Wait();

		return (isFirst, () => { release.Set(); thread.Join(); });
	}

	[TestMethod]
	public void first_acquire_is_first_instance()
	{
		using var first = SingleInstance.TryAcquire(UniqueLocation());
		Assert.IsTrue(first.IsFirstInstance);
	}

	[TestMethod]
	public void second_acquire_same_location_is_not_first_instance()
	{
		var location = UniqueLocation();

		var (heldIsFirst, release) = HoldOnBackgroundThread(location);
		try
		{
			Assert.IsTrue(heldIsFirst);

			using var second = SingleInstance.TryAcquire(location);
			Assert.IsFalse(second.IsFirstInstance);
		}
		finally
		{
			release();
		}
	}

	[TestMethod]
	public void different_locations_both_get_first_instance()
	{
		using var a = SingleInstance.TryAcquire(UniqueLocation());
		using var b = SingleInstance.TryAcquire(UniqueLocation());

		Assert.IsTrue(a.IsFirstInstance);
		Assert.IsTrue(b.IsFirstInstance);
	}

	[TestMethod]
	public void releasing_allows_a_new_first_instance()
	{
		var location = UniqueLocation();

		var (heldIsFirst, release) = HoldOnBackgroundThread(location);
		Assert.IsTrue(heldIsFirst);
		// release the first owner, then a fresh acquire should succeed
		release();

		using var next = SingleInstance.TryAcquire(location);
		Assert.IsTrue(next.IsFirstInstance);
	}

	[TestMethod]
	public void location_matching_ignores_trailing_separator_and_case()
	{
		var baseDir = UniqueLocation();

		var (heldIsFirst, release) = HoldOnBackgroundThread(baseDir);
		try
		{
			Assert.IsTrue(heldIsFirst);

			// same folder expressed with a trailing separator and different case must map to the same lock
			using var second = SingleInstance.TryAcquire(baseDir.ToUpperInvariant() + Path.DirectorySeparatorChar);
			Assert.IsFalse(second.IsFirstInstance);
		}
		finally
		{
			release();
		}
	}
}
