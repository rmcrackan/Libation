using Dinah.Core;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;

namespace LibationAvalonia.ViewModels.Settings
{
	public class DownloadDecryptSettingsVM : ViewModelBase
	{
		private string _folderTemplate;
		private string _fileTemplate;
		private string _chapterFileTemplate;

		public Configuration Config { get; }
		public DownloadDecryptSettingsVM(Configuration config)
		{
			Config = config;
			BadBookAsk = config.BadBook is Configuration.BadBookAction.Ask;
			BadBookAbort = config.BadBook is Configuration.BadBookAction.Abort;
			BadBookRetry = config.BadBook is Configuration.BadBookAction.Retry;
			BadBookIgnore = config.BadBook is Configuration.BadBookAction.Ignore;
			_folderTemplate = config.FolderTemplate;
			_fileTemplate = config.FileTemplate;
			_chapterFileTemplate = config.ChapterFileTemplate;
			InProgressDirectory = config.InProgress;
			UseCoverAsFolderIcon = config.UseCoverAsFolderIcon;
			SaveMetadataToFile = config.SaveMetadataToFile;
		}

		public List<Configuration.KnownDirectories> KnownDirectories { get; } = new()
		{
			Configuration.KnownDirectories.WinTemp,
			Configuration.KnownDirectories.ApplicationData,
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.MyDocs,
			Configuration.KnownDirectories.LibationFiles
		};

		public void SaveSettings(Configuration config)
		{
			config.BadBook
				= BadBookAbort ? Configuration.BadBookAction.Abort
				: BadBookRetry ? Configuration.BadBookAction.Retry
				: BadBookIgnore ? Configuration.BadBookAction.Ignore
				: Configuration.BadBookAction.Ask;

			config.FolderTemplate = FolderTemplate;
			config.FileTemplate = FileTemplate;
			config.ChapterFileTemplate = ChapterFileTemplate;
			config.InProgress = InProgressDirectory;

			config.UseCoverAsFolderIcon = UseCoverAsFolderIcon;
			config.SaveMetadataToFile = SaveMetadataToFile;
		}

		public string UseCoverAsFolderIconText { get; } = Configuration.GetDescription(nameof(Configuration.UseCoverAsFolderIcon));
		public string SaveMetadataToFileText { get; } = Configuration.GetDescription(nameof(Configuration.SaveMetadataToFile));
		public string BadBookGroupboxText { get; } = Configuration.GetDescription(nameof(Configuration.BadBook));
		public string BadBookAskText { get; } = Configuration.BadBookAction.Ask.GetDescription() ?? nameof(Configuration.BadBookAction.Ask);
		public string BadBookAbortText { get; } = Configuration.BadBookAction.Abort.GetDescription() ?? nameof(Configuration.BadBookAction.Abort);
		public string BadBookRetryText { get; } = Configuration.BadBookAction.Retry.GetDescription() ?? nameof(Configuration.BadBookAction.Retry);
		public string BadBookIgnoreText { get; } = Configuration.BadBookAction.Ignore.GetDescription() ?? nameof(Configuration.BadBookAction.Ignore);
		public string FolderTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.FolderTemplate));
		public string FileTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.FileTemplate));
		public string ChapterFileTemplateText { get; } = Configuration.GetDescription(nameof(Configuration.ChapterFileTemplate));
		public string EditCharReplacementText { get; } = Configuration.GetDescription(nameof(Configuration.ReplacementCharacters));
		public string InProgressDescriptionText { get; } = Configuration.GetDescription(nameof(Configuration.InProgress));

		public string FolderTemplate { get => _folderTemplate; set { this.RaiseAndSetIfChanged(ref _folderTemplate, value); } }
		public string FileTemplate { get => _fileTemplate; set { this.RaiseAndSetIfChanged(ref _fileTemplate, value); } }
		public string ChapterFileTemplate { get => _chapterFileTemplate; set { this.RaiseAndSetIfChanged(ref _chapterFileTemplate, value); } }
		public bool UseCoverAsFolderIcon { get; set; }
		public bool SaveMetadataToFile { get; set; }

		public bool BadBookAsk { get; set; }
		public bool BadBookAbort { get; set; }
		public bool BadBookRetry { get; set; }
		public bool BadBookIgnore { get; set; }

		public string InProgressDirectory { get; set; }
	}
}
