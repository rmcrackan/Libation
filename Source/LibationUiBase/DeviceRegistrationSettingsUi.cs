using AudibleApi;
using System.Linq;

namespace LibationUiBase;

/// <summary>Shared copy for the experimental device-registration setting (Avalonia and WinForms).</summary>
public static class DeviceRegistrationSettingsUi
{
	public static EnumDisplay<DeviceRegistrationKind>[] Options { get; } =
	[
		new(DeviceRegistrationKind.CurrentAndroid, "Android emulator (default)"),
		new(DeviceRegistrationKind.RetailAndroid, "Android Pixel (experimental)"),
		new(DeviceRegistrationKind.Mkb79IPhone, "iPhone / audible-cli (experimental; no Widevine)"),
	];

	public static string SettingLabel { get; } = "Device registration (experimental)";

	public static string ReLoginNote { get; }
		= "Changing this does not convert existing accounts. Remove and re-add the account (or run login-external) to register again.";

	public static string ThrottlingWorkaround { get; }
		= "If the official Audible app can play this title, try Settings: pick an experimental device registration, then remove and re-add the account. You can also import credentials from audible-cli.";

	public static EnumDisplay<DeviceRegistrationKind> Display(DeviceRegistrationKind kind)
		=> Options.FirstOrDefault(o => o.Value.Equals(kind)) ?? Options[0];
}
