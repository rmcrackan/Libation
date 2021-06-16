using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using AaxDecrypter;
using AudibleApiDTOs;
using Dinah.Core.Diagnostics;


namespace FileLiberator
{
    public class DownloadedChapters : Chapters
    {
        public DownloadedChapters(ChapterInfo chapterInfo)
        {
            AddChapters(chapterInfo.Chapters
                .Select(c => new AaxDecrypter.Chapter(c.StartOffsetMs / 1000d, (c.StartOffsetMs + c.LengthMs) / 1000d, c.Title)));
        }
    }
}
