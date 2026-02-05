using AAXClean;
using DataLayer;
using FileManager;
using Mpeg4Lib.Boxes;
using Mpeg4Lib.ID3;
using Mpeg4Lib.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace AaxDecrypter;

/// <summary> Read audio codec, bitrate, sample rate, and channel count from MP4 and MP3 audio files. </summary>
public static class AudioFormatDecoder
{
	public static AudioFormat FromMpeg4(string filename)
	{
		using var fileStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		return FromMpeg4(new Mp4File(fileStream));
	}

	public static AudioFormat FromMpeg4(Mp4File mp4File)
	{
		Codec codec;
		if (mp4File.AudioSampleEntry.Dac4 is not null)
		{
			codec = Codec.AC_4;
		}
		else if (mp4File.AudioSampleEntry.Dec3 is not null)
		{
			codec = Codec.EC_3;
		}
		else if (mp4File.AudioSampleEntry.Esds is EsdsBox esds)
		{
			var objectType = esds.ES_Descriptor.DecoderConfig.AudioSpecificConfig.AudioObjectType;
			codec
				= objectType == 2 ? Codec.AAC_LC
				: objectType == 42 ? Codec.xHE_AAC
				: Codec.Unknown;
		}
		else
			return AudioFormat.Default;

		var bitrate = (int)Math.Round(mp4File.AverageBitrate / 1024d);

		return new AudioFormat(codec, bitrate, mp4File.TimeScale, mp4File.AudioChannels);
	}

