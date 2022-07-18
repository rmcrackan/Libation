using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using Dinah.Core;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{
	public partial class SettingsDialog : DialogWindow
	{
		private SettingsPages settingsDisp;
		public SettingsDialog()
		{
			if (Design.IsDesignMode)
				AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();
			InitializeComponent();

			DataContext = settingsDisp = new(Configuration.Instance);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
		protected override async Task SaveAndCloseAsync()
		{


			await base.SaveAndCloseAsync();
		}
		public async void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();



		public void EditFolderTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = editTemplate(Templates.ChapterTitle, settingsDisp.AudioSettings.ChapterTitleTemplate);
			if (newTemplate is not null)
				settingsDisp.AudioSettings.ChapterTitleTemplate = newTemplate;
		}
		

		public void EditFileTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = editTemplate(Templates.ChapterTitle, settingsDisp.AudioSettings.ChapterTitleTemplate);
			if (newTemplate is not null)
				settingsDisp.AudioSettings.ChapterTitleTemplate = newTemplate;
		}
		

		public void EditChapterFileTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = editTemplate(Templates.ChapterTitle, settingsDisp.AudioSettings.ChapterTitleTemplate);
			if (newTemplate is not null)
				settingsDisp.AudioSettings.ChapterTitleTemplate = newTemplate;
		}
		

		public void EditChapterTitleTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = editTemplate(Templates.ChapterTitle, settingsDisp.AudioSettings.ChapterTitleTemplate);
			if (newTemplate is not null)
				settingsDisp.AudioSettings.ChapterTitleTemplate = newTemplate;
		}


		private static string editTemplate(Templates template, string existingTemplate)
		{
			var form = new LibationWinForms.Dialogs.EditTemplateDialog(template, existingTemplate);
			if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				return form.TemplateText;
			else return null;
		}

	}
	internal interface ISettingsTab
	{
		void LoadSettings(Configuration config);
		void SaveSettings(Configuration config);
	}

	public class SettingsPages
	{
		public Configuration config { get; }
		public SettingsPages(Configuration config)
		{
			this.config = config;
			AudioSettings = new(config);
			DownloadDecryptSettings = new(config);
		}
		public AudioSettings AudioSettings { get;}
		public DownloadDecryptSettings DownloadDecryptSettings { get;}
	}

	public class DownloadDecryptSettings : ViewModels.ViewModelBase, ISettingsTab
	{
		private static Func<string, string> desc { get; } = Configuration.GetDescription;

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

		public void LoadSettings(Configuration config)
		{
			BadBookAsk = config.BadBook is Configuration.BadBookAction.Ask;
			BadBookAbort = config.BadBook is Configuration.BadBookAction.Abort;
			BadBookRetry = config.BadBook is Configuration.BadBookAction.Retry;
			BadBookIgnore = config.BadBook is Configuration.BadBookAction.Ignore;
			FolderTemplate = config.FolderTemplate;
			FileTemplate = config.FileTemplate;
			ChapterFileTemplate = config.ChapterFileTemplate;
		}

		public void SaveSettings(Configuration config)
		{
			config.BadBook
				= BadBookAbort ? Configuration.BadBookAction.Abort
				: BadBookRetry ? Configuration.BadBookAction.Retry
				: BadBookIgnore ? Configuration.BadBookAction.Ignore
				: Configuration.BadBookAction.Ask;

			config.FolderTemplate = FolderTemplate;
			config.FileTemplate = FileTemplate;
			config.ChapterFileTemplate = ChapterFileTemplate;
		}

		public string BadBookGroupboxText => desc(nameof(Configuration.BadBook));
		public string BadBookAskText { get; } = Configuration.BadBookAction.Ask.GetDescription();
		public string BadBookAbortText { get; } = Configuration.BadBookAction.Abort.GetDescription();
		public string BadBookRetryText { get; } = Configuration.BadBookAction.Retry.GetDescription();
		public string BadBookIgnoreText { get; } = Configuration.BadBookAction.Ignore.GetDescription();

		public string FolderTemplateText => desc(nameof(Configuration.FolderTemplate));
		public string FileTemplateText => desc(nameof(Configuration.FileTemplate));
		public string ChapterFileTemplateText => desc(nameof(Configuration.ChapterFileTemplate));
		public string? EditCharReplacementText => desc(nameof(Configuration.ReplacementCharacters));

		public string FolderTemplate { get => _folderTemplate; set { this.RaiseAndSetIfChanged(ref _folderTemplate, value); } }
		public string FileTemplate { get => _fileTemplate; set { this.RaiseAndSetIfChanged(ref _fileTemplate, value); } }
		public string ChapterFileTemplate { get => _chapterFileTemplate; set { this.RaiseAndSetIfChanged(ref _chapterFileTemplate, value); } }


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

	public class AudioSettings : ViewModels.ViewModelBase, ISettingsTab
	{

		private bool _splitFilesByChapter;
		private bool _allowLibationFixup;
		private bool _lameTargetBitrate;
		private bool _lameMatchSource;
		private int _lameBitrate;
		private int _lameVBRQuality;
		private string _chapterTitleTemplate;
		private static Func<string, string> desc { get; } = Configuration.GetDescription;
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

		public void SaveSettings(Configuration config)
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
		}

		public string CreateCueSheetText => desc(nameof(Configuration.CreateCueSheet));
		public string AllowLibationFixupText => desc(nameof(Configuration.AllowLibationFixup));
		public string DownloadCoverArtText => desc(nameof(Configuration.DownloadCoverArt));
		public string RetainAaxFileText => desc(nameof(Configuration.RetainAaxFile));
		public string SplitFilesByChapterText => desc(nameof(Configuration.SplitFilesByChapter));
		public string MergeOpeningEndCreditsText => desc(nameof(Configuration.MergeOpeningAndEndCredits));
		public string StripAudibleBrandingText => desc(nameof(Configuration.StripAudibleBrandAudio));
		public string StripUnabridgedText => desc(nameof(Configuration.StripUnabridged));
		public string ChapterTitleTemplateText => desc(nameof(Configuration.ChapterTitleTemplate));

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
