using AAXClean;
using Dinah.Core;
using Dinah.Core.IO;
using Dinah.Core.Net.Http;
using Dinah.Core.StepRunner;
using System;
using System.IO;

namespace AaxDecrypter
{
    public enum OutputFormat { Mp4a,  Mp3 }
    public class AaxcDownloadConverter
    {
        public event EventHandler<AppleTags> RetrievedTags;
        public event EventHandler<byte[]> RetrievedCoverArt;
        public event EventHandler<DownloadProgress> DecryptProgressUpdate;
        public event EventHandler<TimeSpan> DecryptTimeRemaining;

        public string AppName { get; set; } = nameof(AaxcDownloadConverter);

        private string outputFileName { get; }
        private string cacheDir { get; }
        private DownloadLicense downloadLicense { get; }
        private AaxFile aaxFile;
        private OutputFormat OutputFormat;

        private StepSequence steps { get; }
        private NetworkFileStreamPersister nfsPersister;
        private bool isCanceled { get; set; }
        private string jsonDownloadState => Path.Combine(cacheDir, Path.GetFileNameWithoutExtension(outputFileName) + ".json");
        private string tempFile => PathLib.ReplaceExtension(jsonDownloadState, ".aaxc");

        public AaxcDownloadConverter(string outFileName, string cacheDirectory, DownloadLicense dlLic, OutputFormat outputFormat)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(outFileName, nameof(outFileName));
            outputFileName = outFileName;

            var outDir = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(outDir))
                throw new ArgumentNullException(nameof(outDir), "Directory does not exist");
            if (File.Exists(outputFileName))
                File.Delete(outputFileName);

            if (!Directory.Exists(cacheDirectory))
                throw new ArgumentNullException(nameof(cacheDirectory), "Directory does not exist");
            cacheDir = cacheDirectory;

            downloadLicense = ArgumentValidator.EnsureNotNull(dlLic, nameof(dlLic));
            OutputFormat = outputFormat;

