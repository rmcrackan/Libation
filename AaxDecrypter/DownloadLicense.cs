using AAXClean;
using Dinah.Core;

namespace AaxDecrypter
{
    public class DownloadLicense
    {
        public string DownloadUrl { get; }
        public string UserAgent { get; }
        public string AudibleKey { get; init; }
        public string AudibleIV { get; init; }
        public OutputFormat OutputFormat { get; init; }
        public bool TrimOutputToChapterLength { get; init; }
        public ChapterInfo ChapterInfo { get; set; }

        public DownloadLicense(string downloadUrl, string userAgent)
        {
            DownloadUrl = ArgumentValidator.EnsureNotNullOrEmpty(downloadUrl, nameof(downloadUrl));
            UserAgent = ArgumentValidator.EnsureNotNullOrEmpty(userAgent, nameof(userAgent));

            // no null/empty check for key/iv. unencrypted files do not have them
        }
    }
}
