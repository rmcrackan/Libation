using Dinah.Core;
using Mpeg4Lib;
using System.IO;
using System.Text;

namespace AaxDecrypter
{
    public static class Cue
    {
        public static string CreateContents(string filePath, ChapterInfo chapters)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(GetFileLine(filePath, "MP3"));

            var startOffset = chapters.StartOffset;

            var trackCount = 1;
            foreach (var c in chapters.Chapters)
            {
                var startTime = c.StartOffset - startOffset;

                stringBuilder.AppendLine($"TRACK {trackCount++} AUDIO");
                stringBuilder.AppendLine($"  TITLE \"{c.Title}\"");
                stringBuilder.AppendLine($"  INDEX 01 {(int)startTime.TotalMinutes}:{startTime:ss}:{(int)(startTime.Milliseconds * 75d / 1000):D2}");
            }

            return stringBuilder.ToString();
        }

        public static void UpdateFileName(FileInfo cueFileInfo, string audioFilePath)
            => UpdateFileName(cueFileInfo.FullName, audioFilePath);

        public static void UpdateFileName(string cueFilePath, FileInfo audioFileInfo)
            => UpdateFileName(cueFilePath, audioFileInfo.FullName);

        public static void UpdateFileName(FileInfo cueFileInfo, FileInfo audioFileInfo)
            => UpdateFileName(cueFileInfo.FullName, audioFileInfo.FullName);

        public static void UpdateFileName(string cueFilePath, string audioFilePath)
        {
            var cueContents = File.ReadAllLines(cueFilePath);

            for (var i = 0; i < cueContents.Length; i++)
            {
                var line = cueContents[i];
                if (!line.Trim().StartsWith("FILE") || !line.Contains(' '))
                    continue;

                var fileTypeBegins = line.LastIndexOf(" ") + 1;
                cueContents[i] = GetFileLine(audioFilePath, line[fileTypeBegins..]);
                break;
            }

            File.WriteAllLines(cueFilePath, cueContents);
        }

        private static string GetFileLine(string filePath, string audioType) => $"FILE {Path.GetFileName(filePath).SurroundWithQuotes()} {audioType}";
    }
}
