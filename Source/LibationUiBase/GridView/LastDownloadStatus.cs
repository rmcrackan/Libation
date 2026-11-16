using DataLayer;
using LibationFileManager;
using NuGet.Versioning;
using System;
using LibationUiBase.Resources;

namespace LibationUiBase.GridView;

public class LastDownloadStatus : IComparable
{
	public bool IsValid => LastDownloadedVersion is not null && LastDownloaded.HasValue;
	public AudioFormat? LastDownloadedFormat { get; }
	public string? LastDownloadedFileVersion { get; }
	public NuGetVersion? LastDownloadedVersion { get; }
	public DateTime? LastDownloaded { get; }
	public string ToolTipText => IsValid ? string.Format(Strings.LastDownloadStatus_DoubleClickToOpenVRelease, LastDownloadedVersion!.ToVersionString()) : "";

	public LastDownloadStatus() { }
	public LastDownloadStatus(UserDefinedItem udi)
	{
		LastDownloadedVersion = udi.LastDownloadedVersion;
		LastDownloadedFormat = udi.LastDownloadedFormat;
		LastDownloadedFileVersion = udi.LastDownloadedFileVersion;
		LastDownloaded = udi.LastDownloaded;
	}

	public void OpenReleaseUrl()
	{
		if (IsValid)
			Dinah.Core.Go.To.Url($"{AppScaffolding.LibationScaffolding.RepositoryUrl}/releases/tag/v{LastDownloadedVersion.ToVersionString()}");
	}

	public override string ToString()
		=> IsValid ? string.Format(Strings.LastDownloadStatus_LibationV, dateString(), versionString(), LastDownloadedFormat, LastDownloadedVersion.ToVersionString()) : "";

	private string versionString() => LastDownloadedFileVersion is string ver ? string.Format(Strings.LastDownloadStatus_FileV, ver) : "";

	//Call ToShortDateString to use current culture's date format.
	private string dateString() => LastDownloaded.HasValue ? string.Format(Strings.LastDownloadStatus_Text, LastDownloaded.Value.ToShortDateString(), LastDownloaded.Value) : string.Empty;

	public int CompareTo(object? obj)
	{
		if (obj is not LastDownloadStatus second) return -1;
		else if (!IsValid && !second.IsValid) return 0;
		else if (!second.IsValid) return -1;
		else if (!IsValid) return 1;
		else return LastDownloaded!.Value.CompareTo(second.LastDownloaded!.Value);
	}
}
