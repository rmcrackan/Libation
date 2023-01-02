using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using FileManager;

namespace LibationFileManager
{
    public partial class Configuration
    {
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

        [Description("Set cover art as the folder's icon. (Windows only)")]
        public bool UseCoverAsFolderIcon
        {
            get => persistentDictionary.GetNonString<bool>(nameof(UseCoverAsFolderIcon));
            set => persistentDictionary.SetNonString(nameof(UseCoverAsFolderIcon), value);
        }

        [Description("Use the beta version of Libation\r\nNew and experimental features, but probably buggy.\r\n(requires restart to take effect)")]
        public bool BetaOptIn
        {
            get => persistentDictionary.GetNonString<bool>(nameof(BetaOptIn));
            set => persistentDictionary.SetNonString(nameof(BetaOptIn), value);
        }

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

        [Description("Create a cue sheet (.cue)")]
        public bool CreateCueSheet
        {
            get => persistentDictionary.GetNonString<bool>(nameof(CreateCueSheet));
            set => persistentDictionary.SetNonString(nameof(CreateCueSheet), value);
        }

        [Description("Retain the Aax file after successfully decrypting")]
        public bool RetainAaxFile
        {
            get => persistentDictionary.GetNonString<bool>(nameof(RetainAaxFile));
            set => persistentDictionary.SetNonString(nameof(RetainAaxFile), value);
        }

        [Description("Split my books into multiple files by chapter")]
        public bool SplitFilesByChapter
        {
            get => persistentDictionary.GetNonString<bool>(nameof(SplitFilesByChapter));
            set => persistentDictionary.SetNonString(nameof(SplitFilesByChapter), value);
        }

        [Description("Merge Opening/End Credits into the following/preceding chapters")]
        public bool MergeOpeningAndEndCredits
        {
            get => persistentDictionary.GetNonString<bool>(nameof(MergeOpeningAndEndCredits));
            set => persistentDictionary.SetNonString(nameof(MergeOpeningAndEndCredits), value);
        }

        [Description("Strip \"(Unabridged)\" from audiobook metadata tags")]
        public bool StripUnabridged
        {
            get => persistentDictionary.GetNonString<bool>(nameof(StripUnabridged));
            set => persistentDictionary.SetNonString(nameof(StripUnabridged), value);
        }

        [Description("Strip audible branding from the start and end of audiobooks.\r\n(e.g. \"This is Audible\")")]
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

        [Description("Lame encoder target. true = Bitrate, false = Quality")]
        public bool LameTargetBitrate
        {
            get => persistentDictionary.GetNonString<bool>(nameof(LameTargetBitrate));
            set => persistentDictionary.SetNonString(nameof(LameTargetBitrate), value);
        }

        [Description("Lame encoder downsamples to mono")]
        public bool LameDownsampleMono
        {
            get => persistentDictionary.GetNonString<bool>(nameof(LameDownsampleMono));
            set => persistentDictionary.SetNonString(nameof(LameDownsampleMono), value);
        }

        [Description("Lame target bitrate [16,320]")]
        public int LameBitrate
        {
            get => persistentDictionary.GetNonString<int>(nameof(LameBitrate));
            set => persistentDictionary.SetNonString(nameof(LameBitrate), value);
        }

        [Description("Restrict encoder to constant bitrate?")]
        public bool LameConstantBitrate
        {
            get => persistentDictionary.GetNonString<bool>(nameof(LameConstantBitrate));
            set => persistentDictionary.SetNonString(nameof(LameConstantBitrate), value);
        }

        [Description("Match the source bitrate?")]
        public bool LameMatchSourceBR
        {
            get => persistentDictionary.GetNonString<bool>(nameof(LameMatchSourceBR));
            set => persistentDictionary.SetNonString(nameof(LameMatchSourceBR), value);
        }

        [Description("Lame target VBR quality [10,100]")]
        public int LameVBRQuality
        {
            get => persistentDictionary.GetNonString<int>(nameof(LameVBRQuality));
            set => persistentDictionary.SetNonString(nameof(LameVBRQuality), value);
        }

