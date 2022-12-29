using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileManager
{
	/// <summary>Get valid filename. Advanced features incl. parameterized template</summary>
	public class FileNamingTemplate : NamingTemplate
	{
		/// <param name="template">Proposed file name with optional html-styled template tags.</param>
		public FileNamingTemplate(string template) : base(template) { }

		/// <summary>Generate a valid path for this file or directory</summary>
		public LongPath GetFilePath(ReplacementCharacters replacements, bool returnFirstExisting = false)
		{
			string fileName = 
				Template.EndsWith(Path.DirectorySeparatorChar) || Template.EndsWith(Path.AltDirectorySeparatorChar) ?
					FileUtility.RemoveLastCharacter(Template) : 
					Template;

			List<string> pathParts = new();

			var paramReplacements = ParameterReplacements.ToDictionary(r => $"<{formatKey(r.Key)}>", r => formatValue(r.Value, replacements));

			while (!string.IsNullOrEmpty(fileName))
			{
				var file = Path.GetFileName(fileName);

				if (Path.IsPathRooted(Template) && file == string.Empty)
				{
					pathParts.Add(fileName);
					break;
				}
				else
				{
					pathParts.Add(file);
					fileName = Path.GetDirectoryName(fileName);
				}
			}

			pathParts.Reverse();
			var fileNamePart = pathParts[^1];
			pathParts.Remove(fileNamePart);

			var fileExtension = Path.GetExtension(fileNamePart);
			fileNamePart = fileNamePart[..^fileExtension.Length];

			LongPath directory = Path.Join(pathParts.Select(p => replaceFileName(p, paramReplacements, LongPath.MaxFilenameLength)).ToArray());

			//If file already exists, GetValidFilename will append " (n)" to the filename.
			//This could cause the filename length to exceed MaxFilenameLength, so reduce
			//allowable filename length by 5 chars, allowing for up to 99 duplicates.
			return FileUtility
			.GetValidFilename(
				Path.Join(directory, replaceFileName(fileNamePart, paramReplacements, LongPath.MaxFilenameLength - fileExtension.Length - 5)) + fileExtension,
				replacements,
				returnFirstExisting
				);
		}

		private static string replaceFileName(string filename, Dictionary<string,string> paramReplacements, int maxFilenameLength)
		{
			//Filename limits on NTFS and FAT filesystems are based on characters,
			//but on ext* filesystems they're based on bytes. The ext* filesystems
			//don't care about encoding, so how unicode characters are encoded is
			///a choice made by the linux kernel. As best as I can tell, pretty
			//much everyone uses UTF-8.
			int getFilesystemStringLength(StringBuilder str)
			 => LongPath.PlatformID is PlatformID.Win32NT ?
				str.Length
				: Encoding.UTF8.GetByteCount(str.ToString());

			List<StringBuilder> filenameParts = new();
			//Build the filename in parts, replacing replacement parameters with
			//their values, and storing the parts in a list.
			while (!string.IsNullOrEmpty(filename))
			{
				int openIndex = filename.IndexOf('<');
				int closeIndex = filename.IndexOf('>');

				if (openIndex == 0 && closeIndex > 0)
				{
					var key = filename[..(closeIndex + 1)];

					if (paramReplacements.ContainsKey(key))
						filenameParts.Add(new StringBuilder(paramReplacements[key]));
					else
						filenameParts.Add(new StringBuilder(key));

					filename = filename[(closeIndex + 1)..];
				}
				else if (openIndex > 0 && closeIndex > openIndex)
				{
					var other = filename[..openIndex];
					filenameParts.Add(new StringBuilder(other));
					filename = filename[openIndex..];
				}
				else
				{
					filenameParts.Add(new StringBuilder(filename));
					filename = string.Empty;
				}
			}

			//Remove 1 character from the end of the longest filename part until
			//the total filename is less than max filename length
			while (filenameParts.Sum(p => getFilesystemStringLength(p)) > maxFilenameLength)
			{
				int maxLength = filenameParts.Max(p => p.Length);
				var maxEntry = filenameParts.First(p => p.Length == maxLength);

				maxEntry.Remove(maxLength - 1, 1);
			}
			return string.Join("", filenameParts);
		}

		private static string formatValue(object value, ReplacementCharacters replacements)
		{
			if (value is null)
				return "";

			// Other illegal characters will be taken care of later. Must take care of slashes now so params can't introduce new folders.
			// Esp important for file templates.
			return replacements.ReplaceFilenameChars(value.ToString());
		}
	}
}
