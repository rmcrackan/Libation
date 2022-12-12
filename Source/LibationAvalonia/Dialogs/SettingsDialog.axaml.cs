using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using Dinah.Core;
using System.Linq;
using FileManager;

namespace LibationAvalonia.Dialogs
{
	public partial class SettingsDialog : DialogWindow
	{
		private SettingsPages settingsDisp;

		private readonly Configuration config = Configuration.Instance;
		public SettingsDialog()
		{
			if (Design.IsDesignMode)
				AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();
			InitializeComponent();

			DataContext = settingsDisp = new(config);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		protected override async Task SaveAndCloseAsync()
		{
			if (!await settingsDisp.SaveSettingsAsync(config))
				return;

			await MessageBox.VerboseLoggingWarning_ShowIfTrue();
			await base.SaveAndCloseAsync();
		}

		public async void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();

		public void OpenLogFolderButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			Go.To.Folder(((LongPath)Configuration.Instance.LibationFiles).ShortPathName);
		}

		public async void EditFolderTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = await editTemplate(Templates.Folder, settingsDisp.DownloadDecryptSettings.FolderTemplate);
			if (newTemplate is not null)
				settingsDisp.DownloadDecryptSettings.FolderTemplate = newTemplate;
		}

		public async void EditFileTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = await editTemplate(Templates.File, settingsDisp.DownloadDecryptSettings.FileTemplate);
			if (newTemplate is not null)
				settingsDisp.DownloadDecryptSettings.FileTemplate = newTemplate;
		}

		public async void EditChapterFileTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = await editTemplate(Templates.ChapterFile, settingsDisp.DownloadDecryptSettings.ChapterFileTemplate);
			if (newTemplate is not null)
				settingsDisp.DownloadDecryptSettings.ChapterFileTemplate = newTemplate;
		}

		public async void EditCharReplacementButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var form = new EditReplacementChars(config);
			await form.ShowDialog<DialogResult>(this);
		}

		public async void EditChapterTitleTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = await editTemplate(Templates.ChapterTitle, settingsDisp.AudioSettings.ChapterTitleTemplate);
			if (newTemplate is not null)
				settingsDisp.AudioSettings.ChapterTitleTemplate = newTemplate;
		}

		private async Task<string> editTemplate(Templates template, string existingTemplate)
		{
			var form = new EditTemplateDialog(template, existingTemplate);
			if (await form.ShowDialog<DialogResult>(this) == DialogResult.OK)
				return form.TemplateText;
			else return null;
		}
	}

	internal interface ISettingsDisplay
	{
		void LoadSettings(Configuration config);
		Task<bool> SaveSettingsAsync(Configuration config);
	}

	public class SettingsPages : ISettingsDisplay
	{
		public SettingsPages(Configuration config)
		{
			LoadSettings(config);
		}

		public bool IsWindows => AppScaffolding.LibationScaffolding.ReleaseIdentifier is AppScaffolding.ReleaseIdentifier.WindowsAvalonia;
		public ImportantSettings ImportantSettings { get; private set; }
		public ImportSettings ImportSettings { get; private set; }
		public DownloadDecryptSettings DownloadDecryptSettings { get; private set; }
		public AudioSettings AudioSettings { get; private set; }

		public void LoadSettings(Configuration config)
		{
			ImportantSettings = new(config);
			ImportSettings = new(config);
			DownloadDecryptSettings = new(config);
			AudioSettings = new(config);
		}

		public async Task<bool> SaveSettingsAsync(Configuration config)
		{
			var result = await ImportantSettings.SaveSettingsAsync(config);
			result &= await ImportSettings.SaveSettingsAsync(config);
			result &= await DownloadDecryptSettings.SaveSettingsAsync(config);
			result &= await AudioSettings.SaveSettingsAsync(config);

			return result;
		}
	}

	public class ImportantSettings : ISettingsDisplay
	{
		public ImportantSettings(Configuration config)
		{
			LoadSettings(config);
		}

		public void LoadSettings(Configuration config)
		{
			BooksDirectory = config.Books;
			SavePodcastsToParentFolder = config.SavePodcastsToParentFolder;
			LoggingLevel = config.LogLevel;
			BetaOptIn = config.BetaOptIn;
		}

		public async Task<bool> SaveSettingsAsync(Configuration config)
		{
			#region validation

			if (string.IsNullOrWhiteSpace(BooksDirectory))
			{
				await MessageBox.Show("Cannot set Books Location to blank", "Location is blank", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			#endregion

			LongPath lonNewBooks = BooksDirectory;
			if (!System.IO.Directory.Exists(lonNewBooks))
				System.IO.Directory.CreateDirectory(lonNewBooks);
			config.Books = BooksDirectory;
			config.SavePodcastsToParentFolder = SavePodcastsToParentFolder;
			config.LogLevel = LoggingLevel;
			config.BetaOptIn = BetaOptIn;

			return true;
		}


		public List<Configuration.KnownDirectories> KnownDirectories { get; } = new()
		{
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.MyDocs
		};

		public string BooksText { get; } = Configuration.GetDescription(nameof(Configuration.Books));
		public string SavePodcastsToParentFolderText { get; } = Configuration.GetDescription(nameof(Configuration.SavePodcastsToParentFolder));
		public Serilog.Events.LogEventLevel[] LoggingLevels { get; } = Enum.GetValues<Serilog.Events.LogEventLevel>();
		public string BetaOptInText { get; } = Configuration.GetDescription(nameof(Configuration.BetaOptIn));

		public string BooksDirectory { get; set; }
		public bool SavePodcastsToParentFolder { get; set; }
		public Serilog.Events.LogEventLevel LoggingLevel { get; set; }
		public bool BetaOptIn { get; set; }
	}

	public class ImportSettings : ISettingsDisplay
	{
		public ImportSettings(Configuration config)
		{
			LoadSettings(config);
		}

		public void LoadSettings(Configuration config)
		{
			AutoScan = config.AutoScan;
			ShowImportedStats = config.ShowImportedStats;
			ImportEpisodes = config.ImportEpisodes;
			DownloadEpisodes = config.DownloadEpisodes;
			AutoDownloadEpisodes = config.AutoDownloadEpisodes;
		}

		public Task<bool> SaveSettingsAsync(Configuration config)
		{
			config.AutoScan = AutoScan;
			config.ShowImportedStats = ShowImportedStats;
			config.ImportEpisodes = ImportEpisodes;
			config.DownloadEpisodes = DownloadEpisodes;
			config.AutoDownloadEpisodes = AutoDownloadEpisodes;
			return Task.FromResult(true);
		}

		public string AutoScanText { get; } = Configuration.GetDescription(nameof(Configuration.AutoScan));
		public string ShowImportedStatsText { get; } = Configuration.GetDescription(nameof(Configuration.ShowImportedStats));
		public string ImportEpisodesText { get; } = Configuration.GetDescription(nameof(Configuration.ImportEpisodes));
		public string DownloadEpisodesText { get; } = Configuration.GetDescription(nameof(Configuration.DownloadEpisodes));
		public string AutoDownloadEpisodesText { get; } = Configuration.GetDescription(nameof(Configuration.AutoDownloadEpisodes));

		public bool AutoScan { get; set; }
		public bool ShowImportedStats { get; set; }
		public bool ImportEpisodes { get; set; }
		public bool DownloadEpisodes { get; set; }
		public bool AutoDownloadEpisodes { get; set; }
	}
	
	public class DownloadDecryptSettings : ViewModels.ViewModelBase, ISettingsDisplay
	{

		private bool _badBookAsk;
		private bool _badBookAbort;
		private bool _badBookRetry;
		private bool _badBookIgnore;

		private string _folderTemplate;
		private string _fileTemplate;
		private string _chapterFileTemplate;

		public DownloadDecryptSettings(Configuration config)
		{
			LoadSettings(config);
		}

		public Configuration.KnownDirectories InProgressDirectory { get; set; }
		public void LoadSettings(Configuration config)
		{
			BadBookAsk = config.BadBook is Configuration.BadBookAction.Ask;
			BadBookAbort = config.BadBook is Configuration.BadBookAction.Abort;
			BadBookRetry = config.BadBook is Configuration.BadBookAction.Retry;
			BadBookIgnore = config.BadBook is Configuration.BadBookAction.Ignore;
			FolderTemplate = config.FolderTemplate;
			FileTemplate = config.FileTemplate;
			ChapterFileTemplate = config.ChapterFileTemplate;
			InProgressDirectory
				= config.InProgress == Configuration.AppDir_Absolute ? Configuration.KnownDirectories.AppDir
				: Configuration.GetKnownDirectory(config.InProgress);
			UseCoverAsFolderIcon = config.UseCoverAsFolderIcon;
		}

		public async Task<bool> SaveSettingsAsync(Configuration config)
		{
			static Task validationError(string text, string caption)
				=> MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);

			// these 3 should do nothing. Configuration will only init these with a valid value. EditTemplateDialog ensures valid before returning
			if (!Templates.Folder.IsValid(FolderTemplate))
			{
				await validationError($"Not saving change to folder naming template. Invalid format.", "Invalid folder template");
				return false;
			}
			if (!Templates.File.IsValid(FileTemplate))
			{
				await validationError($"Not saving change to file naming template. Invalid format.", "Invalid file template");
				return false;
			}
			if (!Templates.ChapterFile.IsValid(ChapterFileTemplate))
			{
				await validationError($"Not saving change to chapter file naming template. Invalid format.", "Invalid chapter file template");
				return false;
			}

			config.BadBook
				= BadBookAbort ? Configuration.BadBookAction.Abort
				: BadBookRetry ? Configuration.BadBookAction.Retry
				: BadBookIgnore ? Configuration.BadBookAction.Ignore
				: Configuration.BadBookAction.Ask;

			config.FolderTemplate = FolderTemplate;
			config.FileTemplate = FileTemplate;
			config.ChapterFileTemplate = ChapterFileTemplate;
			config.InProgress
				= InProgressDirectory is Configuration.KnownDirectories.AppDir ? Configuration.AppDir_Absolute
				: Configuration.GetKnownDirectoryPath(InProgressDirectory);

			config.UseCoverAsFolderIcon = UseCoverAsFolderIcon;

			return true;
		}

		public string UseCoverAsFolderIconText { get; } = Configuration.GetDescription(nameof(Configuration.UseCoverAsFolderIcon));
		public string BadBookGroupboxText { get; } = Configuration.GetDescription(nameof(Configuration.BadBook));
		public string BadBookAskText { get; } = Configuration.BadBookAction.Ask.GetDescription();
		public string BadBookAbortText { get; } = Configuration.BadBookAction.Abort.GetDescription();
		public string BadBookRetryText { get; } = Configuration.BadBookAction.Retry.GetDescription();
		public string BadBookIgnoreText { get; } = Configuration.BadBookAction.Ignore.GetDescription();
		public string FolderTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.FolderTemplate));
		public string FileTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.FileTemplate));
		public string ChapterFileTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.ChapterFileTemplate));
		public string EditCharReplacementText { get; } = Configuration.GetDescription(nameof(Configuration.ReplacementCharacters));
		public string InProgressDescriptionText { get; } = Configuration.GetDescription(nameof(Configuration.InProgress));

		public string FolderTemplate { get => _folderTemplate; set { this.RaiseAndSetIfChanged(ref _folderTemplate, value); } }
		public string FileTemplate { get => _fileTemplate; set { this.RaiseAndSetIfChanged(ref _fileTemplate, value); } }
		public string ChapterFileTemplate { get => _chapterFileTemplate; set { this.RaiseAndSetIfChanged(ref _chapterFileTemplate, value); } }
		public bool UseCoverAsFolderIcon { get; set; }

		public bool BadBookAsk
		{
			get => _badBookAsk;
			set
			{
				this.RaiseAndSetIfChanged(ref _badBookAsk, value);
				if (value)
				{
					BadBookAbort = false;
					BadBookRetry = false;
					BadBookIgnore = false;
				}
			}
		}
		public bool BadBookAbort
		{
			get => _badBookAbort;
			set
			{
				this.RaiseAndSetIfChanged(ref _badBookAbort, value);
				if (value)
				{
					BadBookAsk = false;
					BadBookRetry = false;
					BadBookIgnore = false;
				}
			}
		}
		public bool BadBookRetry		
		{
			get => _badBookRetry;
			set
			{
				this.RaiseAndSetIfChanged(ref _badBookRetry, value);
				if (value)
				{
					BadBookAsk = false;
					BadBookAbort = false;
					BadBookIgnore = false;
				}
			}
		}
		public bool BadBookIgnore
		{
			get => _badBookIgnore;
			set
			{
				this.RaiseAndSetIfChanged(ref _badBookIgnore, value);
				if (value)
				{
					BadBookAsk = false;
					BadBookAbort = false;
					BadBookRetry = false;
				}
			}
		}
	}

	public class AudioSettings : ViewModels.ViewModelBase, ISettingsDisplay
	{

		private bool _splitFilesByChapter;
		private bool _allowLibationFixup;
		private bool _lameTargetBitrate;
		private bool _lameMatchSource;
		private int _lameBitrate;
		private int _lameVBRQuality;
		private string _chapterTitleTemplate;

		public bool IsMp3Supported => Configuration.IsLinux || Configuration.IsWindows;

		public AudioSettings(Configuration config)
		{
			LoadSettings(config);
		}
		public void LoadSettings(Configuration config)
		{
			CreateCueSheet = config.CreateCueSheet;
			AllowLibationFixup = config.AllowLibationFixup;
			DownloadCoverArt = config.DownloadCoverArt;
			RetainAaxFile = config.RetainAaxFile;
			SplitFilesByChapter = config.SplitFilesByChapter;
			MergeOpeningAndEndCredits = config.MergeOpeningAndEndCredits;
			StripAudibleBrandAudio = config.StripAudibleBrandAudio;
			StripUnabridged = config.StripUnabridged;
			ChapterTitleTemplate = config.ChapterTitleTemplate;
			DecryptToLossy = config.DecryptToLossy;
			LameTargetBitrate = config.LameTargetBitrate;
			LameDownsampleMono = config.LameDownsampleMono;
			LameConstantBitrate = config.LameConstantBitrate;
			LameMatchSource = config.LameMatchSourceBR;
			LameBitrate = config.LameBitrate;
			LameVBRQuality = config.LameVBRQuality;
		}

		public Task<bool> SaveSettingsAsync(Configuration config)
		{
			config.CreateCueSheet = CreateCueSheet;
			config.AllowLibationFixup = AllowLibationFixup;
			config.DownloadCoverArt = DownloadCoverArt;
			config.RetainAaxFile = RetainAaxFile;
			config.SplitFilesByChapter = SplitFilesByChapter;
			config.MergeOpeningAndEndCredits = MergeOpeningAndEndCredits;
			config.StripAudibleBrandAudio = StripAudibleBrandAudio;
			config.StripUnabridged = StripUnabridged;
			config.ChapterTitleTemplate = ChapterTitleTemplate;
			config.DecryptToLossy = DecryptToLossy;
			config.LameTargetBitrate = LameTargetBitrate;
			config.LameDownsampleMono = LameDownsampleMono;
			config.LameConstantBitrate = LameConstantBitrate;
			config.LameMatchSourceBR = LameMatchSource;
			config.LameBitrate = LameBitrate;
			config.LameVBRQuality = LameVBRQuality;

			return Task.FromResult(true);
		}

		public string CreateCueSheetText { get; } = Configuration.GetDescription(nameof(Configuration.CreateCueSheet));
		public string AllowLibationFixupText { get; } = Configuration.GetDescription(nameof(Configuration.AllowLibationFixup));
		public string DownloadCoverArtText { get; } = Configuration.GetDescription(nameof(Configuration.DownloadCoverArt));
		public string RetainAaxFileText { get; } = Configuration.GetDescription(nameof(Configuration.RetainAaxFile));
		public string SplitFilesByChapterText { get; } = Configuration.GetDescription(nameof(Configuration.SplitFilesByChapter));
		public string MergeOpeningEndCreditsText { get; } = Configuration.GetDescription(nameof(Configuration.MergeOpeningAndEndCredits));
		public string StripAudibleBrandingText { get; } = Configuration.GetDescription(nameof(Configuration.StripAudibleBrandAudio));
		public string StripUnabridgedText { get; } = Configuration.GetDescription(nameof(Configuration.StripUnabridged));
		public string ChapterTitleTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.ChapterTitleTemplate));

		public bool CreateCueSheet { get; set; }
		public bool DownloadCoverArt { get; set; }
		public bool RetainAaxFile { get; set; }
		public bool MergeOpeningAndEndCredits { get; set; }
		public bool StripAudibleBrandAudio { get; set; }
		public bool StripUnabridged { get; set; }
		public bool DecryptToLossy { get; set; }

		public bool LameDownsampleMono { get; set; } = Design.IsDesignMode;
		public bool LameConstantBitrate { get; set; } = Design.IsDesignMode;

		public bool SplitFilesByChapter { get => _splitFilesByChapter; set { this.RaiseAndSetIfChanged(ref _splitFilesByChapter, value); } }
		public bool LameTargetBitrate { get => _lameTargetBitrate; set { this.RaiseAndSetIfChanged(ref _lameTargetBitrate, value); } }
		public bool LameMatchSource { get => _lameMatchSource; set { this.RaiseAndSetIfChanged(ref _lameMatchSource, value); } }
		public int LameBitrate { get => _lameBitrate; set { this.RaiseAndSetIfChanged(ref _lameBitrate, value); } }
		public int LameVBRQuality { get => _lameVBRQuality; set { this.RaiseAndSetIfChanged(ref _lameVBRQuality, value); } }


		public string ChapterTitleTemplate { get => _chapterTitleTemplate; set { this.RaiseAndSetIfChanged(ref _chapterTitleTemplate, value); } }


		public bool AllowLibationFixup
		{
			get => _allowLibationFixup;
			set
			{
				this.RaiseAndSetIfChanged(ref _allowLibationFixup, value);
				if (!_allowLibationFixup)
				{
					SplitFilesByChapter = false;
					StripAudibleBrandAudio = false;
					StripUnabridged = false;
					DecryptToLossy = false;
					this.RaisePropertyChanged(nameof(SplitFilesByChapter));
					this.RaisePropertyChanged(nameof(StripAudibleBrandAudio));
					this.RaisePropertyChanged(nameof(StripUnabridged));
					this.RaisePropertyChanged(nameof(DecryptToLossy));
				}
			}
		}
	}
}
