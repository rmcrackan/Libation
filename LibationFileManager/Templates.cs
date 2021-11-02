using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibationFileManager
{
    public abstract class Templates
    {
        protected static string[] Valid => Array.Empty<string>();
        public const string ERROR_NULL_IS_INVALID = "Null template is invalid.";
        public const string ERROR_FULL_PATH_IS_INVALID = @"No colons or full paths allowed. Eg: should not start with C:\";
        public const string ERROR_INVALID_FILE_NAME_CHAR = @"Only file name friendly characters allowed. Eg: no colons or slashes";

        public const string WARNING_EMPTY = "Template is empty.";
        public const string WARNING_WHITE_SPACE = "Template is white space.";
        public const string WARNING_NO_TAGS = "Should use tags. Eg: <title>";
        public const string WARNING_HAS_CHAPTER_TAGS = "Chapter tags should only be used in the template used for naming files which are split by chapter. Eg: <ch title>";
        public const string WARNING_NO_CHAPTER_NUMBER_TAG = "Should include chapter number tag in template used for naming files which are split by chapter. Ie: <ch#> or <ch# 0>";

        public static Templates Folder { get; } = new FolderTemplate();
        public static Templates File { get; } = new FileTemplate();
        public static Templates ChapterFile { get; } = new ChapterFileTemplate();

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string DefaultTemplate { get; }
        protected abstract bool IsChapterized { get; }

        internal string GetValid(string configValue)
        {
            var value = configValue?.Trim();
            return IsValid(value) ? value : DefaultTemplate;
        }

        public static string Sanitize(string template)
        {
            var value = template ?? "";

            // don't use alt slash
            value = value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            // don't allow double slashes
            var sing = $"{Path.DirectorySeparatorChar}";
            var dbl = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}";
            while (value.Contains(dbl))
                value = value.Replace(dbl, sing);

            // trim. don't start or end with slash
            while (true)
            {
                var start = value.Length;
                value = value
                    .Trim()
                    .Trim(Path.DirectorySeparatorChar);
                var end = value.Length;
                if (start == end)
                    break;
            }

            return value;
        }

        public abstract IEnumerable<string> GetErrors(string template);
        public bool IsValid(string template) => !GetErrors(template).Any();

        public abstract IEnumerable<string> GetWarnings(string template);
        public bool HasWarnings(string template) => GetWarnings(template).Any();

        public IEnumerable<TemplateTags> GetTemplateTags()
            => TemplateTags.GetAll()
            // yeah, this line is a little funky but it works when you think through it. also: trust the unit tests
            .Where(t => IsChapterized || !t.IsChapterOnly);

        public int TagCount(string template)
            => GetTemplateTags()
            // for <id><id> == 1, use:
            //   .Count(t => template.Contains($"<{t.TagName}>"))
            // .Sum() impl: <id><id> == 2
            .Sum(t => template.Split($"<{t.TagName}>").Length - 1);

        public static bool ContainsChapterOnlyTags(string template)
            => TemplateTags.GetAll()
            .Where(t => t.IsChapterOnly)
            .Any(t => ContainsTag(template, t.TagName));

        public static bool ContainsTag(string template, string tag) => template.Contains($"<{tag}>");

        protected static string[] GetFileErrors(string template)
        {
            // File name only; not path. all other path chars are valid enough to pass this check and will be handled on final save.

            // null is invalid. whitespace is valid but not recommended
            if (template is null)
                return new[] { ERROR_NULL_IS_INVALID };

            if (template.Contains(':')
                || template.Contains(Path.DirectorySeparatorChar)
                || template.Contains(Path.AltDirectorySeparatorChar)
                )
                return new[] { ERROR_INVALID_FILE_NAME_CHAR };

            return Valid;
        }

        protected IEnumerable<string> GetStandardWarnings(string template)
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

            if (!IsChapterized && ContainsChapterOnlyTags(template))
                warnings.Add(WARNING_HAS_CHAPTER_TAGS);

            return warnings;
        }

        private class FolderTemplate : Templates
        {
			public override string Name => "Folder Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.FolderTemplate));
			public override string DefaultTemplate { get; } = "<title short> [<id>]";
            protected override bool IsChapterized { get; } = false;

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
            
            public override IEnumerable<string> GetWarnings(string template) => GetStandardWarnings(template);
        }

        private class FileTemplate : Templates
        {
            public override string Name => "File Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.FileTemplate));
            public override string DefaultTemplate { get; } = "<title> [<id>]";
            protected override bool IsChapterized { get; } = false;

            public override IEnumerable<string> GetErrors(string template) => GetFileErrors(template);

            public override IEnumerable<string> GetWarnings(string template) => GetStandardWarnings(template);
        }

        private class ChapterFileTemplate : Templates
        {
            public override string Name => "Chapter File Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.ChapterFileTemplate));
            public override string DefaultTemplate { get; } = "<title> [<id>] - <ch# 0> - <ch title>";
            protected override bool IsChapterized { get; } = true;

            public override IEnumerable<string> GetErrors(string template) => GetFileErrors(template);

            public override IEnumerable<string> GetWarnings(string template)
            {
                var warnings = GetStandardWarnings(template).ToList();
                if (template is null)
                    return warnings;

                // recommended to incl. <ch#> or <ch# 0>
                if (!ContainsTag(template, TemplateTags.ChNumber.TagName) && !ContainsTag(template, TemplateTags.ChNumber0.TagName))
                    warnings.Add(WARNING_NO_CHAPTER_NUMBER_TAG);

                return warnings;
			}
        }
    }
}
