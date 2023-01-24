using AAXClean;
using System;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    public interface IDownloadOptions
	{
        event EventHandler<long> DownloadSpeedChanged;
        FileManager.ReplacementCharacters ReplacementCharacters { get; }
        string DownloadUrl { get; }
        string UserAgent { get; }
        string AudibleKey { get; }
        string AudibleIV { get; }
        OutputFormat OutputFormat { get; }
        bool TrimOutputToChapterLength { get; }
        bool RetainEncryptedFile { get; }
        bool StripUnabridged { get; }
        bool CreateCueSheet { get; }
        bool DownloadClipsBookmarks { get; }
        long DownloadSpeedBps { get; }
        ChapterInfo ChapterInfo { get; }
        bool FixupFile { get; }
        NAudio.Lame.LameConfig LameConfig { get; }
        bool Downsample { get; }
        bool MatchSourceBitrate { get; }
        bool MoveMoovToBeginning { get; }
        string GetMultipartFileName(MultiConvertFileProperties props);
        string GetMultipartTitleName(MultiConvertFileProperties props);
        Task<string> SaveClipsAndBookmarks(string fileName);
	}    
}
