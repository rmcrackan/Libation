using AAXClean;
using System;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    public interface IDownloadOptions
    {
        event EventHandler<long> DownloadSpeedChanged;
        string DownloadUrl { get; }
        string UserAgent { get; }
        string AudibleKey { get; }
        string AudibleIV { get; }
        TimeSpan RuntimeLength { get; }
        OutputFormat OutputFormat { get; }
        bool TrimOutputToChapterLength { get; }
        bool RetainEncryptedFile { get; }
        bool StripUnabridged { get; }
        bool CreateCueSheet { get; }
        bool DownloadClipsBookmarks { get; }
        long DownloadSpeedBps { get; }
        ChapterInfo ChapterInfo { get; }
        bool FixupFile { get; }
		string AudibleProductId { get; }
		string Title { get; }
		string Subtitle { get; }
		string Publisher { get; }
		string Language { get; }
		string SeriesName { get; }
		float? SeriesNumber { get; }
		NAudio.Lame.LameConfig LameConfig { get; }
        bool Downsample { get; }
        bool MatchSourceBitrate { get; }
        bool MoveMoovToBeginning { get; }
        string GetMultipartFileName(MultiConvertFileProperties props);
        string GetMultipartTitle(MultiConvertFileProperties props);
        Task<string> SaveClipsAndBookmarksAsync(string fileName);
    }
}
