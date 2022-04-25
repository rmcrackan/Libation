using System;
using AAXClean;
using Dinah.Core.Net.Http;

namespace AaxDecrypter
{
	public abstract class AaxcDownloadConvertBase : AudiobookDownloadBase
	{
		protected OutputFormat OutputFormat { get; }

		protected AaxFile AaxFile;

		protected AaxcDownloadConvertBase(string outFileName, string cacheDirectory, DownloadLicense dlLic, OutputFormat outputFormat)
			: base(outFileName, cacheDirectory, dlLic)
		{
			OutputFormat = outputFormat;
		}

		/// <summary>Setting cover art by this method will insert the art into the audiobook metadata</summary>
		public override void SetCoverArt(byte[] coverArt)
		{
			base.SetCoverArt(coverArt);
			if (coverArt is not null)
				AaxFile?.AppleTags.SetCoverArt(coverArt);
		}

		/// <summary>Optional step to run after Metadata is retrieved</summary>
		public Action<AaxFile> UpdateMetadata { get; set; }

		protected bool Step_GetMetadata()
		{
			AaxFile = new AaxFile(InputFileStream);

			UpdateMetadata?.Invoke(AaxFile);

			OnRetrievedTitle(AaxFile.AppleTags.TitleSansUnabridged);
			OnRetrievedAuthors(AaxFile.AppleTags.FirstAuthor ?? "[unknown]");
			OnRetrievedNarrators(AaxFile.AppleTags.Narrator ?? "[unknown]");
			OnRetrievedCoverArt(AaxFile.AppleTags.Cover);

			return !IsCanceled;
		}

		protected DownloadProgress Step_DownloadAudiobook_Start()
		{
			var zeroProgress = new DownloadProgress
			{
				BytesReceived = 0,
				ProgressPercentage = 0,
				TotalBytesToReceive = InputFileStream.Length
			};

			OnDecryptProgressUpdate(zeroProgress);

			AaxFile.SetDecryptionKey(DownloadLicense.AudibleKey, DownloadLicense.AudibleIV);
			return zeroProgress;
		}

		protected void Step_DownloadAudiobook_End(DownloadProgress zeroProgress)
		{
			AaxFile.Close();

			CloseInputFileStream();

			OnDecryptProgressUpdate(zeroProgress);
		}

		protected void AaxFile_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
		{
			var duration = AaxFile.Duration;
			var remainingSecsToProcess = (duration - e.ProcessPosition).TotalSeconds;
			var estTimeRemaining = remainingSecsToProcess / e.ProcessSpeed;

			if (double.IsNormal(estTimeRemaining))
				OnDecryptTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

			var progressPercent = e.ProcessPosition.TotalSeconds / duration.TotalSeconds;

			OnDecryptProgressUpdate(
				new DownloadProgress
				{
					ProgressPercentage = 100 * progressPercent,
					BytesReceived = (long)(InputFileStream.Length * progressPercent),
					TotalBytesToReceive = InputFileStream.Length
				});
		}

		public override void Cancel()
		{
			IsCanceled = true;
			AaxFile?.Cancel();
			AaxFile?.Dispose();
			CloseInputFileStream();
		}
	}
}
