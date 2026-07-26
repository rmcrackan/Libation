using ApplicationServices;
using AudibleApi.Authorization;
using AudibleUtilities;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibationAvalonia.ViewModels.Settings;

public class AudiobookshelfSettingsVM : ViewModelBase
{
	private readonly Configuration config;
	private bool enabled;
	private string serverUrl = "";
	private string apiToken = "";
	private string statusText = "";
	private bool statusIsError;
	private bool plaintextWarningVisible;
	private List<AudiobookshelfApiService.Library> libraries = [];
	private int selectedLibraryIndex = -1;
	private int selectedFolderIndex = -1;
	private bool isConnecting;
	private ObservableCollection<string> libraryNames = new();
	private ObservableCollection<string> folderNames = new();

	public AudiobookshelfSettingsVM(Configuration config)
	{
		this.config = config;
		enabled = config.AudiobookshelfEnabled;
		serverUrl = config.AudiobookshelfServerUrl ?? "";
		apiToken = AudiobookshelfTokenStorage.DecryptToken(config.AudiobookshelfApiToken) ?? "";

		var osSecretStoreAvailable = IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out _);
		PlaintextWarningVisible = osSecretStoreAvailable && config.TokenStorageMethod == TokenStorageMethod.Plaintext;

		ConnectCommand = ReactiveCommand.CreateFromTask(ConnectAsync);
		_ = RestoreLibrariesAsync();
	}

	public bool Enabled
	{
		get => enabled;
		set => this.RaiseAndSetIfChanged(ref enabled, value);
	}

	public string ServerUrl
	{
		get => serverUrl;
		set => this.RaiseAndSetIfChanged(ref serverUrl, value);
	}

	public string ApiToken
	{
		get => apiToken;
		set => this.RaiseAndSetIfChanged(ref apiToken, value);
	}

	public string StatusText
	{
		get => statusText;
		set => this.RaiseAndSetIfChanged(ref statusText, value);
	}

	public bool StatusIsError
	{
		get => statusIsError;
		set => this.RaiseAndSetIfChanged(ref statusIsError, value);
	}

	public bool PlaintextWarningVisible
	{
		get => plaintextWarningVisible;
		private set => this.RaiseAndSetIfChanged(ref plaintextWarningVisible, value);
	}

	public List<AudiobookshelfApiService.Library> Libraries
	{
		get => libraries;
		set
		{
			this.RaiseAndSetIfChanged(ref libraries, value);
			UpdateLibraryNames();
		}
	}

	public ObservableCollection<string> LibraryNames
	{
		get => libraryNames;
		set => this.RaiseAndSetIfChanged(ref libraryNames, value);
	}

	public int SelectedLibraryIndex
	{
		get => selectedLibraryIndex;
		set
		{
			this.RaiseAndSetIfChanged(ref selectedLibraryIndex, value);
			UpdateFolders();
		}
	}

	public ObservableCollection<string> FolderNames
	{
		get => folderNames;
		set => this.RaiseAndSetIfChanged(ref folderNames, value);
	}

	public int SelectedFolderIndex
	{
		get => selectedFolderIndex;
		set => this.RaiseAndSetIfChanged(ref selectedFolderIndex, value);
	}

	public bool IsConnecting
	{
		get => isConnecting;
		set => this.RaiseAndSetIfChanged(ref isConnecting, value);
	}

	public bool CanConnect => !IsConnecting;

	public ICommand ConnectCommand { get; }

	// Labels from Configuration descriptions
	public string EnabledText { get; } = Configuration.GetDescription(nameof(Configuration.AudiobookshelfEnabled));
	public string ServerUrlText { get; } = Configuration.GetDescription(nameof(Configuration.AudiobookshelfServerUrl));
	public string ApiTokenText { get; } = Configuration.GetDescription(nameof(Configuration.AudiobookshelfApiToken));
	public string LibraryText { get; } = Configuration.GetDescription(nameof(Configuration.AudiobookshelfLibraryId));
	public string FolderText { get; } = Configuration.GetDescription(nameof(Configuration.AudiobookshelfFolderId));
	public string PlaintextWarningText { get; } = "Warning: The API token is stored as plaintext in Settings.json.";
	public string ConnectButtonText { get; } = "Connect / Refresh";

	private void UpdateLibraryNames()
	{
		LibraryNames.Clear();
		foreach (var lib in Libraries)
			LibraryNames.Add($"{lib.Name} ({lib.Id})");
	}

	private void UpdateFolders()
	{
		FolderNames.Clear();
		SelectedFolderIndex = -1;

		if (SelectedLibraryIndex < 0 || SelectedLibraryIndex >= Libraries.Count)
			return;

		var library = Libraries[SelectedLibraryIndex];
		foreach (var folder in library.Folders)
			FolderNames.Add($"{folder.FullPath} ({folder.Id})");

		if (FolderNames.Count > 0)
			SelectedFolderIndex = 0;
	}

	private async Task ConnectAsync()
	{
		IsConnecting = true;
		StatusText = "Connecting...";
		StatusIsError = false;

		try
		{
			if (string.IsNullOrWhiteSpace(ServerUrl) || string.IsNullOrWhiteSpace(ApiToken))
			{
				StatusText = "Please enter both server URL and API token.";
				StatusIsError = true;
				return;
			}

			var libs = await AudiobookshelfApiService.GetLibrariesAsync(ServerUrl.Trim(), ApiToken.Trim());
			if (libs.Count == 0)
			{
				StatusText = "No book libraries found. Check your settings.";
				StatusIsError = true;
				return;
			}

			Libraries = libs;
			SelectedLibraryIndex = 0;

			StatusText = $"Connected. Found {libs.Count} library(ies).";
			StatusIsError = false;
		}
		catch (System.Exception ex)
		{
			StatusText = $"Connection failed: {ex.Message}";
			StatusIsError = true;
		}
		finally
		{
			IsConnecting = false;
		}
	}

	private async Task RestoreLibrariesAsync()
	{
		try
		{
			if (string.IsNullOrWhiteSpace(config.AudiobookshelfServerUrl) || string.IsNullOrWhiteSpace(config.AudiobookshelfApiToken))
				return;

			var libs = await AudiobookshelfApiService.GetLibrariesAsync(config.AudiobookshelfServerUrl, config.AudiobookshelfApiToken);
			Libraries = libs;

			var savedLibIndex = libs.FindIndex(l => l.Id == config.AudiobookshelfLibraryId);
			if (savedLibIndex >= 0)
			{
				SelectedLibraryIndex = savedLibIndex;
				var library = libs[savedLibIndex];
				var savedFolderIndex = library.Folders.FindIndex(f => f.Id == config.AudiobookshelfFolderId);
				if (savedFolderIndex >= 0)
					SelectedFolderIndex = savedFolderIndex;
			}
		}
		catch
		{
			// Silently fail; user can manually reconnect
		}
	}

	public void SaveSettings(Configuration config)
	{
		config.AudiobookshelfEnabled = Enabled;
		config.AudiobookshelfServerUrl = ServerUrl.Trim();
		config.AudiobookshelfApiToken = AudiobookshelfTokenStorage.EncryptToken(ApiToken.Trim());

		if (SelectedLibraryIndex >= 0 && Libraries.Count > SelectedLibraryIndex)
			config.AudiobookshelfLibraryId = Libraries[SelectedLibraryIndex].Id;
		else
			config.AudiobookshelfLibraryId = null;

		if (SelectedLibraryIndex >= 0 && SelectedFolderIndex >= 0)
		{
			var library = Libraries[SelectedLibraryIndex];
			if (library.Folders.Count > SelectedFolderIndex)
				config.AudiobookshelfFolderId = library.Folders[SelectedFolderIndex].Id;
			else
				config.AudiobookshelfFolderId = null;
		}
		else
			config.AudiobookshelfFolderId = null;
	}
}
