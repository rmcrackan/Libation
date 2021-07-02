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
        event EventHandler<AaxcTagLibFile> RetrievedTags;
        event EventHandler<byte[]> RetrievedCoverArt;
        event EventHandler<TimeSpan> DecryptTimeRemaining;
        event EventHandler<int> DecryptProgressUpdate;
        bool Run();
        string AppName { get; set; }
        string outDir { get; }
        string outputFileName { get; }
        DownloadLicense downloadLicense { get; }
        AaxcTagLibFile aaxcTagLib { get; }
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
        bool Step4_RestoreMetadata();
        bool Step5_CreateCue();
        bool Step6_CreateNfo();
    }
    public class AaxcDownloadConverter : IAdvancedAaxcToM4bConverter
    {
        public event EventHandler<AaxcTagLibFile> RetrievedTags;
        public event EventHandler<byte[]> RetrievedCoverArt;
        public event EventHandler<int> DecryptProgressUpdate;
        public event EventHandler<TimeSpan> DecryptTimeRemaining;
        public string AppName { get; set; } = nameof(AaxcDownloadConverter);
        public string outDir { get; private set; }
        public string outputFileName { get; private set; }
        public DownloadLicense downloadLicense { get; private set; }
        public AaxcTagLibFile aaxcTagLib { get; private set; }
        public byte[] coverArt { get; private set; }

        private StepSequence steps { get; }
        private FFMpegAaxcProcesser aaxcProcesser;
        private bool isCanceled { get; set; }

        public static AaxcDownloadConverter Create(string outDirectory, DownloadLicense dlLic)
        {
            var converter = new AaxcDownloadConverter(outDirectory, dlLic);
            converter.SetOutputFilename(Path.GetTempFileName());
            return converter;
        }

        private AaxcDownloadConverter(string outDirectory, DownloadLicense dlLic)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(outDirectory, nameof(outDirectory));
            ArgumentValidator.EnsureNotNull(dlLic, nameof(dlLic));

            if (!Directory.Exists(outDirectory))
                throw new ArgumentNullException(nameof(outDirectory), "Directory does not exist");
            outDir = outDirectory;

            steps = new StepSequence
            {
                Name = "Convert Aax To M4b",

                ["Step 1: Create Dir"] = Step1_CreateDir,
                ["Step 2: Get Aaxc Metadata"] = Step2_GetMetadata,
                ["Step 3: Download Decrypted Audiobook"] = Step3_DownloadAndCombine,
                ["Step 4: Restore Aaxc Metadata"] = Step4_RestoreMetadata,
                ["Step 5: Create Cue"] = Step5_CreateCue,
                ["Step 6: Create Nfo"] = Step6_CreateNfo,
            };

            aaxcProcesser = new FFMpegAaxcProcesser(dlLic);
            aaxcProcesser.ProgressUpdate += AaxcProcesser_ProgressUpdate;

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

            var speedup = (int)(aaxcTagLib.Properties.Duration.TotalSeconds / (long)Elapsed.TotalSeconds);
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
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", downloadLicense.UserAgent);
            var networkFile = NetworkFileAbstraction.CreateAsync(client, new Uri(downloadLicense.DownloadUrl)).GetAwaiter().GetResult();

            aaxcTagLib = new AaxcTagLibFile(networkFile);

            if (coverArt is null && aaxcTagLib.AppleTags.Pictures.Length > 0)
            {
                coverArt = aaxcTagLib.AppleTags.Pictures[0].Data.Data;
            }

            RetrievedTags?.Invoke(this, aaxcTagLib);
            RetrievedCoverArt?.Invoke(this, coverArt);

            return !isCanceled;
        }

        public bool Step3_DownloadAndCombine()
        {
            DecryptProgressUpdate?.Invoke(this, int.MaxValue);

            bool userSuppliedChapters = downloadLicense.ChapterInfo != null;

            string metadataPath = null;

            if (userSuppliedChapters)
            {
                //Only write chaopters to the metadata file. All other aaxc metadata will be
                //wiped out but is restored in Step 3.
                metadataPath = Path.Combine(outDir, Path.GetFileName(outputFileName) + ".ffmeta");
                File.WriteAllText(metadataPath, downloadLicense.ChapterInfo.ToFFMeta(true));
            }

            aaxcProcesser.ProcessBook(
                outputFileName,
                metadataPath)
                .GetAwaiter()
                .GetResult();

            if (!userSuppliedChapters && aaxcProcesser.Succeeded)
                downloadLicense.ChapterInfo = new ChapterInfo(outputFileName);

            if (userSuppliedChapters)
                FileExt.SafeDelete(metadataPath);

            DecryptProgressUpdate?.Invoke(this, 0);

            return aaxcProcesser.Succeeded && !isCanceled;
        }

        private void AaxcProcesser_ProgressUpdate(object sender, AaxcProcessUpdate e)
        {
            double remainingSecsToProcess = (aaxcTagLib.Properties.Duration - e.ProcessPosition).TotalSeconds;
            double estTimeRemaining = remainingSecsToProcess / e.ProcessSpeed;

            if (double.IsNormal(estTimeRemaining))
                DecryptTimeRemaining?.Invoke(this, TimeSpan.FromSeconds(estTimeRemaining));

            double progressPercent = 100 * e.ProcessPosition.TotalSeconds / aaxcTagLib.Properties.Duration.TotalSeconds;

            DecryptProgressUpdate?.Invoke(this, (int)progressPercent);
        }

        /// <summary>
        /// Copy all aacx metadata to m4b file, including cover art.
        /// </summary>
        public bool Step4_RestoreMetadata()
        {
            var outFile = new AaxcTagLibFile(outputFileName);
            outFile.CopyTagsFrom(aaxcTagLib);

            if (outFile.AppleTags.Pictures.Length == 0 && coverArt is not null)
            {
                outFile.AddPicture(coverArt);
            }

            outFile.Save();

            return !isCanceled;
        }

        public bool Step5_CreateCue()
        {
            try
            {
                File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".cue"), Cue.CreateContents(Path.GetFileName(outputFileName), downloadLicense.ChapterInfo));
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, $"{nameof(Step5_CreateCue)}. FAILED");
            }
            return !isCanceled;
        }

        public bool Step6_CreateNfo()
        {
            try
            {
                File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".nfo"), NFO.CreateContents(AppName, aaxcTagLib, downloadLicense.ChapterInfo));
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error(ex, $"{nameof(Step5_CreateCue)}. FAILED");
            }
            return !isCanceled;
        }

        public void Cancel()
        {
            isCanceled = true;
            aaxcProcesser.Cancel();
        }
    }
}
