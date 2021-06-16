using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AaxDecrypter
{
    public abstract class Chapters
    {
        private List<Chapter> _chapterList = new();
        public int Count => _chapterList.Count;
        public Chapter FirstChapter => _chapterList[0];
        public Chapter LastChapter => _chapterList[Count - 1];
        public IEnumerable<Chapter> ChapterList => _chapterList.AsEnumerable();
        public IEnumerable<TimeSpan> GetBeginningTimes() => ChapterList.Select(c => TimeSpan.FromSeconds(c.StartTime));
        protected void AddChapter(Chapter chapter)
        {
            _chapterList.Add(chapter);
        }
        public string GenerateFfmpegChapters()
        {
            var stringBuilder = new StringBuilder();

            foreach (Chapter c in ChapterList)
            {
                stringBuilder.Append("[CHAPTER]\n");
                stringBuilder.Append("TIMEBASE=1/1000\n");
                stringBuilder.Append("START=" + c.StartTime * 1000 + "\n");
                stringBuilder.Append("END=" + c.EndTime * 1000 + "\n");
                stringBuilder.Append("title=" + c.Title + "\n");
            }

            return stringBuilder.ToString();
        }
    }
}
