using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Diagnostics;
using Dinah.Core.IO;
using Dinah.Core.StepRunner;

namespace AaxDecrypter
{
    public interface ISimpleAaxToM4bConverter
    {
        event EventHandler<int> DecryptProgressUpdate;

        bool Run();

        string AppName { get; set; }
        string inputFileName { get; }
        byte[] coverBytes { get;  }
        string outDir { get; }
        string outputFileName { get; }

        Chapters chapters { get; }
        Tags tags { get; }
        EncodingInfo encodingInfo { get; }

        void SetOutputFilename(string outFileName);
    }
    public interface IAdvancedAaxToM4bConverter : ISimpleAaxToM4bConverter
    {
        bool Step1_CreateDir();
        bool Step2_DecryptAax();
        bool Step3_Chapterize();
        bool Step4_InsertCoverArt();
        bool Step5_Cleanup();
        bool Step6_AddTags();
        bool End_CreateCue();
        bool End_CreateNfo();
    }
    /// <summary>full c# app. integrated logging. no UI</summary>
    public class AaxToM4bConverter : IAdvancedAaxToM4bConverter
    {
        public event EventHandler<int> DecryptProgressUpdate;

        public string inputFileName { get; }
        public string decryptKey { get; private set; }

        private StepSequence steps { get; }
        public byte[] coverBytes { get; private set; }

        public string AppName { get; set; } = nameof(AaxToM4bConverter);

        public string outDir { get; private set; }
        public string outputFileName { get; private set; }

        public Chapters chapters { get; private set; }
        public Tags tags { get; private set; }
        public EncodingInfo encodingInfo { get; private set; }

        public static async Task<AaxToM4bConverter> CreateAsync(string inputFile, string decryptKey)
        {
            var converter = new AaxToM4bConverter(inputFile, decryptKey);
            await converter.prelimProcessing();
            converter.printPrelim();

            return converter;
        }
        private AaxToM4bConverter(string inputFile, string decryptKey)
        {
            if (string.IsNullOrWhiteSpace(inputFile)) throw new ArgumentNullException(nameof(inputFile), "Input file may not be null or whitespace");
            if (!File.Exists(inputFile)) throw new ArgumentNullException(nameof(inputFile), "File does not exist");

            steps = new StepSequence
            {
                Name = "Convert Aax To M4b",

                ["Step 1: Create Dir"] = Step1_CreateDir,
                ["Step 2: Decrypt Aax"] = Step2_DecryptAax,
                ["Step 3: Chapterize and tag"] = Step3_Chapterize,
                ["Step 4: Insert Cover Art"] = Step4_InsertCoverArt,
                ["Step 5: Cleanup"] = Step5_Cleanup,
                ["Step 6: Add Tags"] = Step6_AddTags,
                ["End: Create Cue"] = End_CreateCue,
                ["End: Create Nfo"] = End_CreateNfo
            };

            this.inputFileName = inputFile;
            this.decryptKey = decryptKey;
        }

        private async Task prelimProcessing()
        {
            this.tags = new Tags(this.inputFileName);
            this.encodingInfo = new EncodingInfo(this.inputFileName);
            this.chapters = new Chapters(this.inputFileName, this.tags.duration.TotalSeconds);

            var defaultFilename = Path.Combine(
                Path.GetDirectoryName(this.inputFileName),
                getASCIITag(this.tags.author),
                getASCIITag(this.tags.title) + ".m4b"
                );
            SetOutputFilename(defaultFilename);

            await Task.Run(() => saveCover(inputFileName));
        }
        private string getASCIITag(string property)
        {
            foreach (char ch in new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()))
                property = property.Replace(ch.ToString(), "");
            return property;
        }

        private void saveCover(string aaxFile)
        {
            using (var file = TagLib.File.Create(aaxFile, "audio/mp4", TagLib.ReadStyle.Average))
                this.coverBytes = file.Tag.Pictures[0].Data.Data;
        }

