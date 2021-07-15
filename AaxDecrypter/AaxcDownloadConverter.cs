using AAXClean;
using Dinah.Core;
using Dinah.Core.Diagnostics;
using Dinah.Core.IO;
using Dinah.Core.StepRunner;
using System;
using System.IO;

namespace AaxDecrypter
{
    public class AaxcDownloadConverter
    {
        public event EventHandler<AppleTags> RetrievedTags;
        public event EventHandler<byte[]> RetrievedCoverArt;
        public event EventHandler<int> DecryptProgressUpdate;
        public event EventHandler<TimeSpan> DecryptTimeRemaining;
        public string AppName { get; set; } = nameof(AaxcDownloadConverter);
        public string OutputFileName { get; private set; }

        private string cacheDir { get; }
        private DownloadLicense downloadLicense { get; }
        private AaxFile aaxFile;
        private byte[] coverArt;

        private StepSequence steps { get; }
        private NetworkFileStreamPersister nfsPersister;
        private bool isCanceled { get; set; }
        private string jsonDownloadState => Path.Combine(cacheDir, Path.GetFileNameWithoutExtension(OutputFileName) + ".json");
        private string tempFile => PathLib.ReplaceExtension(jsonDownloadState, ".aaxc");

        public AaxcDownloadConverter(string outFileName, string cacheDirectory, DownloadLicense dlLic)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(outFileName, nameof(outFileName));
            OutputFileName = PathLib.ReplaceExtension(outFileName, ".m4b");
            var outDir = Path.GetDirectoryName(OutputFileName);
            if (!Directory.Exists(outDir))
                throw new ArgumentNullException(nameof(outDir), "Directory does not exist");
            if (File.Exists(OutputFileName))
                File.Delete(OutputFileName);

            if (!Directory.Exists(cacheDirectory))
                throw new ArgumentNullException(nameof(cacheDirectory), "Directory does not exist");
            cacheDir = cacheDirectory;

            downloadLicense = ArgumentValidator.EnsureNotNull(dlLic, nameof(dlLic));

            steps = new StepSequence
            {
                Name = "Download and Convert Aaxc To M4b",

                ["Step 1: Get Aaxc Metadata"] = Step1_GetMetadata,
                ["Step 2: Download Decrypted Audiobook"] = Step2_DownloadAndCombine,
                ["Step 3: Create Cue"] = Step3_CreateCue,
                ["Step 4: Create Nfo"] = Step4_CreateNfo,
                ["Step 5: Cleanup"] = Step5_Cleanup,
            };
        }

        public void SetCoverArt(byte[] coverArt)
        {
            if (coverArt is null) return;

            this.coverArt = coverArt;
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
                    //If More thaan ~1 hour has elapsed since getting the download url, it will expire.
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
            nfsPersister.NetworkFileStream.BeginDownloading();

            aaxFile = new AaxFile(nfsPersister.NetworkFileStream);
            coverArt = aaxFile.AppleTags.Cover;

            RetrievedTags?.Invoke(this, aaxFile.AppleTags);
            RetrievedCoverArt?.Invoke(this, coverArt);

            return !isCanceled;
        }
        private NetworkFileStreamPersister NewNetworkFilePersister()
        {
            var headers = new System.Net.WebHeaderCollection();
            headers.Add("User-Agent", downloadLicense.UserAgent);

            NetworkFileStream networkFileStream = new NetworkFileStream(tempFile, new Uri(downloadLicense.DownloadUrl), 0, headers);
            return new NetworkFileStreamPersister(networkFileStream, jsonDownloadState);
        }

        public bool Step2_DownloadAndCombine()
        {
            OutputFormat format = OutputFormat.Mp4a;

            DecryptProgressUpdate?.Invoke(this, 0);

            if (File.Exists(OutputFileName))
                FileExt.SafeDelete(OutputFileName);

            FileStream outFile = File.OpenWrite(OutputFileName);

            aaxFile.ConversionProgressUpdate += AaxFile_ConversionProgressUpdate;
            var decryptionResult = aaxFile.DecryptAaxc(outFile, downloadLicense.AudibleKey, downloadLicense.AudibleIV, format, downloadLicense.ChapterInfo);
            aaxFile.ConversionProgressUpdate -= AaxFile_ConversionProgressUpdate;

            downloadLicense.ChapterInfo = aaxFile.Chapters;

            if (decryptionResult == ConversionResult.NoErrorsDetected
                && coverArt is not null
                && format == OutputFormat.Mp4a)
            {
                //This handles a special case where the aaxc file doesn't contain cover art and
                //Libation downloaded it instead (Animal Farm). Currently only works for Mp4a files.
                using var decryptedBook = new Mp4File(OutputFileName, FileAccess.ReadWrite);
                decryptedBook.AppleTags?.SetCoverArt(coverArt);
                decryptedBook.Save();
                decryptedBook.Close();
            }

            nfsPersister.Dispose();

            DecryptProgressUpdate?.Invoke(this, 0);

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

            DecryptProgressUpdate?.Invoke(this, (int)progressPercent);
        }

        public bool Step3_CreateCue()
        {
            // not a critical step. its failure should not prevent future steps from running
            try
            {
                File.WriteAllText(PathLib.ReplaceExtension(OutputFileName, ".cue"), Cue.CreateContents(Path.GetFileName(OutputFileName), downloadLicense.ChapterInfo));
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
                File.WriteAllText(PathLib.ReplaceExtension(OutputFileName, ".nfo"), NFO.CreateContents(AppName, aaxFile, downloadLicense.ChapterInfo));
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
        }
    }
}
