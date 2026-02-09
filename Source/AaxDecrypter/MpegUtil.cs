using AAXClean.Codecs;
using Mpeg4Lib;
using NAudio.Lame;
using System;
using System.Linq;

namespace AaxDecrypter;

public static class MpegUtil
{
	private const string TagDomain = "com.pilabor.tone";
	public static void ConfigureLameOptions(
		Mpeg4File mp4File,
		LameConfig lameConfig,
		bool downsample,
		bool matchSourceBitrate,
		ChapterInfo? chapters)
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
		lameConfig.ID3 = mp4File.MetadataItems.ToIDTags();

		if (mp4File.MetadataItems.AppleListBox.GetFreeformTagString(TagDomain, "SUBTITLE") is string subtitle)
			lameConfig.ID3.Subtitle = subtitle;

		if (chapters?.Count > 0)
		{
			var cue = Cue.CreateContents(lameConfig.ID3.Title + ".mp3", chapters);
			lameConfig.ID3.UserDefinedText.Add("CUESHEET", cue);
		}

		//Copy over all other freeform tags
		foreach (var t in mp4File.MetadataItems.AppleListBox.Tags.OfType<Mpeg4Lib.Boxes.FreeformTagBox>())
		{
			if (t.Name?.Name is string name &&
				t.Mean?.ReverseDnsDomain is string domain &&
				!lameConfig.ID3.UserDefinedText.ContainsKey(name) &&
				mp4File.MetadataItems.AppleListBox.GetFreeformTagString(domain, name) is string tagStr &&
				!string.IsNullOrWhiteSpace(tagStr))
				lameConfig.ID3.UserDefinedText.Add(name, tagStr);
		}
	}
}
