using Dinah.Core;
using Dinah.Core.Diagnostics;
using Dinah.Core.IO;
using Dinah.Core.StepRunner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AaxDecrypter
{
    public interface ISimpleAaxToM4bConverter2
    {
        event EventHandler<int> DecryptProgressUpdate;
        bool Run();
        string AppName { get; set; }
        string outDir { get; }
        string outputFileName { get; }
        ChapterInfo chapters { get; }
        TagLib.Mpeg4.File tags { get; }
        void SetOutputFilename(string outFileName);

    }
    public interface IAdvancedAaxcToM4bConverter : ISimpleAaxToM4bConverter2
    {
        bool Step1_CreateDir();
        bool Step2_DownloadAndCombine();
        bool Step3_RestoreMetadata();
        bool Step4_CreateCue();
        bool Step5_CreateNfo();
    }
    public class AaxcDownloadConverter : IAdvancedAaxcToM4bConverter
    {
        public string AppName { get; set; } = nameof(AaxcDownloadConverter);
        public string outDir { get; private set; }
        public string outputFileName { get; private set; }
        public ChapterInfo chapters { get; private set; }
        public TagLib.Mpeg4.File tags { get; private set; }
        public event EventHandler<int> DecryptProgressUpdate;

        public string Title => tags.Tag.Title.Replace(" (Unabridged)", "");
        public string Author => tags.Tag.FirstPerformer ?? "[unknown]";
        public string Narrator => string.IsNullOrWhiteSpace(tags.Tag.FirstComposer) ? tags.GetTag(TagLib.TagTypes.Apple).Narrator : tags.Tag.FirstComposer;
        public byte[] CoverArt => tags.Tag.Pictures.Length > 0 ? tags.Tag.Pictures[0].Data.Data : default;



        private StepSequence steps { get; }
        private DownloadLicense downloadLicense { get; set; }
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
                ["Step 2: Restore Aaxc Metadata"] = Step3_RestoreMetadata,
                ["Step 3 Create Cue"] = Step4_CreateCue,
                ["Step 4 Create Nfo"] = Step5_CreateNfo,
            };

            downloadLicense = dlLic;
            this.chapters = chapters;
        }

        private async Task prelimProcessing()
        {
            //Get metadata from the file over http
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", downloadLicense.UserAgent);

            var networkFile = await NetworkFileAbstraction.CreateAsync(client, new Uri(downloadLicense.DownloadUrl));

            tags = await Task.Run(() => TagLib.File.Create(networkFile, "audio/mp4", TagLib.ReadStyle.Average) as TagLib.Mpeg4.File);

            var defaultFilename = Path.Combine(
              outDir,
              PathLib.ToPathSafeString(tags.Tag.FirstPerformer??"[unknown]"),
              PathLib.ToPathSafeString(tags.Tag.Title.Replace(" (Unabridged)", "")) + ".m4b"
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

            var speedup = (int)(tags.Properties.Duration.TotalSeconds / (long)Elapsed.TotalSeconds);
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
            var ffmetaHeader = $";FFMETADATA1\n";

            File.WriteAllText(metadataPath, ffmetaHeader + chapters.ToFFMeta());

            var aaxcProcesser = new FFMpegAaxcProcesser(DecryptSupportLibraries.ffmpegPath);

            aaxcProcesser.ProgressUpdate += AaxcProcesser_ProgressUpdate;

            aaxcProcesser.ProcessBook(
                downloadLicense.DownloadUrl,
                downloadLicense.UserAgent,
                downloadLicense.AudibleKey,
                downloadLicense.AudibleIV,
                metadataPath,
                outputFileName)
                .GetAwaiter()
                .GetResult();

            DecryptProgressUpdate?.Invoke(this, 0);

            FileExt.SafeDelete(metadataPath);
            return aaxcProcesser.Succeeded;
        }

        private void AaxcProcesser_ProgressUpdate(object sender, TimeSpan e)
        {
            double progressPercent = 100 * e.TotalSeconds / tags.Properties.Duration.TotalSeconds;

            DecryptProgressUpdate?.Invoke(this, (int)progressPercent);

            speedSamples.Enqueue(new DataRate
            {
                SampleTime = DateTime.Now,
                ProcessPosition = e
            });

            int sampleNum = 5;

            if (speedSamples.Count < sampleNum) return;

            var oldestSample = speedSamples.Dequeue();
            double harmonicDenom = 0;
            foreach (var sample in speedSamples)
            {
                double inverseRate = (sample.SampleTime - oldestSample.SampleTime).TotalSeconds / (sample.ProcessPosition.TotalSeconds - oldestSample.ProcessPosition.TotalSeconds);
                harmonicDenom += inverseRate;
                oldestSample = sample;
            }
            double averageRate = (sampleNum - 1) / harmonicDenom;

        }

        private Queue<DataRate> speedSamples = new Queue<DataRate>(5);
        private class DataRate
        {
            public DateTime SampleTime;
            public TimeSpan ProcessPosition;
        }

        public bool Step3_RestoreMetadata()
        {
            var outFile = new TagLib.Mpeg4.File(outputFileName, TagLib.ReadStyle.Average);

            var destTags = outFile.GetTag(TagLib.TagTypes.Apple) as TagLib.Mpeg4.AppleTag;
            destTags.Clear();

            var sourceTag = tags.GetTag(TagLib.TagTypes.Apple) as TagLib.Mpeg4.AppleTag;

            //copy all metadata fields in the source file, even those that TagLib doesn't
            //recognize, to the output file.
            foreach (var stag in sourceTag)
            {
                destTags.SetData(stag.BoxType, stag.Children.Cast<TagLib.Mpeg4.AppleDataBox>().ToArray());
            }
            outFile.Save();
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
    }
}
