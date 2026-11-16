using System;
using System.Globalization;
using System.IO;
using System.Linq;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>A choice in the language picker. An empty <see cref="Name"/> follows the system language.</summary>
public record UiLanguageOption(string Name, string Display)
{
	public override string ToString() => Display;
}

/// <summary>The languages the UI can show, and applying the chosen one at startup.</summary>
public static class UiLanguage
{
	/// <summary>
	/// Sets the UI language for the whole process. Call once at startup, before any window opens: text read into
	/// static fields keeps the language it was read in, so a change takes effect on the next start.
	/// An empty or unknown name keeps the system language. Number and date formats always follow the system.
	/// </summary>
	public static void Apply(string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return;

		CultureInfo culture;
		try
		{
			culture = CultureInfo.GetCultureInfo(name);
		}
		catch (CultureNotFoundException)
		{
			return;
		}

		CultureInfo.DefaultThreadCurrentUICulture = culture;
		CultureInfo.CurrentUICulture = culture;
	}

	/// <summary>"System default", English, then every language with a translation shipped next to the app.</summary>
	public static UiLanguageOption[] Options()
	{
		var translated = Directory.EnumerateDirectories(AppContext.BaseDirectory)
			.Where(dir => File.Exists(Path.Combine(dir, "LibationUiBase.resources.dll")))
			.Select(dir => TryGetCulture(Path.GetFileName(dir)))
			.OfType<CultureInfo>();

		return
		[
			new("", Strings.UiLanguage_SystemDefault),
			.. new[] { CultureInfo.GetCultureInfo("en") }
				.Concat(translated)
				.OrderBy(c => c.NativeName, StringComparer.CurrentCultureIgnoreCase)
				.Select(c => new UiLanguageOption(c.Name, c.NativeName))
		];
	}

	private static CultureInfo? TryGetCulture(string name)
	{
		try
		{
			return CultureInfo.GetCultureInfo(name);
		}
		catch (CultureNotFoundException)
		{
			return null;
		}
	}
}
