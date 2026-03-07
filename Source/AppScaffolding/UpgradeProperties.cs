using System;
using System.Text.RegularExpressions;

namespace AppScaffolding;

/// <summary>Result of a version check: definitive up-to-date, update available, or unable to determine.</summary>
public enum VersionCheckOutcome
{
	UpToDate,
	UpdateAvailable,
	UnableToDetermine,
}

/// <summary>Result of checking for a new version. Use <see cref="Outcome"/> to show the right message; <see cref="UpgradeProperties"/> is set only when <see cref="Outcome"/> is <see cref="VersionCheckOutcome.UpdateAvailable"/>.</summary>
public record VersionCheckResult(VersionCheckOutcome Outcome, UpgradeProperties? UpgradeProperties = null);

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

        var text = NoAppBlockRegex().Replace(notes, "");
		text = LinkStripRegex().Replace(text, "$1");
		Notes = text.Trim();
	}

	[GeneratedRegex(@"\[(.*)\]\(.*\)")]
	private static partial Regex LinkStripRegex();

    [GeneratedRegex(@"<!-- BEGIN NO-APP -->.*?<!-- END NO-APP -->", RegexOptions.Singleline)]
    private static partial Regex NoAppBlockRegex();
}
