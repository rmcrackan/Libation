using System;

namespace DataLayer
{
    internal enum AudioFormatEnum : long
    {
        //Defining the enum this way ensures that when comparing:
        //LC_128_44100_stereo > LC_64_44100_stereo > LC_64_22050_stereo > LC_64_22050_stereo
        //This matches how audible interprets these codecs when specifying quality using AudibleApi.DownloadQuality
        //I've never seen mono formats.
        Unknown = 0,
        LC_32_22050_stereo = (32L << 18) | (22050 << 2) | 2,
        LC_64_22050_stereo = (64L << 18) | (22050 << 2) | 2,
        LC_64_44100_stereo = (64L << 18) | (44100 << 2) | 2,
        LC_128_44100_stereo = (128L << 18) | (44100 << 2) | 2,
    }

    public class AudioFormat : IComparable<AudioFormat>, IComparable
    {
        internal int AudioFormatID { get; private set; }
        public int Bitrate { get; private init; }
        public int SampleRate { get; private init; }
        public int Channels { get; private init; }
        public bool IsValid => Bitrate != 0 && SampleRate != 0 && Channels != 0;

        public static AudioFormat FromString(string formatStr)
        {
            if (Enum.TryParse(formatStr, ignoreCase: true, out AudioFormatEnum enumVal))
                return FromEnum(enumVal);
            return FromEnum(AudioFormatEnum.Unknown);
        }

        internal static AudioFormat FromEnum(AudioFormatEnum enumVal)
        {
            var val = (long)enumVal;

            return new()
            {
                Bitrate = (int)(val >> 18),
                SampleRate = (int)(val >> 2) & ushort.MaxValue,
                Channels = (int)(val & 3)
            };
        }
        internal AudioFormatEnum ToEnum()
        {
            var val = (AudioFormatEnum)(((long)Bitrate << 18) | ((long)SampleRate << 2) | (long)Channels);

            return Enum.IsDefined(val) ?
                val : AudioFormatEnum.Unknown;
        }

        public override string ToString()
            => IsValid ?
            $"{Bitrate} Kbps, {SampleRate / 1000d:F1} kHz, {(Channels == 2 ? "Stereo" : Channels)}" :
            "Unknown";

        public int CompareTo(AudioFormat other) => ToEnum().CompareTo(other.ToEnum());

        public int CompareTo(object obj) => CompareTo(obj as AudioFormat);
    }
}
