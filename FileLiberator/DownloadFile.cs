using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
	// currently only used to download the .zip flies for upgrade
	public class DownloadFile : Streamable
	{
		public async Task<string> PerformDownloadFileAsync(string downloadUrl, string proposedDownloadFilePath)
		{
			var client = new HttpClient();

			var progress = new Progress<DownloadProgress>(OnStreamingProgressChanged);

			OnStreamingBegin(proposedDownloadFilePath);

			try
			{
				var actualDownloadedFilePath = await client.DownloadFileAsync(downloadUrl, proposedDownloadFilePath, progress);
				OnFileCreated("Upgrade", actualDownloadedFilePath);
				return actualDownloadedFilePath;
			}
			finally
			{
				OnStreamingCompleted(proposedDownloadFilePath);
			}
		}
	}
}
