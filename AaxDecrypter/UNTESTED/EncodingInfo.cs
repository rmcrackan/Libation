using System;
using System.Diagnostics;
using Dinah.Core.Diagnostics;

namespace AaxDecrypter
{
    public class EncodingInfo
    {
        public int sampleRate { get; } = 44100;
        public int channels { get; } = 2;
        public int originalBitrate { get; }

        public EncodingInfo(string file)
        {
            var info = new ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.ffprobePath,
                Arguments = "-loglevel panic -show_streams -print_format flat \"" + file + "\""
            };
            var end = info.RunHidden().Output;

            foreach (string str2 in end.Split('\n'))
            {
                string[] strArray = str2.Split('=');
                switch (strArray[0])
                {
                    case "streams.stream.0.channels":
                        this.channels = int.Parse(strArray[1].Replace("\"", "").TrimEnd('\r', '\n'));
                        break;
                    case "streams.stream.0.sample_rate":
                        this.sampleRate = int.Parse(strArray[1].Replace("\"", "").TrimEnd('\r', '\n'));
                        break;
                    case "streams.stream.0.bit_rate":
                        string s = strArray[1].Replace("\"", "").TrimEnd('\r', '\n');
                        this.originalBitrate = (int)Math.Round(double.Parse(s) / 1000.0, MidpointRounding.AwayFromZero);
                        break;
                }
            }
        }
    }
}
