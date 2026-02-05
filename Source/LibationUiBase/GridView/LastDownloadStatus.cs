using DataLayer;
using LibationFileManager;
using System;

namespace LibationUiBase.GridView
{
	public class LastDownloadStatus : IComparable
	{
		public bool IsValid => LastDownloadedVersion is not null && LastDownloaded.HasValue;
		public AudioFormat? LastDownloadedFormat { get; }
		public string? LastDownloadedFileVersion { get; }
		public Version? LastDownloadedVersion { get; }
		public DateTime? LastDownloaded { get; }
		public string ToolTipText => IsValid ? $"Double click to open v{LastDownloadedVersion!.ToVersionString()} release notes" : "";

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
			=> IsValid ? $"""
				{dateString()} {versionString()}
				{LastDownloadedFormat}
				Libation v{LastDownloadedVersion.ToVersionString()}
				""" : "";

		private string versionString() => LastDownloadedFileVersion is string ver ? $"(File v.{ver})" : "";

		//Call ToShortDateString to use current culture's date format.
		private string dateString() => LastDownloaded.HasValue ? $"{LastDownloaded.Value.ToShortDateString()} {LastDownloaded.Value:HH:mm}" : string.Empty;

		public int CompareTo(object? obj)
		{
			if (obj is not LastDownloadStatus second) return -1;
			else if (!IsValid && !second.IsValid) return 0;
			else if (!second.IsValid) return -1;
			else if (!IsValid) return 1;
			else return LastDownloaded!.Value.CompareTo(second.LastDownloaded!.Value);
		}
	}
}
