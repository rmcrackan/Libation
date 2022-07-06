using AaxDecrypter;
using AAXClean;
using Dinah.Core;
using DataLayer;
using LibationFileManager;
using FileManager;

namespace FileLiberator
{
    public class DownloadOptions : IDownloadOptions
    {
        public LibraryBook LibraryBook { get; }
        public LibraryBookDto LibraryBookDto { get; }
        public string DownloadUrl { get; }
        public string UserAgent { get; }
        public string AudibleKey { get; init; }
        public string AudibleIV { get; init; }
        public AaxDecrypter.OutputFormat OutputFormat { get; init; }
        public bool TrimOutputToChapterLength { get; init; }
        public bool RetainEncryptedFile { get; init; }
        public bool StripUnabridged { get; init; }
        public bool CreateCueSheet { get; init; }
        public ChapterInfo ChapterInfo { get; init; }
        public bool FixupFile { get; init; }
        public NAudio.Lame.LameConfig LameConfig { get; init; }
        public bool Downsample { get; init; }
        public bool MatchSourceBitrate { get; init; }
        public ReplacementCharacters ReplacementCharacters => Configuration.Instance.ReplacementCharacters;

        public string GetMultipartFileName(MultiConvertFileProperties props)
            => Templates.ChapterFile.GetFilename(LibraryBookDto, props);

        public string GetMultipartTitleName(MultiConvertFileProperties props)
            => Templates.ChapterTitle.GetTitle(LibraryBookDto, props);

        public DownloadOptions(LibraryBook libraryBook, string downloadUrl, string userAgent)
        {
            LibraryBook = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook));
            DownloadUrl = ArgumentValidator.EnsureNotNullOrEmpty(downloadUrl, nameof(downloadUrl));
            UserAgent = ArgumentValidator.EnsureNotNullOrEmpty(userAgent, nameof(userAgent));

            LibraryBookDto = LibraryBook.ToDto();

            // no null/empty check for key/iv. unencrypted files do not have them
        }
    }
}
