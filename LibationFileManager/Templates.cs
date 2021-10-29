using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationFileManager
{
    public abstract class Templates
    {
        public static Templates Folder { get; } = new FolderTemplate();
        public static Templates File { get; } = new FileTemplate();
        public static Templates ChapterFile { get; } = new ChapterFileTemplate();

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string DefaultTemplate { get; }

        public abstract bool IsValid(string template);
        public abstract bool IsRecommended(string template);
        public abstract int TagCount(string template);

        public static bool ContainsChapterOnlyTags(string template)
            => TemplateTags.GetAll()
            .Where(t => t.IsChapterOnly)
            .Any(t => ContainsTag(template, t.TagName));

        public static bool ContainsTag(string template, string tag) => template.Contains($"<{tag}>");

        protected static bool fileIsValid(string template)
            // File name only; not path. all other path chars are valid enough to pass this check and will be handled on final save.
            // null is invalid. whitespace is valid but not recommended
            => template is not null
            && !template.Contains(':')
            && !template.Contains(System.IO.Path.DirectorySeparatorChar)
            && !template.Contains(System.IO.Path.AltDirectorySeparatorChar);

        protected bool isRecommended(string template, bool isChapter)
            => IsValid(template)
            && !string.IsNullOrWhiteSpace(template)
            && TagCount(template) > 0
            && ContainsChapterOnlyTags(template) == isChapter;

        protected static int tagCount(string template, Func<TemplateTags, bool> func)
            => TemplateTags.GetAll()
            .Where(func)
            // for <id><id> == 1, use:
            //   .Count(t => template.Contains($"<{t.TagName}>"))
            // .Sum() impl: <id><id> == 2
            .Sum(t => template.Split($"<{t.TagName}>").Length - 1);

        private class FolderTemplate : Templates
        {
			public override string Name => "Folder Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.FolderTemplate));
			public override string DefaultTemplate { get; } = "<title short> [<id>]";

			public override bool IsValid(string template)
                // must be relative. no colons. all other path chars are valid enough to pass this check and will be handled on final save.
                // null is invalid. whitespace is valid but not recommended
                => template is not null
                && !template.Contains(':');

            public override bool IsRecommended(string template) => isRecommended(template, false);

            public override int TagCount(string template) => tagCount(template, t => !t.IsChapterOnly);
        }

        private class FileTemplate : Templates
        {
            public override string Name => "File Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.FileTemplate));
            public override string DefaultTemplate { get; } = "<title> [<id>]";

            public override bool IsValid(string template) => fileIsValid(template);

            public override bool IsRecommended(string template) => isRecommended(template, false);

            public override int TagCount(string template) => tagCount(template, t => !t.IsChapterOnly);
        }

        private class ChapterFileTemplate : Templates
        {
            public override string Name => "Chapter File Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.ChapterFileTemplate));
            public override string DefaultTemplate { get; } = "<title> [<id>] - <ch# 0> - <ch title>";

            public override bool IsValid(string template) => fileIsValid(template);

            public override bool IsRecommended(string template)
                => isRecommended(template, true)
                // recommended to incl. <ch#> or <ch# 0>
                && (ContainsTag(template, TemplateTags.ChNumber.TagName) || ContainsTag(template, TemplateTags.ChNumber0.TagName));

            public override int TagCount(string template) => tagCount(template, t => true);
        }
    }
}
