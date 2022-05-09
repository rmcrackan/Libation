using System;
using System.Threading;
using Dinah.Core.Net.Http;
using Dinah.Core.StepRunner;
using FileManager;

namespace AaxDecrypter
{
	public class UnencryptedAudiobookDownloader : AudiobookDownloadBase
	{
		protected override StepSequence Steps { get; }

		public UnencryptedAudiobookDownloader(string outFileName, string cacheDirectory, DownloadOptions dlLic)
			: base(outFileName, cacheDirectory, dlLic)
		{
			Steps = new StepSequence
			{
				Name = "Download Mp3 Audiobook",

				["Step 1: Get Mp3 Metadata"] = Step_GetMetadata,
				["Step 2: Download Audiobook"] = Step_DownloadAudiobookAsSingleFile,
				["Step 3: Create Cue"] = Step_CreateCue,
				["Step 4: Cleanup"] = Step_Cleanup,
			};
		}

		public override void Cancel()
		{
			IsCanceled = true;
			CloseInputFileStream();
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

			var realOutputFileName = FileUtility.SaferMoveToValidPath(InputFileStream.SaveFilePath, OutputFileName);
			SetOutputFileName(realOutputFileName);
			OnFileCreated(realOutputFileName);

			return !IsCanceled;
		}
	}
}
