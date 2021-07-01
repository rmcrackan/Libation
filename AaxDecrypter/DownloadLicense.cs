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
            ArgumentValidator.EnsureNotNullOrEmpty(downloadUrl, nameof(downloadUrl));
            ArgumentValidator.EnsureNotNullOrEmpty(audibleKey, nameof(audibleKey));
            ArgumentValidator.EnsureNotNullOrEmpty(audibleIV, nameof(audibleIV));
            ArgumentValidator.EnsureNotNullOrEmpty(userAgent, nameof(userAgent));

            DownloadUrl = downloadUrl;
            AudibleKey = audibleKey;
            AudibleIV = audibleIV;
            UserAgent = userAgent;
        }
    }
}
