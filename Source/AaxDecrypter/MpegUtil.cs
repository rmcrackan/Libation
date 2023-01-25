using AAXClean;
using NAudio.Lame;

namespace AaxDecrypter
{
	public static class MpegUtil
	{
		public static void ConfigureLameOptions(Mp4File mp4File, LameConfig lameConfig, bool downsample, bool matchSourceBitrate)
		{
			double bitrateMultiple = 1;

			if (mp4File.AudioChannels == 2)
			{
				if (downsample)
					bitrateMultiple = 0.5;
				else
					lameConfig.Mode = MPEGMode.Stereo;
			}

			if (matchSourceBitrate)
			{
				int kbps = (int)(mp4File.AverageBitrate * bitrateMultiple / 1024);

				if (lameConfig.VBR is null)
					lameConfig.BitRate = kbps;
				else if (lameConfig.VBR == VBRMode.ABR)
					lameConfig.ABRRateKbps = kbps;
			}
		}
	}
}
