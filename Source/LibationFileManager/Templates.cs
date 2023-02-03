using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AaxDecrypter;
using Dinah.Core;
using FileManager;
using FileManager.NamingTemplate;

namespace LibationFileManager
{
	public interface ITemplate
	{
		static abstract string DefaultTemplate { get; }
		static abstract IEnumerable<TagClass> TagClass { get; }
	}

	public abstract class Templates
	{
		public const string ERROR_FULL_PATH_IS_INVALID = @"No colons or full paths allowed. Eg: should not start with C:\";
		public const string WARNING_NO_CHAPTER_NUMBER_TAG = "Should include chapter number tag in template used for naming files which are split by chapter. Ie: <ch#> or <ch# 0>";

		//Assign the properties in the static constructor will require all
		//Templates users to have a valid configuration file. To allow tests
		//to work without access to Configuration, only load templates on demand.
		private static FolderTemplate _folder;
		private static FileTemplate _file;
		private static ChapterFileTemplate _chapterFile;
		private static ChapterTitleTemplate _chapterTitle;

		public static FolderTemplate Folder => _folder ??= GetTemplate<FolderTemplate>(Configuration.Instance.FolderTemplate);
		public static FileTemplate File => _file ??= GetTemplate<FileTemplate>(Configuration.Instance.FileTemplate);
		public static ChapterFileTemplate ChapterFile => _chapterFile ??= GetTemplate<ChapterFileTemplate>(Configuration.Instance.ChapterFileTemplate);
		public static ChapterTitleTemplate ChapterTitle => _chapterTitle ??= GetTemplate<ChapterTitleTemplate>(Configuration.Instance.ChapterTitleTemplate);

		#region Template Parsing
		public static T GetTemplate<T>(string templateText) where T : Templates, ITemplate, new()
			=> TryGetTemplate<T>(templateText, out var template) ? template : GetDefaultTemplate<T>();

		public static bool TryGetTemplate<T>(string templateText, out T template) where T : Templates, ITemplate, new()
		{
			var namingTemplate = NamingTemplate.Parse(templateText, T.TagClass);

			template = new() { Template = namingTemplate };
			return !namingTemplate.Errors.Any();
		}

		private static T GetDefaultTemplate<T>() where T : Templates, ITemplate, new()
			=> new() { Template = NamingTemplate.Parse(T.DefaultTemplate, T.TagClass) };

		static Templates()
		{
			Configuration.Instance.PropertyChanged +=
				[PropertyChangeFilter(nameof(Configuration.FolderTemplate))]
			(_,e) => _folder = GetTemplate<FolderTemplate>((string)e.NewValue);

			Configuration.Instance.PropertyChanged
				+= [PropertyChangeFilter(nameof(Configuration.FileTemplate))]
			(_, e) => _file = GetTemplate<FileTemplate>((string)e.NewValue);

			Configuration.Instance.PropertyChanged
				+= [PropertyChangeFilter(nameof(Configuration.ChapterFileTemplate))]
			(_, e) => _chapterFile = GetTemplate<ChapterFileTemplate>((string)e.NewValue);

			Configuration.Instance.PropertyChanged
				+= [PropertyChangeFilter(nameof(Configuration.ChapterTitleTemplate))]
			(_, e) => _chapterTitle = GetTemplate<ChapterTitleTemplate>((string)e.NewValue);
		}
		#endregion

		#region Template Properties
		public IEnumerable<TemplateTags> TagsRegistered => Template.TagsRegistered.Cast<TemplateTags>();
		public IEnumerable<TemplateTags> TagsInUse => Template.TagsInUse.Cast<TemplateTags>();
		public abstract string Name { get; }
		public abstract string Description { get; }
		public string TemplateText => Template.TemplateText;
		protected NamingTemplate Template { get; private set; }


		#endregion

		#region validation

		public virtual IEnumerable<string> Errors => Template.Errors;
		public bool IsValid => !Errors.Any();

