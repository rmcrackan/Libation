using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AaxDecrypter;
using Dinah.Core;
using FileManager;
using FileManager.NamingTemplate;
using NameParser;

#nullable enable
namespace LibationFileManager.Templates
{
	public interface ITemplate
	{
		static abstract string Name { get; }
		static abstract string Description { get; }
		static abstract string DefaultTemplate { get; }
		static abstract IEnumerable<TagCollection> TagCollections { get; }
	}

	public abstract class Templates
	{
		public const string ERROR_FULL_PATH_IS_INVALID = @"No colons or full paths allowed. Eg: should not start with C:\";
		public const string WARNING_NO_CHAPTER_NUMBER_TAG = "Should include chapter number tag in template used for naming files which are split by chapter. Ie: <ch#> or <ch# 0>";

		//Assigning the properties in the static constructor will require all
		//Templates users to have a valid configuration file. To allow tests
		//to work without access to Configuration, only load templates on demand.
		private static FolderTemplate? _folder;
		private static FileTemplate? _file;
		private static ChapterFileTemplate? _chapterFile;
		private static ChapterTitleTemplate? _chapterTitle;

		public static FolderTemplate Folder => _folder ??= GetTemplate<FolderTemplate>(Configuration.Instance.FolderTemplate);
		public static FileTemplate File => _file ??= GetTemplate<FileTemplate>(Configuration.Instance.FileTemplate);
		public static ChapterFileTemplate ChapterFile => _chapterFile ??= GetTemplate<ChapterFileTemplate>(Configuration.Instance.ChapterFileTemplate);
		public static ChapterTitleTemplate ChapterTitle => _chapterTitle ??= GetTemplate<ChapterTitleTemplate>(Configuration.Instance.ChapterTitleTemplate);

		#region Template Parsing

		public static T GetTemplate<T>(string? templateText) where T : Templates, ITemplate, new()
			=> TryGetTemplate<T>(templateText ?? "", out var template) ? template : GetDefaultTemplate<T>();

		public static bool TryGetTemplate<T>(string templateText, [NotNullWhen(true)] out T? template) where T : Templates, ITemplate, new()
		{
			var namingTemplate = NamingTemplate.Parse(templateText, T.TagCollections);

			template = new() { NamingTemplate = namingTemplate };
			return !namingTemplate.Errors.Any();
		}

		private static T GetDefaultTemplate<T>() where T : Templates, ITemplate, new()
			=> new() { NamingTemplate = NamingTemplate.Parse(T.DefaultTemplate, T.TagCollections) };

		static Templates()
		{
			Configuration.Instance.PropertyChanged +=
				[PropertyChangeFilter(nameof(Configuration.FolderTemplate))]
			(_, e) => _folder = GetTemplate<FolderTemplate>(e.NewValue as string);

			Configuration.Instance.PropertyChanged +=
				[PropertyChangeFilter(nameof(Configuration.FileTemplate))]
			(_, e) => _file = GetTemplate<FileTemplate>(e.NewValue as string);

			Configuration.Instance.PropertyChanged +=
				[PropertyChangeFilter(nameof(Configuration.ChapterFileTemplate))]
			(_, e) => _chapterFile = GetTemplate<ChapterFileTemplate>(e.NewValue as string);

			Configuration.Instance.PropertyChanged +=
				[PropertyChangeFilter(nameof(Configuration.ChapterTitleTemplate))]
			(_, e) => _chapterTitle = GetTemplate<ChapterTitleTemplate>(e.NewValue as string);

			HumanName.Suffixes.Add("ret");
			HumanName.Titles.Add("professor");
		}

		#endregion

		#region Template Properties

		public IEnumerable<TemplateTags> TagsRegistered
			=> NamingTemplate?.TagsRegistered.Cast<TemplateTags>() ?? Enumerable.Empty<TemplateTags>();
		public IEnumerable<TemplateTags> TagsInUse
			=> NamingTemplate?.TagsInUse.Cast<TemplateTags>() ?? Enumerable.Empty<TemplateTags>();
		public string TemplateText => NamingTemplate?.TemplateText ?? "";

