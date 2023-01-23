using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AAXClean;
using AAXClean.Codecs;
using FileManager;

namespace AaxDecrypter
{
	public class AaxcDownloadMultiConverter : AaxcDownloadConvertBase
	{
		private static TimeSpan minChapterLength { get; } = TimeSpan.FromSeconds(3);
		private List<string> multiPartFilePaths { get; } = new List<string>();
		private FileStream workingFileStream;

		public AaxcDownloadMultiConverter(string outFileName, string cacheDirectory, IDownloadOptions dlOptions)
			: base(outFileName, cacheDirectory, dlOptions) { }

		public override async Task<bool> RunAsync()
		{
			try
			{
				Serilog.Log.Information("Begin download and convert Aaxc To {format}", DownloadOptions.OutputFormat);

				//Step 1
				Serilog.Log.Information("Begin Get Aaxc Metadata");
				if (await Task.Run(Step_GetMetadata))
					Serilog.Log.Information("Completed Get Aaxc Metadata");
				else
				{
					Serilog.Log.Information("Failed to Complete Get Aaxc Metadata");
					return false;
				}

				//Step 2
				Serilog.Log.Information("Begin Download Decrypted Audiobook");
				if (await Step_DownloadAudiobookAsMultipleFilesPerChapter())
					Serilog.Log.Information("Completed Download Decrypted Audiobook");
				else
				{
					Serilog.Log.Information("Failed to Complete Download Decrypted Audiobook");
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
				Serilog.Log.Information("Begin Cleanup");
				if (await Task.Run(Step_Cleanup))
					Serilog.Log.Information("Completed Cleanup");
				else
				{
					Serilog.Log.Information("Failed to Complete Cleanup");
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

		/*
https://github.com/rmcrackan/Libation/pull/127#issuecomment-939088489

If the chapter truly is empty, that is, 0 audio frames in length, then yes it is ignored.
If the chapter is shorter than 3 seconds long but still has some audio frames, those frames are combined with the following chapter and not split into a new file.

I also implemented file naming by chapter title. When 2 or more consecutive chapters are combined, the first of the combined chapter's title is used in the file name. For example, given an audiobook with the following chapters:

00:00:00 - 00:00:02 | Part 1
00:00:02 - 00:35:00 | Chapter 1
00:35:02 - 01:02:00 | Chapter 2
01:02:00 - 01:02:02 | Part 2
01:02:02 - 01:41:00 | Chapter 3
01:41:00 - 02:05:00 | Chapter 4

The book will be split into the following files:

00:00:00 - 00:35:00 | Book - 01 - Part 1.m4b
00:35:00 - 01:02:00 | Book - 02 - Chapter 2.m4b
01:02:00 - 01:41:00 | Book - 03 - Part 2.m4b
01:41:00 - 02:05:00 | Book - 04 - Chapter 4.m4b

That naming may not be desirable for everyone, but it's an easy change to instead use the last of the combined chapter's title in the file name.
		 */
		private async Task<bool> Step_DownloadAudiobookAsMultipleFilesPerChapter()
		{
			var zeroProgress = Step_DownloadAudiobook_Start();

			var chapters = DownloadOptions.ChapterInfo.Chapters;

			// Ensure split files are at least minChapterLength in duration.
			var splitChapters = new ChapterInfo(DownloadOptions.ChapterInfo.StartOffset);

			var runningTotal = TimeSpan.Zero;
			string title = "";

			for (int i = 0; i < chapters.Count; i++)
			{
				if (runningTotal == TimeSpan.Zero)
					title = chapters[i].Title;

				runningTotal += chapters[i].Duration;

				if (runningTotal >= minChapterLength)
				{
					splitChapters.AddChapter(title, runningTotal);
					runningTotal = TimeSpan.Zero;
				}
			}

			// reset, just in case
			multiPartFilePaths.Clear();

			try
			{
				if (DownloadOptions.OutputFormat == OutputFormat.M4b)
					aaxConversion = ConvertToMultiMp4a(splitChapters);
				else
					aaxConversion = ConvertToMultiMp3(splitChapters);

				aaxConversion.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
				await aaxConversion;
				return aaxConversion.IsCompletedSuccessfully;
			}
			catch(Exception ex)
			{
				Serilog.Log.Error(ex, "AAXClean Error");
				workingFileStream?.Close();
				if (workingFileStream?.Name is not null)
					FileUtility.SaferDelete(workingFileStream.Name);
				return false;
			}
			finally
			{
				if (aaxConversion is not null)
					aaxConversion.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

				Step_DownloadAudiobook_End(zeroProgress);
			}
		}

		private Mp4Operation ConvertToMultiMp4a(ChapterInfo splitChapters)
		{
			var chapterCount = 0;
			return AaxFile.ConvertToMultiMp4aAsync
				(
					splitChapters,
					newSplitCallback => Callback(++chapterCount, splitChapters, newSplitCallback),
					DownloadOptions.TrimOutputToChapterLength
				);
		}

		private Mp4Operation ConvertToMultiMp3(ChapterInfo splitChapters)
		{
			var chapterCount = 0;
			return AaxFile.ConvertToMultiMp3Async
				(
					splitChapters,
					newSplitCallback => Callback(++chapterCount, splitChapters, newSplitCallback),
					DownloadOptions.LameConfig,
					DownloadOptions.TrimOutputToChapterLength
				);
		}


		private void Callback(int currentChapter, ChapterInfo splitChapters, NewMP3SplitCallback newSplitCallback)
			=> Callback(currentChapter, splitChapters, newSplitCallback as NewSplitCallback);

		private void Callback(int currentChapter, ChapterInfo splitChapters, NewSplitCallback newSplitCallback)
		{
			MultiConvertFileProperties props = new()
			{
				OutputFileName = OutputFileName,
				PartsPosition = currentChapter,
				PartsTotal = splitChapters.Count,
				Title = newSplitCallback?.Chapter?.Title,
			};
			newSplitCallback.OutputFile = createOutputFileStream(props);
			newSplitCallback.TrackTitle = DownloadOptions.GetMultipartTitleName(props);
			newSplitCallback.TrackNumber = currentChapter;
			newSplitCallback.TrackCount = splitChapters.Count;
		}

		private FileStream createOutputFileStream(MultiConvertFileProperties multiConvertFileProperties)
		{
			var fileName = DownloadOptions.GetMultipartFileName(multiConvertFileProperties);
			var extension = Path.GetExtension(fileName);
			fileName = FileUtility.GetValidFilename(fileName, DownloadOptions.ReplacementCharacters, extension);

			multiPartFilePaths.Add(fileName);

			FileUtility.SaferDelete(fileName);

			workingFileStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			OnFileCreated(fileName);
			return workingFileStream;
		}
	}
}
