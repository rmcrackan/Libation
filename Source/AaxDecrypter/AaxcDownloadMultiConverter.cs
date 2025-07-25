using AAXClean;
using AAXClean.Codecs;
using FileManager;
using System;
using System.IO;
using System.Threading.Tasks;

#nullable enable
namespace AaxDecrypter
{
	public class AaxcDownloadMultiConverter : AaxcDownloadConvertBase
	{
		private static readonly TimeSpan minChapterLength = TimeSpan.FromSeconds(3);
		private FileStream? workingFileStream;

		public AaxcDownloadMultiConverter(string outDirectory, string cacheDirectory, IDownloadOptions dlOptions)
			: base(outDirectory, cacheDirectory, dlOptions)
		{
			AsyncSteps.Name = $"Download, Convert Aaxc To {DownloadOptions.OutputFormat}, and Split";
			AsyncSteps["Step 1: Get Aaxc Metadata"] = () => Task.Run(Step_GetMetadata);
			AsyncSteps["Step 2: Download Decrypted Audiobook"] = Step_DownloadAndDecryptAudiobookAsync;
		}

		protected override void OnInitialized()
		{
			//Finishing configuring lame encoder.
			if (DownloadOptions.OutputFormat == OutputFormat.Mp3)
				MpegUtil.ConfigureLameOptions(
					AaxFile,
					DownloadOptions.LameConfig,
					DownloadOptions.Downsample,
					DownloadOptions.MatchSourceBitrate,
					chapters: null);
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
		protected async override Task<bool> Step_DownloadAndDecryptAudiobookAsync()
		{
			if (AaxFile is null) return false;
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

			try
			{
				await (AaxConversion = decryptMultiAsync(AaxFile, splitChapters));

				if (AaxConversion.IsCompletedSuccessfully)
					await moveMoovToBeginning(AaxFile, workingFileStream?.Name);

				return AaxConversion.IsCompletedSuccessfully;
			}
			finally
			{
				workingFileStream?.Dispose();
				FinalizeDownload();
			}
		}

		private Mp4Operation decryptMultiAsync(Mp4File aaxFile, ChapterInfo splitChapters)
		{
			var chapterCount = 0;
			return
				DownloadOptions.OutputFormat == OutputFormat.M4b
				? aaxFile.ConvertToMultiMp4aAsync
				(
					splitChapters,
					newSplitCallback => newSplit(++chapterCount, splitChapters, newSplitCallback)
				)
				: aaxFile.ConvertToMultiMp3Async
				(
					splitChapters,
					newSplitCallback => newSplit(++chapterCount, splitChapters, newSplitCallback),
					DownloadOptions.LameConfig
				);

			void newSplit(int currentChapter, ChapterInfo splitChapters, INewSplitCallback newSplitCallback)
			{
				moveMoovToBeginning(aaxFile, workingFileStream?.Name).GetAwaiter().GetResult();
				var newTempFile = GetNewTempFilePath(DownloadOptions.OutputFormat.ToString());
				MultiConvertFileProperties props = new()
				{
					OutputFileName = newTempFile.FilePath,
					PartsPosition = currentChapter,
					PartsTotal = splitChapters.Count,
					Title = newSplitCallback.Chapter?.Title,
				};

				newSplitCallback.OutputFile = workingFileStream = createOutputFileStream(props);
				newSplitCallback.TrackTitle = DownloadOptions.GetMultipartTitle(props);
				newSplitCallback.TrackNumber = currentChapter;
				newSplitCallback.TrackCount = splitChapters.Count;

				OnTempFileCreated(newTempFile with { PartProperties = props });
			}

			FileStream createOutputFileStream(MultiConvertFileProperties multiConvertFileProperties)
			{
				FileUtility.SaferDelete(multiConvertFileProperties.OutputFileName);
				return File.Open(multiConvertFileProperties.OutputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			}
		}

		private Mp4Operation moveMoovToBeginning(Mp4File aaxFile, string? filename)
		{
			if (DownloadOptions.OutputFormat is OutputFormat.M4b
				&& DownloadOptions.MoveMoovToBeginning
				&& filename is not null
				&& File.Exists(filename))
			{
				return Mp4File.RelocateMoovAsync(filename);
			}
			else return Mp4Operation.FromCompleted(aaxFile);
		}
	}
}
