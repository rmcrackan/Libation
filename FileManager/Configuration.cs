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

		#region known directories
        public static string AppDir_Relative => @".\LibationFiles";
        public static string AppDir_Absolute => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Exe.FileLocationOnDisk), LIBATION_FILES_KEY));
        public static string MyDocs => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LibationFiles"));
        public static string WinTemp => Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Libation"));
        public static string UserProfile => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Libation"));

        public enum KnownDirectories
        {
            None = 0,

            [Description("My Users folder")]
            UserProfile = 1,

            [Description("The same folder that Libation is running from")]
            AppDir = 2,

            [Description("Windows temporary folder")]
            WinTemp = 3,

            [Description("My Documents")]
            MyDocs = 4,

            [Description("Your settings folder (aka: Libation Files)")]
            LibationFiles = 5
        }
        // use func calls so we always get the latest value of LibationFiles
        private static List<(KnownDirectories directory, Func<string> getPathFunc)> directoryOptionsPaths { get; } = new()
        {
            (KnownDirectories.None, () => null),
            (KnownDirectories.UserProfile, () => UserProfile),
            (KnownDirectories.AppDir, () => AppDir_Relative),
            (KnownDirectories.WinTemp, () => WinTemp),
            (KnownDirectories.MyDocs, () => MyDocs),
            // this is important to not let very early calls try to accidentally load LibationFiles too early
            (KnownDirectories.LibationFiles, () => libationFilesPathCache)
        };
        public static string GetKnownDirectoryPath(KnownDirectories directory)
        {
            var dirFunc = directoryOptionsPaths.SingleOrDefault(dirFunc => dirFunc.directory == directory);
            return dirFunc == default ? null : dirFunc.getPathFunc();
        }
        public static KnownDirectories GetKnownDirectory(string directory)
        {
            // especially important so a very early call doesn't match null => LibationFiles
            if (string.IsNullOrWhiteSpace(directory))
                return KnownDirectories.None;

            var dirFunc = directoryOptionsPaths.SingleOrDefault(dirFunc => dirFunc.getPathFunc() == directory);
            return dirFunc == default ? KnownDirectories.None : dirFunc.directory;
        }
        #endregion

        // default setting and directory creation occur in class responsible for files.
        // config class is only responsible for path. not responsible for setting defaults, dir validation, or dir creation
        // exceptions: appsettings.json, LibationFiles dir, Settings.json

        // temp/working dir(s) should be outside of dropbox
        [Description("Temporary location of files while they're in process of being downloaded and decrypted.\r\nWhen decryption is complete, the final file will be in Books location\r\nRecommend not using a folder which is backed up real time. Eg: Dropbox, iCloud, Google Drive")]
        public string InProgress
        {
            get => persistentDictionary.GetString(nameof(InProgress));
            set => persistentDictionary.Set(nameof(InProgress), value);
        }

        [Description("Allow Libation for fix up audiobook metadata?")]
        public bool AllowLibationFixup
        {
            get => persistentDictionary.Get<bool>(nameof(AllowLibationFixup));
            set => persistentDictionary.Set(nameof(AllowLibationFixup), value);
        }
        // note: any potential file manager static ctors can't compensate if storage dir is changed at run time via settings. this is partly bad architecture. but the side effect is desirable. if changing LibationFiles location: restart app

        // singleton stuff
        public static Configuration Instance { get; } = new Configuration();
        private Configuration() { }

        private const string APPSETTINGS_JSON = "appsettings.json";
        private const string LIBATION_FILES_KEY = "LibationFiles";

        [Description("Location for storage of program-created files")]
        public string LibationFiles
        {
            get
            {
                if (libationFilesPathCache is not null)
                    return libationFilesPathCache;

                // must write here before SettingsFilePath in next step reads cache
                libationFilesPathCache = getLiberationFilesSettingFromJson();

                // load json values into memory. create settings if not exists
                persistentDictionary = new PersistentDictionary(SettingsFilePath);

                return libationFilesPathCache;
            }
        }

        private static string libationFilesPathCache;

        private string getLiberationFilesSettingFromJson()
        {
            string startingContents = null;
            try
            {
                if (File.Exists(APPSETTINGS_JSON))
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
            }
            catch { }

            // not found. write to file. read from file
            var endingContents = new JObject { { LIBATION_FILES_KEY, UserProfile } }.ToString(Formatting.Indented);
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
            // this is WRONG. need to MOVE settings; not DELETE them

            //// if moving from default, delete old settings file and dir (if empty)
            //if (LibationFiles.EqualsInsensitive(AppDir))
            //{
            //    File.Delete(SettingsFilePath);
            //    System.Threading.Thread.Sleep(100);
            //    if (!Directory.EnumerateDirectories(AppDir).Any() && !Directory.EnumerateFiles(AppDir).Any())
            //        Directory.Delete(AppDir);
            //}


            libationFilesPathCache = null;


            var startingContents = File.ReadAllText(APPSETTINGS_JSON);
            var jObj = JObject.Parse(startingContents);

            jObj[LIBATION_FILES_KEY] = directory;

            var endingContents = JsonConvert.SerializeObject(jObj, Formatting.Indented);
            if (startingContents != endingContents)
                File.WriteAllText(APPSETTINGS_JSON, endingContents);


            return true;
        }
    }
}
