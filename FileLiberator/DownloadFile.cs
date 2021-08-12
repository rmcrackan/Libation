using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core.Net.Http;

namespace FileLiberator
{
	// currently only used to download the .zip flies for upgrade
	public class DownloadFile : IStreamable
	{
		public event EventHandler<string> StreamingBegin;
		public event EventHandler<DownloadProgress> StreamingProgressChanged;
		public event EventHandler<string> StreamingCompleted;
		public event EventHandler<TimeSpan> StreamingTimeRemaining;

		public async Task<string> PerformDownloadFileAsync(string downloadUrl, string proposedDownloadFilePath)
		{
			var client = new HttpClient();

			var progress = new Progress<DownloadProgress>();
			progress.ProgressChanged += OnProgressChanged;

			StreamingBegin?.Invoke(this, proposedDownloadFilePath);

			try
			{
				var actualDownloadedFilePath = await client.DownloadFileAsync(downloadUrl, proposedDownloadFilePath, progress);
				return actualDownloadedFilePath;
			}
			finally
			{
				StreamingCompleted?.Invoke(this, proposedDownloadFilePath);
			}
		}
		private void OnProgressChanged(object sender, DownloadProgress e)
		{
			StreamingProgressChanged.Invoke(this, e);
		}
	}
}
