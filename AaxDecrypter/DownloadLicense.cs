using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    public class DownloadLicense
    {
        public string DownloadUrl { get; }
        public string AudibleKey { get; }
        public string AudibleIV { get; }
        public string UserAgent { get; }

        public DownloadLicense(string downloadUrl, string audibleKey, string audibleIV, string userAgent)
        {
            DownloadUrl = downloadUrl;
            AudibleKey = audibleKey;
            AudibleIV = audibleIV;
            UserAgent = userAgent;
        }
    }
}
