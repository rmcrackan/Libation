using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Dinah.Core;
using Dinah.Core.Logging;
using FileManager;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;

namespace LibationFileManager
{
    public class Configuration
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

            if (string.IsNullOrWhiteSpace(pDic.GetString(nameof(InProgress))))
                return false;

            return true;
        }

        #region persistent configuration settings/values

        // note: any potential file manager static ctors can't compensate if storage dir is changed at run time via settings. this is partly bad architecture. but the side effect is desirable. if changing LibationFiles location: restart app

        // default setting and directory creation occur in class responsible for files.
        // config class is only responsible for path. not responsible for setting defaults, dir validation, or dir creation
        // exceptions: appsettings.json, LibationFiles dir, Settings.json

        private PersistentDictionary persistentDictionary;

        public T GetNonString<T>(string propertyName) => persistentDictionary.GetNonString<T>(propertyName);
        public object GetObject(string propertyName) => persistentDictionary.GetObject(propertyName);
        public void SetObject(string propertyName, object newValue) => persistentDictionary.SetNonString(propertyName, newValue);

        /// <summary>WILL ONLY set if already present. WILL NOT create new</summary>
        public void SetWithJsonPath(string jsonPath, string propertyName, string newValue, bool suppressLogging = false)
        {
            var settingWasChanged = persistentDictionary.SetWithJsonPath(jsonPath, propertyName, newValue, suppressLogging);
            if (settingWasChanged)
                configuration?.Reload();
        }

        public string SettingsFilePath => Path.Combine(LibationFiles, "Settings.json");

        public static string GetDescription(string propertyName)
        {
            var attribute = typeof(Configuration)
                .GetProperty(propertyName)
                ?.GetCustomAttributes(typeof(DescriptionAttribute), true)
                .SingleOrDefault()
                as DescriptionAttribute;

            return attribute?.Description;
        }

        public bool Exists(string propertyName) => persistentDictionary.Exists(propertyName);       

        [Description("Location for book storage. Includes destination of newly liberated books")]
        public string Books
        {
            get => persistentDictionary.GetString(nameof(Books));
            set => persistentDictionary.SetString(nameof(Books), value);
        }

        // temp/working dir(s) should be outside of dropbox
        [Description("Temporary location of files while they're in process of being downloaded and decrypted.\r\nWhen decryption is complete, the final file will be in Books location\r\nRecommend not using a folder which is backed up real time. Eg: Dropbox, iCloud, Google Drive")]
        public string InProgress
        {
            get => persistentDictionary.GetString(nameof(InProgress));
            set => persistentDictionary.SetString(nameof(InProgress), value);
        }

        [Description("Allow Libation to fix up audiobook metadata")]
        public bool AllowLibationFixup
        {
            get => persistentDictionary.GetNonString<bool>(nameof(AllowLibationFixup));
            set => persistentDictionary.SetNonString(nameof(AllowLibationFixup), value);
        }

        [Description("Allow Libation to remove audible branding from the start\r\nand end of audiobooks. (e.g. \"This is Audible\")")]
        public bool StripAudibleBrandAudio
        {
            get => persistentDictionary.GetNonString<bool>(nameof(StripAudibleBrandAudio));
            set => persistentDictionary.SetNonString(nameof(StripAudibleBrandAudio), value);
        }

        [Description("Decrypt to lossy format?")]
        public bool DecryptToLossy
        {
            get => persistentDictionary.GetNonString<bool>(nameof(DecryptToLossy));
            set => persistentDictionary.SetNonString(nameof(DecryptToLossy), value);
        }
        
        [Description("Split my books into multiple files by chapter")]
        public bool SplitFilesByChapter
        {
            get => persistentDictionary.GetNonString<bool>(nameof(SplitFilesByChapter));
            set => persistentDictionary.SetNonString(nameof(SplitFilesByChapter), value);
        }

        public enum BadBookAction
        {
            [Description("Ask each time what action to take.")]
            Ask = 0,
            [Description("Stop processing books.")]
            Abort = 1,
            [Description("Retry book later. Skip for now. Continue processing books.")]
            Retry = 2,
            [Description("Permanently ignore book. Continue processing books. Do not try book again.")]
            Ignore = 3
        }

        [Description("When liberating books and there is an error, Libation should:")]
        public BadBookAction BadBook
        {
            get
            {
                var badBookStr = persistentDictionary.GetString(nameof(BadBook));
				return Enum.TryParse<BadBookAction>(badBookStr, out var badBookEnum) ? badBookEnum : BadBookAction.Ask;
            }
            set => persistentDictionary.SetString(nameof(BadBook), value.ToString());
        }

        [Description("Show number of newly imported titles? When unchecked, no pop-up will appear after library scan.")]
        public bool ShowImportedStats
        {
            get => persistentDictionary.GetNonString<bool>(nameof(ShowImportedStats));
            set => persistentDictionary.SetNonString(nameof(ShowImportedStats), value);
        }

        [Description("Import episodes? (eg: podcasts) When unchecked, episodes will not be imported into Libation.")]
        public bool ImportEpisodes
        {
            get => persistentDictionary.GetNonString<bool>(nameof(ImportEpisodes));
            set => persistentDictionary.SetNonString(nameof(ImportEpisodes), value);
        }

        [Description("Download episodes? (eg: podcasts). When unchecked, episodes already in Libation will not be downloaded.")]
        public bool DownloadEpisodes
        {
            get => persistentDictionary.GetNonString<bool>(nameof(DownloadEpisodes));
            set => persistentDictionary.SetNonString(nameof(DownloadEpisodes), value);
		}

        #region templates: custom file naming

        [Description("How to format the folders in which files will be saved")]
		public string FolderTemplate
        {
            get => getTemplate(nameof(FolderTemplate), Templates.Folder);
            set => setTemplate(nameof(FolderTemplate), Templates.Folder, value);
        }

        [Description("How to format the saved pdf and audio files")]
		public string FileTemplate
        {
            get => getTemplate(nameof(FileTemplate), Templates.File);
            set => setTemplate(nameof(FileTemplate), Templates.File, value);
        }

        [Description("How to format the saved audio files when split by chapters")]
        public string ChapterFileTemplate
        {
            get => getTemplate(nameof(ChapterFileTemplate), Templates.ChapterFile);
            set => setTemplate(nameof(ChapterFileTemplate), Templates.ChapterFile, value);
        }

        private string getTemplate(string settingName, Templates templ) => templ.GetValid(persistentDictionary.GetString(settingName));
        private void setTemplate(string settingName, Templates templ, string newValue)
        {
            var template = newValue?.Trim();
            if (templ.IsValid(template))
                persistentDictionary.SetString(settingName, template);
        }
        #endregion

        #endregion

        #region known directories
        public static string AppDir_Relative => $@".\{LIBATION_FILES_KEY}";
        public static string AppDir_Absolute => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Exe.FileLocationOnDisk), LIBATION_FILES_KEY));
        public static string MyDocs => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Libation"));
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
            // this is important to not let very early calls try to accidentally load LibationFiles too early.
            // also, keep this at bottom of this list
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

            // 'First' instead of 'Single' because LibationFiles could match other directories. eg: default value of LibationFiles == UserProfile.
            // since it's a list, order matters and non-LibationFiles will be returned first
            var dirFunc = directoryOptionsPaths.FirstOrDefault(dirFunc => dirFunc.getPathFunc() == directory);
            return dirFunc == default ? KnownDirectories.None : dirFunc.directory;
        }
        #endregion

        #region logging
        private IConfigurationRoot configuration;

        public void ConfigureLogging()
        {
            configuration = new ConfigurationBuilder()
                .AddJsonFile(SettingsFilePath, optional: false, reloadOnChange: true)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        [Description("The importance of a log event")]
        public LogEventLevel LogLevel
        {
            get
            {
                var logLevelStr = persistentDictionary.GetStringFromJsonPath("Serilog", "MinimumLevel");
                return Enum.TryParse<LogEventLevel>(logLevelStr, out var logLevelEnum) ? logLevelEnum : LogEventLevel.Information;
            }
            set
            {
                var valueWasChanged = persistentDictionary.SetWithJsonPath("Serilog", "MinimumLevel", value.ToString());
                if (!valueWasChanged)
                {
                    Log.Logger.Debug("LogLevel.set attempt. No change");
                    return;
                }

                configuration.Reload();

                Log.Logger.Information("Updated LogLevel MinimumLevel. {@DebugInfo}", new
                {
                    LogLevel_Verbose_Enabled = Log.Logger.IsVerboseEnabled(),
                    LogLevel_Debug_Enabled = Log.Logger.IsDebugEnabled(),
                    LogLevel_Information_Enabled = Log.Logger.IsInformationEnabled(),
                    LogLevel_Warning_Enabled = Log.Logger.IsWarningEnabled(),
                    LogLevel_Error_Enabled = Log.Logger.IsErrorEnabled(),
                    LogLevel_Fatal_Enabled = Log.Logger.IsFatalEnabled()
                });
            }
        }
		#endregion

		#region singleton stuff
		public static Configuration Instance { get; } = new Configuration();
        private Configuration() { }
        #endregion

        #region LibationFiles
        private static string APPSETTINGS_JSON { get; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "appsettings.json");
        private const string LIBATION_FILES_KEY = "LibationFiles";

        [Description("Location for storage of program-created files")]
        public string LibationFiles
        {
            get
            {
                if (libationFilesPathCache is not null)
                    return libationFilesPathCache;

                // FIRST: must write here before SettingsFilePath in next step reads cache
                libationFilesPathCache = getLiberationFilesSettingFromJson();

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

        public void SetLibationFiles(string directory)
        {
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
        #endregion
    }
}
