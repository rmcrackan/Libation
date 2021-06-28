using Dinah.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AaxDecrypter
{
    public class ChapterInfo
    {
        private List<Chapter> _chapterList = new List<Chapter>();
        public IEnumerable<Chapter> Chapters => _chapterList.AsEnumerable();
        public int Count => _chapterList.Count;
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
