using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AAXClean;
using Dinah.Core.Net.Http;
using Dinah.Core.StepRunner;
using FileManager;

namespace AaxDecrypter
{
    public class AaxcDownloadConverter : AudiobookDownloadBase
    {
        private static readonly TimeSpan minChapterLength = TimeSpan.FromSeconds(3);
        protected override StepSequence Steps { get; }

        private AaxFile aaxFile;
        private OutputFormat OutputFormat { get; }

        private List<string> multiPartFilePaths { get; } = new List<string>();

        public AaxcDownloadConverter(string outFileName, string cacheDirectory, DownloadLicense dlLic, OutputFormat outputFormat, bool splitFileByChapters)
            : base(outFileName, cacheDirectory, dlLic)
        {
            OutputFormat = outputFormat;

            Steps = new StepSequence
            {
                Name = "Download and Convert Aaxc To " + OutputFormat,

                ["Step 1: Get Aaxc Metadata"] = Step1_GetMetadata,
                ["Step 2: Download Decrypted Audiobook"] = splitFileByChapters
                    ? Step2_DownloadAudiobookAsMultipleFilesPerChapter
                    : Step2_DownloadAudiobookAsSingleFile,
                ["Step 3: Create Cue"] = splitFileByChapters
                    ? () => true
                    : Step3_CreateCue,
                ["Step 4: Cleanup"] = Step4_Cleanup,
            };
        }

        /// <summary>
        /// Setting cover art by this method will insert the art into the audiobook metadata
        /// </summary>
        public override void SetCoverArt(byte[] coverArt)
        {
            base.SetCoverArt(coverArt);

            aaxFile?.AppleTags.SetCoverArt(coverArt);
        }

        protected override bool Step1_GetMetadata()
        {
            aaxFile = new AaxFile(InputFileStream);

            OnRetrievedTitle(aaxFile.AppleTags.TitleSansUnabridged);
            OnRetrievedAuthors(aaxFile.AppleTags.FirstAuthor ?? "[unknown]");
            OnRetrievedNarrators(aaxFile.AppleTags.Narrator ?? "[unknown]");
            OnRetrievedCoverArt(aaxFile.AppleTags.Cover);

            return !IsCanceled;
        }

        protected override bool Step2_DownloadAudiobookAsSingleFile()
        {
            var zeroProgress = Step2_Start();

            FileUtility.SaferDelete(OutputFileName);

            var outputFile = File.Open(OutputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
            var decryptionResult = OutputFormat == OutputFormat.M4b ? aaxFile.ConvertToMp4a(outputFile, DownloadLicense.ChapterInfo) : aaxFile.ConvertToMp3(outputFile);
            aaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

            DownloadLicense.ChapterInfo = aaxFile.Chapters;

            Step2_End(zeroProgress);

            var success = decryptionResult == ConversionResult.NoErrorsDetected && !IsCanceled;
            if (success)
                base.OnFileCreated(OutputFileName);

            return success;
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
        private bool Step2_DownloadAudiobookAsMultipleFilesPerChapter()
        {
            var zeroProgress = Step2_Start();

            var chapters = DownloadLicense.ChapterInfo.Chapters.ToList();

            //Ensure split files are at least minChapterLength in duration.
            var splitChapters = new ChapterInfo();

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

            aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
            if (OutputFormat == OutputFormat.M4b)
                ConvertToMultiMp4b(splitChapters);
            else
                ConvertToMultiMp3(splitChapters);
            aaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

            Step2_End(zeroProgress);

            var success = !IsCanceled;

            if (success)
                foreach (var path in multiPartFilePaths)
                    OnFileCreated(path);

            return success;
        }

        private DownloadProgress Step2_Start()
        {
            var zeroProgress = new DownloadProgress
            {
                BytesReceived = 0,
                ProgressPercentage = 0,
                TotalBytesToReceive = InputFileStream.Length
            };

            OnDecryptProgressUpdate(zeroProgress);

            aaxFile.SetDecryptionKey(DownloadLicense.AudibleKey, DownloadLicense.AudibleIV);
            return zeroProgress;
        }

        private void Step2_End(DownloadProgress zeroProgress)
        {
            aaxFile.Close();

            CloseInputFileStream();

            OnDecryptProgressUpdate(zeroProgress);
        }

        private void ConvertToMultiMp4b(ChapterInfo splitChapters)
        {
            var chapterCount = 0;
            aaxFile.ConvertToMultiMp4a(splitChapters, newSplitCallback =>
                createOutputFileStream(++chapterCount, splitChapters, newSplitCallback)
            );
        }

        private void ConvertToMultiMp3(ChapterInfo splitChapters)
        {
            var chapterCount = 0;
            aaxFile.ConvertToMultiMp3(splitChapters, newSplitCallback =>
            {
                createOutputFileStream(++chapterCount, splitChapters, newSplitCallback);
                newSplitCallback.LameConfig.ID3.Track = chapterCount.ToString();
            });
        }

        private void createOutputFileStream(int currentChapter, ChapterInfo splitChapters, NewSplitCallback newSplitCallback)
		{
            var fileName = FileUtility.GetMultipartFileName(OutputFileName, currentChapter, splitChapters.Count, newSplitCallback.Chapter.Title);
			multiPartFilePaths.Add(fileName);

            FileUtility.SaferDelete(fileName);

            newSplitCallback.OutputFile = File.Open(fileName, FileMode.OpenOrCreate);
		}

		private void AaxFile_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
        {
            var duration = aaxFile.Duration;
            double remainingSecsToProcess = (duration - e.ProcessPosition).TotalSeconds;
            double estTimeRemaining = remainingSecsToProcess / e.ProcessSpeed;

            if (double.IsNormal(estTimeRemaining))
                OnDecryptTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

            double progressPercent = e.ProcessPosition.TotalSeconds / duration.TotalSeconds;

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
            aaxFile?.Cancel();
            aaxFile?.Dispose();
            CloseInputFileStream();
        }
    }
}
