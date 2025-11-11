using AAXClean;
using Avalonia.Collections;
using Avalonia.Controls;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Linq;

#nullable enable
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
		public EnumDisplay<SampleRate> SelectedSampleRate { get; set; }
		public NAudio.Lame.EncoderQuality SelectedEncoderQuality { get; set; }
		
		public AvaloniaList<EnumDisplay<SampleRate>> SampleRates { get; }
				= new(Enum.GetValues<SampleRate>()
				.Where(r => r >= SampleRate.Hz_8000 && r <= SampleRate.Hz_48000)
				.Select(v => new EnumDisplay<SampleRate>(v, $"{((int)v):N0} Hz")));

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
			CreateCueSheet = config.CreateCueSheet;
			CombineNestedChapterTitles = config.CombineNestedChapterTitles;
			AllowLibationFixup = config.AllowLibationFixup;
			DownloadCoverArt = config.DownloadCoverArt;
			RetainAaxFile = config.RetainAaxFile;
			DownloadClipsBookmarks = config.DownloadClipsBookmarks;
			ClipBookmarkFormat = config.ClipsBookmarksFileFormat;
			SplitFilesByChapter = config.SplitFilesByChapter;
			MergeOpeningAndEndCredits = config.MergeOpeningAndEndCredits;
			StripAudibleBrandAudio = config.StripAudibleBrandAudio;
			StripUnabridged = config.StripUnabridged;
			_chapterTitleTemplate = config.ChapterTitleTemplate;
			MoveMoovToBeginning = config.MoveMoovToBeginning;
			LameTargetBitrate = config.LameTargetBitrate;
			LameDownsampleMono = config.LameDownsampleMono;
			LameConstantBitrate = config.LameConstantBitrate;
			LameMatchSource = config.LameMatchSourceBR;
			LameBitrate = config.LameBitrate;
			LameVBRQuality = config.LameVBRQuality;

			SpatialAudioCodec = SpatialAudioCodecs.SingleOrDefault(s => s.Value == config.SpatialAudioCodec) ?? SpatialAudioCodecs[0];
			FileDownloadQuality = DownloadQualities.SingleOrDefault(s => s.Value == config.FileDownloadQuality) ?? DownloadQualities[0];
			SelectedSampleRate = SampleRates.SingleOrDefault(s => s.Value == config.MaxSampleRate) ?? SampleRates[0];
			SelectedEncoderQuality = config.LameEncoderQuality;
			UseWidevine = config.UseWidevine;
			RequestSpatial = config.RequestSpatial;
			Request_xHE_AAC = config.Request_xHE_AAC;
			DecryptToLossy = config.DecryptToLossy;
		}

		public void SaveSettings(Configuration config)
		{
			config.CreateCueSheet = CreateCueSheet;
			config.CombineNestedChapterTitles = CombineNestedChapterTitles;
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
			config.MaxSampleRate = SelectedSampleRate?.Value ?? config.MaxSampleRate;
			config.FileDownloadQuality = FileDownloadQuality?.Value ?? config.FileDownloadQuality;
			config.SpatialAudioCodec = SpatialAudioCodec?.Value ?? config.SpatialAudioCodec;
			config.UseWidevine = UseWidevine;
			config.RequestSpatial = RequestSpatial;
			config.Request_xHE_AAC = Request_xHE_AAC;
		}

		public AvaloniaList<EnumDisplay<Configuration.DownloadQuality>> DownloadQualities { get; } = new([
					new EnumDisplay<Configuration.DownloadQuality>(Configuration.DownloadQuality.Normal),
					new EnumDisplay<Configuration.DownloadQuality>(Configuration.DownloadQuality.High),
				]);
		public AvaloniaList<EnumDisplay<Configuration.SpatialCodec>> SpatialAudioCodecs { get; } = new([
					new EnumDisplay<Configuration.SpatialCodec>(Configuration.SpatialCodec.EC_3, "Dolby Digital Plus (E-AC-3)"),
					new EnumDisplay<Configuration.SpatialCodec>(Configuration.SpatialCodec.AC_4, "Dolby AC-4")
				]);
		public AvaloniaList<Configuration.ClipBookmarkFormat> ClipBookmarkFormats { get; } = new(Enum<Configuration.ClipBookmarkFormat>.GetValues());
		public string FileDownloadQualityText { get; } = Configuration.GetDescription(nameof(Configuration.FileDownloadQuality));
		public string UseWidevineText { get; } = Configuration.GetDescription(nameof(Configuration.UseWidevine));
		public string UseWidevineTip { get; } = Configuration.GetHelpText(nameof(Configuration.UseWidevine));
		public string Request_xHE_AACText { get; } = Configuration.GetDescription(nameof(Configuration.Request_xHE_AAC));
		public string Request_xHE_AACTip { get; } = Configuration.GetHelpText(nameof(Configuration.Request_xHE_AAC));
		public string RequestSpatialText { get; } = Configuration.GetDescription(nameof(Configuration.RequestSpatial));
		public string RequestSpatialTip { get; } = Configuration.GetHelpText(nameof(Configuration.RequestSpatial));
		public string SpatialAudioCodecTip { get; } = Configuration.GetHelpText(nameof(Configuration.SpatialAudioCodec));
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
		public string MoveMoovToBeginningTip => Configuration.GetHelpText(nameof(MoveMoovToBeginning));

		public bool CreateCueSheet { get; set; }
		public bool CombineNestedChapterTitles { get; set; }
		public bool DownloadCoverArt { get; set; }
		public bool RetainAaxFile { get; set; }
		public string RetainAaxFileTip => Configuration.GetHelpText(nameof(RetainAaxFile));
		public bool DownloadClipsBookmarks { get => _downloadClipsBookmarks; set => this.RaiseAndSetIfChanged(ref _downloadClipsBookmarks, value); }

		private bool _useWidevine, _requestSpatial, _request_xHE_AAC;
		public bool UseWidevine { get => _useWidevine; set => this.RaiseAndSetIfChanged(ref _useWidevine, value); }
		public bool Request_xHE_AAC { get => _request_xHE_AAC; set => this.RaiseAndSetIfChanged(ref _request_xHE_AAC, value); }
		public bool RequestSpatial { get => _requestSpatial; set => this.RaiseAndSetIfChanged(ref _requestSpatial, value); }

		public EnumDisplay<Configuration.DownloadQuality> FileDownloadQuality { get; set; }
		public EnumDisplay<Configuration.SpatialCodec> SpatialAudioCodec { get; set; }
		public Configuration.ClipBookmarkFormat ClipBookmarkFormat { get; set; }
		public bool MergeOpeningAndEndCredits { get; set; }
		public string MergeOpeningAndEndCreditsTip => Configuration.GetHelpText(nameof(MergeOpeningAndEndCredits));
		public bool StripAudibleBrandAudio { get; set; }
		public string StripAudibleBrandAudioTip => Configuration.GetHelpText(nameof(StripAudibleBrandAudio));
		public bool StripUnabridged { get; set; }
		public string StripUnabridgedTip => Configuration.GetHelpText(nameof(StripUnabridged));
		public bool DecryptToLossy {
			get => _decryptToLossy;
			set
			{
				this.RaiseAndSetIfChanged(ref _decryptToLossy, value);
				if (DecryptToLossy && SpatialAudioCodec.Value is Configuration.SpatialCodec.AC_4)
				{
					SpatialAudioCodec = SpatialAudioCodecs[0];
					this.RaisePropertyChanged(nameof(SpatialAudioCodec));
				}
			}
		}
		public string DecryptToLossyTip => Configuration.GetHelpText(nameof(DecryptToLossy));
		public bool MoveMoovToBeginning { get; set; }

		public bool LameDownsampleMono { get; set; } = Design.IsDesignMode;
		public string LameDownsampleMonoTip => Configuration.GetHelpText(nameof(LameDownsampleMono));
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
