using AppScaffolding;
using System.Collections.Generic;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>UI-agnostic title and body for a walkthrough MessageBox.</summary>
public readonly record struct WalkthroughMessage(string Title, string Message);

/// <summary>
/// Shared guided-tour copy for WinForms and Avalonia.
/// Settings tab bodies are keyed by each UI's tab header text so screen-reader tips stay on the correct tab.
/// </summary>
public static class WalkthroughMessages
{
	private static string ScreenReaderTip
		=> Strings.WalkthroughMessages_ForBestUseWithScreenReaders;

	private static string ImportantSettingsBody
		=> Strings.WalkthroughMessages_FromHereYouCanChangeWhere;

	private static string ImportLibraryBody
		=> Strings.WalkthroughMessages_InThisTabYouCanChange;

	private static readonly string DownloadDecryptBody
		= string.Format(Strings.WalkthroughMessages_TheseSettingsAllowYouToControl, LibationScaffolding.NamingTemplatesDocUrl);

	private static string AudioFileBody
		=> Strings.WalkthroughMessages_ControlHowAudioFilesAreDecrypted;

	/// <summary>
	/// Tab header text (WinForms <c>TabPage.Text</c> / Avalonia header) -> message body.
	/// Screen-reader tip stays on Import for Classic and Important Settings for Chardonnay.
	/// </summary>
	private static readonly Dictionary<string, string> SettingsTabBodies = new()
	{
		// WinForms
		["Important settings"] = ImportantSettingsBody,
		["Import library"] = ImportLibraryBody + "\r\n\r\n" + ScreenReaderTip,
		["Download/Decrypt"] = DownloadDecryptBody,
		["Audio File Options"] = AudioFileBody,

		// Avalonia. Its headers are translated, so look them up by the same resources.
		[Strings.SettingsDialog_ImportantSettings] = ImportantSettingsBody + "\r\n\r\n" + ScreenReaderTip,
		[Strings.SettingsDialog_ImportLibrary] = ImportLibraryBody,
		[Strings.SettingsDialog_DownloadDecrypt] = DownloadDecryptBody,
		[Strings.SettingsDialog_AudioFileSettings] = AudioFileBody,
	};

	public static WalkthroughMessage AddAccountsProceed { get; }
		= new(Strings.WalkthroughMessages_AddAccounts, Strings.WalkthroughMessages_FirstAddYourAudibleAccountS);

	public static WalkthroughMessage AddAccountOnDialog { get; }
		= new(
			Strings.WalkthroughMessages_AddAnAccount,
			Strings.WalkthroughMessages_AddYourAudibleAccountSThen);

	public static WalkthroughMessage ChangeSettingsProceed { get; }
		= new(Strings.WalkthroughMessages_ChangeSettings, Strings.WalkthroughMessages_NextAdjustLibationSSettings);

	public static WalkthroughMessage NoAccountsYet { get; }
		= new(
			Strings.WalkthroughMessages_AddAnAudibleAccount,
			Strings.WalkthroughMessages_AddAnAudibleAccountThenSync);

	public static WalkthroughMessage SearchingProceed { get; }
		= new(Strings.WalkthroughMessages_Searching, Strings.WalkthroughMessages_YouCanFilterTheGridEntries);

	public static WalkthroughMessage SearchCheatSheet { get; }
		= new(Strings.WalkthroughMessages_SearchCheatSheet, Strings.WalkthroughMessages_LibationProvidesABuiltInCheat);

	public static WalkthroughMessage QuickFiltersProceed { get; }
		= new(Strings.WalkthroughMessages_QuickFilters, Strings.WalkthroughMessages_QueriesThatYouPerformRegularlyCan);

	public static WalkthroughMessage EditQuickFilters { get; }
		= new(Strings.WalkthroughMessages_EditingQuickFilters, Strings.WalkthroughMessages_FromHereYouCanEditDelete);

	public static WalkthroughMessage TourFinished { get; }
		= new(
			Strings.WalkthroughMessages_TourFinished,
			Strings.WalkthroughMessages_YouReNowReadyToBegin);

	public static WalkthroughMessage ScanProceed(int accountCount)
	{
		var accounts = accountCount > 1 ? "accounts" : "account";
		var library = accountCount > 1 ? "libraries" : "library";
		return new(
			string.Format(Strings.WalkthroughMessages_Scan, accounts),
			string.Format(Strings.WalkthroughMessages_FinallyScanYourAudibleToSync, accounts, library));
	}

	public static bool TryGetSettingsTab(string tabHeader, out WalkthroughMessage message)
	{
		if (!SettingsTabBodies.TryGetValue(tabHeader, out var body))
		{
			message = default;
			return false;
		}

		message = new(string.Format(Strings.WalkthroughMessages_Tab, tabHeader), body);
		return true;
	}
}
