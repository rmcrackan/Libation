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
        public string audible_key { get; private set; }
        public string audible_iv { get; private set; }

        private StepSequence steps { get; }
        public byte[] coverBytes { get; private set; }

        public string AppName { get; set; } = nameof(AaxToM4bConverter);

        public string outDir { get; private set; }
        public string outputFileName { get; private set; }

        public Chapters chapters { get; private set; }
        public Tags tags { get; private set; }
        public EncodingInfo encodingInfo { get; private set; }

        public static async Task<AaxToM4bConverter> CreateAsync(string inputFile, string audible_key, string audible_iv, Chapters chapters = null)
        {
            var converter = new AaxToM4bConverter(inputFile, audible_key, audible_iv);
            converter.chapters = chapters ?? new AAXChapters(inputFile);
            await converter.prelimProcessing();
            converter.printPrelim();

            return converter;
        }
        private AaxToM4bConverter(string inputFile, string audible_key, string audible_iv)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(inputFile, nameof(inputFile));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(audible_key, nameof(audible_key));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(audible_iv, nameof(audible_iv));

            if (!File.Exists(inputFile))
                throw new ArgumentNullException(nameof(inputFile), "File does not exist");

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

            inputFileName = inputFile;
            this.audible_key = audible_key;
            this.audible_iv = audible_iv;
        }

        private async Task prelimProcessing()
        {
            tags = new Tags(inputFileName);
            encodingInfo = new EncodingInfo(inputFileName);

            var defaultFilename = Path.Combine(
                Path.GetDirectoryName(inputFileName),
                PathLib.ToPathSafeString(tags.author),
                PathLib.ToPathSafeString(tags.title) + ".m4b"
                );

            // set default name
            SetOutputFilename(defaultFilename);

            await Task.Run(() => saveCover(inputFileName));
        }

        private void saveCover(string aaxFile)
        {
            using var file = TagLib.File.Create(aaxFile, "audio/mp4", TagLib.ReadStyle.Average);
            coverBytes = file.Tag.Pictures[0].Data.Data;
        }

        private void printPrelim()
        {
            Console.WriteLine($"Audible Book ID = {tags.id}");

            Console.WriteLine($"Book: {tags.title}");
            Console.WriteLine($"Author: {tags.author}");
            Console.WriteLine($"Narrator: {tags.narrator}");
            Console.WriteLine($"Year: {tags.year}");
            Console.WriteLine($"Total Time: {tags.duration.GetTotalTimeFormatted()} in {chapters.Count} chapters");
            Console.WriteLine($"WARNING-Source is {encodingInfo.originalBitrate} kbits @ {encodingInfo.sampleRate}Hz, {encodingInfo.channels} channels");
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
            outputFileName = PathLib.ReplaceExtension(outFileName, ".m4b");
            outDir = Path.GetDirectoryName(outputFileName);

            if (File.Exists(outputFileName))
                File.Delete(outputFileName);
        }

        private string outputFileWithNewExt(string extension) => PathLib.ReplaceExtension(outputFileName, extension);

        public bool Step1_CreateDir()
        {
            ProcessRunner.WorkingDir = outDir;
            Directory.CreateDirectory(outDir);
            return true;
        }

        public bool Step2_DecryptAax()
        {
            DecryptProgressUpdate?.Invoke(this, 0);

            var tempRipFile = Path.Combine(outDir, "funny.aac");

            var fail = "WARNING-Decrypt failure. ";

            int returnCode;

            returnCode = decrypt(tempRipFile);
            if (returnCode == -99)
                Console.WriteLine($"{fail}Incorrect decrypt key.");
            else if (returnCode == 100)
                Console.WriteLine($"{fail}Thread completed without changing return code. This shouldn't be possible");
            else if (returnCode == 0)
            {
                // success!
                FileExt.SafeMove(tempRipFile, outputFileWithNewExt(".mp4"));
                DecryptProgressUpdate?.Invoke(this, 100);
                return true;
            }
            else // any other returnCode
                Console.WriteLine($"{fail}Unknown failure code: {returnCode}");

            FileExt.SafeDelete(tempRipFile);
            DecryptProgressUpdate?.Invoke(this, 0);
            return false;
        }


        private int decrypt(string tempRipFile)
        {
            FileExt.SafeDelete(tempRipFile);

            Console.WriteLine($"Decrypting with key={audible_key}, iv={audible_iv}");

            var returnCode = 100;
            var thread = new Thread((b) => returnCode = ngDecrypt(b));
            thread.Start(tempRipFile);

            double fileLen = new FileInfo(inputFileName).Length;
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

        private int ngDecrypt(object tempFileNameObj)
        {
            #region avformat-58.dll HACK EXPLANATION
            /* avformat-58.dll HACK EXPLANATION
             * 
             * FFMPEG refused to copy the aac stream from AAXC files with 44kHz sample rates
             * with error "Scalable configurations are not allowed in ADTS". The adts encoder
             * can be found on github at:              
             * https://github.com/FFmpeg/FFmpeg/blob/master/libavformat/adtsenc.c
             * 
             * adtsenc detects scalable aac by a flag in the aac metadata and throws an error if
             * it is set. It appears that all aaxc files contain aac streams that can be written 
             * to adts, but either the codec is parsing the header incorrectly or the aaxc
             * header is incorrect. 
             * 
             * As a workaround, i've modified avformat-58.dll to allow adtsenc to ignore the 
             * scalable flag and continue. To modify:
             * 
             * Open ffmpeg.exe in x64dbg (https://x64dbg.com)
             * 
             * Navigate to the avformat module and search for the error string "Scalable 
             * configurations are not allowed in ADTS". (00007FFE16AA5899 in example below).
             * 
             * 00007FFE16AA587B | 4C:8D05 DE5E6900     | lea r8,qword ptr ds:[7FFE1713B760]       | 00007FFE1713B760:"960/120 MDCT window is not allowed in ADTS\n"
             * 00007FFE16AA5882 | BA 10000000          | mov edx,10                               |
             * 00007FFE16AA5887 | 4C:89F1              | mov rcx,r14                              |
             * 00007FFE16AA588A | E8 697A1900          | call <JMP.&av_log>                       |
             * 00007FFE16AA588F | B8 B7B1BBBE          | mov eax,BEBBB1B7                         |
             * 00007FFE16AA5894 | E9 D5F8FFFF          | jmp avformat-58.7FFE16AA516E             |
             * 00007FFE16AA5899 | 4C:8D05 F05E6900     | lea r8,qword ptr ds:[7FFE1713B790]       | 00007FFE1713B790:"Scalable configurations are not allowed in ADTS\n"
             * 00007FFE16AA58A0 | BA 10000000          | mov edx,10                               |
             * 00007FFE16AA58A5 | 4C:89F1              | mov rcx,r14                              |
             * 00007FFE16AA58A8 | E8 4B7A1900          | call <JMP.&av_log>                       |
             * 00007FFE16AA58AD | B8 B7B1BBBE          | mov eax,BEBBB1B7                         |
             * 00007FFE16AA58B2 | E9 B7F8FFFF          | jmp avformat-58.7FFE16AA516E             |
             * 00007FFE16AA58B7 | 4C:8D05 4A5E6900     | lea r8,qword ptr ds:[7FFE1713B708]       | 00007FFE1713B708:"MPEG-4 AOT %d is not allowed in ADTS\n"
             * 00007FFE16AA58BE | BA 10000000          | mov edx,10                               |
             * 00007FFE16AA58C3 | 4C:89F1              | mov rcx,r14                              |
             * 00007FFE16AA58C6 | E8 2D7A1900          | call <JMP.&av_log>                       |
             * 00007FFE16AA58CB | B8 B7B1BBBE          | mov eax,BEBBB1B7                         |
             * 00007FFE16AA58D0 | E9 99F8FFFF          | jmp avformat-58.7FFE16AA516E             |
             * 00007FFE16AA58D5 | 4C:8D05 EC5E6900     | lea r8,qword ptr ds:[7FFE1713B7C8]       | 00007FFE1713B7C8:"Extension flag is not allowed in ADTS\n"
             * 00007FFE16AA58DC | BA 10000000          | mov edx,10                               |
             * 00007FFE16AA58E1 | 4C:89F1              | mov rcx,r14                              |
             * 00007FFE16AA58E4 | E8 0F7A1900          | call <JMP.&av_log>                       |
             * 00007FFE16AA58E9 | B8 B7B1BBBE          | mov eax,BEBBB1B7                         |
             * 00007FFE16AA58EE | E9 7BF8FFFF          | jmp avformat-58.7FFE16AA516E             |
             * 00007FFE16AA58F3 | 4C:8D05 365E6900     | lea r8,qword ptr ds:[7FFE1713B730]       | 00007FFE1713B730:"Escape sample rate index illegal in ADTS\n"
             * 00007FFE16AA58FA | BA 10000000          | mov edx,10                               |
             * 00007FFE16AA58FF | 4C:89F1              | mov rcx,r14                              |
             * 00007FFE16AA5902 | E8 F1791900          | call <JMP.&av_log>                       |
             * 00007FFE16AA5907 | B8 B7B1BBBE          | mov eax,BEBBB1B7                         |
             * 00007FFE16AA590C | E9 5DF8FFFF          | jmp avformat-58.7FFE16AA516E             |
             * 
             * Select the instruction  that loads the error string's address, and search for all
             * references. You should only find one referance, a conditional jump
             * (00007FFE16AA513C example below).
             * 
             * 00007FFE16AA511D | 89C2                 | mov edx,eax                              |
             * 00007FFE16AA511F | 89C1                 | mov ecx,eax                              |
             * 00007FFE16AA5121 | 83C0 01              | add eax,1                                |
             * 00007FFE16AA5124 | C1EA 03              | shr edx,3                                |
             * 00007FFE16AA5127 | 83E1 07              | and ecx,7                                |
             * 00007FFE16AA512A | 41:8B1414            | mov edx,dword ptr ds:[r12+rdx]           |
             * 00007FFE16AA512E | 0FCA                 | bswap edx                                |
             * 00007FFE16AA5130 | D3E2                 | shl edx,cl                               |
             * 00007FFE16AA5132 | C1EA FF              | shr edx,FF                               |
             * 00007FFE16AA5135 | 39F8                 | cmp eax,edi                              |
             * 00007FFE16AA5137 | 0F47C7               | cmova eax,edi                            |
             * 00007FFE16AA513A | 85D2                 | test edx,edx                             |
             * 00007FFE16AA513C | 0F85 57070000        | jne avformat-58.7FFE16AA5899             |
             * 
             * Edit that jump with six nop instructions and save the patched assembly.
             */
            #endregion

            var tempFileName = tempFileNameObj as string;

            string args = "-audible_key "
                + audible_key
                + " -audible_iv "
                + audible_iv
                + " -i "
                + "\"" + inputFileName + "\""
                + " -c:a copy -vn -sn -dn -y "
                + "\"" + tempFileName + "\"";

            var info = new ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.ffmpegPath,
                Arguments = args
            };

            var result = info.RunHidden();

            // failed to decrypt
            if (result.Error.Contains("aac bitstream error"))
                return -99;

            return result.ExitCode;
        }

        // temp file names for steps 3, 4, 5
        string tempChapsGuid { get; } = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
        string tempChapsPath => Path.Combine(outDir, $"tempChaps_{tempChapsGuid}.mp4");
        string mp4_file => outputFileWithNewExt(".mp4");
        string ff_txt_file => mp4_file + ".ff.txt";

        public bool Step3_Chapterize()
        {
            var str1 = "";
            if (chapters.FirstChapter.StartTime != 0.0)
            {
                str1 = " -ss " + chapters.FirstChapter.StartTime.ToString("0.000", CultureInfo.InvariantCulture) + " -t " + chapters.LastChapter.EndTime.ToString("0.000", CultureInfo.InvariantCulture) + " ";
            }

            var ffmpegTags = tags.GenerateFfmpegTags();
            var ffmpegChapters = chapters.GenerateFfmpegChapters();
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
            var coverPath = Path.Combine(outDir, "cover-" + Guid.NewGuid() + ".jpg");
            FileExt.CreateFile(coverPath, coverBytes);

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
            FileExt.SafeMove(tempChapsPath, outputFileName);

            return true;
        }

        public bool Step6_AddTags()
        {
            tags.AddAppleTags(outputFileName);
            return true;
        }

        public bool End_CreateCue()
        {
            File.WriteAllText(outputFileWithNewExt(".cue"), Cue.CreateContents(Path.GetFileName(outputFileName), chapters));
            return true;
        }

        public bool End_CreateNfo()
        {
            File.WriteAllText(outputFileWithNewExt(".nfo"), NFO.CreateContents(AppName, tags, encodingInfo, chapters));
            return true;
        }
    }
}
