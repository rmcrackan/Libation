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

		public const int MaxFilenameLength = 255;
		public static readonly int MaxDirectoryLength;
		public static readonly int MaxPathLength;
		private const int WIN_MAX_PATH = 260;
		private const string WIN_LONG_PATH_PREFIX = @"\\?\";
		internal static readonly bool IsWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		internal static readonly bool IsLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
		internal static readonly bool IsOSX = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
	
		public string Path { get; }

		static LongPath()
		{
			if (IsWindows)
			{
				MaxPathLength = short.MaxValue;
				MaxDirectoryLength = MaxPathLength - 13;
			}
			else if (IsOSX)
			{
				MaxPathLength = 1024;
				MaxDirectoryLength = MaxPathLength - MaxFilenameLength;
			}
			else
			{
				MaxPathLength = 4096;
				MaxDirectoryLength = MaxPathLength - MaxFilenameLength;
			}
		}

		[JsonConstructor]
		private LongPath(string path)
		{
			if (IsWindows && path.Length > MaxPathLength)
				throw new System.IO.PathTooLongException($"Path exceeds {MaxPathLength} character limit. ({path})");
			if (!IsWindows && Encoding.UTF8.GetByteCount(path) > MaxPathLength)
				throw new System.IO.PathTooLongException($"Path exceeds {MaxPathLength} byte limit. ({path})");

			Path = path;
		}
		
		//Filename limits on NTFS and FAT filesystems are based on characters,
		//but on ext* filesystems they're based on bytes. The ext* filesystems
		//don't care about encoding, so how unicode characters are encoded is
		///a choice made by the linux kernel. As best as I can tell, pretty
		//much everyone uses UTF-8.
		public static int GetFilesystemStringLength(string filename)
			=> IsWindows ? filename.Length
			: Encoding.UTF8.GetByteCount(filename);

		public static implicit operator LongPath(string path)
		{
			if (path is null) return null;

			if (!IsWindows) return new LongPath(path);

			//File I/O functions in the Windows API convert "/" to "\" as part of converting
			//the name to an NT-style name, except when using the "\\?\" prefix 
			path = path.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);

			if (path.StartsWith(WIN_LONG_PATH_PREFIX))
				return new LongPath(path);
			else if ((path.Length > 2 && path[1] == ':') || path.StartsWith(@"UNC\"))
				return new LongPath(WIN_LONG_PATH_PREFIX + path);
			else if (path.StartsWith(@"\\"))
				//The "\\?\" prefix can also be used with paths constructed according to the
				//universal naming convention (UNC). To specify such a path using UNC, use
				//the "\\?\UNC\" prefix.
				return new LongPath(WIN_LONG_PATH_PREFIX + @"UNC\" + path.Substring(2));
			else
			{
				//These prefixes are not used as part of the path itself. They indicate that
				//the path should be passed to the system with minimal modification, which
				//means that you cannot use forward slashes to represent path separators, or
				//a period to represent the current directory, or double dots to represent the
				//parent directory. Because you cannot use the "\\?\" prefix with a relative
				//path, relative paths are always limited to a total of MAX_PATH characters.
				if (path.Length > WIN_MAX_PATH)
					throw new System.IO.PathTooLongException();
				return new LongPath(path);
			}
		}

		public static implicit operator string(LongPath path) => path?.Path;

		[JsonIgnore]
		public string ShortPathName
		{
			get
			{
				if (!IsWindows) return Path;

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
				if (!IsWindows) return Path;
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
				if (!IsWindows) return Path;
				return
					Path?.StartsWith(WIN_LONG_PATH_PREFIX) == true ? Path.Remove(0, WIN_LONG_PATH_PREFIX.Length)
					:Path;
			}
		}

		public override string ToString() => Path;

		public override int GetHashCode() => Path.GetHashCode();
		public override bool Equals(object obj) => obj is LongPath other && Path == other.Path;
		public static bool operator ==(LongPath path1, LongPath path2) => path1.Equals(path2);
		public static bool operator !=(LongPath path1, LongPath path2) => !path1.Equals(path2);


		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetShortPathName([MarshalAs(UnmanagedType.LPWStr)] string path, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder shortPath, int shortPathLength);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetLongPathName([MarshalAs(UnmanagedType.LPWStr)] string lpszShortPath, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszLongPath, int cchBuffer);

	}
}
