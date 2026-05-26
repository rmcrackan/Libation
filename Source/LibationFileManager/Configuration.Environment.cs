using System;
using System.IO;

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
	public static string ToVersionString(this Version? version)
		=> version is null ? "[unknown]"
		: version.Revision > 1 ? version.ToString(4)
		: version.ToString(3);
}

public partial class Configuration
{
	public static bool IsWindows { get; } = OperatingSystem.IsWindows();
	public static bool IsLinux { get; } = OperatingSystem.IsLinux();
	public static bool IsMacOs { get; } = OperatingSystem.IsMacOS();

	/// <summary>True when running inside a (Linux) Snap sandbox (e.g. snap run libation). WebView login is disabled in this environment to avoid portal/sandbox crashes.</summary>
	public static bool IsRunningUnderSnap { get; } = IsLinux && !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP"));

	/// <summary>True when running inside a Flatpak sandbox (e.g. com.getlibation.Libation from Flathub).</summary>
	public static bool IsRunningUnderFlatpak { get; } = IsLinux && DetectFlatpak();

	/// <summary>Linux Snap or Flatpak. Embedded WebView login is disabled in these environments.</summary>
	public static bool IsRunningInLinuxSandbox { get; } = IsRunningUnderSnap || IsRunningUnderFlatpak;

	public static Version? LibationVersion { get; private set; }
	public static void SetLibationVersion(Version? version) => LibationVersion = version;

	public static OS OS { get; }
		= IsLinux ? OS.Linux
		: IsMacOs ? OS.MacOS
		: IsWindows ? OS.Windows
		: OS.Unknown;

	private static bool DetectFlatpak()
	{
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FLATPAK_ID")))
			return true;

		try
		{
			return File.Exists("/.flatpak-info");
		}
		catch
		{
			return false;
		}
	}
}
