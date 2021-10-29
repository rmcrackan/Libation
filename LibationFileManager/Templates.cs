using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationFileManager
{
    public abstract class Templates
    {
        protected static string[] Valid => Array.Empty<string>();
        public const string ERROR_NULL_IS_INVALID = "Null template is invalid.";
        public const string ERROR_FULL_PATH_IS_INVALID = @"No full paths allowed. Eg: should not start with C:\";
        public const string ERROR_INVALID_FILE_NAME_CHAR = @"Only file name friendly characters allowed. Eg: no colons or slashes";

        public const string WARNING_EMPTY = "Template is empty.";
        public const string WARNING_WHITE_SPACE = "Template is white space.";
        public const string WARNING_NO_TAGS = "Should use tags. Eg: <title>";
        public const string WARNING_HAS_CHAPTER_TAGS = "Chapter tags should only be used in the template used for naming files which are split by chapter. Eg: <ch title>";
        public const string WARNING_NO_CHAPTER_NUMBER_TAG = "Should include chapter number tag in template used for naming files which are split by chapter. Ie: <ch#> or <ch# 0>";
        // actual possible to se?
        public const string WARNING_NO_CHAPTER_TAGS = "Should include chapter tags in template used for naming files which are split by chapter. Eg: <ch title>";

        public static Templates Folder { get; } = new FolderTemplate();
        public static Templates File { get; } = new FileTemplate();
        public static Templates ChapterFile { get; } = new ChapterFileTemplate();

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string DefaultTemplate { get; }

        public abstract IEnumerable<string> GetErrors(string template);
        public bool IsValid(string template) => !GetErrors(template).Any();

        public abstract IEnumerable<string> GetWarnings(string template);
        public bool HasWarnings(string template) => GetWarnings(template).Any();

        public abstract int TagCount(string template);

        public static bool ContainsChapterOnlyTags(string template)
            => TemplateTags.GetAll()
            .Where(t => t.IsChapterOnly)
            .Any(t => ContainsTag(template, t.TagName));

        public static bool ContainsTag(string template, string tag) => template.Contains($"<{tag}>");

        protected static string[] getFileErrors(string template)
        {
            // File name only; not path. all other path chars are valid enough to pass this check and will be handled on final save.

            // null is invalid. whitespace is valid but not recommended
            if (template is null)
                return new[] { ERROR_NULL_IS_INVALID };

            if (template.Contains(':')
                || template.Contains(System.IO.Path.DirectorySeparatorChar)
                || template.Contains(System.IO.Path.AltDirectorySeparatorChar)
                )
                return new[] { ERROR_INVALID_FILE_NAME_CHAR };

            return Valid;
        }

        protected IEnumerable<string> getWarnings(string template, bool isChapter)
        {
            var warnings = GetErrors(template).ToList();
            if (template is null)
                return warnings;

            if (string.IsNullOrEmpty(template))
                warnings.Add(WARNING_EMPTY);
            else if (string.IsNullOrWhiteSpace(template))
                warnings.Add(WARNING_WHITE_SPACE);

            if (TagCount(template) == 0)
                warnings.Add(WARNING_NO_TAGS);

            var containsChapterOnlyTags = ContainsChapterOnlyTags(template);
            if (isChapter && !containsChapterOnlyTags)
                warnings.Add(WARNING_NO_CHAPTER_TAGS);
            if (!isChapter && containsChapterOnlyTags)
                warnings.Add(WARNING_HAS_CHAPTER_TAGS);

            return warnings;
        }

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

            public override IEnumerable<string> GetErrors(string template)
            {
                // null is invalid. whitespace is valid but not recommended
                if (template is null)
                    return new[] { ERROR_NULL_IS_INVALID };

                // must be relative. no colons. all other path chars are valid enough to pass this check and will be handled on final save.
                if (template.Contains(':'))
                    return new[] { ERROR_FULL_PATH_IS_INVALID };

                return Valid;
            }
            
            public override IEnumerable<string> GetWarnings(string template) => getWarnings(template, false);

            public override int TagCount(string template) => tagCount(template, t => !t.IsChapterOnly);
        }

        private class FileTemplate : Templates
        {
            public override string Name => "File Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.FileTemplate));
            public override string DefaultTemplate { get; } = "<title> [<id>]";

            public override IEnumerable<string> GetErrors(string template) => getFileErrors(template);

            public override IEnumerable<string> GetWarnings(string template) => getWarnings(template, false);

            public override int TagCount(string template) => tagCount(template, t => !t.IsChapterOnly);
        }

        private class ChapterFileTemplate : Templates
        {
            public override string Name => "Chapter File Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.ChapterFileTemplate));
            public override string DefaultTemplate { get; } = "<title> [<id>] - <ch# 0> - <ch title>";
            
            public override IEnumerable<string> GetErrors(string template) => getFileErrors(template);

            public override IEnumerable<string> GetWarnings(string template)
            {
                var warnings = getWarnings(template, true).ToList();
                if (template is null)
                    return warnings;

                // recommended to incl. <ch#> or <ch# 0>
                if (!ContainsTag(template, TemplateTags.ChNumber.TagName) && !ContainsTag(template, TemplateTags.ChNumber0.TagName))
                {
                    warnings.Remove(WARNING_NO_CHAPTER_TAGS);
                    warnings.Add(WARNING_NO_CHAPTER_NUMBER_TAG);
                }

                return warnings;
			}

            public override int TagCount(string template) => tagCount(template, t => true);
        }
    }
}
