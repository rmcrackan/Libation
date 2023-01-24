using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using FileManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LibationFileManager
{
	public partial class Configuration
	{
		// note: any potential file manager static ctors can't compensate if storage dir is changed at run time via settings. this is partly bad architecture. but the side effect is desirable. if changing LibationFiles location: restart app

		// default setting and directory creation occur in class responsible for files.
		// config class is only responsible for path. not responsible for setting defaults, dir validation, or dir creation
		// exceptions: appsettings.json, LibationFiles dir, Settings.json

		private PersistentDictionary persistentDictionary;

		public T GetNonString<T>(T defaultValue, [CallerMemberName] string propertyName = "") => persistentDictionary.GetNonString(propertyName, defaultValue);
		public object GetObject([CallerMemberName] string propertyName = "") => persistentDictionary.GetObject(propertyName);
		public string GetString(string defaultValue = null, [CallerMemberName] string propertyName = "") => persistentDictionary.GetString(propertyName, defaultValue);
		public void SetNonString(object newValue, [CallerMemberName] string propertyName = "")
		{
			var existing = getExistingValue(propertyName);
			if (existing?.Equals(newValue) is true) return;

			OnPropertyChanging(propertyName, existing, newValue);
			persistentDictionary.SetNonString(propertyName, newValue);
			OnPropertyChanged(propertyName, newValue);
		}

		public void SetString(string newValue, [CallerMemberName] string propertyName = "")
		{
			var existing = getExistingValue(propertyName);
			if (existing?.Equals(newValue) is true) return;

			OnPropertyChanging(propertyName, existing, newValue);
			persistentDictionary.SetString(propertyName, newValue);
			OnPropertyChanged(propertyName, newValue);
		}

		private object getExistingValue(string propertyName)
		{
			var property = GetType().GetProperty(propertyName);
			if (property is not null) return property.GetValue(this);
			return GetObject(propertyName);
		}

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
		public bool UseCoverAsFolderIcon { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Use the beta version of Libation\r\nNew and experimental features, but probably buggy.\r\n(requires restart to take effect)")]
		public bool BetaOptIn { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Location for book storage. Includes destination of newly liberated books")]
		public string Books { get => GetString(); set => SetString(value); }

		// temp/working dir(s) should be outside of dropbox
		[Description("Temporary location of files while they're in process of being downloaded and decrypted.\r\nWhen decryption is complete, the final file will be in Books location\r\nRecommend not using a folder which is backed up real time. Eg: Dropbox, iCloud, Google Drive")]
		public string InProgress { get
			{
				var tempDir = GetString();
				return string.IsNullOrWhiteSpace(tempDir) ? WinTemp : tempDir;
			}
			set => SetString(value); }

		[Description("Allow Libation to fix up audiobook metadata")]
		public bool AllowLibationFixup { get => GetNonString(defaultValue: true); set => SetNonString(value); }

		[Description("Create a cue sheet (.cue)")]
		public bool CreateCueSheet { get => GetNonString(defaultValue: true); set => SetNonString(value); }

		[Description("Retain the Aax file after successfully decrypting")]
		public bool RetainAaxFile { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Split my books into multiple files by chapter")]
		public bool SplitFilesByChapter { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Merge Opening/End Credits into the following/preceding chapters")]
		public bool MergeOpeningAndEndCredits { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Strip \"(Unabridged)\" from audiobook metadata tags")]
		public bool StripUnabridged { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Strip audible branding from the start and end of audiobooks.\r\n(e.g. \"This is Audible\")")]
		public bool StripAudibleBrandAudio { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Decrypt to lossy format?")]
		public bool DecryptToLossy { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Move the mp4 moov atom to the beginning of the file?")]
		public bool MoveMoovToBeginning { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Lame encoder target. true = Bitrate, false = Quality")]
		public bool LameTargetBitrate { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Lame encoder downsamples to mono")]
		public bool LameDownsampleMono {  get => GetNonString(defaultValue: true); set => SetNonString(value); }

		[Description("Lame target bitrate [16,320]")]
		public int LameBitrate {  get => GetNonString(defaultValue: 64); set => SetNonString(value); }

		[Description("Restrict encoder to constant bitrate?")]
		public bool LameConstantBitrate { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Match the source bitrate?")]
		public bool LameMatchSourceBR { get => GetNonString(defaultValue: true); set => SetNonString(value); }

		[Description("Lame target VBR quality [10,100]")]
		public int LameVBRQuality { get => GetNonString(defaultValue: 2); set => SetNonString(value); }

		[Description("A Dictionary of GridView data property names and bool indicating its column's visibility in ProductsGrid")]
		public Dictionary<string, bool> GridColumnsVisibilities { get => GetNonString(defaultValue: new EquatableDictionary<string, bool>()).Clone(); set => SetNonString(value); }

		[Description("A Dictionary of GridView data property names and int indicating its column's display index in ProductsGrid")]
		public Dictionary<string, int> GridColumnsDisplayIndices { get => GetNonString(defaultValue: new EquatableDictionary<string, int>()).Clone(); set => SetNonString(value); }

		[Description("A Dictionary of GridView data property names and int indicating its column's width in ProductsGrid")]
		public Dictionary<string, int> GridColumnsWidths { get => GetNonString(defaultValue: new EquatableDictionary<string, int>()).Clone(); set => SetNonString(value); }

		[Description("Save cover image alongside audiobook?")]
		public bool DownloadCoverArt { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Download clips and bookmarks?")]
		public bool DownloadClipsBookmarks { get => GetNonString(defaultValue: false); set => SetNonString(value); }
		
		[Description("File format to save clips and bookmarks")]
		public ClipBookmarkFormat ClipsBookmarksFileFormat { get => GetNonString(defaultValue: ClipBookmarkFormat.CSV); set => SetNonString(value); }

		[JsonConverter(typeof(StringEnumConverter))]
		public enum ClipBookmarkFormat
		{
			[Description("Comma-separated values")]
			CSV,
			[Description("Microsoft Excel Spreadsheet")]
			Xlsx,
			[Description("JavaScript Object Notation (JSON)")]
			Json
		}

		[JsonConverter(typeof(StringEnumConverter))]
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
		public BadBookAction BadBook { get => GetNonString(defaultValue: BadBookAction.Ask); set => SetNonString(value); }

		[Description("Show number of newly imported titles? When unchecked, no pop-up will appear after library scan.")]
		public bool ShowImportedStats { get => GetNonString(defaultValue: true); set => SetNonString(value); }

		[Description("Import episodes? (eg: podcasts) When unchecked, episodes will not be imported into Libation.")]
		public bool ImportEpisodes { get => GetNonString(defaultValue: true); set => SetNonString(value); }

		[Description("Download episodes? (eg: podcasts). When unchecked, episodes already in Libation will not be downloaded.")]
		public bool DownloadEpisodes { get => GetNonString(defaultValue: true); set => SetNonString(value); }

		[Description("Automatically run periodic scans in the background?")]
		public bool AutoScan { get => GetNonString(defaultValue: true); set => SetNonString(value); }

		[Description("Auto download books? After scan, download new books in 'checked' accounts.")]
		// poorly named setting. Should just be 'AutoDownload'. It is NOT episode specific
		public bool AutoDownloadEpisodes { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Save all podcast episodes in a series to the series parent folder?")]
		public bool SavePodcastsToParentFolder { get => GetNonString(defaultValue: false); set => SetNonString(value); }

		[Description("Global download speed limit in bytes per second.")]
		public long DownloadSpeedLimit
		{
			get
			{
				var limit = GetNonString(defaultValue: 0L);
				return limit <= 0 ? 0 : Math.Max(limit, AaxDecrypter.NetworkFileStream.MIN_BYTES_PER_SECOND);
			}
			set
			{
				var limit = value <= 0 ? 0 : Math.Max(value, AaxDecrypter.NetworkFileStream.MIN_BYTES_PER_SECOND);
				SetNonString(limit);
			}
		}

		#region templates: custom file naming

		[Description("Edit how filename characters are replaced")]
		public ReplacementCharacters ReplacementCharacters { get => GetNonString(defaultValue: ReplacementCharacters.Default); set => SetNonString(value); }

		[Description("How to format the folders in which files will be saved")]
		public string FolderTemplate
		{
			get => Templates.Folder.GetValid(GetString(defaultValue: Templates.Folder.DefaultTemplate));
			set => setTemplate(Templates.Folder, value);
		}

		[Description("How to format the saved pdf and audio files")]
		public string FileTemplate
		{
			get => Templates.File.GetValid(GetString(defaultValue: Templates.File.DefaultTemplate));
			set => setTemplate(Templates.File, value);
		}

		[Description("How to format the saved audio files when split by chapters")]
		public string ChapterFileTemplate
		{
			get => Templates.ChapterFile.GetValid(GetString(defaultValue: Templates.ChapterFile.DefaultTemplate));
			set => setTemplate(Templates.ChapterFile, value);
		}

		[Description("How to format the file's Tile stored in metadata")]
		public string ChapterTitleTemplate
		{
			get => Templates.ChapterTitle.GetValid(GetString(defaultValue: Templates.ChapterTitle.DefaultTemplate));
			set => setTemplate(Templates.ChapterTitle, value);
		}

		private void setTemplate(Templates templ, string newValue, [CallerMemberName] string propertyName = "")
		{
			var template = newValue?.Trim();
			if (templ.IsValid(template))
				SetString(template, propertyName);
		}
		#endregion
	}
}
