using AAXClean;
using AAXClean.Codecs;
using NAudio.Lame;
using System;

namespace AaxDecrypter
{
	public static class MpegUtil
	{
		private const string TagDomain = "com.pilabor.tone";
		public static void ConfigureLameOptions(
			Mp4File mp4File,
			LameConfig lameConfig,
			bool downsample,
			bool matchSourceBitrate,
			ChapterInfo chapters)
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

			//Setup metadata tags
			lameConfig.ID3 = mp4File.AppleTags.ToIDTags();

			if (mp4File.AppleTags.AppleListBox.GetFreeformTagString(TagDomain, "SUBTITLE") is string subtitle)
				lameConfig.ID3.Subtitle = subtitle;

			if (mp4File.AppleTags.AppleListBox.GetFreeformTagString(TagDomain, "LANGUAGE") is string lang)
				lameConfig.ID3.UserDefinedText.Add("LANGUAGE", lang);

			if (mp4File.AppleTags.AppleListBox.GetFreeformTagString(TagDomain, "SERIES") is string series)
				lameConfig.ID3.UserDefinedText.Add("SERIES", series);

			if (mp4File.AppleTags.AppleListBox.GetFreeformTagString(TagDomain, "PART") is string part)
				lameConfig.ID3.UserDefinedText.Add("PART", part);

			if (chapters?.Count > 0)
			{
				var cue = Cue.CreateContents(lameConfig.ID3.Title + ".mp3", chapters);
				lameConfig.ID3.UserDefinedText.Add("CUESHEET", cue);
			}
		}
	}
}
