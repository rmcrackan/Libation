using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FileManager
{
	public class LongPath
	{
		//https://docs.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation?tabs=cmd

		public const int MaxDirectoryLength = MaxPathLength - 13;
		public const int MaxPathLength = short.MaxValue;
		public const int MaxFilenameLength = 255;

		private const int MAX_PATH = 260;
		private const string LONG_PATH_PREFIX = @"\\?\";

		public string Path { get; init; }
		public override string ToString() => Path;

		internal static readonly PlatformID PlatformID = Environment.OSVersion.Platform;

	
		public static implicit operator LongPath(string path)
		{
			if (PlatformID is PlatformID.Unix) return new LongPath { Path = path };

			if (path is null) return null;

			//File I/O functions in the Windows API convert "/" to "\" as part of converting
			//the name to an NT-style name, except when using the "\\?\" prefix 
			path = path.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);

			if (path.StartsWith(LONG_PATH_PREFIX))
				return new LongPath { Path = path };
			else if ((path.Length > 2 && path[1] == ':') || path.StartsWith(@"UNC\"))
				return new LongPath { Path = LONG_PATH_PREFIX + path };
			else if (path.StartsWith(@"\\"))
				//The "\\?\" prefix can also be used with paths constructed according to the
				//universal naming convention (UNC). To specify such a path using UNC, use
				//the "\\?\UNC\" prefix.
				return new LongPath { Path = LONG_PATH_PREFIX + @"UNC\" + path.Substring(2) };
			else
			{
				//These prefixes are not used as part of the path itself. They indicate that
				//the path should be passed to the system with minimal modification, which
				//means that you cannot use forward slashes to represent path separators, or
				//a period to represent the current directory, or double dots to represent the
				//parent directory. Because you cannot use the "\\?\" prefix with a relative
				//path, relative paths are always limited to a total of MAX_PATH characters.
				if (path.Length > MAX_PATH)
					throw new System.IO.PathTooLongException();
				return new LongPath { Path = path };
			}
		}

		public static implicit operator string(LongPath path) => path?.Path;

		[JsonIgnore]
		public string ShortPathName
		{
			get
			{
				if (PlatformID is PlatformID.Unix) return Path;

				//Short Path names are useful for navigating to the file in windows explorer,
				//which will not recognize paths longer than MAX_PATH. Short path names are not
				//always enabled on every volume. So to check if a volume enables short path
				//names (aka 8dot3 names), run the following command from an elevated command
				//prompt:
				//
				//	fsutil 8dot3name query c:
				//
				//It will say:
				//
				//	"Based on the above settings, 8dot3 name creation is [enabled/disabled] on c:"
				//
				//To enable short names on a volume on the system, run the following command
				//from an elevated command prompt:
				//
				//	fsutil 8dot3name set c: 0
				//
				//or for all volumes on the system:
				//
				//	fsutil 8dot3name set 0
				//
				//Note that after enabling 8dot3 names on a volume, they will only be available
				//for newly-created entries in ther file system. Existing entries made while
				//8dot3 names were disabled will not be reachable by short paths.

				if (Path is null) return null;

				StringBuilder shortPathBuffer = new(MaxPathLength);
				GetShortPathName(Path, shortPathBuffer, MaxPathLength);
				return shortPathBuffer.ToString();
			}
		}

		[JsonIgnore]
		public string LongPathName
		{
			get
			{
				if (PlatformID is PlatformID.Unix) return Path;
				if (Path is null) return null;

				StringBuilder longPathBuffer = new(MaxPathLength);
				GetLongPathName(Path, longPathBuffer, MaxPathLength);
				return longPathBuffer.ToString();
			}
		}

		[JsonIgnore]
		public string PathWithoutPrefix
		{
			get
			{
				if (PlatformID is PlatformID.Unix) return Path;
				return
					Path?.StartsWith(LONG_PATH_PREFIX) == true ? Path.Remove(0, LONG_PATH_PREFIX.Length)
					:Path;
			}
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder shortPath, int shortPathLength);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetLongPathName([MarshalAs(UnmanagedType.LPWStr)] string lpszShortPath, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszLongPath, int cchBuffer);

	}
}
