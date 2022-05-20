using System;

namespace AppScaffolding
{
    public record UpgradeProperties(string ZipUrl, string HtmlUrl, string ZipName, Version LatestRelease);
}
