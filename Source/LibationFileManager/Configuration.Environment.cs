using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationFileManager
{
    public partial class Configuration
    {
        public static bool IsWindows { get; } = OperatingSystem.IsWindows();
        public static bool IsLinux { get; } = OperatingSystem.IsLinux();
        public static bool IsMacOs { get; } = OperatingSystem.IsMacOS();

        public static string OS { get; }
            = IsLinux ? "Linux"
            : IsMacOs ? "MacOS"
            : IsWindows ? "Windows"
            : "UNKNOWN_OS";
    }
}
