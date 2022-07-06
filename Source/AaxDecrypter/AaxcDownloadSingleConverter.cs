using System;
using System.IO;
using System.Threading.Tasks;
using AAXClean;
using AAXClean.Codecs;
using FileManager;

namespace AaxDecrypter
{
	public class AaxcDownloadSingleConverter : AaxcDownloadConvertBase
	{
		public AaxcDownloadSingleConverter(string outFileName, string cacheDirectory, IDownloadOptions dlOptions)
			: base(outFileName, cacheDirectory, dlOptions) { }

		public override async Task<bool> RunAsync()
		{
			try
			{
				Serilog.Log.Information("Begin download and convert Aaxc To {format}", DownloadOptions.OutputFormat);

				//Step 1
				Serilog.Log.Information("Begin Step 1: Get Aaxc Metadata");
				if (await Task.Run(Step_GetMetadata))
					Serilog.Log.Information("Completed Step 1: Get Aaxc Metadata");
				else
				{
					Serilog.Log.Information("Failed to Complete Step 1: Get Aaxc Metadata");
					return false;
				}

				//Step 2
				Serilog.Log.Information("Begin Step 2: Download Decrypted Audiobook");
				if (await Step_DownloadAudiobookAsSingleFile())
					Serilog.Log.Information("Completed Step 2: Download Decrypted Audiobook");
				else
				{
					Serilog.Log.Information("Failed to Complete Step 2: Download Decrypted Audiobook");
					return false;
				}

				//Step 3
				Serilog.Log.Information("Begin Step 3: Create Cue");
				if (await Task.Run(Step_CreateCue))
					Serilog.Log.Information("Completed Step 3: Create Cue");
				else
				{
					Serilog.Log.Information("Failed to Complete Step 3: Create Cue");
					return false;
				}

				//Step 4
				Serilog.Log.Information("Begin Step 4: Cleanup");
				if (await Task.Run(Step_Cleanup))
					Serilog.Log.Information("Completed Step 4: Cleanup");
				else
				{
					Serilog.Log.Information("Failed to Complete Step 4: Cleanup");
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

		private async Task<bool> Step_DownloadAudiobookAsSingleFile()
		{
			var zeroProgress = Step_DownloadAudiobook_Start();

			FileUtility.SaferDelete(OutputFileName);

			var outputFile = File.Open(OutputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			OnFileCreated(OutputFileName);

			AaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;

			ConversionResult decryptionResult;

			if (DownloadOptions.OutputFormat == OutputFormat.M4b)
			{
				if (DownloadOptions.FixupFile)
					decryptionResult = await AaxFile.ConvertToMp4aAsync(outputFile, DownloadOptions.ChapterInfo, DownloadOptions.TrimOutputToChapterLength);
				else
					decryptionResult = await AaxFile.ConvertToMp4aAsync(outputFile);
			}
			else
				decryptionResult = await AaxFile.ConvertToMp3Async(outputFile, DownloadOptions.LameConfig, DownloadOptions.ChapterInfo, DownloadOptions.TrimOutputToChapterLength);

			AaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

			Step_DownloadAudiobook_End(zeroProgress);

			var success = decryptionResult == ConversionResult.NoErrorsDetected && !IsCanceled;
			if (success)
				base.OnFileCreated(OutputFileName);

			return success;
		}
	}
}
