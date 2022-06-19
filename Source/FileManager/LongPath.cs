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
		private const string LONG_PATH_PREFIX = "\\\\?\\";
		private static readonly StringBuilder longPathBuffer = new(MaxPathLength);

		public string Path { get; init; }
		public override string ToString() => Path;

		public static implicit operator LongPath(string path)
		{
			if (path is null) return null;

			//File I/O functions in the Windows API convert "/" to "\" as part of converting
			//the name to an NT-style name, except when using the "\\?\" prefix 
			path = path.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);

			if (path.StartsWith(LONG_PATH_PREFIX))
				return new LongPath { Path = path };
			else if ((path.Length > 2 && path[1] == ':') || path.StartsWith("UNC\\"))
				return new LongPath { Path = LONG_PATH_PREFIX + path };
			else if (path.StartsWith("\\\\"))
				//The "\\?\" prefix can also be used with paths constructed according to the
				//universal naming convention (UNC). To specify such a path using UNC, use
				//the "\\?\UNC\" prefix.
				return new LongPath { Path = LONG_PATH_PREFIX + "UNC\\" + path.Substring(2) };
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

		public static implicit operator string(LongPath path) => path?.Path ?? null;

		[JsonIgnore]
		public string ShortPathName
		{
			get
			{
				if (Path is null) return null;
				GetShortPathName(Path, longPathBuffer, MAX_PATH);
				return longPathBuffer.ToString();
			}
		}

		[JsonIgnore]
		public string LongPathName
		{
			get
			{
				if (Path is null) return null;
				GetLongPathName(Path, longPathBuffer, MaxPathLength);
				return longPathBuffer.ToString();
			}
		}

		[JsonIgnore]
		public string PathWithoutPrefix
			=> Path?.StartsWith(LONG_PATH_PREFIX) == true ?
			Path.Remove(0, LONG_PATH_PREFIX.Length) :
			Path;


		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetShortPathName(string path, StringBuilder shortPath, int shortPathLength);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetLongPathName(string lpszShortPath, StringBuilder lpszLongPath, int cchBuffer);
	}
}
