using System;
using System.IO;
using System.Linq;
using System.Text;
using Dinah.Core;

namespace AaxDecrypter
{
    public static class Cue
    {
        public static string CreateContents(string filePath, Chapters chapters)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(GetFileLine(filePath, "MP3"));

            var beginningTimes = chapters.GetBeginningTimes().ToList();
            for (var i = 0; i < beginningTimes.Count; i++)
            {
                var chapter = i + 1;

                var timeSpan = beginningTimes[i];
                var minutes = Math.Floor(timeSpan.TotalMinutes).ToString();
                var seconds = timeSpan.Seconds.ToString("D2");
                var milliseconds = (timeSpan.Milliseconds / 10).ToString("D2");
                var time = minutes + ":" + seconds + ":" + milliseconds;

                stringBuilder.AppendLine($"TRACK {chapter} AUDIO");
                stringBuilder.AppendLine($"  TITLE \"Chapter {chapter:D2}\"");
                stringBuilder.AppendLine($"  INDEX 01 {time}");
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
                if (!line.Trim().StartsWith("FILE") || !line.Contains(" "))
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
