using AudibleApi;
using System.Linq;

namespace LibationUiBase;

/// <summary>Shared copy for the experimental device-registration setting (Avalonia and WinForms).</summary>
public static class DeviceRegistrationSettingsUi
{
	public static EnumDisplay<DeviceRegistrationKind>[] Options { get; } =
		DeviceRegistrationProfile.AllProfiles.Select(p => new EnumDisplay<DeviceRegistrationKind>(p.Kind, p.Description)).ToArray();

	public static string SettingLabel { get; } = "Device registration (experimental)";

	public static string ReLoginNote { get; }
		= "Changing this does not convert existing accounts. Remove the account, save or close the Accounts dialog, then re-add the account (or run login-external) to register again.";

	/// <summary>
	/// Steps that actually persist a fresh device registration. Removing alone is not enough if the
	/// Accounts dialog is still open with the removal uncommitted.
	/// </summary>
	public static string RemoveSaveReAddAccountSteps { get; }
		= "Remove the account, save or close the Accounts dialog, then re-add the account.";

	public static EnumDisplay<DeviceRegistrationKind> Display(DeviceRegistrationKind kind)
		=> Options.FirstOrDefault(o => o.Value.Equals(kind)) ?? Options[0];
}
