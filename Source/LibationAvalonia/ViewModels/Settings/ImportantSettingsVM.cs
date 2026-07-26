using AudibleApi.Authorization;
using AudibleUtilities;
using Dinah.Core;
using FileManager;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibationAvalonia.ViewModels.Settings;

public class ImportantSettingsVM : ViewModelBase
{
	private EnumDisplay<Configuration.Theme> themeVariant;
	private readonly Configuration config;
	private bool encryptTokens;
	private bool plaintextWarningVisible;
	private bool convertButtonEnabled;
	private bool osStoreUnavailableVisible;
	private string osStoreUnavailableMessage = "";
	private TokenStorageMethod initialTokenStorageMethod;
	private bool osSecretStoreAvailable;

	public ImportantSettingsVM(Configuration config)
	{
		this.config = config;

		BooksDirectory = config.Books?.PathWithoutPrefix ?? "";
		SavePodcastsToParentFolder = config.SavePodcastsToParentFolder;
		OverwriteExisting = config.OverwriteExisting;
		CreationTime = DateTimeSources.SingleOrDefault(v => v.Value == config.CreationTime) ?? DateTimeSources[0];
		LastWriteTime = DateTimeSources.SingleOrDefault(v => v.Value == config.LastWriteTime) ?? DateTimeSources[0];
		UseWebView = config.UseWebView;
		LoggingLevel = config.LogLevel;
		GridScaleFactor = scaleFactorToLinearRange(config.GridScaleFactor);
		GridFontScaleFactor = scaleFactorToLinearRange(config.GridFontScaleFactor);

		themeVariant = Themes.Single(v => v.Value == config.ThemeVariant);

		initialTokenStorageMethod = config.TokenStorageMethod;
		osSecretStoreAvailable = IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out var unavailableReason);
		if (!osSecretStoreAvailable)
		{
			OsStoreUnavailableVisible = true;
			OsStoreUnavailableMessage = TokenStorageSettingsUi.OsStoreUnavailableMessage(unavailableReason);
			encryptTokens = false;
			CanEditEncryptTokens = false;
		}
		else
		{
			OsStoreUnavailableVisible = false;
			encryptTokens = TokenStorageSettingsUi.CheckboxFromMethod(config.TokenStorageMethod);
			CanEditEncryptTokens = true;
		}

