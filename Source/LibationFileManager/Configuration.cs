using System;
using System.IO;
using System.Linq;
using Dinah.Core;
using FileManager;


namespace LibationFileManager
{
	public partial class Configuration : PropertyChangeFilter
	{
		public bool LibationSettingsAreValid
			=> File.Exists(APPSETTINGS_JSON)
			&& SettingsFileIsValid(SettingsFilePath);

		public static bool SettingsFileIsValid(string settingsFile)
		{
			if (!Directory.Exists(Path.GetDirectoryName(settingsFile)) || !File.Exists(settingsFile))
				return false;

			var pDic = new PersistentDictionary(settingsFile, isReadOnly: true);

			var booksDir = pDic.GetString(nameof(Books));
			if (booksDir is null || !Directory.Exists(booksDir))
				return false;

			return true;
		}

		#region singleton stuff
		public static Configuration Instance { get; } = new Configuration();
		private Configuration() { }
		#endregion
	}
}
