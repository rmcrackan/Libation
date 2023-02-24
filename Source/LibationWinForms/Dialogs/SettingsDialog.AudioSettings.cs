using System;
using LibationFileManager;
using System.Linq;
using LibationUiBase;

namespace LibationWinForms.Dialogs
{
	partial class SettingsDialog
	{
		private void Load_AudioSettings(Configuration config)
		{
			this.allowLibationFixupCbox.Text = desc(nameof(config.AllowLibationFixup));
			this.createCueSheetCbox.Text = desc(nameof(config.CreateCueSheet));
			this.downloadCoverArtCbox.Text = desc(nameof(config.DownloadCoverArt));
			this.retainAaxFileCbox.Text = desc(nameof(config.RetainAaxFile));
			this.splitFilesByChapterCbox.Text = desc(nameof(config.SplitFilesByChapter));
			this.mergeOpeningEndCreditsCbox.Text = desc(nameof(config.MergeOpeningAndEndCredits));
			this.stripAudibleBrandingCbox.Text = desc(nameof(config.StripAudibleBrandAudio));
			this.stripUnabridgedCbox.Text = desc(nameof(config.StripUnabridged));
			this.moveMoovAtomCbox.Text = desc(nameof(config.MoveMoovToBeginning));

			clipsBookmarksFormatCb.Items.AddRange(
				new object[]
				{
					Configuration.ClipBookmarkFormat.CSV,
					Configuration.ClipBookmarkFormat.Xlsx,
					Configuration.ClipBookmarkFormat.Json
				});

			maxSampleRateCb.Items.AddRange(
				new object[]
				{
					new SampleRateSelection(AAXClean.SampleRate.Hz_44100),
					new SampleRateSelection(AAXClean.SampleRate.Hz_32000),
					new SampleRateSelection(AAXClean.SampleRate.Hz_24000),
					new SampleRateSelection(AAXClean.SampleRate.Hz_22050),
					new SampleRateSelection(AAXClean.SampleRate.Hz_16000),
					new SampleRateSelection(AAXClean.SampleRate.Hz_12000)
				});

			encoderQualityCb.Items.AddRange(
				new object[]
				{
					NAudio.Lame.EncoderQuality.High,
					NAudio.Lame.EncoderQuality.Standard,
					NAudio.Lame.EncoderQuality.Fast,
				});

			allowLibationFixupCbox.Checked = config.AllowLibationFixup;
			createCueSheetCbox.Checked = config.CreateCueSheet;
			downloadCoverArtCbox.Checked = config.DownloadCoverArt;
			downloadClipsBookmarksCbox.Checked = config.DownloadClipsBookmarks;
			clipsBookmarksFormatCb.SelectedItem = config.ClipsBookmarksFileFormat;
			retainAaxFileCbox.Checked = config.RetainAaxFile;
			splitFilesByChapterCbox.Checked = config.SplitFilesByChapter;
			mergeOpeningEndCreditsCbox.Checked = config.MergeOpeningAndEndCredits;
			stripUnabridgedCbox.Checked = config.StripUnabridged;
			stripAudibleBrandingCbox.Checked = config.StripAudibleBrandAudio;
			convertLosslessRb.Checked = !config.DecryptToLossy;
			convertLossyRb.Checked = config.DecryptToLossy;
			moveMoovAtomCbox.Checked = config.MoveMoovToBeginning;

			lameTargetBitrateRb.Checked = config.LameTargetBitrate;
			lameTargetQualityRb.Checked = !config.LameTargetBitrate;
			maxSampleRateCb.SelectedItem = maxSampleRateCb.Items.Cast<SampleRateSelection>().Single(s => s.SampleRate == config.MaxSampleRate);
			encoderQualityCb.SelectedItem = config.LameEncoderQuality;
			lameDownsampleMonoCbox.Checked = config.LameDownsampleMono;
			lameBitrateTb.Value = config.LameBitrate;
			lameConstantBitrateCbox.Checked = config.LameConstantBitrate;
			LameMatchSourceBRCbox.Checked = config.LameMatchSourceBR;
			lameVBRQualityTb.Value = config.LameVBRQuality;

			chapterTitleTemplateGb.Text = desc(nameof(config.ChapterTitleTemplate));
			chapterTitleTemplateTb.Text = config.ChapterTitleTemplate;

			lameTargetRb_CheckedChanged(this, EventArgs.Empty);
			LameMatchSourceBRCbox_CheckedChanged(this, EventArgs.Empty);
			convertFormatRb_CheckedChanged(this, EventArgs.Empty);
			allowLibationFixupCbox_CheckedChanged(this, EventArgs.Empty);
			splitFilesByChapterCbox_CheckedChanged(this, EventArgs.Empty);
			downloadClipsBookmarksCbox_CheckedChanged(this, EventArgs.Empty);
		}

