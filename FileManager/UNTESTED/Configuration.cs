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

        public bool IsComplete
            => File.Exists(APPSETTINGS_JSON)
            && Directory.Exists(LibationFiles)
            && Directory.Exists(Books)
            && File.Exists(SettingsJsonPath)
            && !string.IsNullOrWhiteSpace(LocaleCountryCode)
            && !string.IsNullOrWhiteSpace(DownloadsInProgressEnum)
            && !string.IsNullOrWhiteSpace(DecryptInProgressEnum);

        [Description("Your user-specific key used to decrypt your audible files (*.aax) into audio files you can use anywhere (*.m4b). Leave alone in most cases")]
        public string DecryptKey
        {
            get => persistentDictionary[nameof(DecryptKey)];
            set => persistentDictionary[nameof(DecryptKey)] = value;
        }

        [Description("Location for book storage. Includes destination of newly liberated books")]
        public string Books
        {
            get => persistentDictionary[nameof(Books)];
            set => persistentDictionary[nameof(Books)] = value;
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
        private Dictionary<string, string> cache { get; } = new Dictionary<string, string>();

        // default setting and directory creation occur in class responsible for files.
        // config class is only responsible for path. not responsible for setting defaults, dir validation, or dir creation
        // exceptions: appsettings.json, LibationFiles dir, Settings.json

        // temp/working dir(s) should be outside of dropbox
        [Description("Temporary location of files while they're in process of being downloaded.\r\nWhen download is complete, the final file will be in [LibationFiles]\\DownloadsFinal")]
        public string DownloadsInProgressEnum
        {
            get => persistentDictionary[nameof(DownloadsInProgressEnum)];
            set => persistentDictionary[nameof(DownloadsInProgressEnum)] = value;
        }

        // temp/working dir(s) should be outside of dropbox
        [Description("Temporary location of files while they're in process of being decrypted.\r\nWhen decryption is complete, the final file will be in Books location")]
        public string DecryptInProgressEnum
        {
            get => persistentDictionary[nameof(DecryptInProgressEnum)];
            set => persistentDictionary[nameof(DecryptInProgressEnum)] = value;
        }

        public string LocaleCountryCode
        {
            get => persistentDictionary[nameof(LocaleCountryCode)];
            set => persistentDictionary[nameof(LocaleCountryCode)] = value;
        }

        // note: any potential file manager static ctors can't compensate if storage dir is changed at run time via settings. this is partly bad architecture. but the side effect is desirable. if changing LibationFiles location: restart app

        // singleton stuff
        public static Configuration Instance { get; } = new Configuration();
        private Configuration() { }

        private const string APPSETTINGS_JSON = "appsettings.json";
        private const string LIBATION_FILES = "LibationFiles";

        [Description("Location for storage of program-created files")]
        public string LibationFiles
            => cache.ContainsKey(LIBATION_FILES)
            ? cache[LIBATION_FILES]
            : getLibationFiles();
        private string getLibationFiles()
        {
            var value = getLiberationFilesSettingFromJson();

            if (wellKnownPaths.ContainsKey(value))
                value = wellKnownPaths[value];

            // must write here before SettingsJsonPath in next step tries to read from dictionary
            cache[LIBATION_FILES] = value;

            // load json values into memory. create if not exists
            persistentDictionary = new PersistentDictionary(SettingsJsonPath);
            persistentDictionary.EnsureEntries<Configuration>();

            return value;
        }
        private string getLiberationFilesSettingFromJson()
        {
            static string createSettingsJson()
            {
                var dir = APP_DIR;
                File.WriteAllText(APPSETTINGS_JSON, new JObject { { LIBATION_FILES, dir } }.ToString(Formatting.Indented));
                return dir;
            }

            if (!File.Exists(APPSETTINGS_JSON))
                return createSettingsJson();

            var appSettingsContents = File.ReadAllText(APPSETTINGS_JSON);

            JObject jObj;
            try
            {
                jObj = JObject.Parse(appSettingsContents);
            }
            catch
            {
                return createSettingsJson();
            }

            if (!jObj.ContainsKey(LIBATION_FILES))
                return createSettingsJson();

            var value = jObj[LIBATION_FILES].Value<string>();
            return value;
        }

        private string SettingsJsonPath => Path.Combine(LibationFiles, "Settings.json");

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
                File.Delete(SettingsJsonPath);
                System.Threading.Thread.Sleep(100);
                if (!Directory.EnumerateDirectories(AppDir).Any() && !Directory.EnumerateFiles(AppDir).Any())
                    Directory.Delete(AppDir);
            }


            cache.Remove(LIBATION_FILES);


            var contents = File.ReadAllText(APPSETTINGS_JSON);
            var jObj = JObject.Parse(contents);

            jObj[LIBATION_FILES] = directory;

            var output = JsonConvert.SerializeObject(jObj, Formatting.Indented);
            File.WriteAllText(APPSETTINGS_JSON, output);


            return true;
        }
    }
}
