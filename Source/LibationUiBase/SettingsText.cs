using System;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// Display text for settings and enum values. The English lives in [Description] attributes in LibationFileManager,
/// which can't see this project's resources, so each one has a copy in Strings.resx for translators: "Setting_" plus
/// the Configuration property name, or "Enum_" plus the enum's type and member names. A test keeps the copies equal.
/// </summary>
public static class SettingsText
{
	/// <summary>The label for a <see cref="Configuration"/> property, e.g. <c>SettingsText.Label(nameof(Configuration.Books))</c>.</summary>
	public static string Label(string propertyName)
		=> Translation($"Setting_{propertyName}") ?? Configuration.GetDescription(propertyName);

	/// <summary>The display name of an enum value: its translation, else <paramref name="english"/>, its [Description], or its name.</summary>
	public static string Of<T>(T value, string? english = null) where T : Enum
		=> Translation($"Enum_{typeof(T).Name}_{value}") ?? english ?? value.GetDescription() ?? value.ToString();

	private static string? Translation(string key) => Strings.ResourceManager.GetString(key, Strings.Culture);
}
