using System;
using LibationFileManager;
using System.Linq;

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
			this.stripAudibleBrandingCbox.Text = desc(nameof(config.StripAudibleBrandAudio));
			this.stripUnabridgedCbox.Text = desc(nameof(config.StripUnabridged));

			allowLibationFixupCbox.Checked = config.AllowLibationFixup;
			createCueSheetCbox.Checked = config.CreateCueSheet;
			downloadCoverArtCbox.Checked = config.DownloadCoverArt;
			retainAaxFileCbox.Checked = config.RetainAaxFile;
			splitFilesByChapterCbox.Checked = config.SplitFilesByChapter;
			stripUnabridgedCbox.Checked = config.StripUnabridged;
			stripAudibleBrandingCbox.Checked = config.StripAudibleBrandAudio;
			convertLosslessRb.Checked = !config.DecryptToLossy;
			convertLossyRb.Checked = config.DecryptToLossy;

			lameTargetBitrateRb.Checked = config.LameTargetBitrate;
			lameTargetQualityRb.Checked = !config.LameTargetBitrate;
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
		}

		private void Save_AudioSettings(Configuration config)
		{
			config.AllowLibationFixup = allowLibationFixupCbox.Checked;
			config.CreateCueSheet = createCueSheetCbox.Checked;
			config.DownloadCoverArt = downloadCoverArtCbox.Checked;
			config.RetainAaxFile = retainAaxFileCbox.Checked;
			config.SplitFilesByChapter = splitFilesByChapterCbox.Checked;
			config.StripUnabridged = stripUnabridgedCbox.Checked;
			config.StripAudibleBrandAudio = stripAudibleBrandingCbox.Checked;
			config.DecryptToLossy = convertLossyRb.Checked;

			config.LameTargetBitrate = lameTargetBitrateRb.Checked;
			config.LameDownsampleMono = lameDownsampleMonoCbox.Checked;
			config.LameBitrate = lameBitrateTb.Value;
			config.LameConstantBitrate = lameConstantBitrateCbox.Checked;
			config.LameMatchSourceBR = LameMatchSourceBRCbox.Checked;
			config.LameVBRQuality = lameVBRQualityTb.Value;

			config.ChapterTitleTemplate = chapterTitleTemplateTb.Text;
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

		private void chapterTitleTemplateBtn_Click(object sender, EventArgs e) => editTemplate(Templates.ChapterTitle, chapterTitleTemplateTb);

		private void convertFormatRb_CheckedChanged(object sender, EventArgs e)
		{
			lameTargetRb_CheckedChanged(sender, e);
			LameMatchSourceBRCbox_CheckedChanged(sender, e);
		}
		private void allowLibationFixupCbox_CheckedChanged(object sender, EventArgs e)
		{
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
