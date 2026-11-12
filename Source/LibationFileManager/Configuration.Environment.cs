using NuGet.Versioning;
using System;
using System.Linq;

namespace LibationFileManager;

[Flags]
public enum OS
{
	Unknown,
	Windows = 0x100000,
	Linux = 0x200000,
	MacOS = 0x400000,
}

public static class Extensions
{
	public static string ToVersionString(this NuGetVersion? version)
		=> version is null ? "[unknown]"
		: version.IsLegacyVersion  && version.Revision > 1 && !version.ReleaseLabels.Any() ? version.ToString("x.y.z.r", VersionFormatter.Instance)
		: version.ToString();
}

public partial class Configuration
{
	public static bool IsWindows { get; } = OperatingSystem.IsWindows();
	public static bool IsLinux { get; } = OperatingSystem.IsLinux();
	public static bool IsMacOs { get; } = OperatingSystem.IsMacOS();

	/// <summary>True when running inside a (Linux) Snap sandbox (e.g. snap run libation). WebView login is disabled in this environment to avoid portal/sandbox crashes.</summary>
	public static bool IsRunningUnderSnap { get; } = IsLinux && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP"));

	/// <summary>True when running under a Linux Wayland desktop session.</summary>
	public static bool IsWayland { get; } = IsLinux && IsWaylandSession();

	public static bool IsWaylandSession(Func<string, string?>? getEnv = null)
	{
		getEnv ??= Environment.GetEnvironmentVariable;
		return !string.IsNullOrEmpty(getEnv("WAYLAND_DISPLAY"))
			|| string.Equals(getEnv("XDG_SESSION_TYPE"), "wayland", StringComparison.OrdinalIgnoreCase);
	}

	public static NuGetVersion? LibationVersion { get; private set; }
	public static void SetLibationVersion(NuGetVersion? version) => LibationVersion = version;

	public static OS OS { get; }
		= IsLinux ? OS.Linux
		: IsMacOs ? OS.MacOS
		: IsWindows ? OS.Windows
		: OS.Unknown;
}
