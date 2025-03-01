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

			if (pDic.GetString(nameof(Books)) is not string booksDir)
				return false;

			if (!Directory.Exists(booksDir))
			{
				if (Path.GetDirectoryName(settingsFile) is not string dir)
					throw new DirectoryNotFoundException(settingsFile);

				//"Books" is not null, so setup has already been run.
				//Since Books can't be found, try to create it
				//and then revert to the default books directory
				foreach (string d in new string[] { booksDir, DefaultBooksDirectory })
				{
					try
					{
						Directory.CreateDirectory(d);

						pDic.SetString(nameof(Books), d);

						return Directory.Exists(d);
					}
					catch { /* Do Nothing */ }
				}
				return false;
			}

			return true;
		}

		#region singleton stuff
		public static Configuration Instance { get; } = new Configuration();
		private Configuration() { }
		#endregion
	}
}