        [Description("A Dictionary of GridView data property names and bool indicating its column's visibility in ProductsGrid")]
        public Dictionary<string, bool> GridColumnsVisibilities
        {
            get => persistentDictionary.GetNonString<Dictionary<string, bool>>(nameof(GridColumnsVisibilities));
            set => persistentDictionary.SetNonString(nameof(GridColumnsVisibilities), value);
        }

        [Description("A Dictionary of GridView data property names and int indicating its column's display index in ProductsGrid")]
        public Dictionary<string, int> GridColumnsDisplayIndices
        {
            get => persistentDictionary.GetNonString<Dictionary<string, int>>(nameof(GridColumnsDisplayIndices));
            set => persistentDictionary.SetNonString(nameof(GridColumnsDisplayIndices), value);
        }

        [Description("A Dictionary of GridView data property names and int indicating its column's width in ProductsGrid")]
        public Dictionary<string, int> GridColumnsWidths
        {
            get => persistentDictionary.GetNonString<Dictionary<string, int>>(nameof(GridColumnsWidths));
            set => persistentDictionary.SetNonString(nameof(GridColumnsWidths), value);
        }

        [Description("Save cover image alongside audiobook?")]
        public bool DownloadCoverArt
        {
            get => persistentDictionary.GetNonString<bool>(nameof(DownloadCoverArt));
            set => persistentDictionary.SetNonString(nameof(DownloadCoverArt), value);
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

        public event EventHandler AutoScanChanged;

        [Description("Automatically run periodic scans in the background?")]
        public bool AutoScan
        {
            get => persistentDictionary.GetNonString<bool>(nameof(AutoScan));
            set
            {
                if (AutoScan != value)
                {
                    persistentDictionary.SetNonString(nameof(AutoScan), value);
                    AutoScanChanged?.Invoke(null, null);
                }
            }
        }

        [Description("Auto download books? After scan, download new books in 'checked' accounts.")]
        // poorly named setting. Should just be 'AutoDownload'. It is NOT episode specific
        public bool AutoDownloadEpisodes
        {
            get => persistentDictionary.GetNonString<bool>(nameof(AutoDownloadEpisodes));
            set => persistentDictionary.SetNonString(nameof(AutoDownloadEpisodes), value);
        }

        [Description("Save all podcast episodes in a series to the series parent folder?")]
        public bool SavePodcastsToParentFolder
        {
            get => persistentDictionary.GetNonString<bool>(nameof(SavePodcastsToParentFolder));
            set => persistentDictionary.SetNonString(nameof(SavePodcastsToParentFolder), value);
        }

		[Description("Global download speed limit in bytes per second.")]
		public long DownloadSpeedLimit
		{
            get
            {
                AaxDecrypter.NetworkFileStream.GlobalSpeedLimit = persistentDictionary.GetNonString<long>(nameof(DownloadSpeedLimit));
                return AaxDecrypter.NetworkFileStream.GlobalSpeedLimit;
			}
            set
            {
                AaxDecrypter.NetworkFileStream.GlobalSpeedLimit = value;
				persistentDictionary.SetNonString(nameof(DownloadSpeedLimit), AaxDecrypter.NetworkFileStream.GlobalSpeedLimit);
            }
		}

		#region templates: custom file naming

		[Description("Edit how filename characters are replaced")]
        public ReplacementCharacters ReplacementCharacters
        {
            get => persistentDictionary.GetNonString<ReplacementCharacters>(nameof(ReplacementCharacters));
            set => persistentDictionary.SetNonString(nameof(ReplacementCharacters), value);
        }

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

        [Description("How to format the file's Tile stored in metadata")]
        public string ChapterTitleTemplate
        {
            get => getTemplate(nameof(ChapterTitleTemplate), Templates.ChapterTitle);
            set => setTemplate(nameof(ChapterTitleTemplate), Templates.ChapterTitle, value);
        }

        private string getTemplate(string settingName, Templates templ) => templ.GetValid(persistentDictionary.GetString(settingName));
        private void setTemplate(string settingName, Templates templ, string newValue)
        {
            var template = newValue?.Trim();
            if (templ.IsValid(template))
                persistentDictionary.SetString(settingName, template);
        }
        #endregion
    }
}
