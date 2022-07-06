using AAXClean;

namespace AaxDecrypter
{
    public interface IDownloadOptions
	{
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
        ChapterInfo ChapterInfo { get; }
        bool FixupFile { get; }
        NAudio.Lame.LameConfig LameConfig { get; }
        bool Downsample { get; }
        bool MatchSourceBitrate { get; }
        string GetMultipartFileName(MultiConvertFileProperties props);
        string GetMultipartTitleName(MultiConvertFileProperties props);
    }    
}
