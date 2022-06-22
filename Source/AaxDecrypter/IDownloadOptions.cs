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
        ChapterInfo ChapterInfo { get; set; }
        NAudio.Lame.LameConfig LameConfig { get; set; }
        bool Downsample { get; set; }
        bool MatchSourceBitrate { get; set; }
        string GetMultipartFileName(MultiConvertFileProperties props);
        string GetMultipartTitleName(MultiConvertFileProperties props);
    }    
}
