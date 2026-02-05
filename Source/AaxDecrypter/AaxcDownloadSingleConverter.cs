using AAXClean;
using AAXClean.Codecs;
using Dinah.Core.Net.Http;
using FileManager;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AaxDecrypter;

public class AaxcDownloadSingleConverter : AaxcDownloadConvertBase
{
	private readonly AverageSpeed averageSpeed = new();
	private TempFile? outputTempFile;

	public AaxcDownloadSingleConverter(string outDirectory, string cacheDirectory, IDownloadOptions dlOptions)
		: base(outDirectory, cacheDirectory, dlOptions)
	{
		var step = 1;

		AsyncSteps.Name = $"Download and Convert Aaxc To {DownloadOptions.OutputFormat}";
		AsyncSteps[$"Step {step++}: Get Aaxc Metadata"] = () => Task.Run(Step_GetMetadata);
		AsyncSteps[$"Step {step++}: Download Decrypted Audiobook"] = Step_DownloadAndDecryptAudiobookAsync;
		if (DownloadOptions.MoveMoovToBeginning && DownloadOptions.OutputFormat is OutputFormat.M4b)
			AsyncSteps[$"Step {step++}: Move moov atom to beginning"] = Step_MoveMoov;
		AsyncSteps[$"Step {step++}: Create Cue"] = Step_CreateCueAsync;
	}

	protected override void OnInitialized()
	{
		//Finishing configuring lame encoder.
		if (DownloadOptions.OutputFormat == OutputFormat.Mp3)
		{
			if (AaxFile is null)
				throw new InvalidOperationException($"AaxFile is null during {nameof(OnInitialized)} in {nameof(AaxcDownloadConvertBase)}.");
			if (DownloadOptions.LameConfig is null)
				throw new InvalidOperationException($"LameConfig is null during {nameof(OnInitialized)} in {nameof(DownloadOptions)}.");

			MpegUtil.ConfigureLameOptions(
				AaxFile,
				DownloadOptions.LameConfig,
				DownloadOptions.Downsample,
				DownloadOptions.MatchSourceBitrate,
				DownloadOptions.ChapterInfo);
		}
	}

	protected override async Task<bool> Step_DownloadAndDecryptAudiobookAsync()
	{
		if (AaxFile is null) return false;
		outputTempFile = GetNewTempFilePath(DownloadOptions.OutputFormat.ToString());
		FileUtility.SaferDelete(outputTempFile.FilePath);

		using var outputFile = File.Open(outputTempFile.FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		OnTempFileCreated(outputTempFile);

		try
		{
			await (AaxConversion = decryptAsync(AaxFile, outputFile));

			return AaxConversion.IsCompletedSuccessfully;
		}
		finally
		{
			FinalizeDownload();
		}
	}

	private async Task<bool> Step_MoveMoov()
	{
		if (outputTempFile is null) return false;
		AaxConversion = Mp4File.RelocateMoovAsync(outputTempFile.FilePath);
		AaxConversion.ConversionProgressUpdate += AaxConversion_MoovProgressUpdate;
		await AaxConversion;
		AaxConversion.ConversionProgressUpdate -= AaxConversion_MoovProgressUpdate;
		return AaxConversion.IsCompletedSuccessfully;
	}

	private void AaxConversion_MoovProgressUpdate(object? sender, ConversionProgressEventArgs e)
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

	private Mp4Operation decryptAsync(Mp4File aaxFile, Stream outputFile)
		=> DownloadOptions.OutputFormat == OutputFormat.Mp3
		? aaxFile.ConvertToMp3Async
		(
			outputFile,
			DownloadOptions.LameConfig,
			DownloadOptions.ChapterInfo
		)
		: DownloadOptions.FixupFile
		? aaxFile.ConvertToMp4aAsync
		(
			outputFile,
			DownloadOptions.ChapterInfo
		)
		: aaxFile.ConvertToMp4aAsync(outputFile);
}