	public static AudioFormat FromMpeg3(LongPath mp3Filename)
	{
		using var mp3File = File.Open(mp3Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		if (Id3Header.Create(mp3File) is Id3Header id3header)
			id3header.SeekForwardToPosition(mp3File, mp3File.Position + id3header.Size);
		else
		{
			Serilog.Log.Logger.Debug("File appears not to have ID3 tags.");
			mp3File.Position = 0;
		}

		if (!SeekToFirstKeyFrame(mp3File))
		{
			Serilog.Log.Logger.Warning("Invalid frame sync read from file at end of ID3 tag.");
			return AudioFormat.Default;
		}

		var mpegSize = mp3File.Length - mp3File.Position;
		if (mpegSize < 64)
		{
			Serilog.Log.Logger.Warning("Remaining file length is too short to contain any mp3 frames. {File}", mp3Filename);
			return AudioFormat.Default;
		}

		#region read first mp3 frame header
		//https://www.codeproject.com/Articles/8295/MPEG-Audio-Frame-Header#VBRIHeader
		var reader = new BitReader(mp3File.ReadBlock(4));
		reader.Position = 11; //Skip frame header magic bits
		var versionId = (Version)reader.Read(2);
		var layerDesc = (Layer)reader.Read(2);

		if (layerDesc is not Layer.Layer_3)
		{
			Serilog.Log.Logger.Warning("Could not read mp3 data from {layerVersion} file.", layerDesc);
			return AudioFormat.Default;
		}

		if (versionId is Version.Reserved)
		{
			Serilog.Log.Logger.Warning("Mp3 data data cannot be read from a file with version = 'Reserved'");
			return AudioFormat.Default;
		}

		var protectionBit = reader.ReadBool();
		var bitrateIndex = reader.Read(4);
		var freqIndex = reader.Read(2);
		_ = reader.ReadBool();                  //Padding bit
		_ = reader.ReadBool();                  //Private bit
		var channelMode = reader.Read(2);
		_ = reader.Read(2);                     //Mode extension
		_ = reader.ReadBool();                  //Copyright
		_ = reader.ReadBool();                  //Original
		_ = reader.Read(2);                     //Emphasis
		#endregion

		//Read the sample rate,and channels from the first frame's header.
		var sampleRate = Mp3SampleRateIndex[versionId][freqIndex];
		var channelCount = channelMode == 3 ? 1 : 2;

		//Try to read variable bitrate info from the first frame.
		//Revert to fixed bitrate from frame header if not found.
		var bitrate
			= TryReadXingBitrate(out var br) ? br
			: TryReadVbriBitrate(out br) ? br
			: Mp3BitrateIndex[versionId][bitrateIndex];

		return new AudioFormat(Codec.Mp3, bitrate, sampleRate, channelCount);

		#region Variable bitrate header readers
		bool TryReadXingBitrate(out int bitrate)
		{
			const int XingHeader = 0x58696e67;
			const int InfoHeader = 0x496e666f;

			var sideInfoSize = GetSideInfo(channelCount == 2, versionId) + (protectionBit ? 0 : 2);
			mp3File.Position += sideInfoSize;

			if (mp3File.ReadUInt32BE() is XingHeader or InfoHeader)
			{
				//Xing or Info header (common)
				var flags = mp3File.ReadUInt32BE();
				bool hasFramesField = (flags & 1) == 1;
				bool hasBytesField = (flags & 2) == 2;

				if (hasFramesField)
				{
					var numFrames = mp3File.ReadUInt32BE();
					if (hasBytesField)
					{
						mpegSize = mp3File.ReadUInt32BE();
					}

					var samplesPerFrame = GetSamplesPerFrame(sampleRate);
					var duration = samplesPerFrame * numFrames / sampleRate;
					bitrate = (short)(mpegSize / duration / 1024 * 8);
					return true;
				}
			}
			else
				mp3File.Position -= sideInfoSize + 4;

			bitrate = 0;
			return false;
		}

		bool TryReadVbriBitrate(out int bitrate)
		{
			const int VBRIHeader = 0x56425249;

			mp3File.Position += 32;

			if (mp3File.ReadUInt32BE() is VBRIHeader)
			{
				//VBRI header (rare)
				_ = mp3File.ReadBlock(6);
				mpegSize = mp3File.ReadUInt32BE();
				var numFrames = mp3File.ReadUInt32BE();

				var samplesPerFrame = GetSamplesPerFrame(sampleRate);
				var duration = samplesPerFrame * numFrames / sampleRate;
				bitrate = (short)(mpegSize / duration / 1024 * 8);
				return true;
			}
			bitrate = 0;
			return false;
		}
		#endregion
	}

	#region MP3 frame decoding helpers
	private static bool SeekToFirstKeyFrame(Stream file)
	{
		//Frame headers begin with first 11 bits set.
		const int MaxSeekBytes = 4096;
		var maxPosition = Math.Min(file.Length, file.Position + MaxSeekBytes) - 2;

		while (file.Position < maxPosition)
		{
			if (file.ReadByte() == 0xff)
			{
				if ((file.ReadByte() & 0xe0) == 0xe0)
				{
					file.Position -= 2;
					return true;
				}
				file.Position--;
			}
		}
		return false;
	}

	private enum Version
	{
		Version_2_5,
		Reserved,
		Version_2,
		Version_1
	}

	private enum Layer
	{
		Reserved,
		Layer_3,
		Layer_2,
		Layer_1
	}

	private static double GetSamplesPerFrame(int sampleRate) => sampleRate >= 32000 ? 1152 : 576;

	private static byte GetSideInfo(bool stereo, Version version) => (stereo, version) switch
	{
		(true, Version.Version_1) => 32,
		(true, Version.Version_2 or Version.Version_2_5) => 17,
		(false, Version.Version_1) => 17,
		(false, Version.Version_2 or Version.Version_2_5) => 9,
		_ => 0,
	};

	private static readonly Dictionary<Version, ushort[]> Mp3SampleRateIndex = new()
	{
		{ Version.Version_2_5, [11025, 12000,  8000] },
		{ Version.Version_2,   [22050, 24000, 16000] },
		{ Version.Version_1,   [44100, 48000, 32000] },
	};

	private static readonly Dictionary<Version, short[]> Mp3BitrateIndex = new()
	{
		{ Version.Version_2_5, [-1, 8,16,24,32,40,48,56, 64, 80, 96,112,128,144,160,-1]},
		{ Version.Version_2,   [-1, 8,16,24,32,40,48,56, 64, 80, 96,112,128,144,160,-1]},
		{ Version.Version_1,   [-1,32,40,48,56,64,80,96,112,128,160,192,224,256,320,-1]}
	};
	#endregion
}
