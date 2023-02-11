using NPOI.XWPF.UserModel;
using System;
using System.Text.RegularExpressions;

namespace AppScaffolding
{
	public partial record UpgradeProperties
	{
		public string ZipUrl { get; }
		public string HtmlUrl { get; }
		public string ZipName { get; }
		public Version LatestRelease { get; }
		public string Notes { get; }

		public UpgradeProperties(string zipUrl, string htmlUrl, string zipName, Version latestRelease, string notes)
		{
			ZipName = zipName;
			HtmlUrl = htmlUrl;
			ZipUrl = zipUrl;
			LatestRelease = latestRelease;
			Notes = LinkStripRegex().Replace(notes, "$1");
		}

		[GeneratedRegex(@"\[(.*)\]\(.*\)")]
		private static partial Regex LinkStripRegex();
	}
}
