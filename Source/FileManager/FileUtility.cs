using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dinah.Core;
using Polly;
using Polly.Retry;

namespace FileManager
{
	public static class FileUtility
	{
		/// <summary>
		/// "txt" => ".txt"
		/// <br />".txt" => ".txt"
		/// <br />null or whitespace => ""
		/// </summary>
		public static string GetStandardizedExtension(string extension)
			=> string.IsNullOrWhiteSpace(extension)
			? (extension ?? "")?.Trim()
			: '.' + extension.Trim().Trim('.');

		/// <summary>
		/// Return position with correct number of leading zeros.
		/// <br />- 2 of 9 => "2"
		/// <br />- 2 of 90 => "02"
		/// <br />- 2 of 900 => "002"
		/// </summary>
		/// <param name="position">position in sequence. The 'x' in 'x of y'</param>
		/// <param name="total">total qty in sequence. The 'y' in 'x of y'</param>
		public static string GetSequenceFormatted(int position, int total)
		{
			ArgumentValidator.EnsureGreaterThan(position, nameof(position), 0);
			ArgumentValidator.EnsureGreaterThan(total, nameof(total), 0);
			if (position > total)
				throw new ArgumentException($"{position} may not be greater than {total}");

			return position.ToString().PadLeft(total.ToString().Length, '0');
		}


		/// <summary>
		/// Ensure valid file name path: 
		/// <br/>- remove invalid chars
		/// <br/>- ensure uniqueness
		/// <br/>- enforce max file length
		/// </summary>
		public static LongPath GetValidFilename(LongPath path, string illegalCharacterReplacements = "", bool returnFirstExisting = false)
		{
			ArgumentValidator.EnsureNotNull(path, nameof(path));

			// remove invalid chars
			path = GetSafePath(path, illegalCharacterReplacements);

			// ensure uniqueness and check lengths
			var dir = Path.GetDirectoryName(path);
			dir = dir?.Truncate(LongPath.MaxDirectoryLength) ?? string.Empty;

			var extension = Path.GetExtension(path);

			var filename = Path.GetFileNameWithoutExtension(path).Truncate(LongPath.MaxFilenameLength - extension.Length);
			var fileStem = Path.Combine(dir, filename);


			var fullfilename = fileStem.Truncate(LongPath.MaxPathLength - extension.Length) + extension;

			fullfilename = removeInvalidWhitespace(fullfilename);

			var i = 0;
			while (File.Exists(fullfilename) && !returnFirstExisting)
			{
				var increm = $" ({++i})";
				fullfilename = fileStem.Truncate(LongPath.MaxPathLength - increm.Length - extension.Length) + increm + extension;
			}

			return fullfilename;
		}

		// GetInvalidFileNameChars contains everything in GetInvalidPathChars plus ':', '*', '?', '\\', '/'

		/// <summary>Use with file name, not full path. Valid path charaters which are invalid file name characters will be replaced: ':', '\\', '/'</summary>
		public static string GetSafeFileName(string str, string illegalCharacterReplacements = "")
			=> string.Join(illegalCharacterReplacements ?? "", str.Split(Path.GetInvalidFileNameChars()));

		/// <summary>Use with full path, not file name. Valid path charaters which are invalid file name characters will be retained: '\\', '/'</summary>
		public static LongPath GetSafePath(LongPath path, string illegalCharacterReplacements = "")
		{
			ArgumentValidator.EnsureNotNull(path, nameof(path));

			var pathNoPrefix = path.PathWithoutPrefix;

			pathNoPrefix = replaceColons(pathNoPrefix, "꞉");
			pathNoPrefix = replaceIllegalWithUnicodeAnalog(pathNoPrefix);
			pathNoPrefix = replaceInvalidChars(pathNoPrefix, illegalCharacterReplacements);
			pathNoPrefix = removeDoubleSlashes(pathNoPrefix);

			return pathNoPrefix;
		}

		private static char[] invalidChars { get; } = Path.GetInvalidPathChars().Union(new[] {
				'*', '?',
				// these are weird. If you run Path.GetInvalidPathChars() in Visual Studio's "C# Interactive", then these characters are included.
				// In live code, Path.GetInvalidPathChars() does not include them
				'"', '<', '>'
			}).ToArray();
		private static string replaceInvalidChars(string path, string illegalCharacterReplacements)
			=> string.Join(illegalCharacterReplacements ?? "", path.Split(invalidChars));

		private static string removeDoubleSlashes(string path)
		{
			if (path.Length < 2)
				return path;

			// exception: don't try to condense the initial dbl bk slashes in a path. eg: \\192.168.0.1

			var remainder = path[1..];
			var dblSeparator = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}";
			while (remainder.Contains(dblSeparator))
				remainder = remainder.Replace(dblSeparator, $"{Path.DirectorySeparatorChar}");

			return path[0] + remainder;
		}

		private static string replaceIllegalWithUnicodeAnalog(string path)
		{
			char[] replaced = path.ToCharArray();

			char GetQuote(int position)
			{
				if (
					position == 0
					|| (position > 0
						&& position < replaced.Length
						&& !char.IsLetter(replaced[position - 1])
						&& !char.IsNumber(replaced[position - 1])
						)
					) return '“';
				else if (
						position == replaced.Length - 1
						|| (position >= 0
							&& position < replaced.Length - 1
							&& !char.IsLetter(replaced[position + 1])
							&& !char.IsNumber(replaced[position + 1])
							)
						) return '”';
				else return '＂';
			}

			for (int i = 0; i < replaced.Length; i++)
			{
				replaced[i] = replaced[i] switch
				{
					'?' => '？',
					'*' => '✱',
					'<' => '＜',
					'>' => '＞',
					'"' => GetQuote(i),
					_ => replaced[i]
				};
			}
			return new string(replaced);
		}

