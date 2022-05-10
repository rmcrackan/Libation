using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.Dialogs
{
	partial class SettingsDialog
	{
		private void lameTargetRb_CheckedChanged(object sender, EventArgs e)
		{
			lameBitrateGb.Enabled = lameTargetBitrateRb.Checked;
			lameQualityGb.Enabled = !lameTargetBitrateRb.Checked;
		}

		private void LameMatchSourceBRCbox_CheckedChanged(object sender, EventArgs e)
		{
			lameBitrateTb.Enabled = !LameMatchSourceBRCbox.Checked;
		}

		private void convertFormatRb_CheckedChanged(object sender, EventArgs e)
		{
			lameOptionsGb.Enabled = convertLossyRb.Checked;
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
