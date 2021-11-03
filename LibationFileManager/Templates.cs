using Dinah.Core;
using FileManager;
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

        public static FolderTemplate Folder { get; } = new FolderTemplate();
        public static FileTemplate File { get; } = new FileTemplate();
        public static ChapterFileTemplate ChapterFile { get; } = new ChapterFileTemplate();

        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string DefaultTemplate { get; }
        protected abstract bool IsChapterized { get; }

        protected Templates() { }

		#region validation
		internal string GetValid(string configValue)
        {
            var value = configValue?.Trim();
            return IsValid(value) ? value : DefaultTemplate;
        }

        public abstract IEnumerable<string> GetErrors(string template);
        public bool IsValid(string template) => !GetErrors(template).Any();

        public abstract IEnumerable<string> GetWarnings(string template);
        public bool HasWarnings(string template) => GetWarnings(template).Any();

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

        internal int TagCount(string template)
            => GetTemplateTags()
            // for <id><id> == 1, use:
            //   .Count(t => template.Contains($"<{t.TagName}>"))
            // .Sum() impl: <id><id> == 2
            .Sum(t => template.Split($"<{t.TagName}>").Length - 1);

        internal static bool ContainsChapterOnlyTags(string template)
            => TemplateTags.GetAll()
            .Where(t => t.IsChapterOnly)
            .Any(t => ContainsTag(template, t.TagName));

        internal static bool ContainsTag(string template, string tag) => template.Contains($"<{tag}>");
        #endregion

        #region to file name
        /// <summary>
        /// EditTemplateDialog: Get template generated filename for portion of path
        /// </summary>
        public string GetPortionFilename(LibraryBookDto libraryBookDto, string template)
            => string.IsNullOrWhiteSpace(template)
            ? ""
            : getFileNamingTemplate(libraryBookDto, template, null, null)
            .GetFilePath();

        internal static FileNamingTemplate getFileNamingTemplate(LibraryBookDto libraryBookDto, string template, string dirFullPath, string extension)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(template, nameof(template));
            ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));

            dirFullPath = dirFullPath?.Trim() ?? "";
            var t = template + FileUtility.GetStandardizedExtension(extension);
            var fullfilename = dirFullPath == "" ? t : Path.Combine(dirFullPath, t);

            var fileNamingTemplate = new FileNamingTemplate(fullfilename) { IllegalCharacterReplacements = "_" };

            var title = libraryBookDto.Title ?? "";
            var titleShort = title.IndexOf(':') < 1 ? title : title.Substring(0, title.IndexOf(':'));

            fileNamingTemplate.AddParameterReplacement(TemplateTags.Id, libraryBookDto.AudibleProductId);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Title, title);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.TitleShort, titleShort);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Author, libraryBookDto.AuthorNames);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.FirstAuthor, libraryBookDto.FirstAuthor);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Narrator, libraryBookDto.NarratorNames);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.FirstNarrator, libraryBookDto.FirstNarrator);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Series, libraryBookDto.SeriesName);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.SeriesNumber, libraryBookDto.SeriesNumber);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Account, libraryBookDto.Account);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.Locale, libraryBookDto.Locale);

            return fileNamingTemplate;
        }
        #endregion

        public IEnumerable<TemplateTags> GetTemplateTags()
            => TemplateTags.GetAll()
            // yeah, this line is a little funky but it works when you think through it. also: trust the unit tests
            .Where(t => IsChapterized || !t.IsChapterOnly);

        public string Sanitize(string template)
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

        public class FolderTemplate : Templates
        {
			public override string Name => "Folder Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.FolderTemplate));
			public override string DefaultTemplate { get; } = "<title short> [<id>]";
            protected override bool IsChapterized { get; } = false;

            internal FolderTemplate() : base() { }

            #region validation
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
            #endregion

            #region to file name
            /// <summary>USES LIVE CONFIGURATION VALUES</summary>
            public string GetFilename(LibraryBookDto libraryBookDto)
                => getFileNamingTemplate(libraryBookDto, Configuration.Instance.FolderTemplate, AudibleFileStorage.BooksDirectory, null)
                .GetFilePath();
            #endregion
        }

        public class FileTemplate : Templates
        {
            public override string Name => "File Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.FileTemplate));
            public override string DefaultTemplate { get; } = "<title> [<id>]";
            protected override bool IsChapterized { get; } = false;

            internal FileTemplate() : base() { }

            #region validation
            public override IEnumerable<string> GetErrors(string template) => GetFileErrors(template);

            public override IEnumerable<string> GetWarnings(string template) => GetStandardWarnings(template);
            #endregion

            #region to file name
            /// <summary>USES LIVE CONFIGURATION VALUES</summary>
            public string GetFilename(LibraryBookDto libraryBookDto, string dirFullPath, string extension)
                => getFileNamingTemplate(libraryBookDto, Configuration.Instance.FileTemplate, dirFullPath, extension)
                .GetFilePath();
            #endregion
        }

        public class ChapterFileTemplate : Templates
        {
            public override string Name => "Chapter File Template";
            public override string Description => Configuration.GetDescription(nameof(Configuration.ChapterFileTemplate));
            public override string DefaultTemplate { get; } = "<title> [<id>] - <ch# 0> - <ch title>";
            protected override bool IsChapterized { get; } = true;

            internal ChapterFileTemplate() : base() { }

            #region validation
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
            #endregion

            #region to file name
            /// <summary>USES LIVE CONFIGURATION VALUES</summary>
            public string GetFilename(LibraryBookDto libraryBookDto, AaxDecrypter.MultiConvertFileProperties props)
                => GetPortionFilename(libraryBookDto, Configuration.Instance.ChapterFileTemplate, props, AudibleFileStorage.DecryptInProgressDirectory);

            public string GetPortionFilename(LibraryBookDto libraryBookDto, string template, AaxDecrypter.MultiConvertFileProperties props, string fullDirPath)
            {
                var fileNamingTemplate = getFileNamingTemplate(libraryBookDto, template, fullDirPath, Path.GetExtension(props.OutputFileName));

                fileNamingTemplate.AddParameterReplacement(TemplateTags.ChCount, props.PartsTotal);
                fileNamingTemplate.AddParameterReplacement(TemplateTags.ChNumber, props.PartsPosition);
                fileNamingTemplate.AddParameterReplacement(TemplateTags.ChNumber0, FileUtility.GetSequenceFormatted(props.PartsPosition, props.PartsTotal));
                fileNamingTemplate.AddParameterReplacement(TemplateTags.ChTitle, props.Title ?? "");

                return fileNamingTemplate.GetFilePath();
            }
            #endregion
        }
    }
}
