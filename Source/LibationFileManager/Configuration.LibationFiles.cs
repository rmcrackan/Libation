using System;
using System.ComponentModel;
using System.IO;
using FileManager;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;

namespace LibationFileManager
{
    public partial class Configuration
    {
        private static string getAppsettingsFile()
        {
            const string appsettings_filename = "appsettings.json";
            const string empty_json = "{ }";

			System.Collections.Generic.Queue<string> searchDirs = new();

            searchDirs.Enqueue(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
            searchDirs.Enqueue(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Libation"));
			searchDirs.Enqueue(UserProfile);

            do
            {
                var appsettingsFile = Path.Combine(searchDirs.Dequeue(), appsettings_filename);

                if (File.Exists(appsettingsFile))
                    return appsettingsFile;
                try
                {
                    File.WriteAllText(appsettingsFile, empty_json);
                    return appsettingsFile;
				}
                catch { }
			}
            while (searchDirs.Count > 0);

            //We Could not find or create appsettings.json.
            //As a Hail Mary, create it in temp files.
            var tempAppsettings = Path.GetTempFileName();
			File.WriteAllText(tempAppsettings, empty_json);
			return tempAppsettings;
		}

        private static string APPSETTINGS_JSON { get; } = getAppsettingsFile();

		private const string LIBATION_FILES_KEY = "LibationFiles";

        [Description("Location for storage of program-created files")]
        public string LibationFiles
        {
            get
            {
                if (libationFilesPathCache is not null)
                    return libationFilesPathCache;

                // FIRST: must write here before SettingsFilePath in next step reads cache
                libationFilesPathCache = getLibationFilesSettingFromJson();

                // SECOND. before setting to json file with SetWithJsonPath, PersistentDictionary must exist
                persistentDictionary = new PersistentDictionary(SettingsFilePath);

                // Config init in ensureSerilogConfig() only happens when serilog setting is first created (prob on 1st run).
                // This Set() enforces current LibationFiles every time we restart Libation or redirect LibationFiles
                var logPath = Path.Combine(LibationFiles, "Log.log");

                // BAD: Serilog.WriteTo[1].Args
                //      "[1]" assumes ordinal position
                // GOOD: Serilog.WriteTo[?(@.Name=='File')].Args
                var jsonpath = "Serilog.WriteTo[?(@.Name=='File')].Args";

                SetWithJsonPath(jsonpath, "path", logPath, true);

                return libationFilesPathCache;
            }
        }

        private static string libationFilesPathCache { get; set; }

        private string getLibationFilesSettingFromJson()
        {
            string startingContents = null;
            try
            {
                startingContents = File.ReadAllText(APPSETTINGS_JSON);
                var startingJObj = JObject.Parse(startingContents);

                if (startingJObj.ContainsKey(LIBATION_FILES_KEY))
                {
                    var startingValue = startingJObj[LIBATION_FILES_KEY].Value<string>();
                    if (!string.IsNullOrWhiteSpace(startingValue))
                        return startingValue;
                }
            }
            catch { }

            // not found. write to file. read from file
            var endingContents = new JObject { { LIBATION_FILES_KEY, UserProfile.ToString() } }.ToString(Formatting.Indented);
            if (startingContents != endingContents)
            {
                File.WriteAllText(APPSETTINGS_JSON, endingContents);
                System.Threading.Thread.Sleep(100);
            }

            // do not check whether directory exists. special/meta directory (eg: AppDir) is valid
            // verify from live file. no try/catch. want failures to be visible
            var jObjFinal = JObject.Parse(File.ReadAllText(APPSETTINGS_JSON));
            var valueFinal = jObjFinal[LIBATION_FILES_KEY].Value<string>();
            return valueFinal;
        }

        public void SetLibationFiles(string directory)
        {
            // ensure exists
            if (!File.Exists(APPSETTINGS_JSON))
            {
                // getter creates new file, loads PersistentDictionary
                var _ = LibationFiles;
                System.Threading.Thread.Sleep(100);
            }

            libationFilesPathCache = null;

            var startingContents = File.ReadAllText(APPSETTINGS_JSON);
            var jObj = JObject.Parse(startingContents);

            jObj[LIBATION_FILES_KEY] = directory;

            var endingContents = JsonConvert.SerializeObject(jObj, Formatting.Indented);
            if (startingContents == endingContents)
                return;

            // now it's set in the file again but no settings have moved yet
            File.WriteAllText(APPSETTINGS_JSON, endingContents);

            try
            {
                Log.Logger.Information("Libation files changed {@DebugInfo}", new { APPSETTINGS_JSON, LIBATION_FILES_KEY, directory });
            }
            catch { }
        }
    }
}
