using Avalonia.Collections;
using Avalonia.Controls;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System.Linq;

namespace LibationAvalonia.ViewModels.Settings
{
	public class AudioSettingsVM : ViewModelBase, ISettingsDisplay
	{
		private bool _downloadClipsBookmarks;
		private bool _decryptToLossy;
		private bool _splitFilesByChapter;
		private bool _allowLibationFixup;
		private bool _lameTargetBitrate;
		private bool _lameMatchSource;
		private int _lameBitrate;
		private int _lameVBRQuality;
		private string _chapterTitleTemplate;
		public SampleRateSelection SelectedSampleRate { get; set; }
		public NAudio.Lame.EncoderQuality SelectedEncoderQuality { get; set; }

		public AvaloniaList<SampleRateSelection> SampleRates { get; }
			= new(
				new[]
				{
					AAXClean.SampleRate.Hz_44100,
					AAXClean.SampleRate.Hz_32000,
					AAXClean.SampleRate.Hz_24000,
					AAXClean.SampleRate.Hz_22050,
					AAXClean.SampleRate.Hz_16000,
					AAXClean.SampleRate.Hz_12000,
				}
				.Select(s => new SampleRateSelection(s)));

		public AvaloniaList<NAudio.Lame.EncoderQuality> EncoderQualities { get; }
		= new(
			new[]
			{
				NAudio.Lame.EncoderQuality.High,
				NAudio.Lame.EncoderQuality.Standard,
				NAudio.Lame.EncoderQuality.Fast,
			});


		public AudioSettingsVM(Configuration config)
		{
			LoadSettings(config);
		}
		public void LoadSettings(Configuration config)
		{
			CreateCueSheet = config.CreateCueSheet;
			AllowLibationFixup = config.AllowLibationFixup;
			DownloadCoverArt = config.DownloadCoverArt;
			RetainAaxFile = config.RetainAaxFile;
			DownloadClipsBookmarks = config.DownloadClipsBookmarks;
			ClipBookmarkFormat = config.ClipsBookmarksFileFormat;
			SplitFilesByChapter = config.SplitFilesByChapter;
			MergeOpeningAndEndCredits = config.MergeOpeningAndEndCredits;
			StripAudibleBrandAudio = config.StripAudibleBrandAudio;
			StripUnabridged = config.StripUnabridged;
			ChapterTitleTemplate = config.ChapterTitleTemplate;
			DecryptToLossy = config.DecryptToLossy;
			MoveMoovToBeginning = config.MoveMoovToBeginning;
			LameTargetBitrate = config.LameTargetBitrate;
			LameDownsampleMono = config.LameDownsampleMono;
			LameConstantBitrate = config.LameConstantBitrate;
			LameMatchSource = config.LameMatchSourceBR;
			LameBitrate = config.LameBitrate;
			LameVBRQuality = config.LameVBRQuality;

			SelectedSampleRate = SampleRates.FirstOrDefault(s => s.SampleRate == config.MaxSampleRate);
			SelectedEncoderQuality = config.LameEncoderQuality;
		}

		public void SaveSettings(Configuration config)
		{
			config.CreateCueSheet = CreateCueSheet;
			config.AllowLibationFixup = AllowLibationFixup;
			config.DownloadCoverArt = DownloadCoverArt;
			config.RetainAaxFile = RetainAaxFile;
			config.DownloadClipsBookmarks = DownloadClipsBookmarks;
			config.ClipsBookmarksFileFormat = ClipBookmarkFormat;
			config.SplitFilesByChapter = SplitFilesByChapter;
			config.MergeOpeningAndEndCredits = MergeOpeningAndEndCredits;
			config.StripAudibleBrandAudio = StripAudibleBrandAudio;
			config.StripUnabridged = StripUnabridged;
			config.ChapterTitleTemplate = ChapterTitleTemplate;
			config.DecryptToLossy = DecryptToLossy;
			config.MoveMoovToBeginning = MoveMoovToBeginning;
			config.LameTargetBitrate = LameTargetBitrate;
			config.LameDownsampleMono = LameDownsampleMono;
			config.LameConstantBitrate = LameConstantBitrate;
			config.LameMatchSourceBR = LameMatchSource;
			config.LameBitrate = LameBitrate;
			config.LameVBRQuality = LameVBRQuality;

			config.LameEncoderQuality = SelectedEncoderQuality;
			config.MaxSampleRate = SelectedSampleRate?.SampleRate ?? config.MaxSampleRate;
		}

		public AvaloniaList<Configuration.ClipBookmarkFormat> ClipBookmarkFormats { get; } = new(Enum<Configuration.ClipBookmarkFormat>.GetValues());
		public string CreateCueSheetText { get; } = Configuration.GetDescription(nameof(Configuration.CreateCueSheet));
		public string AllowLibationFixupText { get; } = Configuration.GetDescription(nameof(Configuration.AllowLibationFixup));
		public string DownloadCoverArtText { get; } = Configuration.GetDescription(nameof(Configuration.DownloadCoverArt));
		public string RetainAaxFileText { get; } = Configuration.GetDescription(nameof(Configuration.RetainAaxFile));
		public string SplitFilesByChapterText { get; } = Configuration.GetDescription(nameof(Configuration.SplitFilesByChapter));
		public string MergeOpeningEndCreditsText { get; } = Configuration.GetDescription(nameof(Configuration.MergeOpeningAndEndCredits));
		public string StripAudibleBrandingText { get; } = Configuration.GetDescription(nameof(Configuration.StripAudibleBrandAudio));
		public string StripUnabridgedText { get; } = Configuration.GetDescription(nameof(Configuration.StripUnabridged));
		public string ChapterTitleTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.ChapterTitleTemplate));
		public string MoveMoovToBeginningText { get; } = Configuration.GetDescription(nameof(Configuration.MoveMoovToBeginning));

		public bool CreateCueSheet { get; set; }
		public bool DownloadCoverArt { get; set; }
		public bool RetainAaxFile { get; set; }
		public bool DownloadClipsBookmarks { get => _downloadClipsBookmarks; set => this.RaiseAndSetIfChanged(ref _downloadClipsBookmarks, value); }
		public Configuration.ClipBookmarkFormat ClipBookmarkFormat { get; set; }
		public bool MergeOpeningAndEndCredits { get; set; }
		public bool StripAudibleBrandAudio { get; set; }
		public bool StripUnabridged { get; set; }
		public bool DecryptToLossy { get => _decryptToLossy; set => this.RaiseAndSetIfChanged(ref _decryptToLossy, value); }
		public bool MoveMoovToBeginning { get; set; }

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
				if (!this.RaiseAndSetIfChanged(ref _allowLibationFixup, value))
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
