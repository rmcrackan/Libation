using AudibleApi;
using System.Linq;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>Shared copy for the experimental device-registration setting (Avalonia and WinForms).</summary>
public static class DeviceRegistrationSettingsUi
{
	public static EnumDisplay<DeviceRegistrationKind>[] Options { get; } =
		DeviceRegistrationProfile.AllProfiles.Select(p => new EnumDisplay<DeviceRegistrationKind>(p.Kind, SettingsText.Of(p.Kind, p.Description))).ToArray();

	public static string SettingLabel { get; } = Strings.DeviceRegistrationSettingsUi_DeviceRegistrationExperimental;

	public static string ReLoginNote { get; }
		= Strings.DeviceRegistrationSettingsUi_ChangingThisDoesNotConvertExisting;

	/// <summary>
	/// Steps that actually persist a fresh device registration. Removing alone is not enough if the
	/// Accounts dialog is still open with the removal uncommitted.
	/// </summary>
	public static string RemoveSaveReAddAccountSteps { get; }
		= Strings.DeviceRegistrationSettingsUi_RemoveTheAccountSaveOrClose;

	public static EnumDisplay<DeviceRegistrationKind> Display(DeviceRegistrationKind kind)
		=> Options.FirstOrDefault(o => o.Value.Equals(kind)) ?? Options[0];
}
