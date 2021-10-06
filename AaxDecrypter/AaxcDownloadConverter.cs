using AAXClean;
using Dinah.Core;
using Dinah.Core.IO;
using Dinah.Core.Net.Http;
using Dinah.Core.StepRunner;
using System;
using System.IO;
using System.Linq;

namespace AaxDecrypter
{
    public class AaxcDownloadConverter : AudiobookDownloadBase
    {
        const int MAX_FILENAME_LENGTH = 255;
        private static readonly TimeSpan minChapterLength = TimeSpan.FromSeconds(3);
        protected override StepSequence steps { get; }

        private AaxFile aaxFile;
        private OutputFormat OutputFormat { get; }

        public AaxcDownloadConverter(string outFileName, string cacheDirectory, DownloadLicense dlLic, OutputFormat outputFormat, bool splitFileByChapters)
            :base(outFileName, cacheDirectory, dlLic)
        {
            OutputFormat = outputFormat;

            steps = new StepSequence
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

            return !isCanceled;
        }

        protected override bool Step2_DownloadAudiobookAsSingleFile()
        {
            var zeroProgress = Step2_Start();
            
            if (File.Exists(outputFileName))
                FileExt.SafeDelete(outputFileName);

            var outputFile =  File.Open(outputFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
            var decryptionResult = OutputFormat == OutputFormat.M4b ? aaxFile.ConvertToMp4a(outputFile, downloadLicense.ChapterInfo) : aaxFile.ConvertToMp3(outputFile);
            aaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

            downloadLicense.ChapterInfo = aaxFile.Chapters;
            
            Step2_End(zeroProgress);

            return decryptionResult == ConversionResult.NoErrorsDetected && !isCanceled;
        }

        private bool Step2_DownloadAudiobookAsMultipleFilesPerChapter()
        {
            var zeroProgress = Step2_Start();

            var chapters = downloadLicense.ChapterInfo.Chapters.ToList();

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

            aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
            if(OutputFormat == OutputFormat.M4b) 
                ConvertToMultiMp4b(splitChapters);
            else 
                ConvertToMultiMp3(splitChapters);
            aaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

            Step2_End(zeroProgress);

            return !isCanceled;
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

            aaxFile.SetDecryptionKey(downloadLicense.AudibleKey, downloadLicense.AudibleIV);
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
            {
                var fileName = GetMultipartFileName(outputFileName, ++chapterCount, newSplitCallback.Chapter.Title);
                if (File.Exists(fileName))
                    FileExt.SafeDelete(fileName);
                newSplitCallback.OutputFile = File.Open(fileName, FileMode.OpenOrCreate);
            });
        }
        
        private void ConvertToMultiMp3(ChapterInfo splitChapters)
        {
            var chapterCount = 0;
            aaxFile.ConvertToMultiMp3(splitChapters, newSplitCallback =>
            {
                var fileName = GetMultipartFileName(outputFileName, ++chapterCount, newSplitCallback.Chapter.Title);
                if (File.Exists(fileName))
                    FileExt.SafeDelete(fileName);
                newSplitCallback.OutputFile = File.Open(fileName, FileMode.OpenOrCreate);
                newSplitCallback.LameConfig.ID3.Track = chapterCount.ToString();
            });
        }

        private static string GetMultipartFileName(string baseFileName, int chapterCount, string chapterTitle)
        {
            string extension = Path.GetExtension(baseFileName);

            var fileNameChars = $"{Path.GetFileNameWithoutExtension(baseFileName)} - {chapterCount:D2} - {chapterTitle}".ToCharArray();

            //Replace illegal path characters with spaces.
            for (int i = 0; i <fileNameChars.Length; i++)
			{
                foreach (var illegal in Path.GetInvalidFileNameChars())
				{
                    if (fileNameChars[i] == illegal)
					{
                        fileNameChars[i] = ' ';
                        break;
					}
				}
			}

            var fileName = new string(fileNameChars).Truncate(MAX_FILENAME_LENGTH - extension.Length);

            return Path.Combine(Path.GetDirectoryName(baseFileName), fileName + extension);
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
            isCanceled = true;
            aaxFile?.Cancel();
            aaxFile?.Dispose();
            CloseInputFileStream();
        }

		protected override int GetSpeedup(TimeSpan elapsed)
            => (int)(aaxFile.Duration.TotalSeconds / (long)elapsed.TotalSeconds);
	}
}
