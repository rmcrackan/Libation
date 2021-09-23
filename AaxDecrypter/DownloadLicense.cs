using AAXClean;
using Dinah.Core;

namespace AaxDecrypter
{
    public class DownloadLicense
    {
        public string DownloadUrl { get; }
        public string AudibleKey { get; }
        public string AudibleIV { get; }
        public string UserAgent { get; }
        public ChapterInfo ChapterInfo { get; set; }

        public DownloadLicense(string downloadUrl, string audibleKey, string audibleIV, string userAgent)
        {
            DownloadUrl = ArgumentValidator.EnsureNotNullOrEmpty(downloadUrl, nameof(downloadUrl));
            UserAgent = ArgumentValidator.EnsureNotNullOrEmpty(userAgent, nameof(userAgent));

            // no null/empty check. unencrypted files do not have these
            AudibleKey = audibleKey;
            AudibleIV = audibleIV;
        }
    }
}