		RefreshTokenStorageUiState();
		ConvertExistingTokensCommand = ReactiveCommand.CreateFromTask(ConvertExistingTokensAsync);
	}

	public async Task<TokenStorageSaveDecision> PromptOnSaveAsync(object? owner)
		=> await TokenStorageSettingsUi.PromptOnPreferenceSaveAsync(owner, initialTokenStorageMethod, SelectedTokenStorageMethod);

	public async Task<bool> ConvertAfterSaveAsync(object? owner)
		=> await TokenStorageSettingsUi.RunConversionAsync(owner, SelectedTokenStorageMethod);

	public void SaveSettings(Configuration config)
	{
		config.Books = GetBooksDirectory();
		config.SavePodcastsToParentFolder = SavePodcastsToParentFolder;
		config.OverwriteExisting = OverwriteExisting;
		config.CreationTime = CreationTime.Value;
		config.LastWriteTime = LastWriteTime.Value;
		config.UseWebView = UseWebView;
		config.LogLevel = LoggingLevel;
		config.TokenStorageMethod = SelectedTokenStorageMethod;
		initialTokenStorageMethod = SelectedTokenStorageMethod;
		RefreshTokenStorageUiState();
	}

	public LongPath GetBooksDirectory()
		=> Configuration.GetKnownDirectory(BooksDirectory) is Configuration.KnownDirectories.None ? BooksDirectory : System.IO.Path.Combine(BooksDirectory, "Books");

	private static float scaleFactorToLinearRange(float scaleFactor)
		=> float.Round(100 * MathF.Log2(scaleFactor));
	private static float linearRangeToScaleFactor(float value)
		=> MathF.Pow(2, value / 100f);

	public void ApplyDisplaySettings()
	{
		config.GridFontScaleFactor = linearRangeToScaleFactor(GridFontScaleFactor);
		config.GridScaleFactor = linearRangeToScaleFactor(GridScaleFactor);
	}
	public void OpenLogFolderButton()
	{
		if (System.IO.File.Exists(LogFileFilter.LogFilePath))
			Go.To.File(LogFileFilter.LogFilePath);
		else
			Go.To.Folder(Configuration.Instance.LibationFiles.Location.ShortPathName);
	}

	private async Task ConvertExistingTokensAsync()
	{
		var converted = await TokenStorageSettingsUi.ConfirmAndConvertAsync(null, SelectedTokenStorageMethod);
		if (converted)
			RefreshTokenStorageUiState();
	}

	private void RefreshTokenStorageUiState()
	{
		var method = SelectedTokenStorageMethod;
		PlaintextWarningVisible = osSecretStoreAvailable && method == TokenStorageMethod.Plaintext;
		var alignment = AccountTokenStorage.GetAccountsAlignment(method);
		ConvertButtonEnabled = TokenStorageSettingsUi.IsConvertButtonEnabled(alignment);
	}

	private TokenStorageMethod SelectedTokenStorageMethod
		=> TokenStorageSettingsUi.MethodFromCheckbox(EncryptTokens);

	public List<Configuration.KnownDirectories> KnownDirectories { get; } = new()
	{
		Configuration.KnownDirectories.LibationFiles,
		Configuration.KnownDirectories.MyMusic,
		Configuration.KnownDirectories.MyDocs,
		Configuration.KnownDirectories.AppDir,
		Configuration.KnownDirectories.UserProfile
	};

	public string BooksText { get; } = Configuration.GetDescription(nameof(Configuration.Books));
	public string SavePodcastsToParentFolderText { get; } = Configuration.GetDescription(nameof(Configuration.SavePodcastsToParentFolder));
	public string OverwriteExistingText { get; } = Configuration.GetDescription(nameof(Configuration.OverwriteExisting));
	public string CreationTimeText { get; } = Configuration.GetDescription(nameof(Configuration.CreationTime));
	public string LastWriteTimeText { get; } = Configuration.GetDescription(nameof(Configuration.LastWriteTime));
	public EnumDisplay<Configuration.DateTimeSource>[] DateTimeSources { get; }
		= Enum.GetValues<Configuration.DateTimeSource>()
		.Select(v => new EnumDisplay<Configuration.DateTimeSource>(v))
		.ToArray();

	public string UseWebViewText { get; } = Configuration.GetDescription(nameof(Configuration.UseWebView));
	/// <summary>When true, the Use WebView setting is disabled (e.g. when running in Linux Snap to avoid portal/sandbox crashes).</summary>
	public bool UseWebViewSettingDisabled => Configuration.IsRunningUnderSnap;
	public string UseWebViewSnapMessage { get; } = Configuration.IsRunningUnderSnap ? "Disabled when running in Linux Snap (avoids login crash). Use external browser instead." : "";
	public Serilog.Events.LogEventLevel[] LoggingLevels { get; } = Enum.GetValues<Serilog.Events.LogEventLevel>();
	public string GridScaleFactorText { get; } = Configuration.GetDescription(nameof(Configuration.GridScaleFactor));
	public string GridFontScaleFactorText { get; } = Configuration.GetDescription(nameof(Configuration.GridFontScaleFactor));
	public string BetaOptInText { get; } = Configuration.GetDescription(nameof(Configuration.BetaOptIn));
	public EnumDisplay<Configuration.Theme>[] Themes { get; }
		= Enum.GetValues<Configuration.Theme>()
		.Select(v => new EnumDisplay<Configuration.Theme>(v))
		.ToArray();

	public string EncryptTokensText { get; } = TokenStorageSettingsUi.CheckboxText;
	public string PlaintextWarningText { get; } = TokenStorageSettingsUi.PlaintextWarningText;
	public string ConvertButtonText { get; } = TokenStorageSettingsUi.ConvertButtonText;
	public string ConvertButtonToolTip { get; } = TokenStorageSettingsUi.ConvertButtonToolTip;
	public bool CanEditEncryptTokens { get; }

	public string BooksDirectory { get; set; }
	public bool SavePodcastsToParentFolder { get; set; }
	public bool OverwriteExisting { get; set; }
	public float GridScaleFactor { get; set; }
	public float GridFontScaleFactor { get; set; }
	public EnumDisplay<Configuration.DateTimeSource> CreationTime { get; set; }
	public EnumDisplay<Configuration.DateTimeSource> LastWriteTime { get; set; }
	public bool UseWebView { get; set; }
	public Serilog.Events.LogEventLevel LoggingLevel { get; set; }

	public bool EncryptTokens
	{
		get => encryptTokens;
		set
		{
			this.RaiseAndSetIfChanged(ref encryptTokens, value);
			RefreshTokenStorageUiState();
		}
	}

	public bool PlaintextWarningVisible
	{
		get => plaintextWarningVisible;
		private set => this.RaiseAndSetIfChanged(ref plaintextWarningVisible, value);
	}

	public bool ConvertButtonEnabled
	{
		get => convertButtonEnabled;
		private set => this.RaiseAndSetIfChanged(ref convertButtonEnabled, value);
	}

	public bool OsStoreUnavailableVisible
	{
		get => osStoreUnavailableVisible;
		private set => this.RaiseAndSetIfChanged(ref osStoreUnavailableVisible, value);
	}

	public string OsStoreUnavailableMessage
	{
		get => osStoreUnavailableMessage;
		private set => this.RaiseAndSetIfChanged(ref osStoreUnavailableMessage, value);
	}

	public ICommand ConvertExistingTokensCommand { get; }

	public EnumDisplay<Configuration.Theme> ThemeVariant
	{
		get => themeVariant;
		set => this.RaiseAndSetIfChanged(ref themeVariant, value);
	}
}
