using System;
using System.IO;

#nullable enable
namespace FileManager
{
	public static class FileSystemTest
	{
		/// <summary>
		/// Additional characters which are illegal for filenames in Windows environments.
		/// Double quotes and slashes are already illegal filename characters on all platforms,
		/// so they are not included here.
		/// </summary>
		public static string AdditionalInvalidWindowsFilenameCharacters { get; } = "<>|:*?";

		/// <summary>
		/// Test if the directory supports filenames with characters that are invalid on Windows (:, *, ?, &lt;, &gt;, |).
		/// </summary>
		public static bool CanWriteWindowsInvalidChars(LongPath? directoryName)
		{
			if (directoryName is null)
				return false;
			var testFile = Path.Combine(directoryName, AdditionalInvalidWindowsFilenameCharacters + Guid.NewGuid().ToString());
			return CanWriteFile(testFile);
		}

		/// <summary>
		/// Test if the directory supports filenames with 255 unicode characters.
		/// </summary>
		public static bool CanWrite255UnicodeChars(LongPath? directoryName)
		{
			if (directoryName is null)
				return false;
			const char unicodeChar = 'ü';
			var testFileName = new string(unicodeChar, 255);
			var testFile = Path.Combine(directoryName, testFileName);
			return CanWriteFile(testFile);
		}

		/// <summary>
		/// Test if a directory has write access by attempting to create an empty file in it.
		/// <para/>Returns true even if the temporary file can not be deleted.
		/// </summary>
		public static bool CanWriteDirectory(LongPath directoryName)
		{
			if (!Directory.Exists(directoryName))
				return false;

			Serilog.Log.Logger.Debug("Testing write permissions for directory: {@DirectoryName}", directoryName);
			var testFilePath = Path.Combine(directoryName, Guid.NewGuid().ToString());
			return CanWriteFile(testFilePath);
		}

		private static bool CanWriteFile(LongPath filename)
		{
			try
			{
				Serilog.Log.Logger.Debug("Testing ability to write filename: {@filename}", filename);
				File.WriteAllBytes(filename, []);
				Serilog.Log.Logger.Debug("Deleting test file after successful write: {@filename}", filename);
				try
				{
					FileUtility.SaferDelete(filename);
				}
				catch (Exception ex)
				{
					//An error deleting the file doesn't constitute a write failure.
					Serilog.Log.Logger.Debug(ex, "Error deleting test file: {@filename}", filename);
				}
				return true;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Debug(ex, "Error writing test file: {@filename}", filename);
				return false;
			}
		}
	}
}