		public virtual IEnumerable<string> Warnings => Template.Warnings;
		public bool HasWarnings => Warnings.Any();

		#endregion

		#region to file name

		public string GetName(LibraryBookDto libraryBookDto, MultiConvertFileProperties multiChapProps)
		{
			ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));
			ArgumentValidator.EnsureNotNull(multiChapProps, nameof(multiChapProps));
			return string.Join("", Template.Evaluate(libraryBookDto, multiChapProps).Select(p => p.Value));
		}

		public LongPath GetFilename(LibraryBookDto libraryBookDto, string baseDir, string fileExtension, ReplacementCharacters replacements = null, bool returnFirstExisting = false)
		{
			ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));
			ArgumentValidator.EnsureNotNull(baseDir, nameof(baseDir));
			ArgumentValidator.EnsureNotNull(fileExtension, nameof(fileExtension));

			replacements ??= Configuration.Instance.ReplacementCharacters;
			return GetFilename(baseDir, fileExtension, returnFirstExisting, replacements, libraryBookDto);
		}

		public LongPath GetFilename(LibraryBookDto libraryBookDto, MultiConvertFileProperties multiChapProps, string baseDir = "", string fileExtension = null, ReplacementCharacters replacements = null)
		{
			ArgumentValidator.EnsureNotNull(libraryBookDto, nameof(libraryBookDto));
			ArgumentValidator.EnsureNotNull(multiChapProps, nameof(multiChapProps));
			ArgumentValidator.EnsureNotNull(baseDir, nameof(baseDir));

			replacements ??= Configuration.Instance.ReplacementCharacters;
			fileExtension ??= Path.GetExtension(multiChapProps.OutputFileName);
			return GetFilename(baseDir, fileExtension, false, replacements, libraryBookDto, multiChapProps);
		}

		protected virtual IEnumerable<string> GetTemplatePartsStrings(List<TemplatePart> parts, ReplacementCharacters replacements)
			=> parts.Select(p => replacements.ReplaceFilenameChars(p.Value));

		private LongPath GetFilename(string baseDir, string fileExtension, bool returnFirstExisting, ReplacementCharacters replacements,  params object[] dtos)
		{
			fileExtension = FileUtility.GetStandardizedExtension(fileExtension);

			var parts = Template.Evaluate(dtos).ToList();
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

			var fullPath = Path.Combine(pathParts.Select(p => string.Join("", p)).Prepend(baseDir).ToArray());

			return  FileUtility.GetValidFilename(fullPath, replacements, fileExtension, returnFirstExisting);
		}

		/// <summary>
		/// Organize template parts into directories. 
		/// </summary>
		/// <returns>A List of template directories. Each directory is a list of template part strings</returns>
		private List<List<string>> GetPathParts(IEnumerable<string> templateParts)
		{
			List<List<string>> directories = new();
			List<string> dir = new();

			foreach (var part in templateParts)
			{
				int slashIndex, lastIndex = 0;
				while((slashIndex = part.IndexOf(Path.DirectorySeparatorChar, lastIndex)) > -1)
				{
					dir.Add(part[lastIndex..slashIndex]);
					directories.Add(dir);
					dir = new();

					lastIndex = slashIndex + 1;
				}
				dir.Add(part[lastIndex..]);
			}
			directories.Add(dir);

			return directories;
		}

		#endregion

		#region Registered Template Properties

		private static readonly PropertyTagClass<LibraryBookDto> filePropertyTags = GetFilePropertyTags();
		private static readonly ConditionalTagClass<LibraryBookDto> conditionalTags = GetConditionalTags();
		private static readonly List<TagClass> chapterPropertyTags = GetChapterPropertyTags();

		private static ConditionalTagClass<LibraryBookDto> GetConditionalTags()
		{
			ConditionalTagClass<LibraryBookDto> lbConditions = new();

			lbConditions.RegisterCondition(TemplateTags.IfSeries, lb => !string.IsNullOrWhiteSpace(lb.SeriesName));
			lbConditions.RegisterCondition(TemplateTags.IfPodcast, lb => lb.IsPodcast);

			return lbConditions;
		}

		private static PropertyTagClass<LibraryBookDto> GetFilePropertyTags()
		{
			PropertyTagClass<LibraryBookDto> lbProperties = new();
			lbProperties.RegisterProperty(TemplateTags.Id, lb => lb.AudibleProductId);
			lbProperties.RegisterProperty(TemplateTags.Title, lb => lb.Title ?? "", StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.TitleShort, lb => lb.Title.IndexOf(':') < 1 ? lb.Title : lb.Title.Substring(0, lb.Title.IndexOf(':')), StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.Author, lb => lb.AuthorNames, StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.FirstAuthor, lb => lb.FirstAuthor, StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.Narrator, lb => lb.NarratorNames, StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.FirstNarrator, lb => lb.FirstNarrator, StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.Series, lb => lb.SeriesName ?? "", StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.SeriesNumber, lb => lb.SeriesNumber);
			lbProperties.RegisterProperty(TemplateTags.Language, lb => lb.Language);
			lbProperties.RegisterProperty(TemplateTags.LanguageShort, lb => getLanguageShort(lb.Language));
			lbProperties.RegisterProperty(TemplateTags.Bitrate, lb => lb.BitRate, IntegerFormatter); 
			lbProperties.RegisterProperty(TemplateTags.SampleRate, lb => lb.SampleRate, IntegerFormatter);
			lbProperties.RegisterProperty(TemplateTags.Channels, lb => lb.Channels, IntegerFormatter);
			lbProperties.RegisterProperty(TemplateTags.Account, lb => lb.Account, StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.Locale, lb => lb.Locale, StringFormatter);
			lbProperties.RegisterProperty(TemplateTags.YearPublished, lb => lb.YearPublished, IntegerFormatter);
			lbProperties.RegisterProperty(TemplateTags.DatePublished, lb => lb.DatePublished, DateTimeFormatter);
			lbProperties.RegisterProperty(TemplateTags.DateAdded, lb => lb.DateAdded, DateTimeFormatter);
			lbProperties.RegisterProperty(TemplateTags.FileDate, lb => lb.FileDate, DateTimeFormatter);
			return lbProperties;
		}

		private static List<TagClass> GetChapterPropertyTags()
		{
			PropertyTagClass<LibraryBookDto> lbProperties = new();
			PropertyTagClass<MultiConvertFileProperties> multiConvertProperties = new();

			lbProperties.RegisterProperty(TemplateTags.Title, lb => lb.Title ?? "");
			lbProperties.RegisterProperty(TemplateTags.TitleShort, lb => lb.Title.IndexOf(':') < 1 ? lb.Title : lb.Title.Substring(0, lb.Title.IndexOf(':')));
			lbProperties.RegisterProperty(TemplateTags.Series, lb => lb.SeriesName ?? "");

			multiConvertProperties.RegisterProperty(TemplateTags.ChCount, lb => lb.PartsTotal, IntegerFormatter);
			multiConvertProperties.RegisterProperty(TemplateTags.ChNumber, lb => lb.PartsPosition, IntegerFormatter);
			multiConvertProperties.RegisterProperty(TemplateTags.ChNumber0, m => m.PartsPosition.ToString("D" + ((int)Math.Log10(m.PartsTotal) + 1)));
			multiConvertProperties.RegisterProperty(TemplateTags.ChTitle, m => m.Title ?? "", StringFormatter);

			return new List<TagClass> { lbProperties, multiConvertProperties };
		}

		#endregion

		#region Tag Formatters

		private static string getLanguageShort(string language)
		{
			if (language is null)
				return null;

			language = language.Trim();
			if (language.Length <= 3)
				return language.ToUpper();
			return language[..3].ToUpper();
		}

		private static string StringFormatter(ITemplateTag templateTag, string value, string formatString)
		{
			if (string.Compare(formatString, "u", ignoreCase: true) == 0) return value?.ToUpper();
			else if (string.Compare(formatString, "l", ignoreCase: true) == 0) return value?.ToLower();
			else return value;
		}

		private static string IntegerFormatter(ITemplateTag templateTag, int value, string formatString)
		{
			if (int.TryParse(formatString, out var numDigits))
				return value.ToString($"D{numDigits}");
			return value.ToString();
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
			public override string Name => "Folder Template";
			public override string Description => Configuration.GetDescription(nameof(Configuration.FolderTemplate));
			public static string DefaultTemplate { get; } = "<title short> [<id>]";
			public static IEnumerable<TagClass> TagClass => new TagClass[] { filePropertyTags, conditionalTags };

			public override IEnumerable<string> Errors
				=> TemplateText?.Length >= 2 && Path.IsPathFullyQualified(TemplateText) ? base.Errors.Append(ERROR_FULL_PATH_IS_INVALID) : base.Errors;

			protected override List<string> GetTemplatePartsStrings(List<TemplatePart> parts, ReplacementCharacters replacements)
			{
				foreach (var tp in parts)
				{
					//FolderTemplate literals can have directory separator characters
					if (tp.TemplateTag is null)
						tp.Value = replacements.ReplacePathChars(tp.Value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
					else
						tp.Value = replacements.ReplaceFilenameChars(tp.Value);
				}
				if (parts.Count > 0)
				{
					//Remove DirectorySeparatorChar at beginning and end of template
					if (parts[0].Value.Length > 0 && parts[0].Value[0] == Path.DirectorySeparatorChar)
						parts[0].Value = parts[0].Value.Remove(0,1);

					if (parts[^1].Value.Length > 0 && parts[^1].Value[^1] == Path.DirectorySeparatorChar)
						parts[^1].Value = parts[^1].Value.Remove(parts[^1].Value.Length - 1, 1);
				}
				return parts.Select(p => p.Value).ToList();
			}
		}

		public class FileTemplate : Templates, ITemplate
		{
			public override string Name => "File Template";
			public override string Description => Configuration.GetDescription(nameof(Configuration.FileTemplate));
			public static string DefaultTemplate { get; } = "<title> [<id>]";
			public static IEnumerable<TagClass> TagClass { get; } = new TagClass[] { filePropertyTags, conditionalTags };
		}

		public class ChapterFileTemplate : Templates, ITemplate
		{
			public override string Name => "Chapter File Template";
			public override string Description => Configuration.GetDescription(nameof(Configuration.ChapterFileTemplate));
			public static string DefaultTemplate { get; } = "<title> [<id>] - <ch# 0> - <ch title>";
			public static IEnumerable<TagClass> TagClass { get; }
				= chapterPropertyTags.Append(filePropertyTags).Append(conditionalTags);

			public override IEnumerable<string> Warnings
				=> Template.TagsInUse.Any(t => t.TagName.In(TemplateTags.ChNumber.TagName, TemplateTags.ChNumber0.TagName))
				? base.Warnings
				: base.Warnings.Append(WARNING_NO_CHAPTER_NUMBER_TAG);
		}

		public class ChapterTitleTemplate : Templates, ITemplate
		{
			public override string Name => "Chapter Title Template";
			public override string Description => Configuration.GetDescription(nameof(Configuration.ChapterTitleTemplate));
			public static string DefaultTemplate => "<ch#> - <title short>: <ch title>";
			public static IEnumerable<TagClass> TagClass { get; }
				= chapterPropertyTags.Append(conditionalTags);

			protected override IEnumerable<string> GetTemplatePartsStrings(List<TemplatePart> parts, ReplacementCharacters replacements)
				=> parts.Select(p => p.Value);
		}
	}
}
