using ApplicationServices;
using AudibleUtilities;
using System.Linq;

namespace LibationUiBase;

/// <summary>Shared non-UI helpers and timing for the Classic and Chardonnay guided tours.</summary>
public static class WalkthroughHelpers
{
	public static class Timing
	{
		public const int BeforeHighlightMs = 750;
		public const int AfterHighlightMs = 500;
		public const int FlashIntervalMs = 200;
		public const int TypeCharDelayMs = 150;
		public const int AfterFilterMs = 1000;
		public const int FlashCount = 3;
	}

	public static string? GetFirstAuthorName()
	{
		var books = DbContexts.GetLibrary_Flat_NoTracking();
		return books.SelectMany(lb => lb.Book.Authors).FirstOrDefault(a => !string.IsNullOrWhiteSpace(a.Name))?.Name;
	}

	public static int GetConfiguredAccountCount()
	{
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		return persister.AccountsSettings.Accounts.Count;
	}
}
