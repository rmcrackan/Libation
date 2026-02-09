using System;

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
	public static Version? LibationVersion { get; private set; }
	public static void SetLibationVersion(Version? version) => LibationVersion = version;

	public static OS OS { get; }
		= IsLinux ? OS.Linux
		: IsMacOs ? OS.MacOS
		: IsWindows ? OS.Windows
		: OS.Unknown;
}
