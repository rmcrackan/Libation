using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FileManager
{
    public class Configuration
    {
        // settings will be persisted when all are true
        // - property (not field)
        // - string
        // - public getter
        // - public setter

        #region // properties to test reflection
        /*
        // field should NOT be populated
        public string TestField;
        // int should NOT be populated
        public int TestInt { get; set; }
        // read-only should NOT be populated
        public string TestGet { get; } // get only: should NOT get auto-populated
        // set-only should NOT be populated
        public string TestSet { private get; set; }

        // get and set: SHOULD be auto-populated
        public string TestGetSet { get; set; }
        */
        #endregion

        private PersistentDictionary persistentDictionary;

        public bool FilesExist
            => File.Exists(APPSETTINGS_JSON)
            && File.Exists(SettingsFilePath)
            && Directory.Exists(LibationFiles)
            && Directory.Exists(Books);

        public string SettingsFilePath => Path.Combine(LibationFiles, "Settings.json");

        [Description("Location for book storage. Includes destination of newly liberated books")]
        public string Books
        {
            get => persistentDictionary.GetString(nameof(Books));
            set => persistentDictionary.Set(nameof(Books), value);
        }

        private const string APP_DIR = "AppDir";
        public static string AppDir { get; } = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Exe.FileLocationOnDisk), LIBATION_FILES));
        public static string MyDocs { get; } = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), LIBATION_FILES));
        public static string WinTemp { get; } = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Libation"));

        private Dictionary<string, string> wellKnownPaths { get; } = new Dictionary<string, string>
        {
            [APP_DIR] = AppDir,
            ["MyDocs"] = MyDocs,
            ["WinTemp"] = WinTemp
        };
        private string libationFilesPathCache;

        // default setting and directory creation occur in class responsible for files.
        // config class is only responsible for path. not responsible for setting defaults, dir validation, or dir creation
        // exceptions: appsettings.json, LibationFiles dir, Settings.json

        // temp/working dir(s) should be outside of dropbox
        [Description("Temporary location of files while they're in process of being downloaded.\r\nWhen download is complete, the final file will be in [LibationFiles]\\DownloadsFinal")]
        public string DownloadsInProgressEnum
        {
            get => persistentDictionary.GetString(nameof(DownloadsInProgressEnum));
            set => persistentDictionary.Set(nameof(DownloadsInProgressEnum), value);
        }

        // temp/working dir(s) should be outside of dropbox
        [Description("Temporary location of files while they're in process of being decrypted.\r\nWhen decryption is complete, the final file will be in Books location")]
        public string DecryptInProgressEnum
        {
            get => persistentDictionary.GetString(nameof(DecryptInProgressEnum));
            set => persistentDictionary.Set(nameof(DecryptInProgressEnum), value);
        }

        [Description("Download chapter titles from Audible?")]
        public bool DownloadChapters
        {
            get => persistentDictionary.Get<bool>(nameof(DownloadChapters));
            set => persistentDictionary.Set(nameof(DownloadChapters), value);
        }
        // note: any potential file manager static ctors can't compensate if storage dir is changed at run time via settings. this is partly bad architecture. but the side effect is desirable. if changing LibationFiles location: restart app

        // singleton stuff
        public static Configuration Instance { get; } = new Configuration();
        private Configuration() { }

        private const string APPSETTINGS_JSON = "appsettings.json";
        private const string LIBATION_FILES = "LibationFiles";

        [Description("Location for storage of program-created files")]
        public string LibationFiles => libationFilesPathCache ?? getLibationFiles();
        private string getLibationFiles()
        {
            var value = getLiberationFilesSettingFromJson();

            // this looks weird but is correct for translating wellKnownPaths
            if (wellKnownPaths.ContainsKey(value))
                value = wellKnownPaths[value];

            // must write here before SettingsFilePath in next step reads cache
            libationFilesPathCache = value;

            // load json values into memory. create if not exists
            persistentDictionary = new PersistentDictionary(SettingsFilePath);

            return libationFilesPathCache;
        }
        private string getLiberationFilesSettingFromJson()
        {
            try
            {
                if (File.Exists(APPSETTINGS_JSON))
                {
                    var appSettingsContents = File.ReadAllText(APPSETTINGS_JSON);
                    var jObj = JObject.Parse(appSettingsContents);

                    if (jObj.ContainsKey(LIBATION_FILES))
                    {
                        var value = jObj[LIBATION_FILES].Value<string>();

                        // do not check whether directory exists. special/meta directory (eg: AppDir) is valid
                        if (!string.IsNullOrWhiteSpace(value))
                            return value;
                    }
                }
            }
            catch { }

            File.WriteAllText(APPSETTINGS_JSON, new JObject { { LIBATION_FILES, APP_DIR } }.ToString(Formatting.Indented));
            return APP_DIR;
        }

        public object GetObject(string propertyName) => persistentDictionary.GetObject(propertyName);
        public void SetObject(string propertyName, object newValue) => persistentDictionary.Set(propertyName, newValue);
        public void SetWithJsonPath(string jsonPath, string propertyName, string newValue) => persistentDictionary.SetWithJsonPath(jsonPath, propertyName, newValue);

        public static string GetDescription(string propertyName)
        {
            var attribute = typeof(Configuration)
                .GetProperty(propertyName)
                ?.GetCustomAttributes(typeof(DescriptionAttribute), true)
                .SingleOrDefault()
                as DescriptionAttribute;

            return attribute?.Description;
        }

        public bool TrySetLibationFiles(string directory)
        {
            if (!Directory.Exists(directory) && !wellKnownPaths.ContainsKey(directory))
                return false;

            // if moving from default, delete old settings file and dir (if empty)
            if (LibationFiles.EqualsInsensitive(AppDir))
            {
                File.Delete(SettingsFilePath);
                System.Threading.Thread.Sleep(100);
                if (!Directory.EnumerateDirectories(AppDir).Any() && !Directory.EnumerateFiles(AppDir).Any())
                    Directory.Delete(AppDir);
            }


            libationFilesPathCache = null;


            var contents = File.ReadAllText(APPSETTINGS_JSON);
            var jObj = JObject.Parse(contents);

            jObj[LIBATION_FILES] = directory;

            var output = JsonConvert.SerializeObject(jObj, Formatting.Indented);
            File.WriteAllText(APPSETTINGS_JSON, output);


            return true;
        }
    }
}
