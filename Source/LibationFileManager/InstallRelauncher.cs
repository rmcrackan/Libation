using System;
using System.Diagnostics;
using System.IO;

namespace LibationFileManager;

/// <summary>
/// Starts a fresh copy of Libation after startup has repaired the install folder underneath the running one.
/// <para/>
/// The running process cannot simply carry on: it has already loaded the assemblies that the rollback just
/// replaced on disk. Restarting is the only way to pick up the restored files, so the user is asked whether
/// to do it now.
/// </summary>
public static class InstallRelauncher
{
	/// <summary>
	/// Set on the child process so it knows not to offer a restart of its own.
	/// <para/>
	/// A rollback deletes the pending upgrade marker before it returns, so it normally cannot happen twice.
	/// But that delete is best-effort and swallows its own failure, and a marker that outlives a rollback
	/// would otherwise mean every launch rolls back and offers to restart again. One relaunch, then.
	/// </summary>
	public const string RelaunchedEnvironmentVariable = "LIBATION_RELAUNCHED_AFTER_ROLLBACK";

	/// <summary>True when this process was started by <see cref="TryRelaunch"/>.</summary>
	public static bool WasRelaunched
		=> Environment.GetEnvironmentVariable(RelaunchedEnvironmentVariable) == "1";

	/// <summary>
	/// Overridable so tests can assert on the decision to relaunch without starting a process.
	/// </summary>
	public static Func<string, bool> StartProcess { get; set; } = StartDetached;

	/// <summary>
	/// Starts a new Libation process. Best effort: if it does not work the user can start Libation
	/// themselves, which is what the message tells them to do anyway.
	/// </summary>
	/// <returns>True when a new process was started.</returns>
	public static bool TryRelaunch()
	{
		try
		{
			var executable = Environment.ProcessPath;
			if (string.IsNullOrWhiteSpace(executable) || !File.Exists(executable))
			{
				StartupLog.Error($"Cannot restart Libation: no executable at '{executable}'");
				return false;
			}

			var started = StartProcess(executable);
			StartupLog.Information(started
				? $"Restarting Libation from {executable}"
				: $"Could not restart Libation from {executable}");

			return started;
		}
		catch (Exception ex)
		{
			StartupLog.Error(ex, "Could not restart Libation");
			return false;
		}
	}

	private static bool StartDetached(string executable)
	{
		// UseShellExecute must stay false: setting an environment variable for the child is not allowed
		// with the shell, and the marker is what stops a restart loop.
		var startInfo = new ProcessStartInfo(executable)
		{
			// The install folder was just rewritten, so start from it rather than inheriting a stale one.
			WorkingDirectory = Configuration.ProcessDirectory,
			UseShellExecute = false,
		};
		startInfo.Environment[RelaunchedEnvironmentVariable] = "1";

		return Process.Start(startInfo) is not null;
	}
}
