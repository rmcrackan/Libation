using AAXClean;
using AAXClean.Codecs;
using Dinah.Core.Net.Http;
using FileManager;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AaxDecrypter
{
	public class AaxcDownloadSingleConverter : AaxcDownloadConvertBase
	{
		private readonly AverageSpeed averageSpeed = new();
		public AaxcDownloadSingleConverter(string outFileName, string cacheDirectory, IDownloadOptions dlOptions)
			: base(outFileName, cacheDirectory, dlOptions)
		{

			AsyncSteps.Name = $"Download and Convert Aaxc To {DownloadOptions.OutputFormat}";
			AsyncSteps["Step 1: Get Aaxc Metadata"] = () => Task.Run(Step_GetMetadata);
			AsyncSteps["Step 2: Download Decrypted Audiobook"] = Step_DownloadAndDecryptAudiobookAsync;
			AsyncSteps["Step 3: Download Clips and Bookmarks"] = Step_DownloadClipsBookmarksAsync;
			AsyncSteps["Step 4: Create Cue"] = Step_CreateCueAsync;
		}

		protected async override Task<bool> Step_DownloadAndDecryptAudiobookAsync()
		{
			FileUtility.SaferDelete(OutputFileName);

			using var outputFile = File.Open(OutputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			OnFileCreated(OutputFileName);

			try
			{
				await (AaxConversion = decryptAsync(outputFile));

				if (AaxConversion.IsCompletedSuccessfully
					&& DownloadOptions.MoveMoovToBeginning
					&& DownloadOptions.OutputFormat is OutputFormat.M4b)
				{
					outputFile.Close();
					AaxConversion = Mp4File.RelocateMoovAsync(OutputFileName);
					AaxConversion.ConversionProgressUpdate += AaxConversion_MoovProgressUpdate;
					await AaxConversion;
					AaxConversion.ConversionProgressUpdate -= AaxConversion_MoovProgressUpdate;
				}

				return AaxConversion.IsCompletedSuccessfully;
			}
			finally
			{
				FinalizeDownload();
			}
		}

		private void AaxConversion_MoovProgressUpdate(object sender, ConversionProgressEventArgs e)
		{
			averageSpeed.AddPosition(e.ProcessPosition.TotalSeconds);

			var remainingTimeToProcess = (e.TotalDuration - e.ProcessPosition).TotalSeconds;
			var estTimeRemaining = remainingTimeToProcess / averageSpeed.Average;

			if (double.IsNormal(estTimeRemaining))
				OnDecryptTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

			var progressPercent = 100d * (1 - remainingTimeToProcess / e.TotalDuration.TotalSeconds);

			OnDecryptProgressUpdate(
				new DownloadProgress
				{
					ProgressPercentage = progressPercent,
					BytesReceived = (long)(InputFileStream.Length * progressPercent),
					TotalBytesToReceive = InputFileStream.Length
				});
		}

		private Mp4Operation decryptAsync(Stream outputFile)
			=> DownloadOptions.OutputFormat == OutputFormat.Mp3
			? AaxFile.ConvertToMp3Async
			(
				outputFile,
				DownloadOptions.LameConfig,
				DownloadOptions.ChapterInfo,
				DownloadOptions.TrimOutputToChapterLength
			)
			: DownloadOptions.FixupFile
			? AaxFile.ConvertToMp4aAsync
			(
				outputFile,
				DownloadOptions.ChapterInfo,
				DownloadOptions.TrimOutputToChapterLength
			)
			: AaxFile.ConvertToMp4aAsync(outputFile);
	}
}
