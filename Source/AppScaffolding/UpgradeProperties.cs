using System;
using System.Text.RegularExpressions;

namespace AppScaffolding
{
	public record UpgradeProperties
	{
		private static readonly Regex linkstripper = new Regex(@"\[(.*)\]\(.*\)");
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
			Notes = stripMarkdownLinks(notes);
		}
		private string stripMarkdownLinks(string body)
		{
			body = body.Replace(@"\", "");
			var matches = linkstripper.Matches(body);

			foreach (Match match in matches)
				body = body.Replace(match.Groups[0].Value, match.Groups[1].Value);

			return body;
		}
	}
}
