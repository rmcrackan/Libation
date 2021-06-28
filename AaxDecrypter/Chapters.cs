using Dinah.Core;
using Dinah.Core.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AaxDecrypter
{
    public class ChapterInfo
    {
        private List<Chapter> _chapterList = new List<Chapter>();
        public IEnumerable<Chapter> Chapters => _chapterList.AsEnumerable();
        public int Count => _chapterList.Count;

        public ChapterInfo() { }
        public ChapterInfo(string audiobookFile) 
        {
            var info = new ProcessStartInfo
            {
                FileName = DecryptSupportLibraries.ffprobePath,
                Arguments = "-loglevel panic -show_chapters -print_format xml \"" + audiobookFile + "\""
            };
            var xml = info.RunHidden().Output;

            var xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.LoadXml(xml);
            var chaptersXml = xmlDocument.SelectNodes("/ffprobe/chapters/chapter")
                .Cast<System.Xml.XmlNode>()
                .Where(n => n.Name == "chapter");

            foreach (var cnode in chaptersXml)
            {
                double startTime = double.Parse(cnode.Attributes["start_time"].Value.Replace(",", "."), CultureInfo.InvariantCulture);
                double endTime = double.Parse(cnode.Attributes["end_time"].Value.Replace(",", "."), CultureInfo.InvariantCulture);

                string chapterTitle = cnode.ChildNodes
                    .Cast<System.Xml.XmlNode>()
                    .Where(childnode => childnode.Attributes["key"].Value == "title")
                    .Select(childnode => childnode.Attributes["value"].Value)
                    .FirstOrDefault();

                AddChapter(new Chapter(chapterTitle, (long)(startTime * 1000), (long)((endTime - startTime) * 1000)));
            }
        }
        public void AddChapter(Chapter chapter)
        {
            ArgumentValidator.EnsureNotNull(chapter, nameof(chapter));
            _chapterList.Add(chapter);
        }
        public string ToFFMeta(bool includeFFMetaHeader)
        {
            var ffmetaChapters = new StringBuilder();

            if (includeFFMetaHeader)
                ffmetaChapters.AppendLine(";FFMETADATA1\n");

            foreach (var c in Chapters)
            {
                ffmetaChapters.AppendLine(c.ToFFMeta());
            }
            return ffmetaChapters.ToString();
        }
    }
    public class Chapter
    {
        public string Title { get; }
        public long StartOffsetMs { get; }
        public long EndOffsetMs { get; }
        public Chapter(string title, long startOffsetMs, long lengthMs)
        {
            ArgumentValidator.EnsureNotNullOrEmpty(title, nameof(title));
            ArgumentValidator.EnsureGreaterThan(startOffsetMs, nameof(startOffsetMs), -1);
            ArgumentValidator.EnsureGreaterThan(lengthMs, nameof(lengthMs), 0);

            Title = title;
            StartOffsetMs = startOffsetMs;
            EndOffsetMs = StartOffsetMs + lengthMs;
        }

        public string ToFFMeta()
        {
            return "[CHAPTER]\n" +
                "TIMEBASE=1/1000\n" +
                "START=" + StartOffsetMs + "\n" +
                "END=" + EndOffsetMs + "\n" +
                "title=" + Title;
        }
    }
}
