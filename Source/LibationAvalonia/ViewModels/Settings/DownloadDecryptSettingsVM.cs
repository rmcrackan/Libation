using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.ViewModels.Settings;

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

		_dailyDownloadLimit = DailyDownloadLimitScopes.SingleOrDefault(s => s.Value == config.DailyDownloadLimit) ?? DailyDownloadLimitScopes[0];
		_dailyDownloadLimitQuantity = config.DailyDownloadLimitQuantity;
		_dailyDownloadLimitUnit = DailyDownloadLimitUnits.SingleOrDefault(u => u.Value == config.DailyDownloadLimitUnit) ?? DailyDownloadLimitUnits[0];
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

		config.DailyDownloadLimit = DailyDownloadLimit?.Value ?? config.DailyDownloadLimit;
		config.DailyDownloadLimitQuantity = DailyDownloadLimitQuantity;
		config.DailyDownloadLimitUnit = DailyDownloadLimitUnit?.Value ?? config.DailyDownloadLimitUnit;
	}

	#region daily download limit

	public EnumDisplay<Configuration.DailyLimitScope>[] DailyDownloadLimitScopes { get; }
		= Enum.GetValues<Configuration.DailyLimitScope>()
		.Select(v => new EnumDisplay<Configuration.DailyLimitScope>(v))
		.ToArray();

	public EnumDisplay<Configuration.DailyLimitUnit>[] DailyDownloadLimitUnits { get; }
		= Enum.GetValues<Configuration.DailyLimitUnit>()
		.Select(v => new EnumDisplay<Configuration.DailyLimitUnit>(v))
		.ToArray();

	private EnumDisplay<Configuration.DailyLimitScope> _dailyDownloadLimit;
	public EnumDisplay<Configuration.DailyLimitScope> DailyDownloadLimit
	{
		get => _dailyDownloadLimit;
		set
		{
			this.RaiseAndSetIfChanged(ref _dailyDownloadLimit, value);
			this.RaisePropertyChanged(nameof(DailyDownloadLimitQuantityVisible));
		}
	}

	private int _dailyDownloadLimitQuantity;
	public int DailyDownloadLimitQuantity { get => _dailyDownloadLimitQuantity; set => this.RaiseAndSetIfChanged(ref _dailyDownloadLimitQuantity, value); }

	private EnumDisplay<Configuration.DailyLimitUnit> _dailyDownloadLimitUnit;
	public EnumDisplay<Configuration.DailyLimitUnit> DailyDownloadLimitUnit
	{
		get => _dailyDownloadLimitUnit;
		set
		{
			this.RaiseAndSetIfChanged(ref _dailyDownloadLimitUnit, value);
			this.RaisePropertyChanged(nameof(DailyDownloadLimitApproximateNoteVisible));
		}
	}

	/// <summary>The quantity and unit only mean anything once a scope other than "No limit" is chosen.</summary>
	public bool DailyDownloadLimitQuantityVisible => DailyDownloadLimit?.Value is not Configuration.DailyLimitScope.NoLimit;

	public bool DailyDownloadLimitApproximateNoteVisible
		=> DailyDownloadLimitQuantityVisible && DailyDownloadLimitUnit?.Value is not Configuration.DailyLimitUnit.Books;

	public string DailyDownloadLimitGroupboxText { get; } = Configuration.GetDescription(nameof(Configuration.DailyDownloadLimit));
	public string DailyDownloadLimitTip { get; } = Configuration.GetHelpText(nameof(Configuration.DailyDownloadLimit));
	public string DailyDownloadLimitQuantityText { get; } = Configuration.GetDescription(nameof(Configuration.DailyDownloadLimitQuantity));
	public string DailyDownloadLimitUnitText { get; } = Configuration.GetDescription(nameof(Configuration.DailyDownloadLimitUnit));
	public string DailyDownloadLimitApproximateNoteText { get; }
		= "MB and GB are approximate: Libation does not know precisely how large an audiobook is before downloading it, so it assumes about 400 MB per book when deciding whether another download would exceed the limit.";
	public string DailyDownloadLimitDescriptionText { get; }
		= "Rolling 24 hours, not a calendar day. Only downloads Libation completes are counted; books you download from the Audible app or website are not.";

	#endregion

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