		private readonly NamingTemplate? _namingTemplate;
		protected NamingTemplate NamingTemplate
		{
			get => _namingTemplate ?? throw new NullReferenceException(nameof(_namingTemplate));
			private init => _namingTemplate = value;
		}

		#endregion

		#region validation

		public virtual IEnumerable<string> Errors => NamingTemplate.Errors;
		public bool IsValid => !Errors.Any();

		public virtual IEnumerable<string> Warnings => NamingTemplate.Warnings;
		public bool HasWarnings => Warnings.Any();

		#endregion

		#region to file name

		public string GetName(LibraryBookDto libraryBookDto, MultiConvertFileProperties multiChapProps)
		{
			ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));
			ArgumentValidator.EnsureNotNull(multiChapProps, nameof(multiChapProps));
			return string.Concat(NamingTemplate.Evaluate(libraryBookDto, multiChapProps).Select(p => p.Value));
		}

		public LongPath GetFilename(LibraryBookDto libraryBookDto, string baseDir, string fileExtension, ReplacementCharacters? replacements = null, bool returnFirstExisting = false)
		{
			ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));
			ArgumentValidator.EnsureNotNull(baseDir, nameof(baseDir));
			ArgumentValidator.EnsureNotNull(fileExtension, nameof(fileExtension));

			replacements ??= Configuration.Instance.ReplacementCharacters;
			return GetFilename(baseDir, fileExtension, replacements, returnFirstExisting, libraryBookDto);
		}

		public LongPath GetFilename(LibraryBookDto libraryBookDto, MultiConvertFileProperties multiChapProps, string baseDir, string fileExtension, ReplacementCharacters? replacements = null, bool returnFirstExisting = false)
		{
			ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));
			ArgumentValidator.EnsureNotNull(multiChapProps, nameof(multiChapProps));
			ArgumentValidator.EnsureNotNull(baseDir, nameof(baseDir));
			ArgumentValidator.EnsureNotNull(fileExtension, nameof(fileExtension));

			replacements ??= Configuration.Instance.ReplacementCharacters;
			return GetFilename(baseDir, fileExtension, replacements, returnFirstExisting, libraryBookDto, multiChapProps);
		}

		protected virtual IEnumerable<string> GetTemplatePartsStrings(List<TemplatePart> parts, ReplacementCharacters replacements)
			=> parts.Select(p => replacements.ReplaceFilenameChars(p.Value));

		private LongPath GetFilename(string baseDir, string fileExtension, ReplacementCharacters replacements, bool returnFirstExisting, params object[] dtos)
		{
			fileExtension = FileUtility.GetStandardizedExtension(fileExtension);

			var parts = NamingTemplate.Evaluate(dtos).ToList();
			var pathParts = GetPathParts(GetTemplatePartsStrings(parts, replacements));

			//Remove 1 character from the end of the longest filename part until
			//the total filename is less than max filename length
			for (int i = 0; i < pathParts.Count; i++)
			{
				var part = pathParts[i];

				//If file already exists, GetValidFilename will append " (n)" to the filename.
				//This could cause the filename length to exceed MaxFilenameLength, so reduce
				//allowable filename length by 5 chars, allowing for up to 99 duplicates.
				var maxFilenameLength = LongPath.MaxFilenameLength -
					(i < pathParts.Count - 1 || string.IsNullOrEmpty(fileExtension) ? 0 : fileExtension.Length + 5);

				while (part.Sum(LongPath.GetFilesystemStringLength) > maxFilenameLength)
				{
					int maxLength = part.Max(p => p.Length);
					var maxEntry = part.First(p => p.Length == maxLength);

					var maxIndex = part.IndexOf(maxEntry);
					part.RemoveAt(maxIndex);
					part.Insert(maxIndex, maxEntry.Remove(maxLength - 1, 1));
				}
			}

			var fullPath = Path.Combine(pathParts.Select(fileParts => string.Concat(fileParts)).Prepend(baseDir).ToArray());

			return FileUtility.GetValidFilename(fullPath, replacements, fileExtension, returnFirstExisting);
		}

		/// <summary>
		/// Organize template parts into directories. Any Extra slashes will be
		/// returned as empty directories and are taken care of by Path.Combine()
		/// </summary>
		/// <returns>A List of template directories. Each directory is a list of template part strings</returns>
		private static List<List<string>> GetPathParts(IEnumerable<string> templateParts)
		{
			List<List<string>> directories = new();
			List<string> dir = new();

			foreach (var part in templateParts)
			{
				int slashIndex, lastIndex = 0;
				while ((slashIndex = part.IndexOf(Path.DirectorySeparatorChar, lastIndex)) > -1)
				{
					dir.Add(part[lastIndex..slashIndex]);
					RemoveSpaces(dir);
					directories.Add(dir);
					dir = new();

					lastIndex = slashIndex + 1;
				}
				dir.Add(part[lastIndex..]);
			}
			RemoveSpaces(dir);
			directories.Add(dir);

			return directories;
		}

		/// <summary>
		/// Remove spaces from the filename parts to ensure that after concatenation
		/// <br>-</br> There is no leading or trailing white space
		/// <br>-</br> There are no multispace instances
		/// </summary>
		private static void RemoveSpaces(List<string> parts)
		{
			while (parts.Count > 0 && string.IsNullOrWhiteSpace(parts[0]))
				parts.RemoveAt(0);

			while (parts.Count > 0 && string.IsNullOrWhiteSpace(parts[^1]))
				parts.RemoveAt(parts.Count - 1);

			if (parts.Count == 0) return;

			parts[0] = parts[0].TrimStart();
			parts[^1] = parts[^1].TrimEnd();

			//Replace all multispace substrings with single space
			for (int i = 0; i < parts.Count; i++)
			{
				string original;
				do
				{
					original = parts[i];
					parts[i] = original.Replace("  ", " ");
				} while (original.Length != parts[i].Length);
			}

			//Remove instances of double spaces at part boundaries
			for (int i = 1; i < parts.Count; i++)
			{
				if (parts[i - 1].EndsWith(' ') && parts[i].StartsWith(' '))
				{
					parts[i] = parts[i].Substring(1);

					if (parts[i].Length == 0)
					{
						parts.RemoveAt(i);
						i--;
					}
				}
			}
		}

		#endregion

		#region Registered Template Properties

		private static readonly PropertyTagCollection<LibraryBookDto> filePropertyTags =
			new(caseSensative: true, StringFormatter, DateTimeFormatter, IntegerFormatter, FloatFormatter)
		{
			//Don't allow formatting of Id
			{ TemplateTags.Id, lb => lb.AudibleProductId, v => v ?? "" },
			{ TemplateTags.Title, lb => lb.TitleWithSubtitle },
			{ TemplateTags.TitleShort, lb => getTitleShort(lb.Title) },
			{ TemplateTags.AudibleTitle, lb => lb.Title },
			{ TemplateTags.AudibleSubtitle, lb => lb.Subtitle },
			{ TemplateTags.Author, lb => lb.Authors, NameListFormat.Formatter },
			{ TemplateTags.FirstAuthor, lb => lb.FirstAuthor, FormattableFormatter },
			{ TemplateTags.Narrator, lb => lb.Narrators, NameListFormat.Formatter },
			{ TemplateTags.FirstNarrator, lb => lb.FirstNarrator, FormattableFormatter },
			{ TemplateTags.Series, lb => lb.Series, SeriesListFormat.Formatter },
			{ TemplateTags.FirstSeries, lb => lb.FirstSeries, FormattableFormatter },
			{ TemplateTags.SeriesNumber, lb => lb.FirstSeries?.Number },
			{ TemplateTags.Language, lb => lb.Language },
			//Don't allow formatting of LanguageShort
			{ TemplateTags.LanguageShort, lb =>lb.Language, getLanguageShort },
			{ TemplateTags.Bitrate, lb => (int?)(lb.IsPodcastParent ? null : lb.BitRate) },
			{ TemplateTags.SampleRate, lb => (int?)(lb.IsPodcastParent ? null : lb.SampleRate) },
			{ TemplateTags.Channels, lb => (int?)(lb.IsPodcastParent ? null : lb.Channels) },
			{ TemplateTags.Account, lb => lb.Account },
			{ TemplateTags.AccountNickname, lb => lb.AccountNickname },
			{ TemplateTags.Locale, lb => lb.Locale },
			{ TemplateTags.YearPublished, lb => lb.YearPublished },
			{ TemplateTags.DatePublished, lb => lb.DatePublished },
			{ TemplateTags.DateAdded, lb => lb.DateAdded },
			{ TemplateTags.FileDate, lb => lb.FileDate },
			};

		private static readonly List<TagCollection> chapterPropertyTags = new()
		{
			new PropertyTagCollection<LibraryBookDto>(caseSensative: true, StringFormatter)
			{
				{ TemplateTags.Title, lb => lb.TitleWithSubtitle },
				{ TemplateTags.TitleShort, lb => getTitleShort(lb.Title) },
				{ TemplateTags.AudibleTitle, lb => lb.Title },
				{ TemplateTags.AudibleSubtitle, lb => lb.Subtitle },
				{ TemplateTags.Series, lb => lb.Series, SeriesListFormat.Formatter },
				{ TemplateTags.FirstSeries, lb => lb.FirstSeries, FormattableFormatter },
			},
			new PropertyTagCollection<MultiConvertFileProperties>(caseSensative: true, StringFormatter, IntegerFormatter, DateTimeFormatter)
			{
				{ TemplateTags.ChCount, m => m.PartsTotal },
				{ TemplateTags.ChNumber, m => m.PartsPosition },
				{ TemplateTags.ChNumber0, m => m.PartsPosition.ToString("D" + ((int)Math.Log10(m.PartsTotal) + 1)) },
				{ TemplateTags.ChTitle, m => m.Title },
				{ TemplateTags.FileDate, m => m.FileDate }
			}
		};

		private static readonly ConditionalTagCollection<LibraryBookDto> conditionalTags = new()
		{
			{ TemplateTags.IfSeries, lb => lb.IsSeries || lb.IsPodcastParent },
			{ TemplateTags.IfPodcast, lb => lb.IsPodcast || lb.IsPodcastParent },
			{ TemplateTags.IfBookseries, lb => lb.IsSeries && !lb.IsPodcast && !lb.IsPodcastParent },
		};

		private static readonly ConditionalTagCollection<LibraryBookDto> folderConditionalTags = new()
		{
			{ TemplateTags.IfPodcastParent, lb => lb.IsPodcastParent }
		};

		#endregion

		#region Tag Formatters

		private static string? getTitleShort(string? title)
			=> title?.IndexOf(':') > 0 ? title.Substring(0, title.IndexOf(':')) : title;

		private static string getLanguageShort(string? language)
		{
			if (language is null)
				return "";

			language = language.Trim();
			if (language.Length <= 3)
				return language.ToUpper();
			return language[..3].ToUpper();
		}

		private static string FormattableFormatter(ITemplateTag templateTag, IFormattable? value, string formatString)
			=> value?.ToString(formatString, null) ?? "";

		private static string StringFormatter(ITemplateTag templateTag, string value, string formatString)
		{
			if (value is null) return "";
			else if (string.Compare(formatString, "u", ignoreCase: true) == 0) return value.ToUpper();
			else if (string.Compare(formatString, "l", ignoreCase: true) == 0) return value.ToLower();
			else return value;
		}

		private static string IntegerFormatter(ITemplateTag templateTag, int value, string formatString)
			=> FloatFormatter(templateTag, value, formatString);

		private static string FloatFormatter(ITemplateTag templateTag, float value, string formatString)
		{
			if (int.TryParse(formatString, out var numDigits) && numDigits > 0)
			{
				//Zero-pad the integer part
				var strValue = value.ToString();
				var decIndex = strValue.IndexOf(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
				var zeroPad = decIndex == -1 ? int.Max(0, numDigits - strValue.Length) : int.Max(0, numDigits - decIndex);

				return new string('0', zeroPad) + strValue;
			}
			return value.ToString(formatString);
		}

		private static string DateTimeFormatter(ITemplateTag templateTag, DateTime value, string formatString)
		{
			if (string.IsNullOrEmpty(formatString))
				return value.ToString(TemplateTags.DEFAULT_DATE_FORMAT);
			return value.ToString(formatString);
		}

		#endregion

		public class FolderTemplate : Templates, ITemplate
		{
			public static string Name { get; } = "Folder Template";
			public static string Description { get; } = Configuration.GetDescription(nameof(Configuration.FolderTemplate)) ?? "";
			public static string DefaultTemplate { get; } = "<title short> [<id>]";
			public static IEnumerable<TagCollection> TagCollections
				=> new TagCollection[] { filePropertyTags, conditionalTags, folderConditionalTags };

			public override IEnumerable<string> Errors
				=> TemplateText?.Length >= 2 && Path.IsPathFullyQualified(TemplateText) ? base.Errors.Append(ERROR_FULL_PATH_IS_INVALID) : base.Errors;

			protected override List<string> GetTemplatePartsStrings(List<TemplatePart> parts, ReplacementCharacters replacements)
				=> parts
				.Select(tp => tp.TemplateTag is null
					//FolderTemplate literals can have directory separator characters
					? replacements.ReplacePathChars(tp.Value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar))
					: replacements.ReplaceFilenameChars(tp.Value)
				).ToList();
		}

		public class FileTemplate : Templates, ITemplate
		{
			public static string Name { get; } = "File Template";
			public static string Description { get; } = Configuration.GetDescription(nameof(Configuration.FileTemplate)) ?? "";
			public static string DefaultTemplate { get; } = "<title> [<id>]";
			public static IEnumerable<TagCollection> TagCollections { get; } = new TagCollection[] { filePropertyTags, conditionalTags };
		}

		public class ChapterFileTemplate : Templates, ITemplate
		{
			public static string Name { get; } = "Chapter File Template";
			public static string Description { get; } = Configuration.GetDescription(nameof(Configuration.ChapterFileTemplate)) ?? "";
			public static string DefaultTemplate { get; } = "<title> [<id>] - <ch# 0> - <ch title>";
			public static IEnumerable<TagCollection> TagCollections { get; } = chapterPropertyTags.Append(filePropertyTags).Append(conditionalTags);

			public override IEnumerable<string> Warnings
				=> NamingTemplate.TagsInUse.Any(t => t.TagName.In(TemplateTags.ChNumber.TagName, TemplateTags.ChNumber0.TagName))
				? base.Warnings
				: base.Warnings.Append(WARNING_NO_CHAPTER_NUMBER_TAG);
		}

		public class ChapterTitleTemplate : Templates, ITemplate
		{
			public static string Name { get; } = "Chapter Title Template";
			public static string Description { get; } = Configuration.GetDescription(nameof(Configuration.ChapterTitleTemplate)) ?? "";
			public static string DefaultTemplate => "<ch#> - <title short>: <ch title>";
			public static IEnumerable<TagCollection> TagCollections { get; } = chapterPropertyTags.Append(conditionalTags);

			protected override IEnumerable<string> GetTemplatePartsStrings(List<TemplatePart> parts, ReplacementCharacters replacements)
				=> parts.Select(p => p.Value);
		}
	}
}