		private void Save_AudioSettings(Configuration config)
		{
			config.AllowLibationFixup = allowLibationFixupCbox.Checked;
			config.CreateCueSheet = createCueSheetCbox.Checked;
			config.DownloadCoverArt = downloadCoverArtCbox.Checked;
			config.DownloadClipsBookmarks = downloadClipsBookmarksCbox.Checked;
			config.ClipsBookmarksFileFormat = (Configuration.ClipBookmarkFormat)clipsBookmarksFormatCb.SelectedItem;
			config.RetainAaxFile = retainAaxFileCbox.Checked;
			config.SplitFilesByChapter = splitFilesByChapterCbox.Checked;
			config.MergeOpeningAndEndCredits = mergeOpeningEndCreditsCbox.Checked;
			config.StripUnabridged = stripUnabridgedCbox.Checked;
			config.StripAudibleBrandAudio = stripAudibleBrandingCbox.Checked;
			config.DecryptToLossy = convertLossyRb.Checked;
			config.MoveMoovToBeginning = moveMoovAtomCbox.Checked;

			config.LameTargetBitrate = lameTargetBitrateRb.Checked;
			config.MaxSampleRate = ((SampleRateSelection)maxSampleRateCb.SelectedItem).SampleRate;
			config.LameEncoderQuality = (NAudio.Lame.EncoderQuality)encoderQualityCb.SelectedItem;
			encoderQualityCb.SelectedItem = config.LameEncoderQuality;
			config.LameDownsampleMono = lameDownsampleMonoCbox.Checked;
			config.LameBitrate = lameBitrateTb.Value;
			config.LameConstantBitrate = lameConstantBitrateCbox.Checked;
			config.LameMatchSourceBR = LameMatchSourceBRCbox.Checked;
			config.LameVBRQuality = lameVBRQualityTb.Value;

			config.ChapterTitleTemplate = chapterTitleTemplateTb.Text;
		}


		private void downloadClipsBookmarksCbox_CheckedChanged(object sender, EventArgs e)
		{
			clipsBookmarksFormatCb.Enabled = downloadClipsBookmarksCbox.Checked;
		}

		private void lameTargetRb_CheckedChanged(object sender, EventArgs e)
		{
			lameBitrateGb.Enabled = lameTargetBitrateRb.Checked;
			lameQualityGb.Enabled = !lameTargetBitrateRb.Checked;
		}

		private void LameMatchSourceBRCbox_CheckedChanged(object sender, EventArgs e)
		{
			lameBitrateTb.Enabled = !LameMatchSourceBRCbox.Checked;
		}

		private void splitFilesByChapterCbox_CheckedChanged(object sender, EventArgs e)
		{
			chapterTitleTemplateGb.Enabled = splitFilesByChapterCbox.Checked;
		}

		private void chapterTitleTemplateBtn_Click(object sender, EventArgs e)
			=> editTemplate(TemplateEditor<Templates.ChapterTitleTemplate>.CreateNameEditor(chapterTitleTemplateTb.Text), chapterTitleTemplateTb);

		private void convertFormatRb_CheckedChanged(object sender, EventArgs e)
		{
			moveMoovAtomCbox.Enabled = convertLosslessRb.Checked;
			lameTargetRb_CheckedChanged(sender, e);
			LameMatchSourceBRCbox_CheckedChanged(sender, e);
		}
		private void allowLibationFixupCbox_CheckedChanged(object sender, EventArgs e)
		{
			audiobookFixupsGb.Enabled = allowLibationFixupCbox.Checked;
			convertLosslessRb.Enabled = allowLibationFixupCbox.Checked;
			convertLossyRb.Enabled = allowLibationFixupCbox.Checked;
			splitFilesByChapterCbox.Enabled = allowLibationFixupCbox.Checked;
			stripUnabridgedCbox.Enabled = allowLibationFixupCbox.Checked;
			stripAudibleBrandingCbox.Enabled = allowLibationFixupCbox.Checked;

			if (!allowLibationFixupCbox.Checked)
			{
				convertLosslessRb.Checked = true;
				splitFilesByChapterCbox.Checked = false;
				stripUnabridgedCbox.Checked = false;
				stripAudibleBrandingCbox.Checked = false;
			}
		}
	}
}
