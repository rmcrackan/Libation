using DataLayer;
using System;

namespace LibationUiBase
{
	public class LastDownloadStatus : IComparable
	{
		public bool IsValid => LastDownloadedVersion is not null && LastDownloaded.HasValue;
		public Version LastDownloadedVersion { get; }
		public DateTime? LastDownloaded { get; }
		public string ToolTipText => IsValid ? $"Double click to open v{LastDownloadedVersion.ToString(3)} release notes" : "";

		public LastDownloadStatus() { }
		public LastDownloadStatus(UserDefinedItem udi)
		{
			LastDownloadedVersion = udi.LastDownloadedVersion;
			LastDownloaded = udi.LastDownloaded;
		}

		public void OpenReleaseUrl()
		{
			if (IsValid)
				Dinah.Core.Go.To.Url($"{AppScaffolding.LibationScaffolding.RepositoryUrl}/releases/tag/v{LastDownloadedVersion.ToString(3)}");
		}

		public override string ToString()
			=> IsValid ? $"{dateString()}\n\nLibation v{LastDownloadedVersion.ToString(3)}" : "";

		//Call ToShortDateString to use current culture's date format.
		private string dateString() => $"{LastDownloaded.Value.ToShortDateString()} {LastDownloaded.Value:HH:mm}";

		public int CompareTo(object obj)
		{
			if (obj is not LastDownloadStatus second) return -1;
			else if (IsValid && !second.IsValid) return -1;
			else if (!IsValid && second.IsValid) return 1;
			else if (!IsValid && !second.IsValid) return 0;
			else return LastDownloaded.Value.CompareTo(second.LastDownloaded.Value);
		}
	}
}
