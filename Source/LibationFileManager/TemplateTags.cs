using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;

namespace LibationFileManager
{
    public sealed class TemplateTags : Enumeration<TemplateTags>
    {
        public string TagName => DisplayName;
        public string Description { get; }
        public bool IsChapterOnly { get; }

        private static int value = 0;
        private TemplateTags(string tagName, string description, bool isChapterOnly = false) : base(value++, tagName)
        {
            Description = description;
            IsChapterOnly = isChapterOnly;
        }

        // putting these first is the incredibly lazy way to make them show up first in the EditTemplateDialog
        public static TemplateTags ChCount { get; } = new TemplateTags("ch count", "Number of chapters", true);
        public static TemplateTags ChTitle { get; } = new TemplateTags("ch title", "Chapter title", true);
        public static TemplateTags ChNumber { get; } = new TemplateTags("ch#", "Chapter #", true);
        public static TemplateTags ChNumber0 { get; } = new TemplateTags("ch# 0", "Chapter # with leading zeros", true);

        public static TemplateTags Id { get; } = new TemplateTags("id", "Audible ID");
        public static TemplateTags Title { get; } = new TemplateTags("title", "Full title");
        public static TemplateTags TitleShort { get; } = new TemplateTags("title short", "Title. Stop at first colon");
        public static TemplateTags Author { get; } = new TemplateTags("author", "Author(s)");
        public static TemplateTags FirstAuthor { get; } = new TemplateTags("first author", "First author");
        public static TemplateTags Narrator { get; } = new TemplateTags("narrator", "Narrator(s)");
        public static TemplateTags FirstNarrator { get; } = new TemplateTags("first narrator", "First narrator");
        public static TemplateTags Series { get; } = new TemplateTags("series", "Name of series");
        // can't also have a leading zeros version. Too many weird edge cases. Eg: "1-4"
        public static TemplateTags SeriesNumber { get; } = new TemplateTags("series#", "Number order in series");
        public static TemplateTags Bitrate { get; } = new TemplateTags("bitrate", "File's orig. bitrate");
        public static TemplateTags SampleRate { get; } = new TemplateTags("samplerate", "File's orig. sample rate");
        public static TemplateTags Channels { get; } = new TemplateTags("channels", "Number of audio channels");
        public static TemplateTags Account { get; } = new TemplateTags("account", "Audible account of this book");
        public static TemplateTags Locale { get; } = new TemplateTags("locale", "Region/country");
        public static TemplateTags YearPublished { get; } = new TemplateTags("year", "Year published");

        // Special case. Isn't mapped to a replacement in Templates.cs
        // Included here for display by EditTemplateDialog
        public static TemplateTags IfSeries { get; } = new TemplateTags("if series->...<-if series", "Only include if part of a series");
    }
}
