using AAXClean;
using NAudio.Lame;
using System;

namespace AaxDecrypter
{
	public static class MpegUtil
	{
		public static void ConfigureLameOptions(Mp4File mp4File, LameConfig lameConfig, bool downsample, bool matchSourceBitrate)
		{
			double bitrateMultiple = 1;

			if (mp4File.TimeScale < lameConfig.OutputSampleRate)
			{
				lameConfig.OutputSampleRate = mp4File.TimeScale;
			}
			else if (mp4File.TimeScale > lameConfig.OutputSampleRate)
			{
				bitrateMultiple *= (double)lameConfig.OutputSampleRate / mp4File.TimeScale;
			}

			if (mp4File.AudioChannels == 2)
			{
				if (downsample)
					bitrateMultiple /= 2;
				else
					lameConfig.Mode = MPEGMode.Stereo;
			}

			if (matchSourceBitrate)
			{
				int kbps = (int)Math.Round(mp4File.AverageBitrate * bitrateMultiple / 1024);

				if (lameConfig.VBR is null)
					lameConfig.BitRate = kbps;
				else if (lameConfig.VBR == VBRMode.ABR)
					lameConfig.ABRRateKbps = kbps;
			}
		}
	}
}
