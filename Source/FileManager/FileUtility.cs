using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dinah.Core;
using Polly;
using Polly.Retry;

#nullable enable
namespace FileManager
{
	public static class FileUtility
	{
		/// <summary>
		/// "txt" => ".txt"
		/// <br />".txt" => ".txt"
		/// <br />null or whitespace => ""
		/// </summary>
		[return: NotNull]
		public static string GetStandardizedExtension(string? extension)
			=> string.IsNullOrWhiteSpace(extension)
			? string.Empty
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
		public static LongPath GetValidFilename(LongPath path, ReplacementCharacters replacements, string? fileExtension, bool returnFirstExisting = false)
		{
			ArgumentValidator.EnsureNotNull(path, nameof(path));
			ArgumentValidator.EnsureNotNull(replacements, nameof(replacements));

			fileExtension = GetStandardizedExtension(fileExtension);

			var pathStr = removeInvalidWhitespace(path.Path);
			var pathWithoutExtension = pathStr.EndsWithInsensitive(fileExtension)
				? pathStr[..^fileExtension.Length]
				: path.Path;

			// remove invalid chars, but leave file extension untouched
			pathWithoutExtension = GetSafePath(pathWithoutExtension, replacements);

			// ensure uniqueness and check lengths
			var dir = Path.GetDirectoryName(pathWithoutExtension)?.TruncateFilename(LongPath.MaxDirectoryLength) ?? string.Empty;

			var filenameWithoutExtension = Path.GetFileName(pathWithoutExtension);
			var fileStem
				= Path.Combine(dir, filenameWithoutExtension.TruncateFilename(LongPath.MaxFilenameLength - fileExtension.Length))
				.TruncateFilename(LongPath.MaxPathLength - fileExtension.Length);

			var fullfilename = removeInvalidWhitespace(fileStem) + fileExtension;

			var i = 0;
			while (File.Exists(fullfilename) && !returnFirstExisting)
			{
				var increm = $" ({++i})";
				fullfilename = fileStem.TruncateFilename(LongPath.MaxPathLength - increm.Length - fileExtension.Length) + increm + fileExtension;
			}

			return fullfilename;
		}

		/// <summary>Use with full path, not file name. Valid path characters which are invalid file name characters will be retained: '\\', '/'</summary>
		public static LongPath GetSafePath(LongPath path, ReplacementCharacters replacements)
		{
			ArgumentValidator.EnsureNotNull(path, nameof(path));
			ArgumentValidator.EnsureNotNull(replacements, nameof(replacements));

			var pathNoPrefix = path.PathWithoutPrefix;

			pathNoPrefix = replacements.ReplacePathChars(pathNoPrefix);
			pathNoPrefix = removeDoubleSlashes(pathNoPrefix);

			return pathNoPrefix;
		}

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
			// take separator back off
			fullfilename = RemoveLastCharacter(fullfilename);

			fullfilename = removeDoubleSlashes(fullfilename);
			return fullfilename;
		}

		public static string RemoveLastCharacter(this string str) => string.IsNullOrEmpty(str) ? str : str[..^1];

		public static string TruncateFilename(this string filenameStr, int limit)
		{
			if (LongPath.IsWindows) return filenameStr.Truncate(limit);

			int index = filenameStr.Length;

			while (index > 0 && System.Text.Encoding.UTF8.GetByteCount(filenameStr, 0, index) > limit)
				index--;

			return filenameStr[..index];
		}

		/// <summary>
		/// Move file.
		/// <br/>- Ensure valid file name path: remove invalid chars, enforce max file length
		/// <br/>- Perform <see cref="SaferMove"/>
		/// </summary>
		/// <param name="source">Name of the file to move</param>
		/// <param name="destination">The new path and name for the file.</param>
		/// <param name="replacements">Rules for replacing illegal file path characters</param>
		/// <param name="extension">File extension override to use for <paramref name="destination"/></param>
		/// <param name="overwrite">If <c>false</c> and <paramref name="destination"/> exists, append " (n)" to filename and try again.</param>
		/// <returns>The actual destination filename</returns>
		public static LongPath SaferMoveToValidPath(
			LongPath source,
			LongPath destination,
			ReplacementCharacters replacements,
			string? extension = null,
			bool overwrite = false)
		{
			extension ??= Path.GetExtension(source);
			destination = GetValidFilename(destination, replacements, extension, overwrite);
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
					if (dir is null)
						throw new DirectoryNotFoundException();

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
			var enumOptions = new EnumerationOptions
			{ 
				RecurseSubdirectories = searchOption == SearchOption.AllDirectories,
				IgnoreInaccessible = true,
				ReturnSpecialDirectories = false,
				MatchType = MatchType.Simple
			};
			return Directory.EnumerateFiles(path.Path, searchPattern, enumOptions).Select(p => (LongPath) p);
		}

		/// <summary>
		/// Creates a subdirectory or subdirectories on the specified path.
		/// The specified path can be relative to this instance of the <see cref="DirectoryInfo"/> class.
		/// <para/>
		/// Fixes an issue with <see cref="DirectoryInfo.CreateSubdirectory(string)"/> where it fails when the parent <see cref="DirectoryInfo"/> is a drive root.
		/// </summary>
		/// <param name="path">The specified path. This cannot be a different disk volume or Universal Naming Convention (UNC) name.</param>
		/// <returns>The last directory specified in <paramref name="path"/></returns>
		public static DirectoryInfo CreateSubdirectoryEx(this DirectoryInfo parent, string path)
		{
			if (parent.Root.FullName != parent.FullName || Path.IsPathRooted(path))
				return parent.CreateSubdirectory(path);

			// parent is a drive root and subDirectory is relative
			//Solves a problem with DirectoryInfo.CreateSubdirectory where it fails
			//If the parent DirectoryInfo is a drive root.
			var fullPath = Path.GetFullPath(Path.Combine(parent.FullName, path));
			var directoryInfo = new DirectoryInfo(fullPath);
			directoryInfo.Create();
			return directoryInfo;
		}
	}
}
