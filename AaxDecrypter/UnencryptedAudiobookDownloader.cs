using Dinah.Core.IO;
using Dinah.Core.Net.Http;
using Dinah.Core.StepRunner;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace AaxDecrypter
{
	public class UnencryptedAudiobookDownloader : AudiobookDownloadBase
	{
		protected override StepSequence steps { get; }

		public UnencryptedAudiobookDownloader(string outFileName, string cacheDirectory, DownloadLicense dlLic)
			: base(outFileName, cacheDirectory, dlLic)
		{

			steps = new StepSequence
			{
				Name = "Download Mp3 Audiobook",

				["Step 1: Get Mp3 Metadata"] = Step1_GetMetadata,
				["Step 2: Download Audiobook"] = Step2_DownloadAudiobookAsSingleFile,
				["Step 3: Create Cue"] = Step3_CreateCue,
				["Step 4: Cleanup"] = Step4_Cleanup,
			};
		}

		public override void Cancel()
		{
			isCanceled = true;
			CloseInputFileStream();
		}

		protected override int GetSpeedup(TimeSpan elapsed)
		{
			//Not implemented
			return 0;
		}

		protected override bool Step1_GetMetadata()
		{
			OnRetrievedCoverArt(null);

			return !isCanceled;
		}

		protected override bool Step2_DownloadAudiobookAsSingleFile()
		{
			DateTime startTime = DateTime.Now;

			//MUST put InputFileStream.Length first, because it starts background downloader.

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

			if (File.Exists(outputFileName))
				FileExt.SafeDelete(outputFileName);

			FileExt.SafeMove(InputFileStream.SaveFilePath, outputFileName);

			return !isCanceled;
		}
	}
}
