using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationFileManager
{
	[Flags]
	public enum OS
	{
		Unknown,
		Windows = 0x100000,
		Linux = 0x200000,
		MacOS = 0x400000,
	}

	public partial class Configuration
    {
		public static bool IsWindows { get; } = OperatingSystem.IsWindows();
        public static bool IsLinux { get; } = OperatingSystem.IsLinux();
        public static bool IsMacOs { get; } = OperatingSystem.IsMacOS();

		public static OS OS { get; }
			= IsLinux ? OS.Linux
			: IsMacOs ? OS.MacOS
			: IsWindows ? OS.Windows
			: OS.Unknown;
    }
}
