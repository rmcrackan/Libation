using AAXClean;
using AAXClean.Codecs;
using Dinah.Core.Net.Http;
using FileManager;
using System;
using System.Diagnostics;
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
			var step = 1;

			AsyncSteps.Name = $"Download and Convert Aaxc To {DownloadOptions.OutputFormat}";
			AsyncSteps[$"Step {step++}: Get Aaxc Metadata"] = () => Task.Run(Step_GetMetadata);
			AsyncSteps[$"Step {step++}: Download Decrypted Audiobook"] = Step_DownloadAndDecryptAudiobookAsync;
			if (DownloadOptions.MoveMoovToBeginning && DownloadOptions.OutputFormat is OutputFormat.M4b)
				AsyncSteps[$"Step {step++}: Move moov atom to beginning"] = Step_MoveMoov;
			AsyncSteps[$"Step {step++}: Download Clips and Bookmarks"] = Step_DownloadClipsBookmarksAsync;
			AsyncSteps[$"Step {step++}: Create Cue"] = Step_CreateCueAsync;
		}

		protected async override Task<bool> Step_DownloadAndDecryptAudiobookAsync()
		{
			FileUtility.SaferDelete(OutputFileName);

			using var outputFile = File.Open(OutputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			OnFileCreated(OutputFileName);

			try
			{
				await (AaxConversion = decryptAsync(outputFile));				

				return AaxConversion.IsCompletedSuccessfully;
			}
			finally
			{
				FinalizeDownload();
			}
		}

		private async Task<bool> Step_MoveMoov()
		{
			AaxConversion = Mp4File.RelocateMoovAsync(OutputFileName);
			AaxConversion.ConversionProgressUpdate += AaxConversion_MoovProgressUpdate;
			await AaxConversion;
			AaxConversion.ConversionProgressUpdate -= AaxConversion_MoovProgressUpdate;
			return AaxConversion.IsCompletedSuccessfully;
		}

		private void AaxConversion_MoovProgressUpdate(object sender, ConversionProgressEventArgs e)
		{
			averageSpeed.AddPosition(e.ProcessPosition.TotalSeconds);

			var remainingTimeToProcess = (e.EndTime - e.ProcessPosition).TotalSeconds;
			var estTimeRemaining = remainingTimeToProcess / averageSpeed.Average;

			if (double.IsNormal(estTimeRemaining))
				OnDecryptTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

			OnDecryptProgressUpdate(
				new DownloadProgress
				{
					ProgressPercentage = 100 * e.FractionCompleted,
					BytesReceived = (long)(InputFileStream.Length * e.FractionCompleted),
					TotalBytesToReceive = InputFileStream.Length
				});
		}

		private Mp4Operation decryptAsync(Stream outputFile)
			=> DownloadOptions.OutputFormat == OutputFormat.Mp3
			? AaxFile.ConvertToMp3Async
			(
				outputFile,
				DownloadOptions.LameConfig,
				DownloadOptions.ChapterInfo
			)
			: DownloadOptions.FixupFile
			? AaxFile.ConvertToMp4aAsync
			(
				outputFile,
				DownloadOptions.ChapterInfo
			)
			: AaxFile.ConvertToMp4aAsync(outputFile);
	}
}
