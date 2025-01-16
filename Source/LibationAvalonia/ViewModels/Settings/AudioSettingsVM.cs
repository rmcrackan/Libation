using AAXClean;
using Avalonia.Collections;
using Avalonia.Controls;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Linq;

namespace LibationAvalonia.ViewModels.Settings
{
	public class AudioSettingsVM : ViewModelBase
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
		public EnumDiaplay<SampleRate> SelectedSampleRate { get; set; }
		public NAudio.Lame.EncoderQuality SelectedEncoderQuality { get; set; }

		public AvaloniaList<EnumDiaplay<SampleRate>> SampleRates { get; }
				= new(Enum.GetValues<SampleRate>()
				.Where(r => r >= SampleRate.Hz_8000 && r <= SampleRate.Hz_48000)
				.Select(v => new EnumDiaplay<SampleRate>(v, $"{(int)v} Hz")));

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
			CombineNestedChapterTitles = config.CombineNestedChapterTitles;
			AllowLibationFixup = config.AllowLibationFixup;
			DownloadCoverArt = config.DownloadCoverArt;
			RetainAaxFile = config.RetainAaxFile;
			DownloadClipsBookmarks = config.DownloadClipsBookmarks;
			FileDownloadQuality = config.FileDownloadQuality;
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

			SelectedSampleRate = SampleRates.SingleOrDefault(s => s.Value == config.MaxSampleRate);
			SelectedEncoderQuality = config.LameEncoderQuality;
		}

		public void SaveSettings(Configuration config)
		{
			config.CreateCueSheet = CreateCueSheet;
			config.CombineNestedChapterTitles = CombineNestedChapterTitles;
			config.AllowLibationFixup = AllowLibationFixup;
			config.DownloadCoverArt = DownloadCoverArt;
			config.RetainAaxFile = RetainAaxFile;
			config.DownloadClipsBookmarks = DownloadClipsBookmarks;
			config.FileDownloadQuality = FileDownloadQuality;
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
			config.MaxSampleRate = SelectedSampleRate?.Value ?? config.MaxSampleRate;
		}

		public AvaloniaList<Configuration.DownloadQuality> DownloadQualities { get; } = new(Enum<Configuration.DownloadQuality>.GetValues());
		public AvaloniaList<Configuration.ClipBookmarkFormat> ClipBookmarkFormats { get; } = new(Enum<Configuration.ClipBookmarkFormat>.GetValues());
		public string FileDownloadQualityText { get; } = Configuration.GetDescription(nameof(Configuration.FileDownloadQuality));
		public string CreateCueSheetText { get; } = Configuration.GetDescription(nameof(Configuration.CreateCueSheet));
		public string CombineNestedChapterTitlesText { get; } = Configuration.GetDescription(nameof(Configuration.CombineNestedChapterTitles));
		public string CombineNestedChapterTitlesTip => Configuration.GetHelpText(nameof(CombineNestedChapterTitles));
		public string AllowLibationFixupText { get; } = Configuration.GetDescription(nameof(Configuration.AllowLibationFixup));
		public string AllowLibationFixupTip => Configuration.GetHelpText(nameof(AllowLibationFixup));
		public string DownloadCoverArtText { get; } = Configuration.GetDescription(nameof(Configuration.DownloadCoverArt));
		public string RetainAaxFileText { get; } = Configuration.GetDescription(nameof(Configuration.RetainAaxFile));
		public string SplitFilesByChapterText { get; } = Configuration.GetDescription(nameof(Configuration.SplitFilesByChapter));
		public string MergeOpeningEndCreditsText { get; } = Configuration.GetDescription(nameof(Configuration.MergeOpeningAndEndCredits));
		public string StripAudibleBrandingText { get; } = Configuration.GetDescription(nameof(Configuration.StripAudibleBrandAudio));
		public string StripUnabridgedText { get; } = Configuration.GetDescription(nameof(Configuration.StripUnabridged));
		public string ChapterTitleTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.ChapterTitleTemplate));
		public string MoveMoovToBeginningText { get; } = Configuration.GetDescription(nameof(Configuration.MoveMoovToBeginning));

		public bool CreateCueSheet { get; set; }
		public bool CombineNestedChapterTitles { get; set; }
		public bool DownloadCoverArt { get; set; }
		public bool RetainAaxFile { get; set; }
		public bool DownloadClipsBookmarks { get => _downloadClipsBookmarks; set => this.RaiseAndSetIfChanged(ref _downloadClipsBookmarks, value); }
		public Configuration.DownloadQuality FileDownloadQuality { get; set; }
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
