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

				//Step 5
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

			try
			{
				ConversionResult decryptionResult = await decryptAsync(outputFile);
				var success = decryptionResult == ConversionResult.NoErrorsDetected && !IsCanceled;
				if (success)
					base.OnFileCreated(OutputFileName);

				return success;
			}
			catch(Exception ex)
			{
				Serilog.Log.Error(ex, "AAXClean Error");
				FileUtility.SaferDelete(OutputFileName);
				return false;
			}
			finally
			{
				outputFile.Close();
				AaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

				Step_DownloadAudiobook_End(zeroProgress);
			}
		}

		private Task<ConversionResult> decryptAsync(Stream outputFile)
			=> DownloadOptions.OutputFormat == OutputFormat.Mp3 ? 
			AaxFile.ConvertToMp3Async
			(
				outputFile,
				DownloadOptions.LameConfig,
				DownloadOptions.ChapterInfo,
				DownloadOptions.TrimOutputToChapterLength
			)
			: DownloadOptions.FixupFile ?
				AaxFile.ConvertToMp4aAsync
				(
					outputFile,
					DownloadOptions.ChapterInfo,
					DownloadOptions.TrimOutputToChapterLength
				)
				: AaxFile.ConvertToMp4aAsync(outputFile);
	}
}
