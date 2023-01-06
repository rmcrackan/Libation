using System;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core.Net.Http;
using FileManager;

namespace AaxDecrypter
{
	public class UnencryptedAudiobookDownloader : AudiobookDownloadBase
	{

		public UnencryptedAudiobookDownloader(string outFileName, string cacheDirectory, IDownloadOptions dlLic)
			: base(outFileName, cacheDirectory, dlLic) { }

		public override async Task<bool> RunAsync()
		{
			try
			{
				Serilog.Log.Information("Begin downloading unencrypted audiobook.");

				//Step 1
				Serilog.Log.Information("Begin Step 1: Get Mp3 Metadata");
				if (await Task.Run(Step_GetMetadata))
					Serilog.Log.Information("Completed Step 1: Get Mp3 Metadata");
				else
				{
					Serilog.Log.Information("Failed to Complete Step 1: Get Mp3 Metadata");
					return false;
				}

				//Step 2
				Serilog.Log.Information("Begin Step 2: Download Audiobook");
				if (await Task.Run(Step_DownloadAudiobookAsSingleFile))
					Serilog.Log.Information("Completed Step 2: Download Audiobook");
				else
				{
					Serilog.Log.Information("Failed to Complete Step 2: Download Audiobook");
					return false;
				}

				//Step 3
				if (DownloadOptions.DownloadClipsBookmarks)
				{
					Serilog.Log.Information("Begin Downloading Clips and Bookmarks");
					if (await Task.Run(Step_DownloadClipsBookmarks))
						Serilog.Log.Information("Completed Downloading Clips and Bookmarks");
					else
					{
						Serilog.Log.Information("Failed to Download Clips and Bookmarks");
						return false;
					}
				}

				//Step 4
				Serilog.Log.Information("Begin Step 3: Cleanup");
				if (await Task.Run(Step_Cleanup))
					Serilog.Log.Information("Completed Step 3: Cleanup");
				else
				{
					Serilog.Log.Information("Failed to Complete Step 3: Cleanup");
					return false;
				}

				Serilog.Log.Information("Completed download and convert Aaxc To {format}", DownloadOptions.OutputFormat);
				return true;
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Error encountered in download and convert Aaxc To {format}", DownloadOptions.OutputFormat);
				return false;
			}
		}

		public override Task CancelAsync()
		{
			IsCanceled = true;
			CloseInputFileStream();
			return Task.CompletedTask;
		}

		protected bool Step_GetMetadata()
		{
			OnRetrievedCoverArt(null);

			return !IsCanceled;
		}

		private bool Step_DownloadAudiobookAsSingleFile()
		{
			DateTime startTime = DateTime.Now;

			// MUST put InputFileStream.Length first, because it starts background downloader.

			while (InputFileStream.Length > InputFileStream.WritePosition && !InputFileStream.IsCancelled) 
			{
				var rate = InputFileStream.WritePosition / (DateTime.Now - startTime).TotalSeconds;

				var estTimeRemaining = (InputFileStream.Length - InputFileStream.WritePosition) / rate;

				if (double.IsNormal(estTimeRemaining))
					OnDecryptTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

				var progressPercent = (double)InputFileStream.WritePosition / InputFileStream.Length;

				OnDecryptProgressUpdate(
					new DownloadProgress
					{
						ProgressPercentage = 100 * progressPercent,
						BytesReceived = (long)(InputFileStream.Length * progressPercent),
						TotalBytesToReceive = InputFileStream.Length
					});
				Thread.Sleep(200);
			}

			CloseInputFileStream();
			
			var realOutputFileName = FileUtility.SaferMoveToValidPath(InputFileStream.SaveFilePath, OutputFileName, DownloadOptions.ReplacementCharacters);
			SetOutputFileName(realOutputFileName);
			OnFileCreated(realOutputFileName);

			return !IsCanceled;
		}
	}
}