        private void printPrelim()
        {
            Console.WriteLine("Audible Book ID = " + tags.id);

            Console.WriteLine("Book: " + tags.title);
            Console.WriteLine("Author: " + tags.author);
            Console.WriteLine("Narrator: " + tags.narrator);
            Console.WriteLine("Year: " + tags.year);
            Console.WriteLine("Total Time: "
                + tags.duration.GetTotalTimeFormatted()
                + " in " + chapters.Count() + " chapters");
            Console.WriteLine("WARNING-Source is "
                + encodingInfo.originalBitrate + " kbits @ "
                + encodingInfo.sampleRate + "Hz, "
                + encodingInfo.channels + " channels");
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

        public void SetOutputFilename(string outFileName)
        {
            this.outputFileName = outFileName;

            if (Path.GetExtension(this.outputFileName) != ".m4b")
                this.outputFileName = outputFileWithNewExt(".m4b");

            this.outDir = Path.GetDirectoryName(this.outputFileName);
        }

        private string outputFileWithNewExt(string extension)
            => Path.Combine(this.outDir, Path.GetFileNameWithoutExtension(this.outputFileName) + '.' + extension.Trim('.'));

        public bool Step1_CreateDir()
        {
            ProcessRunner.WorkingDir = this.outDir;
            Directory.CreateDirectory(this.outDir);
            return true;
        }

        public bool Step2_DecryptAax()
        {
            DecryptProgressUpdate?.Invoke(this, 0);

            var tempRipFile = Path.Combine(this.outDir, "funny.aac");

            var fail = "WARNING-Decrypt failure. ";

            int returnCode;
            if (string.IsNullOrWhiteSpace(decryptKey))
            {
                returnCode = getKey_decrypt(tempRipFile);
            }
            else
            {
                returnCode = decrypt(tempRipFile);
                if (returnCode == -99)
                {
                    Console.WriteLine($"{fail}Incorrect decrypt key: {decryptKey}");
                    this.decryptKey = null;
                    returnCode = getKey_decrypt(tempRipFile);
                }
            }

            if (returnCode == 100)
                Console.WriteLine($"{fail}Thread completed without changing return code. This shouldn't be possible");
            else if (returnCode == 0)
            {
                // success!
                FileExt.SafeMove(tempRipFile, outputFileWithNewExt(".mp4"));
                DecryptProgressUpdate?.Invoke(this, 100);
                return true;
            }
            else if (returnCode == -99)
                Console.WriteLine($"{fail}Incorrect decrypt key: {decryptKey}");
            else // any other returnCode
                Console.WriteLine($"{fail}Unknown failure code: {returnCode}");

            FileExt.SafeDelete(tempRipFile);
            DecryptProgressUpdate?.Invoke(this, 0);
            return false;
        }

        private int getKey_decrypt(string tempRipFile)
        {
            getKey();
            return decrypt(tempRipFile);
        }
        private void getKey()
        {
            Console.WriteLine("Discovering decrypt key");

            Console.WriteLine("Getting file hash");
            var checksum = BytesCracker.GetChecksum(inputFileName);
            Console.WriteLine("File hash calculated: " + checksum);

            Console.WriteLine("Cracking activation bytes");
            var activation_bytes = BytesCracker.GetActivationBytes(checksum);
            this.decryptKey = activation_bytes;
            Console.WriteLine("Activation bytes cracked. Decrypt key: " + activation_bytes);
        }

        private int decrypt(string tempRipFile)
        {
            FileExt.SafeDelete(tempRipFile);

            Console.WriteLine("Decrypting with key " + decryptKey);

            var returnCode = 100;
            var thread = new Thread(() => returnCode = this.ngDecrypt());
            thread.Start();

            double fileLen = new FileInfo(this.inputFileName).Length;
            while (thread.IsAlive && returnCode == 100)
            {
                Thread.Sleep(500);
                if (File.Exists(tempRipFile))
                {
                    double tempLen = new FileInfo(tempRipFile).Length;
                    var percentProgress = tempLen / fileLen * 100.0;
                    DecryptProgressUpdate?.Invoke(this, (int)percentProgress);
                }
            }

            return returnCode;
        }

        private int ngDecrypt()
        {
            var info = new ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.mp4trackdumpPath,
                Arguments = "-c " + this.encodingInfo.channels + " -r " + this.encodingInfo.sampleRate + " \"" + this.inputFileName + "\""
            };
            info.EnvironmentVariables["VARIABLE"] = decryptKey;

            var (output, exitCode) = info.RunHidden();

            // bad checksum -- bad decrypt key
            if (output.Contains("checksums mismatch, aborting!"))
                return -99;

            return exitCode;
        }

        // temp file names for steps 3, 4, 5
        string tempChapsPath => Path.Combine(this.outDir, "tempChaps.mp4");
        string mp4_file => outputFileWithNewExt(".mp4");
        string ff_txt_file => mp4_file + ".ff.txt";

        public bool Step3_Chapterize()
        {
            string str1 = "";
            if (this.chapters.FirstChapterStart != 0.0)
            {
                str1 = " -ss " + this.chapters.FirstChapterStart.ToString("0.000", CultureInfo.InvariantCulture) + " -t " + (this.chapters.LastChapterStart - 1.0).ToString("0.000", CultureInfo.InvariantCulture) + " ";
            }

            string ffmpegTags = this.tags.GenerateFfmpegTags();
            string ffmpegChapters = this.chapters.GenerateFfmpegChapters();
            File.WriteAllText(ff_txt_file, ffmpegTags + ffmpegChapters);

            var tagAndChapterInfo = new ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.ffmpegPath,
                Arguments = "-y -i \"" + mp4_file + "\" -f ffmetadata -i \"" + ff_txt_file + "\" -map_metadata 1  -bsf:a aac_adtstoasc -c:a copy" + str1 + " -map 0 \"" + tempChapsPath + "\""
            };
            tagAndChapterInfo.RunHidden();

            return true;
        }

        public bool Step4_InsertCoverArt()
        {
            // save cover image as temp file
            var coverPath = Path.Combine(this.outDir, "cover-" + Guid.NewGuid() + ".jpg");
            FileExt.CreateFile(coverPath, this.coverBytes);

            var insertCoverArtInfo = new ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.atomicParsleyPath,
                Arguments = "\"" + tempChapsPath + "\" --encodingTool \"" + AppName + "\" --artwork \"" + coverPath + "\" --overWrite"
            };
            insertCoverArtInfo.RunHidden();

            // delete temp file
            FileExt.SafeDelete(coverPath);

            return true;
        }

        public bool Step5_Cleanup()
        {
            FileExt.SafeDelete(mp4_file);
            FileExt.SafeDelete(ff_txt_file);
            FileExt.SafeMove(tempChapsPath, this.outputFileName);

            return true;
        }

        public bool Step6_AddTags()
        {
            this.tags.AddAppleTags(this.outputFileName);
            return true;
        }

        public bool End_CreateCue()
        {
            File.WriteAllText(outputFileWithNewExt(".cue"), this.chapters.GetCuefromChapters(Path.GetFileName(this.outputFileName)));
            return true;
        }

        public bool End_CreateNfo()
        {
            File.WriteAllText(outputFileWithNewExt(".nfo"), NFO.CreateNfoContents(AppName, this.tags, this.encodingInfo, this.chapters));
            return true;
        }
    }
}
