using System;
using System.Collections;
using System.Collections.Generic;
using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using System.Globalization;
using System.Linq;
using System.Reflection;
using AudibleApi;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

/// <summary>
/// Settings and enum text is written in [Description] attributes and copied into Strings.resx for translators.
/// When one of these fails, update the "Setting_" or "Enum_" entry in Strings.resx to match.
/// </summary>
[TestClass]
public class SettingsTextTests
{
	private static readonly Type[] EnumsShownInTheUi =
	[
		typeof(Configuration.BadBookAction),
		typeof(Configuration.DailyLimitScope),
		typeof(Configuration.DailyLimitUnit),
		typeof(Configuration.DateTimeSource),
		typeof(Configuration.DownloadQuality),
		typeof(Configuration.KnownDirectories),
		typeof(Configuration.Theme),
	];

	private static Dictionary<string, string> EnglishEntries(string prefix)
		=> Strings.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)!
			.Cast<DictionaryEntry>()
			.Where(e => ((string)e.Key).StartsWith(prefix, StringComparison.Ordinal))
			.ToDictionary(e => (string)e.Key, e => (string)e.Value!);

	private static Dictionary<string, string> ExpectedSettings()
		=> typeof(Configuration).GetProperties()
			.Where(p => p.GetCustomAttribute<DescriptionAttribute>() is not null)
			.ToDictionary(p => $"Setting_{p.Name}", p => p.GetCustomAttribute<DescriptionAttribute>()!.Description);

	private static Dictionary<string, string> ExpectedEnums()
		=> EnumsShownInTheUi
			.SelectMany(t => Enum.GetValues(t).Cast<Enum>()
				.Where(v => !v.Equals(Configuration.KnownDirectories.None))
				.Select(v => (Key: $"Enum_{t.Name}_{v}", English: v.GetDescription() ?? v.ToString())))
			.Concat(DeviceRegistrationProfile.AllProfiles.Select(p => (Key: $"Enum_DeviceRegistrationKind_{p.Kind}", English: p.Description)))
			.ToDictionary(e => e.Key, e => e.English);

	[TestMethod]
	public void Every_described_setting_has_the_same_english_in_strings()
		=> CollectionAssert.AreEquivalent(ExpectedSettings(), EnglishEntries("Setting_"));

	[TestMethod]
	public void Every_enum_value_shown_in_the_ui_has_the_same_english_in_strings()
		=> CollectionAssert.AreEquivalent(ExpectedEnums(), EnglishEntries("Enum_"));

	[TestMethod]
	public void English_labels_read_the_same_as_their_attributes()
	{
		Assert.AreEqual(Configuration.GetDescription(nameof(Configuration.Books)), SettingsText.Label(nameof(Configuration.Books)));
		Assert.AreEqual("Light", SettingsText.Of(Configuration.Theme.Light));
		Assert.AreEqual("Ask each time what action to take.", SettingsText.Of(Configuration.BadBookAction.Ask));
	}
}