            steps = new StepSequence
            {
                Name = "Download and Convert Aaxc To " + (outputFormat == OutputFormat.Mp4a ? "M4b" : "Mp3"),

                ["Step 1: Get Aaxc Metadata"] = Step1_GetMetadata,
                ["Step 2: Download Decrypted Audiobook"] = Step2_DownloadAndCombine,
                ["Step 3: Create Cue"] = Step3_CreateCue,
                ["Step 4: Create Nfo"] = Step4_CreateNfo,
                ["Step 5: Cleanup"] = Step5_Cleanup,
            };
        }

        /// <summary>
        /// Setting cover art by this method will insert the art into the audiobook metadata
        /// </summary>
        public void SetCoverArt(byte[] coverArt)
        {
            if (coverArt is null) return;

            aaxFile?.AppleTags.SetCoverArt(coverArt);

            RetrievedCoverArt?.Invoke(this, coverArt);
        }

        public bool Run()
        {
            var (IsSuccess, Elapsed) = steps.Run();

            if (!IsSuccess)
            {
                Console.WriteLine("WARNING-Conversion failed");
                return false;
            }

            var speedup = (int)(aaxFile.Duration.TotalSeconds / (long)Elapsed.TotalSeconds);
            Console.WriteLine("Speedup is " + speedup + "x realtime.");
            Console.WriteLine("Done");
            return true;
        }

        public bool Step1_GetMetadata()
        {
            //Get metadata from the file over http
                       
            if (File.Exists(jsonDownloadState))
            {
                try
                {
                    nfsPersister = new NetworkFileStreamPersister(jsonDownloadState);
                    //If More than ~1 hour has elapsed since getting the download url, it will expire.
                    //The new url will be to the same file.
                    nfsPersister.NetworkFileStream.SetUriForSameFile(new Uri(downloadLicense.DownloadUrl));
                }
                catch
                {
                    FileExt.SafeDelete(jsonDownloadState);
                    FileExt.SafeDelete(tempFile);
                    nfsPersister = NewNetworkFilePersister();
                }
            }
            else
            {
                nfsPersister = NewNetworkFilePersister();
            }

            aaxFile = new AaxFile(nfsPersister.NetworkFileStream);

            RetrievedTags?.Invoke(this, aaxFile.AppleTags);
            RetrievedCoverArt?.Invoke(this, aaxFile.AppleTags.Cover);

            return !isCanceled;
        }
        private NetworkFileStreamPersister NewNetworkFilePersister()
        {
			var headers = new System.Net.WebHeaderCollection
			{
				{ "User-Agent", downloadLicense.UserAgent }
			};

			var networkFileStream = new NetworkFileStream(tempFile, new Uri(downloadLicense.DownloadUrl), 0, headers);
            return new NetworkFileStreamPersister(networkFileStream, jsonDownloadState);
        }

        public bool Step2_DownloadAndCombine()
        {
            var zeroProgress = new DownloadProgress 
            { 
                BytesReceived = 0,
                ProgressPercentage = 0, 
                TotalBytesToReceive = nfsPersister.NetworkFileStream.Length 
            };

            DecryptProgressUpdate?.Invoke(this, zeroProgress);

            if (File.Exists(outputFileName))
                FileExt.SafeDelete(outputFileName);

            FileStream outFile = File.OpenWrite(outputFileName);

            aaxFile.SetDecryptionKey(downloadLicense.AudibleKey, downloadLicense.AudibleIV);

            aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
            var decryptionResult = OutputFormat == OutputFormat.Mp4a ? aaxFile.ConvertToMp4a(outFile, downloadLicense.ChapterInfo) : aaxFile.ConvertToMp3(outFile);
            aaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

            aaxFile.Close();

            downloadLicense.ChapterInfo = aaxFile.Chapters;

            nfsPersister.Dispose();

            DecryptProgressUpdate?.Invoke(this, zeroProgress);

            return decryptionResult == ConversionResult.NoErrorsDetected && !isCanceled;
        }

        private void AaxFile_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
        {
            var duration = aaxFile.Duration;
            double remainingSecsToProcess = (duration - e.ProcessPosition).TotalSeconds;
            double estTimeRemaining = remainingSecsToProcess / e.ProcessSpeed;

            if (double.IsNormal(estTimeRemaining))
                DecryptTimeRemaining?.Invoke(this, TimeSpan.FromSeconds(estTimeRemaining));

            double progressPercent = 100 * e.ProcessPosition.TotalSeconds / duration.TotalSeconds;

            DecryptProgressUpdate?.Invoke(this,
                new DownloadProgress 
                {
                    ProgressPercentage = progressPercent,
                    BytesReceived = (long)(nfsPersister.NetworkFileStream.Length * progressPercent),
                    TotalBytesToReceive = nfsPersister.NetworkFileStream.Length
                });
        }

        public bool Step3_CreateCue()
        {
            // not a critical step. its failure should not prevent future steps from running
            try
            {
                File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".cue"), Cue.CreateContents(Path.GetFileName(outputFileName), downloadLicense.ChapterInfo));
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, $"{nameof(Step3_CreateCue)}. FAILED");
            }
            return !isCanceled;
        }

        public bool Step4_CreateNfo()
        {
            // not a critical step. its failure should not prevent future steps from running
            try
            {
                File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".nfo"), NFO.CreateContents(AppName, aaxFile, downloadLicense.ChapterInfo));
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, $"{nameof(Step4_CreateNfo)}. FAILED");
            }
            return !isCanceled;
        }

        public bool Step5_Cleanup()
        {
            FileExt.SafeDelete(jsonDownloadState);
            FileExt.SafeDelete(tempFile);
            return !isCanceled;
        }

        public void Cancel()
        {
            isCanceled = true;
            aaxFile?.Cancel();
            aaxFile?.Dispose();
            nfsPersister?.NetworkFileStream?.Close();
            nfsPersister?.Dispose();
        }
    }
}
