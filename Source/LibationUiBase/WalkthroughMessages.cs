using AppScaffolding;
using System.Collections.Generic;

namespace LibationUiBase;

/// <summary>UI-agnostic title and body for a walkthrough MessageBox.</summary>
public readonly record struct WalkthroughMessage(string Title, string Message);

/// <summary>
/// Shared guided-tour copy for WinForms and Avalonia.
/// Settings tab bodies are keyed by each UI's tab header text so screen-reader tips stay on the correct tab.
/// </summary>
public static class WalkthroughMessages
{
	private const string ScreenReaderTip
		= """For best use with screen readers, uncheck "Use Libation's built-in web browser to log into Audible?".""";

	private const string ImportantSettingsBody
		= "From here you can change where liberated books are stored and how detailed Libation's logs are.\r\n\r\nIf you experience a problem and need help, you'll be asked to provide your log file. In certain circumstances we may need you to reproduce the error with a higher level of logging detail.";

	private const string ImportLibraryBody
		= "In this tab you can change how your library is scanned and imported into Libation, as well as automatic liberation.";

	private static readonly string DownloadDecryptBody
		= "These settings allow you to control how liberated files and folders are named and stored.\r\nYou can customize the 'Naming Templates' to use any number of the audiobook's properties to build a customized file and folder naming format. Learn more about the syntax from the wiki at\r\n\r\n"
		+ LibationScaffolding.NamingTemplatesDocUrl;

	private const string AudioFileBody
		= "Control how audio files are decrypted, including audio format and metadata handling.\r\n\r\nIf you choose to split your audiobook into multiple files by chapter marker, you may edit the chapter file 'Naming Template' to control how each chapter file is named.";

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

		// Avalonia
		["Important Settings"] = ImportantSettingsBody + "\r\n\r\n" + ScreenReaderTip,
		["Import Library"] = ImportLibraryBody,
		["Audio File Settings"] = AudioFileBody,
	};

	public static WalkthroughMessage AddAccountsProceed { get; }
		= new("Add Accounts", "First, add your Audible account(s).");

	public static WalkthroughMessage AddAccountOnDialog { get; }
		= new(
			"Add an Account",
			"Add your Audible account(s), then save.\r\n\r\nMost accounts use email and a normal region. If yours is a pre-Amazon username login, choose a pre-amazon locale and enter that username.");

	public static WalkthroughMessage ChangeSettingsProceed { get; }
		= new("Change Settings", "Next, adjust Libation's settings");

	public static WalkthroughMessage NoAccountsYet { get; }
		= new(
			"Add an Audible Account",
			"Add an Audible account, then sync your library through the 'Import' menu.");

	public static WalkthroughMessage SearchingProceed { get; }
		= new("Searching", "You can filter the grid entries by searching");

	public static WalkthroughMessage SearchCheatSheet { get; }
		= new("Search Cheat Sheet", "Libation provides a built-in cheat sheet for its query language");

	public static WalkthroughMessage QuickFiltersProceed { get; }
		= new("Quick Filters", "Queries that you perform regularly can be added to 'Quick Filters'");

	public static WalkthroughMessage EditQuickFilters { get; }
		= new("Editing Quick Filters", "From here you can edit, delete, and change the order of Quick Filters");

	public static WalkthroughMessage TourFinished { get; }
		= new(
			"Tour Finished",
			"You're now ready to begin using Libation.\r\n\r\nOne thing worth knowing: Audible keeps a separate library for each marketplace. If you have ever changed your Amazon address to another country to buy a title, it lives in that country's library and a normal scan will not find it. Settings > Accounts > Marketplaces checks the other marketplaces for you.\r\n\r\nEnjoy!");

	public static WalkthroughMessage ScanProceed(int accountCount)
	{
		var accounts = accountCount > 1 ? "accounts" : "account";
		var library = accountCount > 1 ? "libraries" : "library";
		return new(
			$"Scan {accounts}",
			$"Finally, scan your Audible {accounts} to sync your {library} with Libation.\r\n\r\nIf this is your first time scanning an account, you'll be prompted to enter your account's password to log into your Audible account.");
	}

	public static bool TryGetSettingsTab(string tabHeader, out WalkthroughMessage message)
	{
		if (!SettingsTabBodies.TryGetValue(tabHeader, out var body))
		{
			message = default;
			return false;
		}

		message = new(tabHeader + " Tab", body);
		return true;
	}
}
