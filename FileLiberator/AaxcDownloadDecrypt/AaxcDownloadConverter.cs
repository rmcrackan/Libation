using AaxDecrypter;
using AudibleApi;
using AudibleApiDTOs;
using Dinah.Core;
using Dinah.Core.Diagnostics;
using Dinah.Core.IO;
using Dinah.Core.StepRunner;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileLiberator.AaxcDownloadDecrypt
{
    public interface ISimpleAaxToM4bConverter2
    {
        event EventHandler<int> DecryptProgressUpdate;

        bool Run();

        string AppName { get; set; }
        string outDir { get; }
        string outputFileName { get; }
        ChapterInfo chapters { get; }
        Tags tags { get; }
        void SetOutputFilename(string outFileName);

    }
    public interface IAdvancedAaxcToM4bConverter : ISimpleAaxToM4bConverter2
    {
        bool Step1_CreateDir();
        bool Step2_DownloadAndCombine();
        bool Step3_InsertCoverArt();
        bool Step4_CreateCue();
        bool Step5_CreateNfo();
        bool Step6_Cleanup();
    }
    class AaxcDownloadConverter : IAdvancedAaxcToM4bConverter
    {
        public string AppName { get; set; } = nameof(AaxcDownloadConverter);
        public string outDir { get; private set; }
        public string outputFileName { get; private set; }
        public ChapterInfo chapters { get; private set; }
        public Tags tags { get; private set; }

        public event EventHandler<int> DecryptProgressUpdate;

        private StepSequence steps { get; }
        private DownloadLicense downloadLicense { get; set; }
        private string coverArtPath => Path.Combine(outDir, Path.GetFileName(outputFileName) + ".jpg");
        private string metadataPath => Path.Combine(outDir, Path.GetFileName(outputFileName) + ".ffmeta");

        public static async Task<AaxcDownloadConverter> CreateAsync(string outDirectory, DownloadLicense dlLic, ChapterInfo chapters)
        {
            var converter = new AaxcDownloadConverter(outDirectory, dlLic, chapters);           
            await converter.prelimProcessing();
            return converter;
        }

        private AaxcDownloadConverter(string outDirectory, DownloadLicense dlLic, ChapterInfo chapters)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(outDirectory, nameof(outDirectory));
            ArgumentValidator.EnsureNotNull(dlLic, nameof(dlLic));
            ArgumentValidator.EnsureNotNull(chapters, nameof(chapters));

            if (!Directory.Exists(outDirectory))
                throw new ArgumentNullException(nameof(outDirectory), "Directory does not exist");
            outDir = outDirectory;

            steps = new StepSequence
            {
                Name = "Convert Aax To M4b",

                ["Step 1: Create Dir"] = Step1_CreateDir,
                ["Step 2: Download and Combine Audiobook"] = Step2_DownloadAndCombine,
                ["Step 3 Insert Cover Art"] = Step3_InsertCoverArt,
                ["Step 4 Create Cue"] = Step4_CreateCue,
                ["Step 5 Create Nfo"] = Step5_CreateNfo,
                ["Step 6: Cleanup"] = Step6_Cleanup,
            };

            downloadLicense = dlLic;
            this.chapters = chapters;
        }

        private async Task prelimProcessing()
        {
            //Get metadata from the file over http
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", Resources.UserAgent);

            var networkFile = await NetworkFileAbstraction.CreateAsync(client, new Uri(downloadLicense.DownloadUrl));
            var tagLibFile = await Task.Run(()=>TagLib.File.Create(networkFile, "audio/mp4", TagLib.ReadStyle.Average));

            tags = new Tags(tagLibFile);

            var defaultFilename = Path.Combine(
              outDir,
              PathLib.ToPathSafeString(tags.author),
              PathLib.ToPathSafeString(tags.title) + ".m4b"
              );

            SetOutputFilename(defaultFilename);
        }

        public void SetOutputFilename(string outFileName)
        {
            outputFileName = PathLib.ReplaceExtension(outFileName, ".m4b");
            outDir = Path.GetDirectoryName(outputFileName);

            if (File.Exists(outputFileName))
                File.Delete(outputFileName);
        }

        public bool Run()
        {
            var (IsSuccess, Elapsed) = steps.Run();

            if (!IsSuccess)
            {
                Console.WriteLine("WARNING-Conversion failed");
                return false;
            }

            var speedup = (int)(tags.duration.TotalSeconds / (long)Elapsed.TotalSeconds);
            Console.WriteLine("Speedup is " + speedup + "x realtime.");
            Console.WriteLine("Done");
            return true;
        }

        public bool Step1_CreateDir()
        {
            ProcessRunner.WorkingDir = outDir;
            Directory.CreateDirectory(outDir);

            return true;
        }

        public bool Step2_DownloadAndCombine()
        {
            var ffmpegTags = tags.GenerateFfmpegTags();
            var ffmpegChapters = GenerateFfmpegChapters(chapters);

            File.WriteAllText(metadataPath, ffmpegTags + ffmpegChapters);

            var aaxcProcesser = new FFMpegAaxcProcesser(DecryptSupportLibraries.ffmpegPath);

            aaxcProcesser.ProgressUpdate += (_, e) => 
            DecryptProgressUpdate?.Invoke(this, (int)e.ProgressPercent);

            aaxcProcesser.ProcessBook(
                downloadLicense.DownloadUrl,
                Resources.UserAgent,
                downloadLicense.AudibleKey,
                downloadLicense.AudibleIV,
                metadataPath,
                outputFileName)
                .GetAwaiter()
                .GetResult();

            return aaxcProcesser.Succeeded;
        }

        private static string GenerateFfmpegChapters(ChapterInfo chapters)
        {
            var stringBuilder = new System.Text.StringBuilder();

            foreach (AudibleApiDTOs.Chapter c in chapters.Chapters)
            {
                stringBuilder.Append("[CHAPTER]\n");
                stringBuilder.Append("TIMEBASE=1/1000\n");
                stringBuilder.Append("START=" + c.StartOffsetMs + "\n");
                stringBuilder.Append("END=" + (c.StartOffsetMs + c.LengthMs) + "\n");
                stringBuilder.Append("title=" + c.Title + "\n");
            }

            return stringBuilder.ToString();
        }

        public bool Step3_InsertCoverArt()
        {

            File.WriteAllBytes(coverArtPath, tags.coverArt);

            var insertCoverArtInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.atomicParsleyPath,
                Arguments = "\"" + outputFileName + "\" --encodingTool \"" + AppName + "\" --artwork \"" + coverArtPath + "\" --overWrite"
            };
            insertCoverArtInfo.RunHidden();

            // delete temp file
            FileExt.SafeDelete(coverArtPath);

            return true;
        }
        public bool Step4_CreateCue()
        {
            File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".cue"), Cue.CreateContents(Path.GetFileName(outputFileName), chapters));
            return true;
        }

        public bool Step5_CreateNfo()
        {
            File.WriteAllText(PathLib.ReplaceExtension(outputFileName, ".nfo"), NFO.CreateContents(AppName, tags, chapters));
            return true;
        }

        public bool Step6_Cleanup()
        {
            FileExt.SafeDelete(metadataPath);
            return true;
        }
    }
}
