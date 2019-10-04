using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Dinah.Core.Diagnostics;

namespace AaxDecrypter
{
    public class Chapters
    {
        private List<double> markers { get; }

        public double FirstChapterStart => markers[0];
        public double LastChapterStart => markers[markers.Count - 1];

        public Chapters(string file, double totalTime)
        {
            this.markers = getAAXChapters(file);

            // add end time
            this.markers.Add(totalTime);
        }

        private static List<double> getAAXChapters(string file)
        {
            var info = new ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.ffprobePath,
                Arguments = "-loglevel panic -show_chapters -print_format xml \"" + file + "\""
            };
            var xml = info.RunHidden().Output;

            var xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.LoadXml(xml);
            var chapters = xmlDocument.SelectNodes("/ffprobe/chapters/chapter")
                .Cast<System.Xml.XmlNode>()
                .Select(xmlNode => double.Parse(xmlNode.Attributes["start_time"].Value.Replace(",", "."), CultureInfo.InvariantCulture))
                .ToList();
            return chapters;
        }

        // subtract 1 b/c end time marker is a real entry but isn't a real chapter
        public int Count() => this.markers.Count - 1;

        public string GetCuefromChapters(string fileName)
        {
            var stringBuilder = new StringBuilder();
            if (fileName != "")
            {
                stringBuilder.Append("FILE \"" + fileName + "\" MP4\n");
            }

            for (var i = 0; i < Count(); i++)
            {
                var chapter = i + 1;

                var timeSpan = TimeSpan.FromSeconds(this.markers[i]);
                var minutes = Math.Floor(timeSpan.TotalMinutes).ToString();
                var seconds = timeSpan.Seconds.ToString("D2");
                var milliseconds = (timeSpan.Milliseconds / 10).ToString("D2");
                string str = minutes + ":" + seconds + ":" + milliseconds;

                stringBuilder.Append("TRACK " + chapter + " AUDIO\n");
                stringBuilder.Append("  TITLE \"Chapter " + chapter.ToString("D2") + "\"\n");
                stringBuilder.Append("  INDEX 01 " + str + "\n");
            }

            return stringBuilder.ToString();
        }

        public string GenerateFfmpegChapters()
        {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < Count(); i++)
            {
                var chapter = i + 1;

                var start = this.markers[i] * 1000.0;
                var end = this.markers[i + 1] * 1000.0;
                var chapterName = chapter.ToString("D3");

                stringBuilder.Append("[CHAPTER]\n");
                stringBuilder.Append("TIMEBASE=1/1000\n");
                stringBuilder.Append("START=" + start + "\n");
                stringBuilder.Append("END=" + end + "\n");
                stringBuilder.Append("title=" + chapterName + "\n");
            }

            return stringBuilder.ToString();
        }
    }
}
