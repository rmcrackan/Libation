using AudibleApi;
using LibationFileManager;
using System;
using System.Collections.Generic;

namespace AudibleUtilities;

public record ThrottledRecoveryResult(
	bool WidevineEnabled,
	bool DeviceRegistrationUpdated,
	IReadOnlyList<Account> MarkedAccounts);

/// <summary>
/// Recovers accounts from Audible download throttling caused by legacy device registrations
/// or missing Widevine configuration by switching settings to Android + Widevine and marking
/// accounts for a fresh device registration without deleting their library or account configuration.
/// </summary>
public static class ThrottledDeviceRecovery
{
	/// <summary>
	/// Returns true when a download denial indicates throttling under a legacy registration or non-Widevine profile.
	/// </summary>
	public static bool ShouldRecover(ContentLicenseDeniedException ex, Account? account, Configuration config)
	{
		ArgumentNullException.ThrowIfNull(ex);
		ArgumentNullException.ThrowIfNull(config);

		if (!ex.IsCustomerThrottled)
			return false;

		return !config.UseWidevine
			|| config.DeviceRegistrationKind != DeviceRegistrationKind.CurrentAndroid
			|| (account?.HasObsoleteDeviceRegistration() == true);
	}

	/// <summary>
	/// Thunks settings to CurrentAndroid and Widevine, and marks affected accounts as requiring re-registration.
	/// Note: When Widevine is enabled, ALL accounts require re-registration regardless of whether their device type was already Android.
	/// </summary>
	public static ThrottledRecoveryResult ApplyRecovery(
		AccountsSettings accountsSettings,
		Configuration config,
		Account? throttledAccount = null)
	{
		ArgumentNullException.ThrowIfNull(accountsSettings);
		ArgumentNullException.ThrowIfNull(config);

		bool widevineWasDisabled = !config.UseWidevine;
		bool deviceKindWasNotCurrentAndroid = config.DeviceRegistrationKind != DeviceRegistrationKind.CurrentAndroid;

		// Thunk global settings
		config.UseWidevine = true;
		config.DeviceRegistrationKind = DeviceRegistrationKind.CurrentAndroid;

		var markedAccounts = new List<Account>();

		if (throttledAccount is not null)
		{
			throttledAccount.RequiresReRegistration = true;
			markedAccounts.Add(throttledAccount);
		}

		foreach (var account in accountsSettings.GetAll())
		{
			if (account == throttledAccount)
				continue;

			// If Widevine was newly enabled, every account needs a relogin.
			// If Widevine was already enabled, any account with an obsolete registration needs a relogin.
			if (widevineWasDisabled || account.HasObsoleteDeviceRegistration())
			{
				account.RequiresReRegistration = true;
				markedAccounts.Add(account);
			}
		}

		return new ThrottledRecoveryResult(
			WidevineEnabled: widevineWasDisabled,
			DeviceRegistrationUpdated: deviceKindWasNotCurrentAndroid,
			MarkedAccounts: markedAccounts);
	}

	public const string WidevinePromptTitle = "Widevine Re-registration Required";
	public const string WidevinePromptMessage =
		"In order to enable Widevine content, Libation will need to log into your accounts again with a fresh device registration.\r\n\r\n" +
		"Do you want Libation to mark your accounts to prompt for login before the next download?\r\n\r\n" +
		"(All of your account settings and marketplaces will be preserved.)";

	/// <summary>
	/// Marks all accounts as requiring re-registration when Widevine is enabled, and sets DeviceRegistrationKind to CurrentAndroid.
	/// </summary>
	public static void MarkAllAccountsForWidevine(AccountsSettings accountsSettings, Configuration config)
	{
		ArgumentNullException.ThrowIfNull(accountsSettings);
		ArgumentNullException.ThrowIfNull(config);

		config.DeviceRegistrationKind = DeviceRegistrationKind.CurrentAndroid;
		foreach (var account in accountsSettings.GetAll())
		{
			account.RequiresReRegistration = true;
		}
	}

	/// <summary>
	/// Marks any accounts with obsolete device registrations as requiring re-registration.
	/// </summary>
	public static int MarkObsoleteAccounts(AccountsSettings accountsSettings)
	{
		ArgumentNullException.ThrowIfNull(accountsSettings);

		int markedCount = 0;
		foreach (var account in accountsSettings.GetAll())
		{
			if (account.HasObsoleteDeviceRegistration())
			{
				account.RequiresReRegistration = true;
				markedCount++;
			}
		}
		return markedCount;
	}
}
