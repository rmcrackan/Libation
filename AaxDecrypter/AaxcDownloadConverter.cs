using AAXClean;
using Dinah.Core.IO;
using Dinah.Core.Net.Http;
using Dinah.Core.StepRunner;
using System;
using System.IO;

namespace AaxDecrypter
{
    public class AaxcDownloadConverter : AudiobookDownloadBase
    {
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

            aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
            if(OutputFormat == OutputFormat.M4b) 
                ConvertToMultiMp4b();
            else 
                ConvertToMultiMp3();
            aaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

            Step2_End(zeroProgress);

            return true;
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

        private void ConvertToMultiMp4b()
        {
            var chapterCount = 0;
            aaxFile.ConvertToMultiMp4a(downloadLicense.ChapterInfo, newSplitCallback =>
            {
                chapterCount++;
                var fileName = Path.ChangeExtension(outputFileName, $"{chapterCount}.m4b");
                if (File.Exists(fileName))
                    FileExt.SafeDelete(fileName);
                newSplitCallback.OutputFile = File.Open(fileName, FileMode.OpenOrCreate);
            });
        }
        
        private void ConvertToMultiMp3()
        {
            var chapterCount = 0;
            aaxFile.ConvertToMultiMp3(downloadLicense.ChapterInfo, newSplitCallback =>
            {
                chapterCount++;
                var fileName = Path.ChangeExtension(outputFileName, $"{chapterCount}.mp3");
                if (File.Exists(fileName))
                    FileExt.SafeDelete(fileName);
                newSplitCallback.OutputFile = File.Open(fileName, FileMode.OpenOrCreate);
                newSplitCallback.LameConfig.ID3.Track = chapterCount.ToString();
            });
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
