using System;
using System.IO;
using System.Linq;
using Dinah.Core;
using FileManager;

#nullable enable
namespace LibationFileManager
{
	public partial class Configuration : PropertyChangeFilter
	{
		public bool LibationSettingsAreValid => SettingsFileIsValid(SettingsFilePath);

		public static bool SettingsFileIsValid(string settingsFile)
		{
			if (!Directory.Exists(Path.GetDirectoryName(settingsFile)) || !File.Exists(settingsFile))
				return false;

			var pDic = new PersistentDictionary(settingsFile, isReadOnly: false);

			var booksDir = pDic.GetString(nameof(Books));

			if (booksDir is null) return false;

			if (!Directory.Exists(booksDir))
			{
				if (Path.GetDirectoryName(settingsFile) is not string dir)
					throw new DirectoryNotFoundException(settingsFile);

				//"Books" is not null, so setup has already been run.
				//Since Books can't be found, try to create it in Libation settings folder
				booksDir = Path.Combine(dir, nameof(Books));
				try
				{
					Directory.CreateDirectory(booksDir);

					pDic.SetString(nameof(Books), booksDir);

					return booksDir is not null && Directory.Exists(booksDir);
				}
				catch { return false; }
			}

			return true;
		}

		#region singleton stuff
		public static Configuration Instance { get; } = new Configuration();
		private Configuration() { }
		#endregion
	}
}
