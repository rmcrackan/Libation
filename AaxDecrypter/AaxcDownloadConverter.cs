using AAXClean;
using Dinah.Core;
using Dinah.Core.Diagnostics;
using Dinah.Core.IO;
using Dinah.Core.StepRunner;
using System;
using System.IO;

namespace AaxDecrypter
{
    public interface ISimpleAaxcToM4bConverter
    {
        event EventHandler<AppleTags> RetrievedTags;
        event EventHandler<byte[]> RetrievedCoverArt;
        event EventHandler<TimeSpan> DecryptTimeRemaining;
        event EventHandler<int> DecryptProgressUpdate;
        bool Run();
        string AppName { get; set; }
        string outDir { get; }
        string outputFileName { get; }
        DownloadLicense downloadLicense { get; }
        AaxFile aaxFile { get; }
        byte[] coverArt { get; }
        void SetCoverArt(byte[] coverArt);
        void SetOutputFilename(string outFileName);
    }
    public interface IAdvancedAaxcToM4bConverter : ISimpleAaxcToM4bConverter
    {
        void Cancel();
        bool Step1_CreateDir();
        bool Step2_GetMetadata();
        bool Step3_DownloadAndCombine();
        bool Step4_CreateCue();
        bool Step5_CreateNfo();
        bool Step6_Cleanup();
    }
    public class AaxcDownloadConverter : IAdvancedAaxcToM4bConverter
    {
        public event EventHandler<AppleTags> RetrievedTags;
        public event EventHandler<byte[]> RetrievedCoverArt;
        public event EventHandler<int> DecryptProgressUpdate;
        public event EventHandler<TimeSpan> DecryptTimeRemaining;
        public string AppName { get; set; } = nameof(AaxcDownloadConverter);
        public string outDir { get; private set; }
        public string cacheDir { get; private set; }
        public string outputFileName { get; private set; }
        public DownloadLicense downloadLicense { get; private set; }
        public AaxFile aaxFile { get; private set; }
        public byte[] coverArt { get; private set; }

        private StepSequence steps { get; }
        private NetworkFileStreamPersister nfsPersister;
        private bool isCanceled { get; set; }
        private string jsonDownloadState => Path.Combine(cacheDir, Path.GetFileNameWithoutExtension(outputFileName) + ".json");
        private string tempFile => PathLib.ReplaceExtension(jsonDownloadState, ".aaxc");

        public static AaxcDownloadConverter Create(string cacheDirectory, string outDirectory, DownloadLicense dlLic)
        {
            var converter = new AaxcDownloadConverter(cacheDirectory, outDirectory, dlLic);
            converter.SetOutputFilename(Path.GetTempFileName());
            return converter;
        }

        private AaxcDownloadConverter(string cacheDirectory, string outDirectory, DownloadLicense dlLic)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(outDirectory, nameof(outDirectory));
            ArgumentValidator.EnsureNotNull(dlLic, nameof(dlLic));

            if (!Directory.Exists(outDirectory))
                throw new ArgumentNullException(nameof(cacheDirectory), "Directory does not exist");
            if (!Directory.Exists(outDirectory))
                throw new ArgumentNullException(nameof(outDirectory), "Directory does not exist");

            cacheDir = cacheDirectory;
            outDir = outDirectory;

            steps = new StepSequence
            {
                Name = "Download and Convert Aaxc To M4b",

                ["Step 1: Create Dir"] = Step1_CreateDir,
                ["Step 2: Get Aaxc Metadata"] = Step2_GetMetadata,
                ["Step 3: Download Decrypted Audiobook"] = Step3_DownloadAndCombine,
                ["Step 4: Create Cue"] = Step4_CreateCue,
                ["Step 5: Create Nfo"] = Step5_CreateNfo,
                ["Step 6: Cleanup"] = Step6_Cleanup,
            };

            downloadLicense = dlLic;
        }

        public void SetOutputFilename(string outFileName)
        {
            outputFileName = PathLib.ReplaceExtension(outFileName, ".m4b");
            outDir = Path.GetDirectoryName(outputFileName);

            if (File.Exists(outputFileName))
                File.Delete(outputFileName);
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

        public bool Step1_CreateDir()
        {
            ProcessRunner.WorkingDir = outDir;
            Directory.CreateDirectory(outDir);

            return !isCanceled;
        }

        public bool Step2_GetMetadata()
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

        public bool Step3_DownloadAndCombine()
        {
            OutputFormat format = OutputFormat.Mp4a;

            DecryptProgressUpdate?.Invoke(this, 0);

            if (File.Exists(outputFileName))
                FileExt.SafeDelete(outputFileName);

            FileStream outFile = File.OpenWrite(outputFileName);

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
                using var decryptedBook = new Mp4File(outputFileName, FileAccess.ReadWrite);
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

        public bool Step4_CreateCue()
        {
            try
            {
                File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".cue"), Cue.CreateContents(Path.GetFileName(outputFileName), downloadLicense.ChapterInfo));
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, $"{nameof(Step4_CreateCue)}. FAILED");
            }
            return !isCanceled;
        }

        public bool Step5_CreateNfo()
        {
            try
            {
                File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".nfo"), NFO.CreateContents(AppName, aaxFile, downloadLicense.ChapterInfo));
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, $"{nameof(Step5_CreateNfo)}. FAILED");
            }
            return !isCanceled;
        }

        public bool Step6_Cleanup()
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
