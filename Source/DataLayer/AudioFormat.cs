#nullable enable
using Newtonsoft.Json;

namespace DataLayer;

public enum Codec : byte
{
	Unknown,
	Mp3,
	AAC_LC,
	xHE_AAC,
	EC_3,
	AC_4
}

public class AudioFormat
{
	public static AudioFormat Default => new(Codec.Unknown, 0, 0, 0);
	[JsonIgnore]
	public bool IsDefault => Codec is Codec.Unknown && BitRate == 0 && SampleRate == 0 && ChannelCount == 0;
	[JsonIgnore]
	public Codec Codec { get; set; }
	public int SampleRate { get; set; }
	public int ChannelCount { get; set; }
	public int BitRate { get; set; }

	public AudioFormat(Codec codec, int bitRate, int sampleRate, int channelCount)
	{
		Codec = codec;
		BitRate = bitRate;
		SampleRate = sampleRate;
		ChannelCount = channelCount;
	}

	public string CodecString => Codec switch
	{
		Codec.Mp3 => "mp3",
		Codec.AAC_LC => "AAC-LC",
		Codec.xHE_AAC => "xHE-AAC",
		Codec.EC_3 => "EC-3",
		Codec.AC_4 => "AC-4",
		Codec.Unknown or _ => "[Unknown]",
	};

	//Property     | Start | Num  |   Max   | Current Max |
	//             |  Bit  | Bits |  Value  | Value Used  |
	//-----------------------------------------------------
	//Codec	       |   35  |   4  |      15 |         5   |
	//BitRate      |   23  |  12  |   4_095 |       768   |
	//SampleRate   |    5  |  18  | 262_143 |    48_000   |
	//ChannelCount |    0  |   5  |      31 |         6   |
	public long Serialize() =>
		((long)Codec << 35) |
		((long)BitRate << 23) |
		((long)SampleRate << 5) |
		(long)ChannelCount;

	public static AudioFormat Deserialize(long value)
	{
		var codec = (Codec)((value >> 35) & 15);
		var bitRate = (int)((value >> 23) & 4_095);
		var sampleRate = (int)((value >> 5) & 262_143);
		var channelCount = (int)(value & 31);
		return new AudioFormat(codec, bitRate, sampleRate, channelCount);
	}

	public override string ToString()
		=> IsDefault ? "[Unknown Audio Format]"
		: $"{CodecString} ({ChannelCount}ch | {SampleRate:N0}Hz | {BitRate}kbps)";
}
