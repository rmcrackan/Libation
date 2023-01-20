using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dinah.Core;
using Dinah.Core.Collections.Generic;
using FileManager;

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
		public static ChapterTitleTemplate ChapterTitle { get; } = new ChapterTitleTemplate();

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

			if (ReplacementCharacters.ContainsInvalidFilenameChar(template.Replace("<","").Replace(">","")))
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
		public string GetPortionFilename(LibraryBookDto libraryBookDto, string template, string fileExtension)
			=> string.IsNullOrWhiteSpace(template)
			? ""
			: getFileNamingTemplate(libraryBookDto, template, null, fileExtension, Configuration.Instance.ReplacementCharacters)
			.GetFilePath(fileExtension).PathWithoutPrefix;

		public const string DEFAULT_DATE_FORMAT = "yyyy-MM-dd";
		private static Regex fileDateTagRegex { get; } = new Regex(@"<file\s*?date\s*?(?:\[([^\[\]]*?)\]){0,1}\s*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex dateAddedTagRegex { get; } = new Regex(@"<date\s*?added\s*?(?:\[([^\[\]]*?)\]){0,1}\s*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex datePublishedTagRegex { get; } = new Regex(@"<pub\s*?date\s*?(?:\[([^\[\]]*?)\]){0,1}\s*?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex ifSeriesRegex { get; } = new Regex("<if series->(.*?)<-if series>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		internal static FileNamingTemplate getFileNamingTemplate(LibraryBookDto libraryBookDto, string template, string dirFullPath, string extension, ReplacementCharacters replacements)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(template, nameof(template));
			ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));

			replacements ??= Configuration.Instance.ReplacementCharacters;
			dirFullPath = dirFullPath?.Trim() ?? "";

			// for non-series, remove <if series-> and <-if series> tags and everything in between
			// for series, remove <if series-> and <-if series> tags, what's in between will remain
			template = ifSeriesRegex.Replace(
				template,
				string.IsNullOrWhiteSpace(libraryBookDto.SeriesName) ? "" : "$1");

			//Get date replacement parameters. Sanitizes the format text and replaces
			//the template with the sanitized text before creating FileNamingTemplate
			var fileDateParams = getSanitizeDateReplacementParameters(fileDateTagRegex, ref template, replacements, libraryBookDto.FileDate);
			var dateAddedParams = getSanitizeDateReplacementParameters(dateAddedTagRegex, ref template, replacements, libraryBookDto.DateAdded);
			var pubDateParams = getSanitizeDateReplacementParameters(datePublishedTagRegex, ref template, replacements, libraryBookDto.DatePublished);

			var t = template + FileUtility.GetStandardizedExtension(extension);
			var fullfilename = dirFullPath == "" ? t : Path.Combine(dirFullPath, t);

			var fileNamingTemplate = new FileNamingTemplate(fullfilename, replacements);

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
			fileNamingTemplate.AddParameterReplacement(TemplateTags.Bitrate, libraryBookDto.BitRate);
			fileNamingTemplate.AddParameterReplacement(TemplateTags.SampleRate, libraryBookDto.SampleRate);
			fileNamingTemplate.AddParameterReplacement(TemplateTags.Channels, libraryBookDto.Channels);
			fileNamingTemplate.AddParameterReplacement(TemplateTags.Account, libraryBookDto.Account);
			fileNamingTemplate.AddParameterReplacement(TemplateTags.Locale, libraryBookDto.Locale);
            fileNamingTemplate.AddParameterReplacement(TemplateTags.YearPublished, libraryBookDto.YearPublished?.ToString() ?? "1900");

			//Add the sanitized replacement parameters
			foreach (var param in fileDateParams)
				fileNamingTemplate.ParameterReplacements.AddIfNotContains(param);
			foreach (var param in dateAddedParams)
				fileNamingTemplate.ParameterReplacements.AddIfNotContains(param);
			foreach (var param in pubDateParams)
				fileNamingTemplate.ParameterReplacements.AddIfNotContains(param);

			return fileNamingTemplate;
		}
		#endregion

		#region DateTime Tags

		/// <param name="template">the file naming template. Any found date tags will be sanitized,
		/// and the template's original date tag will be replaced with the sanitized tag.</param>
		/// <returns>A list of parameter replacement key-value pairs</returns>
		private static List<KeyValuePair<string, object>> getSanitizeDateReplacementParameters(Regex datePattern, ref string template, ReplacementCharacters replacements, DateTime? dateTime)
		{
			List<KeyValuePair<string, object>> dateParams = new();

			foreach (Match dateTag in datePattern.Matches(template))
			{
				var sanitizedTag = sanitizeDateParameterTag(dateTag, replacements, out var sanitizedFormatter);
				if (tryFormatDateTime(dateTime, sanitizedFormatter, replacements, out var formattedDateString))
				{
					dateParams.Add(new(sanitizedTag, formattedDateString));
					template = template.Replace(dateTag.Value, sanitizedTag);
				}
			}
			return dateParams;
		}

		/// <returns>a date parameter replacement tag with the format string sanitized</returns>
		private static string sanitizeDateParameterTag(Match dateTag, ReplacementCharacters replacements, out string sanitizedFormatter)
		{
			if (dateTag.Groups.Count != 2 || string.IsNullOrWhiteSpace(dateTag.Groups[1].Value))
			{
				sanitizedFormatter = DEFAULT_DATE_FORMAT;
				return dateTag.Value;
			}

			var formatter = dateTag.Groups[1].Value;

			sanitizedFormatter = replacements.ReplaceFilenameChars(formatter).Trim();

			return dateTag.Value.Replace(formatter, sanitizedFormatter);
		}

		private static bool tryFormatDateTime(DateTime? dateTime, string sanitizedFormatter, ReplacementCharacters replacements, out string formattedDateString)
		{
			if (!dateTime.HasValue)
			{
				formattedDateString = string.Empty;
				return true;
			}

			try
			{
				formattedDateString = replacements.ReplaceFilenameChars(dateTime.Value.ToString(sanitizedFormatter)).Trim();
				return true;
			}
			catch
			{
				formattedDateString = null;
				return false;
			}
		}
		#endregion

		public virtual IEnumerable<TemplateTags> GetTemplateTags()
			=> TemplateTags.GetAll()
			// yeah, this line is a little funky but it works when you think through it. also: trust the unit tests
			.Where(t => IsChapterized || !t.IsChapterOnly);

		public string Sanitize(string template, ReplacementCharacters replacements)
		{
			var value = template ?? "";

			// Replace invalid filename characters in the DateTime format provider so we don't trip any alarms.
			// Illegal filename characters in the formatter are allowed because they will be replaced by
			// getFileNamingTemplate()
			value = fileDateTagRegex.Replace(value, m => sanitizeDateParameterTag(m, replacements, out _));
			value = dateAddedTagRegex.Replace(value, m => sanitizeDateParameterTag(m, replacements, out _));
			value = datePublishedTagRegex.Replace(value, m => sanitizeDateParameterTag(m, replacements, out _));

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

				// must be relative. no colons. all other path chars are valid enough to pass this check and will be handled on final save.
				if (ReplacementCharacters.ContainsInvalidPathChar(template.Replace("<", "").Replace(">", "")))
					return new[] { ERROR_INVALID_FILE_NAME_CHAR };

				return Valid;
			}
			
			public override IEnumerable<string> GetWarnings(string template) => GetStandardWarnings(template);
			#endregion

			#region to file name
			/// <summary>USES LIVE CONFIGURATION VALUES</summary>
			public string GetFilename(LibraryBookDto libraryBookDto, string baseDir = null)
				=> getFileNamingTemplate(libraryBookDto, Configuration.Instance.FolderTemplate, baseDir ?? AudibleFileStorage.BooksDirectory, null, Configuration.Instance.ReplacementCharacters)
				.GetFilePath(string.Empty);
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
			public string GetFilename(LibraryBookDto libraryBookDto, string dirFullPath, string extension, bool returnFirstExisting = false)
				=> getFileNamingTemplate(libraryBookDto, Configuration.Instance.FileTemplate, dirFullPath, extension, Configuration.Instance.ReplacementCharacters)
				.GetFilePath(extension, returnFirstExisting);
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

			public string GetPortionFilename(LibraryBookDto libraryBookDto, string template,  AaxDecrypter.MultiConvertFileProperties props, string fullDirPath, ReplacementCharacters replacements = null)
			{
				if (string.IsNullOrWhiteSpace(template)) return string.Empty;

				replacements ??= Configuration.Instance.ReplacementCharacters;
				var fileExtension = Path.GetExtension(props.OutputFileName);
				var fileNamingTemplate = getFileNamingTemplate(libraryBookDto, template, fullDirPath, fileExtension, replacements);

				fileNamingTemplate.AddParameterReplacement(TemplateTags.ChCount, props.PartsTotal);
				fileNamingTemplate.AddParameterReplacement(TemplateTags.ChNumber, props.PartsPosition);
				fileNamingTemplate.AddParameterReplacement(TemplateTags.ChNumber0, FileUtility.GetSequenceFormatted(props.PartsPosition, props.PartsTotal));
				fileNamingTemplate.AddParameterReplacement(TemplateTags.ChTitle, props.Title ?? "");

				foreach (Match dateTag in fileDateTagRegex.Matches(fileNamingTemplate.Template))
				{
					var sanitizedTag = sanitizeDateParameterTag(dateTag, replacements, out string sanitizedFormatter);
					if (tryFormatDateTime(props.FileDate, sanitizedFormatter, replacements, out var formattedDateString))
						fileNamingTemplate.ParameterReplacements[sanitizedTag] = formattedDateString;
				}

				return fileNamingTemplate.GetFilePath(fileExtension).PathWithoutPrefix;
			}
			#endregion
		}

		public class ChapterTitleTemplate : Templates
		{
			private List<TemplateTags> _templateTags { get; } = new()
			{
				TemplateTags.Title,
				TemplateTags.TitleShort,
				TemplateTags.Series,
				TemplateTags.ChCount,
				TemplateTags.ChNumber,
				TemplateTags.ChNumber0,
				TemplateTags.ChTitle,
			};
			public override string Name => "Chapter Title Template";

			public override string Description => Configuration.GetDescription(nameof(Configuration.ChapterTitleTemplate));

			public override string DefaultTemplate => "<ch#> - <title short>: <ch title>";

			protected override bool IsChapterized => true;

			public override IEnumerable<string> GetErrors(string template)
				=> new List<string>();

			public override IEnumerable<string> GetWarnings(string template)
				=> GetStandardWarnings(template).ToList();

			public string GetTitle(LibraryBookDto libraryBookDto, AaxDecrypter.MultiConvertFileProperties props)
				=> GetPortionTitle(libraryBookDto, Configuration.Instance.ChapterTitleTemplate, props);

			public string GetPortionTitle(LibraryBookDto libraryBookDto, string template, AaxDecrypter.MultiConvertFileProperties props)
			{
				if (string.IsNullOrEmpty(template)) return string.Empty;

				ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));

				var fileNamingTemplate = new MetadataNamingTemplate(template);
			  
				var title = libraryBookDto.Title ?? "";
				var titleShort = title.IndexOf(':') < 1 ? title : title.Substring(0, title.IndexOf(':'));

				fileNamingTemplate.AddParameterReplacement(TemplateTags.Title, title);
				fileNamingTemplate.AddParameterReplacement(TemplateTags.TitleShort, titleShort);
				fileNamingTemplate.AddParameterReplacement(TemplateTags.Series, libraryBookDto.SeriesName);
				fileNamingTemplate.AddParameterReplacement(TemplateTags.ChCount, props.PartsTotal);
				fileNamingTemplate.AddParameterReplacement(TemplateTags.ChNumber, props.PartsPosition);
				fileNamingTemplate.AddParameterReplacement(TemplateTags.ChNumber0, FileUtility.GetSequenceFormatted(props.PartsPosition, props.PartsTotal));
				fileNamingTemplate.AddParameterReplacement(TemplateTags.ChTitle, props.Title ?? "");

				return fileNamingTemplate.GetTagContents();
			}
			public override IEnumerable<TemplateTags> GetTemplateTags() => _templateTags;
		}
	}
}