		private static string replaceColons(string path, string illegalCharacterReplacements)
		{
			// replace all colons except within the first 2 chars
			var builder = new System.Text.StringBuilder();
			for (var i = 0; i < path.Length; i++)
			{
				var c = path[i];
				if (i >= 2 && c == ':')
					builder.Append(illegalCharacterReplacements);
				else
					builder.Append(c);
			}
			return builder.ToString();
		}
		private static string removeInvalidWhitespace_pattern { get; } = $@"[\s\.]*\{Path.DirectorySeparatorChar}\s*";
		private static Regex removeInvalidWhitespace_regex { get; } = new(removeInvalidWhitespace_pattern, RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

		/// <summary>no part of the path may begin or end in whitespace</summary>
		private static string removeInvalidWhitespace(string fullfilename)
		{
			// no whitespace at beginning or end
			// replace whitespace around path slashes
			//    regex (with space added for clarity)
			//    \s*  \\  \s*    =>    \
			// no ending dots. beginning dots are valid

			// regex is easier by ending with separator
			fullfilename += Path.DirectorySeparatorChar;
			fullfilename = removeInvalidWhitespace_regex.Replace(fullfilename, Path.DirectorySeparatorChar.ToString());
			// take seperator back off
			fullfilename = RemoveLastCharacter(fullfilename);

			fullfilename = removeDoubleSlashes(fullfilename);
			return fullfilename;
		}

		public static string RemoveLastCharacter(this string str) => string.IsNullOrEmpty(str) ? str : str[..^1];

		/// <summary>
		/// Move file.
		/// <br/>- Ensure valid file name path: remove invalid chars, ensure uniqueness, enforce max file length
		/// <br/>- Perform <see cref="SaferMove"/>
		/// <br/>- Return valid path
		/// </summary>
		public static string SaferMoveToValidPath(LongPath source, LongPath destination)
		{
			destination = GetValidFilename(destination);
			SaferMove(source, destination);
			return destination;
		}

		private static int maxRetryAttempts { get; } = 3;
		private static TimeSpan pauseBetweenFailures { get; } = TimeSpan.FromMilliseconds(100);
		private static RetryPolicy retryPolicy { get; } =
			Policy
			.Handle<Exception>()
			.WaitAndRetry(maxRetryAttempts, i => pauseBetweenFailures);

		/// <summary>Delete file. No error when source does not exist. Retry up to 3 times before throwing exception.</summary>
		public static void SaferDelete(LongPath source)
			=> retryPolicy.Execute(() =>
			{
				try
				{
					if (!File.Exists(source))
					{
						Serilog.Log.Logger.Debug("No file to delete: {@DebugText}", new { source });
						return;
					}

					Serilog.Log.Logger.Debug("Attempt to delete file: {@DebugText}", new { source });
					File.Delete(source);
					Serilog.Log.Logger.Information("File successfully deleted: {@DebugText}", new { source });
				}
				catch (Exception e)
				{
					Serilog.Log.Logger.Error(e, "Failed to delete file: {@DebugText}", new { source });
					throw;
				}
			});

		/// <summary>Move file. No error when source does not exist. Retry up to 3 times before throwing exception.</summary>
		public static void SaferMove(LongPath source, LongPath destination)
			=> retryPolicy.Execute(() =>
			{
				try
				{
					if (!File.Exists(source))
					{
						Serilog.Log.Logger.Debug("No file to move: {@DebugText}", new { source });
						return;
					}

					SaferDelete(destination);

					var dir = Path.GetDirectoryName(destination);
					Serilog.Log.Logger.Debug("Attempt to create directory: {@DebugText}", new { dir });
					Directory.CreateDirectory(dir);

					Serilog.Log.Logger.Debug("Attempt to move file: {@DebugText}", new { source, destination });
					File.Move(source, destination);
					Serilog.Log.Logger.Information("File successfully moved: {@DebugText}", new { source, destination });
				}
				catch (Exception e)
				{
					Serilog.Log.Logger.Error(e, "Failed to move file: {@DebugText}", new { source, destination });
					throw;
				}
			});

		/// <summary>
		/// A safer way to get all the files in a directory and sub directory without crashing on UnauthorizedException or PathTooLongException
		/// </summary>
		/// <param name="rootPath">Starting directory</param>
		/// <param name="patternMatch">Filename pattern match</param>
		/// <param name="searchOption">Search subdirectories or only top level directory for files</param>
		/// <returns>List of files</returns>
		public static IEnumerable<LongPath> SaferEnumerateFiles(LongPath path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			var foundFiles = Enumerable.Empty<LongPath>();

			if (searchOption == SearchOption.AllDirectories)
			{
				try
				{
					IEnumerable <LongPath> subDirs = Directory.EnumerateDirectories(path).Select(p => (LongPath)p);
					// Add files in subdirectories recursively to the list
					foreach (string dir in subDirs)
						foundFiles = foundFiles.Concat(SaferEnumerateFiles(dir, searchPattern, searchOption));
				}
				catch (UnauthorizedAccessException) { }
				catch (PathTooLongException) { }
			}

			try
			{
				// Add files from the current directory
				foundFiles = foundFiles.Concat(Directory.EnumerateFiles(path, searchPattern).Select(f => (LongPath)f));
			}
			catch (UnauthorizedAccessException) { }

			return foundFiles;
		}
	}
}
