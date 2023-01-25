using AAXClean;
using Dinah.Core.Net.Http;
using System;
using System.Threading.Tasks;

namespace AaxDecrypter
{
	public abstract class AaxcDownloadConvertBase : AudiobookDownloadBase
	{
		public event EventHandler<AppleTags> RetrievedMetadata;

		protected AaxFile AaxFile { get; private set; }
		private Mp4Operation aaxConversion;
		protected Mp4Operation AaxConversion
		{
			get => aaxConversion;
			set
			{
				if (aaxConversion is not null)
					aaxConversion.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

				if (value is not null)
				{
					aaxConversion = value;
					aaxConversion.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
				}
			}
		}

		protected AaxcDownloadConvertBase(string outFileName, string cacheDirectory, IDownloadOptions dlOptions)
			: base(outFileName, cacheDirectory, dlOptions) { }

		/// <summary>Setting cover art by this method will insert the art into the audiobook metadata</summary>
		public override void SetCoverArt(byte[] coverArt)
		{
			base.SetCoverArt(coverArt);
			if (coverArt is not null && AaxFile?.AppleTags is not null)
				AaxFile.AppleTags.Cover = coverArt;
		}

		public override async Task CancelAsync()
		{
			IsCanceled = true;
			await (AaxConversion?.CancelAsync() ?? Task.CompletedTask);
			FinalizeDownload();
		}

		protected override void FinalizeDownload()
		{
			AaxConversion = null;
			base.FinalizeDownload();
		}

		protected bool Step_GetMetadata()
		{
			AaxFile = new AaxFile(InputFileStream);
			AaxFile.SetDecryptionKey(DownloadOptions.AudibleKey, DownloadOptions.AudibleIV);

			if (DownloadOptions.StripUnabridged)
			{
				AaxFile.AppleTags.Title = AaxFile.AppleTags.TitleSansUnabridged;
				AaxFile.AppleTags.Album = AaxFile.AppleTags.Album?.Replace(" (Unabridged)", "");
			}

			if (DownloadOptions.FixupFile && !string.IsNullOrWhiteSpace(AaxFile.AppleTags.Narrator))
				AaxFile.AppleTags.AppleListBox.EditOrAddTag("TCOM", AaxFile.AppleTags.Narrator);

			//Finishing configuring lame encoder.
			if (DownloadOptions.OutputFormat == OutputFormat.Mp3)
				MpegUtil.ConfigureLameOptions(
					AaxFile,
					DownloadOptions.LameConfig,
					DownloadOptions.Downsample,
					DownloadOptions.MatchSourceBitrate);

			OnRetrievedTitle(AaxFile.AppleTags.TitleSansUnabridged);
			OnRetrievedAuthors(AaxFile.AppleTags.FirstAuthor ?? "[unknown]");
			OnRetrievedNarrators(AaxFile.AppleTags.Narrator ?? "[unknown]");
			OnRetrievedCoverArt(AaxFile.AppleTags.Cover);

			RetrievedMetadata?.Invoke(this, AaxFile.AppleTags);

			return !IsCanceled;
		}

		private void AaxFile_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
		{
			var remainingSecsToProcess = (e.TotalDuration - e.ProcessPosition).TotalSeconds;
			var estTimeRemaining = remainingSecsToProcess / e.ProcessSpeed;

			if (double.IsNormal(estTimeRemaining))
				OnDecryptTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

			var progressPercent = e.ProcessPosition / e.TotalDuration;

			OnDecryptProgressUpdate(
				new DownloadProgress
				{
					ProgressPercentage = 100 * progressPercent,
					BytesReceived = (long)(InputFileStream.Length * progressPercent),
					TotalBytesToReceive = InputFileStream.Length
				});
		}
	}
}
