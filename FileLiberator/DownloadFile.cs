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

			var progress = new Progress<DownloadProgress>();
			progress.ProgressChanged += (_, e) => OnStreamingProgressChanged(e);

			OnStreamingBegin(proposedDownloadFilePath);

			try
			{
				var actualDownloadedFilePath = await client.DownloadFileAsync(downloadUrl, proposedDownloadFilePath, progress);
				return actualDownloadedFilePath;
			}
			finally
			{
				OnStreamingCompleted(proposedDownloadFilePath);
			}
		}
	}
}
