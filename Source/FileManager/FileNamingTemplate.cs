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

		/// <summary>Optional step 2: Replace all illegal characters with this. Default=<see cref="string.Empty"/></summary>
		public string IllegalCharacterReplacements { get; set; }

		/// <summary>Generate a valid path for this file or directory</summary>
		public LongPath GetFilePath(bool returnFirstExisting = false)
		{
			string fileName = Template;
			List<string> pathParts = new();

			var paramReplacements = ParameterReplacements.ToDictionary(r => $"<{formatKey(r.Key)}>", r => formatValue(r.Value));

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
					file = replaceFileName(file, paramReplacements);
					fileName = Path.GetDirectoryName(fileName);
					pathParts.Add(file);
				}
			}

			pathParts.Reverse();

			return FileUtility.GetValidFilename(Path.Join(pathParts.ToArray()), IllegalCharacterReplacements, returnFirstExisting);
		}

		private string replaceFileName(string filename, Dictionary<string,string> paramReplacements)
		{
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
			while (filenameParts.Sum(p => p.Length) > LongPath.MaxFilenameLength)
			{
				int maxLength = filenameParts.Max(p => p.Length);
				var maxEntry = filenameParts.First(p => p.Length == maxLength);

				maxEntry.Remove(maxLength - 1, 1);
			}
			return string.Join("", filenameParts);
		}

		private string formatValue(object value)
		{
			if (value is null)
				return "";

			// Other illegal characters will be taken care of later. Must take care of slashes now so params can't introduce new folders.
			// Esp important for file templates.
			return value
				.ToString()
				.Replace($"{System.IO.Path.DirectorySeparatorChar}", IllegalCharacterReplacements)
				.Replace($"{System.IO.Path.AltDirectorySeparatorChar}", IllegalCharacterReplacements);
		}
	}
}
