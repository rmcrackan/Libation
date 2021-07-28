using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
	// currently only used to download the .zip flies for upgrade
	public class DownloadFile : IDownloadable
	{
		public event EventHandler<string> DownloadBegin;
		public event EventHandler<DownloadProgress> DownloadProgressChanged;
		public event EventHandler<string> DownloadCompleted;

		public async Task<string> PerformDownloadFileAsync(string downloadUrl, string proposedDownloadFilePath)
		{
			var client = new HttpClient();

			var progress = new Progress<DownloadProgress>();
			progress.ProgressChanged += (_, e) => DownloadProgressChanged?.Invoke(this, e);

			DownloadBegin?.Invoke(this, proposedDownloadFilePath);

			try
			{
				var actualDownloadedFilePath = await client.DownloadFileAsync(downloadUrl, proposedDownloadFilePath, progress);
				return actualDownloadedFilePath;
			}
			finally
			{
				DownloadCompleted?.Invoke(this, proposedDownloadFilePath);
			}
		}
	}
}
