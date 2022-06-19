using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AAXClean;
using AAXClean.Codecs;
using Dinah.Core.StepRunner;
using FileManager;

namespace AaxDecrypter
{
	public class AaxcDownloadMultiConverter : AaxcDownloadConvertBase
	{
		protected override StepSequence Steps { get; }

        private Func<MultiConvertFileProperties, string> multipartFileNameCallback { get; }

        private static TimeSpan minChapterLength { get; } = TimeSpan.FromSeconds(3);
		private List<string> multiPartFilePaths { get; } = new List<string>();

        public AaxcDownloadMultiConverter(string outFileName, string cacheDirectory, DownloadOptions dlLic,
            Func<MultiConvertFileProperties, string> multipartFileNameCallback = null)
			: base(outFileName, cacheDirectory, dlLic)
        {
            Steps = new StepSequence
            {
                Name = "Download and Convert Aaxc To " + DownloadOptions.OutputFormat,

                ["Step 1: Get Aaxc Metadata"] = Step_GetMetadata,
                ["Step 2: Download Decrypted Audiobook"] = Step_DownloadAudiobookAsMultipleFilesPerChapter,
                ["Step 3: Cleanup"] = Step_Cleanup,
            };
            this.multipartFileNameCallback = multipartFileNameCallback ?? MultiConvertFileProperties.DefaultMultipartFilename;
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
        private bool Step_DownloadAudiobookAsMultipleFilesPerChapter()
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

            ConversionResult result;

            AaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
            if (DownloadOptions.OutputFormat == OutputFormat.M4b)
                result = ConvertToMultiMp4a(splitChapters);
            else
                result = ConvertToMultiMp3(splitChapters);
            AaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

            Step_DownloadAudiobook_End(zeroProgress);

            return result == ConversionResult.NoErrorsDetected;
        }

        private ConversionResult ConvertToMultiMp4a(ChapterInfo splitChapters)
        {
            var chapterCount = 0;
            return AaxFile.ConvertToMultiMp4a(splitChapters, newSplitCallback =>
            {
                createOutputFileStream(++chapterCount, splitChapters, newSplitCallback);

                newSplitCallback.TrackNumber = chapterCount;
                newSplitCallback.TrackCount = splitChapters.Count;

            }, DownloadOptions.TrimOutputToChapterLength);
        }

        private ConversionResult ConvertToMultiMp3(ChapterInfo splitChapters)
        {
            var chapterCount = 0;
            return AaxFile.ConvertToMultiMp3(splitChapters, newSplitCallback =>
            {
                createOutputFileStream(++chapterCount, splitChapters, newSplitCallback);

                newSplitCallback.TrackNumber = chapterCount;
                newSplitCallback.TrackCount = splitChapters.Count;

            }, DownloadOptions.LameConfig, DownloadOptions.TrimOutputToChapterLength);
        }

        private void createOutputFileStream(int currentChapter, ChapterInfo splitChapters, NewSplitCallback newSplitCallback)
        {
            var fileName = multipartFileNameCallback(new()
            {
                OutputFileName = OutputFileName,
                PartsPosition = currentChapter,
                PartsTotal = splitChapters.Count,
                Title = newSplitCallback?.Chapter?.Title,

            });
            fileName = FileUtility.GetValidFilename(fileName);

            multiPartFilePaths.Add(fileName);

            FileUtility.SaferDelete(fileName);

            newSplitCallback.OutputFile = File.Open(fileName, FileMode.OpenOrCreate);

            OnFileCreated(fileName);
        }
    }
}
