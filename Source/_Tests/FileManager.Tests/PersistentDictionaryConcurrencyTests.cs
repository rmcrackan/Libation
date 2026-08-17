using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace FileManager.Tests;

/// <summary>
/// Configuration is a process-wide singleton whose properties are read from the UI thread, from
/// BackgroundWorker callbacks and from download workers at the same time, so every
/// <see cref="PersistentDictionary"/> member has to tolerate concurrent callers.
/// </summary>
[TestClass]
public class PersistentDictionaryConcurrencyTests
{
	private const int Threads = 16;
	private const int Keys = 200;
	// a data race needs a few attempts to show up reliably; a fresh dictionary each round means
	// every round races on an empty cache, which is where the colliding inserts happen
	private const int Rounds = 20;

	private static string createSettingsFile(string contents = "{}")
	{
		var dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "LibationTest_" + Guid.NewGuid().ToString("N"))).FullName;
		var file = Path.Combine(dir, "Settings.json");
		File.WriteAllText(file, contents);
		return file;
	}

	private static void deleteSettingsFile(string file)
	{
		try { Directory.Delete(Path.GetDirectoryName(file)!, recursive: true); } catch { /* ignore */ }
	}

	private static void assertNoFailures(ConcurrentQueue<Exception> failures)
	{
		if (failures.IsEmpty)
			return;

		var messages = new List<string>();
		foreach (var ex in failures)
			messages.Add(ex.ToString());

		Assert.Fail($"{failures.Count} concurrent operation(s) threw:{Environment.NewLine}{string.Join(Environment.NewLine, messages)}");
	}

	/// <summary>Runs <paramref name="body"/> on <see cref="Threads"/> threads released at the same instant.</summary>
	private static void runConcurrently(Action<int> body)
	{
		var failures = new ConcurrentQueue<Exception>();
		using var startLine = new Barrier(Threads);

		var threads = new Thread[Threads];
		for (var i = 0; i < Threads; i++)
		{
			var thread = i;
			threads[i] = new Thread(() =>
			{
				try
				{
					startLine.SignalAndWait();
					body(thread);
				}
				catch (Exception ex)
				{
					failures.Enqueue(ex);
				}
			});
			threads[i].Start();
		}

		foreach (var thread in threads)
			thread.Join();

		assertNoFailures(failures);
	}

	[TestMethod]
	public void GetNonString_ConcurrentReadersOfUnsetProperties_DoNotCorruptCache()
	{
		var file = createSettingsFile();
		try
		{
			for (var round = 0; round < Rounds; round++)
			{
				var dictionary = new PersistentDictionary(file);

				// every thread walks the same key set so the cache misses collide
				runConcurrently(_ =>
				{
					for (var i = 0; i < Keys; i++)
						Assert.IsFalse(dictionary.GetNonString($"Bool{i}", defaultValue: false));
				});
			}
		}
		finally
		{
			deleteSettingsFile(file);
		}
	}

	[TestMethod]
	public void GetString_ConcurrentReadersOfUnsetProperties_DoNotCorruptCache()
	{
		var file = createSettingsFile();
		try
		{
			for (var round = 0; round < Rounds; round++)
			{
				var dictionary = new PersistentDictionary(file);

				runConcurrently(_ =>
				{
					for (var i = 0; i < Keys; i++)
						Assert.AreEqual($"default{i}", dictionary.GetString($"String{i}", $"default{i}"));
				});
			}
		}
		finally
		{
			deleteSettingsFile(file);
		}
	}

	[TestMethod]
	public void GetAndSet_Concurrently_ReadsBackEveryWrittenValue()
	{
		var file = createSettingsFile();
		try
		{
			var dictionary = new PersistentDictionary(file);

			// each thread owns its own keys, so a write is always visible to its own later read
			runConcurrently(thread =>
			{
				for (var i = 0; i < 25; i++)
				{
					var boolName = $"Bool_{thread}_{i}";
					dictionary.SetNonString(boolName, true);
					Assert.IsTrue(dictionary.GetNonString(boolName, defaultValue: false));

					var stringName = $"String_{thread}_{i}";
					dictionary.SetString(stringName, $"value_{thread}_{i}");
					Assert.AreEqual($"value_{thread}_{i}", dictionary.GetString(stringName));
				}
			});

			// the file must still be valid json holding every write
			var reread = new PersistentDictionary(file);
			for (var thread = 0; thread < Threads; thread++)
			{
				for (var i = 0; i < 25; i++)
				{
					Assert.IsTrue(reread.GetNonString($"Bool_{thread}_{i}", defaultValue: false), $"Bool_{thread}_{i} was lost");
					Assert.AreEqual($"value_{thread}_{i}", reread.GetString($"String_{thread}_{i}"), $"String_{thread}_{i} was lost");
				}
			}
		}
		finally
		{
			deleteSettingsFile(file);
		}
	}

	[TestMethod]
	public void GetStringFromJsonPath_ConcurrentReaders_DoNotCorruptCache()
	{
		var nested = new JObject();
		for (var i = 0; i < Keys; i++)
			nested[$"Name{i}"] = $"value{i}";

		var file = createSettingsFile(new JObject { ["Nested"] = nested }.ToString());
		try
		{
			for (var round = 0; round < Rounds; round++)
			{
				var dictionary = new PersistentDictionary(file);

				runConcurrently(_ =>
				{
					for (var i = 0; i < Keys; i++)
					{
						Assert.AreEqual($"value{i}", dictionary.GetStringFromJsonPath($"Nested.Name{i}"));
						Assert.IsNull(dictionary.GetStringFromJsonPath($"Nested.Missing{i}"));
					}
				});
			}
		}
		finally
		{
			deleteSettingsFile(file);
		}
	}

	[TestMethod]
	public void ReadWhileWriting_DoesNotThrow()
	{
		var file = createSettingsFile();
		try
		{
			var dictionary = new PersistentDictionary(file);

			// readers keep hitting the file (Exists/GetJObject never cache) while writers rewrite it
			runConcurrently(thread =>
			{
				if (thread % 2 == 0)
					for (var i = 0; i < Keys; i++)
						dictionary.SetNonString($"Written_{thread}", i);
				else
					for (var i = 0; i < Keys; i++)
					{
						dictionary.Exists($"Written_{i}");
						Assert.IsNotNull(dictionary.GetJObject());
					}
			});
		}
		finally
		{
			deleteSettingsFile(file);
		}
	}
}
