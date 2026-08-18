using Microsoft.Win32;
using System;
using System.Runtime.Versioning;

namespace LibationFileManager;

/// <summary>Enforcement state of Windows Application Control, which on consumer PCs is Smart App Control.</summary>
public enum ApplicationControlState
{
	/// <summary>Not Windows, or the state could not be read or recognised.</summary>
	Unknown,
	Off,
	/// <summary>Blocking unsigned code that the reputation service does not recognise.</summary>
	Enforcing,
	/// <summary>Observing only. This mode never blocks.</summary>
	Evaluation,
}

/// <summary>
/// Reads whether Windows is enforcing Application Control, so Libation can explain a blocked file
/// and avoid an in-app upgrade that would leave the install unable to start.
/// </summary>
public static class ApplicationControlPolicy
{
	private const string PolicyKeyPath = @"SYSTEM\CurrentControlSet\Control\CI\Policy";
	private const string PolicyValueName = "VerifiedAndReputablePolicyState";

	private static ApplicationControlState? _state;

	/// <summary>
	/// Read once per process. Changing the setting requires a reboot, so the answer cannot go stale
	/// while Libation runs.
	/// </summary>
	public static ApplicationControlState GetState()
	{
		if (_state is ApplicationControlState cached)
			return cached;

		var state = ApplicationControlState.Unknown;

		if (OperatingSystem.IsWindows())
		{
			try
			{
				state = ReadWindowsState();
			}
			catch (Exception ex)
			{
				// Covers Windows blocking Microsoft.Win32.Registry.dll itself, which is exactly the
				// situation this check describes, so it must not become a second failure.
				Serilog.Log.Logger.Debug(ex, "Could not read the Application Control policy state");
			}
		}

		_state = state;
		return state;
	}

	/// <summary>
	/// True only when Windows is actively blocking. Anything unreadable or unrecognised counts as not
	/// blocking: the alternative is telling someone to turn Smart App Control off - which cannot be
	/// undone - on a PC that was never blocking anything.
	/// </summary>
	public static bool IsEnforcing => GetState() is ApplicationControlState.Enforcing;

	/// <summary>
	/// Interprets the raw VerifiedAndReputablePolicyState value. Null covers both an absent key and an
	/// absent value: Windows builds that never shipped Smart App Control have the key without the value.
	/// </summary>
	public static ApplicationControlState FromPolicyValue(int? policyValue)
		=> policyValue switch
		{
			0 => ApplicationControlState.Off,
			1 => ApplicationControlState.Enforcing,
			2 => ApplicationControlState.Evaluation,
			_ => ApplicationControlState.Unknown,
		};

	[SupportedOSPlatform("windows")]
	private static ApplicationControlState ReadWindowsState()
	{
		using var key = Registry.LocalMachine.OpenSubKey(PolicyKeyPath);
		return FromPolicyValue(key?.GetValue(PolicyValueName) as int?);
	}
}
