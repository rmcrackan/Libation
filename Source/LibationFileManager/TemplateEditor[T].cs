using AaxDecrypter;
using FileManager;
using System.Collections.Generic;
using System;
using System.IO;

#nullable enable
namespace LibationFileManager
{
	public interface ITemplateEditor
	{
		bool IsFolder { get; }
		bool IsFilePath { get; }
		LongPath BaseDirectory { get; }
		string DefaultTemplate { get; }
		string TemplateName { get; }
		string TemplateDescription { get; }
		Templates EditingTemplate { get; }
		bool SetTemplateText(string templateText);
		string? GetFolderName();
		string? GetFileName();
		string? GetName();
	}

	public class TemplateEditor<T> : ITemplateEditor where T : Templates, ITemplate, new()
	{
		public bool IsFolder => EditingTemplate is Templates.FolderTemplate;
		public bool IsFilePath => EditingTemplate is not Templates.ChapterTitleTemplate;
		public LongPath BaseDirectory { get; private init; }
		public string DefaultTemplate { get; private init; }
		public string TemplateName { get; private init; }
		public string TemplateDescription { get; private init; }
		private Templates? Folder { get; set; }
		private Templates? File { get; set; }
		private Templates? Name { get; set; }
		public Templates EditingTemplate
		{
			get => _editingTemplate;
			private set => _editingTemplate = !IsFilePath ? Name = value : IsFolder ? Folder = value : File = value;
		}

		private Templates _editingTemplate;

		public bool SetTemplateText(string templateText)
		{
			if (Templates.TryGetTemplate<T>(templateText, out var template))
			{
				EditingTemplate = template;
				return true;
			}
			return false;
		}

		private static readonly LibraryBookDto libraryBookDto
			= new()
			{
				Account = "myaccount@example.co",
				AccountNickname = "my account",
				DateAdded = new DateTime(2022, 6, 9, 0, 0, 0),
				DatePublished = new DateTime(2017, 2, 27, 0, 0, 0),
				AudibleProductId = "123456789",
				Title = "A Study in Scarlet",
				TitleWithSubtitle = "A Study in Scarlet: A Sherlock Holmes Novel",
				Subtitle = "A Sherlock Holmes Novel",
				Locale = "us",
				YearPublished = 2017,
				Authors = new List<string> { "Arthur Conan Doyle", "Stephen Fry - introductions" },
				Narrators = new List<string> { "Stephen Fry" },
				SeriesName = "Sherlock Holmes",
				SeriesNumber = 1,
				BitRate = 128,
				SampleRate = 44100,
				Channels = 2,
				Language = "English"
			};

		private static readonly MultiConvertFileProperties partFileProperties
			= new()
			{
				OutputFileName = "",
				PartsPosition = 4,
				PartsTotal = 10,
				Title = "A Flight for Life"
			};

		public string? GetFolderName()
		{
			/*
			* Path must be rooted for windows to allow long file paths. This is
			* only necessary for folder templates because they may contain several
			* subdirectories. Without rooting, we won't be allowed to create a
			* relative path longer than MAX_PATH.
			*/
			var dir = Folder?.GetFilename(libraryBookDto, BaseDirectory, "");
			if (dir is null) return null;
			return Path.GetRelativePath(BaseDirectory, dir);
		}

		public string? GetFileName()
			=> File?.GetFilename(libraryBookDto, partFileProperties, "", "");
		public string? GetName()
			=> Name?.GetName(libraryBookDto, partFileProperties);

		private TemplateEditor(
			Templates editingTemplate,
			LongPath baseDirectory,
			string defaultTemplate,
			string templateName,
			string templateDescription)
		{
			_editingTemplate = editingTemplate;
			BaseDirectory = baseDirectory;
			DefaultTemplate = defaultTemplate;
			TemplateName = templateName;
			TemplateDescription = templateDescription;
		}

		public static ITemplateEditor CreateFilenameEditor(LongPath baseDir, string templateText)
		{
			if (!Templates.TryGetTemplate<T>(templateText, out var template))
				throw new ArgumentException($"Failed to parse {nameof(templateText)}");

			var templateEditor = new TemplateEditor<T>(template, baseDir, T.DefaultTemplate, T.Name, T.Description);

			if (!templateEditor.IsFolder && !templateEditor.IsFilePath)
				throw new InvalidOperationException($"This method is only for File and Folder templates. Use {nameof(CreateNameEditor)} for name templates");
			
			if (templateEditor.IsFolder)
				templateEditor.File = Templates.File;
			else
				templateEditor.Folder = Templates.Folder;

			return templateEditor;
		}

		public static ITemplateEditor CreateNameEditor(string templateText)
		{
			if (!Templates.TryGetTemplate<T>(templateText, out var nameTemplate))
				throw new ArgumentException($"Failed to parse {nameof(templateText)}");

			var templateEditor = new TemplateEditor<T>(nameTemplate, "", T.DefaultTemplate, T.Name, T.Description);

			if (templateEditor.IsFolder || templateEditor.IsFilePath)
				throw new InvalidOperationException($"This method is only for name templates. Use {nameof(CreateFilenameEditor)} for file templates");

			return templateEditor;
		}
	}
}
