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
            markers = getAAXChapters(file);

            // add end time
            markers.Add(totalTime);
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

        // subtract 1 b/c end time marker is a real entry but isn't a real chapter. ie: fencepost
        public int Count => markers.Count - 1;

        public IEnumerable<TimeSpan> GetBeginningTimes()
        {
            for (var i = 0; i < Count; i++)
                yield return TimeSpan.FromSeconds(markers[i]);
        }

        public string GenerateFfmpegChapters()
        {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < Count; i++)
            {
                var chapter = i + 1;

                var start = markers[i] * 1000.0;
                var end = markers[i + 1] * 1000.0;
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
