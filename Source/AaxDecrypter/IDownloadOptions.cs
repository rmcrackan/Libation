using AAXClean;
using Mpeg4Lib;
using System;

#nullable enable
namespace AaxDecrypter
{
    public class KeyData
    {
        public byte[] KeyPart1 { get; }
        public byte[]? KeyPart2 { get; }

        public KeyData(byte[] keyPart1, byte[]? keyPart2 = null)
        {
            KeyPart1 = keyPart1;
            KeyPart2 = keyPart2;
        }

        [Newtonsoft.Json.JsonConstructor]
        public KeyData(string keyPart1, string? keyPart2 = null)
        {
            ArgumentNullException.ThrowIfNull(keyPart1, nameof(keyPart1));
            KeyPart1 = Convert.FromHexString(keyPart1);
            if (keyPart2 != null)
                KeyPart2 = Convert.FromHexString(keyPart2);
        }
	}    

	public interface IDownloadOptions
    {
        event EventHandler<long> DownloadSpeedChanged;
        string DownloadUrl { get; }
        string UserAgent { get; }
		KeyData[]? DecryptionKeys { get; }
        TimeSpan RuntimeLength { get; }
        OutputFormat OutputFormat { get; }
        bool StripUnabridged { get; }
        bool CreateCueSheet { get; }
        long DownloadSpeedBps { get; }
        ChapterInfo ChapterInfo { get; }
        bool FixupFile { get; }
		string? AudibleProductId { get; }
		string? Title { get; }
		string? Subtitle { get; }
		string? Publisher { get; }
		string? Language { get; }
		string? SeriesName { get; }
		string? SeriesNumber { get; }
		NAudio.Lame.LameConfig? LameConfig { get; }
        bool Downsample { get; }
        bool MatchSourceBitrate { get; }
        bool MoveMoovToBeginning { get; }
        string GetMultipartTitle(MultiConvertFileProperties props);
        public FileType? InputType { get; }
    }
}
