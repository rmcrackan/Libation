using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace LibationFileManager;

/// <summary>
/// Process-wide guard that prevents more than one Libation instance from running against the same
/// LibationFiles folder at the same time. Concurrent instances race on the SQLite database, the
/// Lucene search index, and the log file, which can corrupt state (see issue #1931).
/// <para/>
/// The lock is keyed on the LibationFiles location, so separate portable installs that use different
/// folders may still run simultaneously. The returned instance must be kept alive for the lifetime of
/// the process and disposed at exit. The guard fails open: any error acquiring it is treated as "first
/// instance" so it can never block startup on its own.
/// </summary>
public sealed class SingleInstance : IDisposable
{
	private readonly Mutex? _mutex;
	private readonly bool _hasHandle;

	/// <summary>True if this process acquired the lock (no other instance is running for this folder).</summary>
	public bool IsFirstInstance => _hasHandle;

	private SingleInstance(Mutex? mutex, bool hasHandle)
	{
		_mutex = mutex;
		_hasHandle = hasHandle;
	}

	/// <summary>
	/// Attempts to acquire the single-instance lock for <paramref name="libationFilesLocation"/>.
	/// </summary>
	public static SingleInstance TryAcquire(string libationFilesLocation)
	{
		Mutex? mutex = null;
		try
		{
			mutex = new Mutex(initiallyOwned: false, buildMutexName(libationFilesLocation));

			bool hasHandle;
			try
			{
				hasHandle = mutex.WaitOne(TimeSpan.Zero, exitContext: false);
			}
			catch (AbandonedMutexException)
			{
				// The previous owner exited without releasing (e.g. it crashed). Ownership transfers to us.
				hasHandle = true;
			}

			return new SingleInstance(mutex, hasHandle);
		}
		catch (Exception ex)
		{
			// Never let the guard itself prevent startup. Fail open by treating this as the first instance.
			Serilog.Log.Logger.Warning(ex, "Could not evaluate the single-instance lock; continuing without it.");
			mutex?.Dispose();
			return new SingleInstance(null, true);
		}
	}

	private static string buildMutexName(string libationFilesLocation)
	{
		var normalized = (libationFilesLocation ?? string.Empty).Trim().TrimEnd('/', '\\').ToUpperInvariant();
		var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
		// Local (per-session) scope is sufficient: two OS users cannot safely share one files folder,
		// and each user gets their own default folder. The name must be filesystem-safe and unique per folder.
		return $"Libation-SingleInstance-{hash}";
	}

	public void Dispose()
	{
		try
		{
			if (_hasHandle)
				_mutex?.ReleaseMutex();
		}
		catch
		{
			// best effort; the OS releases the handle on process exit regardless
		}

		_mutex?.Dispose();
	}
}
