using AudibleApi;
using AudibleUtilities;
using CommandLine;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LibationCli;

[Verb("login-external", HelpText = "Sign in with Audible using an external browser: open the printed URL, then paste the final URL from the address bar.")]
internal class LoginExternalOptions : OptionsBase
{
	[Option('a', "account", Required = true, HelpText = "Audible login id (email) for this account.")]
	public string? AccountId { get; set; }

	[Option('l', "locale", Required = true, HelpText = "Audible marketplace / locale name or country code (e.g. us, uk, de, germany).")]
	public string? Locale { get; set; }

	[Option("response-url", Required = false, HelpText = "Final browser URL after login. Use when stdin is not a TTY (e.g. scripts, Docker).")]
	public string? ResponseUrl { get; set; }

	protected override async Task ProcessAsync()
	{
		var accountId = AccountId?.Trim();
		var localeInput = Locale?.Trim();
		if (string.IsNullOrEmpty(accountId) || string.IsNullOrEmpty(localeInput))
		{
			PrintVerbUsage("ERROR", "=====", "Both --account and --locale are required.");
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		var locale = ResolveLocale(localeInput);
		if (IsEmptyLocale(locale))
		{
			var known = string.Join(", ",
				Localization.Locales
					.Where(l => !l.WithUsername)
					.Select(l => $"{l.CountryCode} ({l.Name})"));
			PrintVerbUsage(
				"ERROR",
				"=====",
				$"Unknown locale '{localeInput}'. Use a country code or locale name, for example: {known}");
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		// Persist by canonical locale name ("germany"), not the user input ("de").
		var account = persister.AccountsSettings.Upsert(accountId, locale.Name);

		if (account.IdentityTokens?.IsValid == true)
		{
			Console.WriteLine(
				$"Account '{accountId}' ({locale.Name}) is already authenticated. No browser login needed.");
			return;
		}

		var presetResponse = ResponseUrl?.Trim();
		if (string.IsNullOrEmpty(presetResponse) && Console.IsInputRedirected)
		{
			Console.Error.WriteLine(
				"Standard input is redirected. Provide the post-login URL with --response-url \"...\".");
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		var loginExternal = new CliLoginExternal(presetResponse);
		try
		{
			_ = await EzApiCreator.GetApiAsync(
				loginExternal,
				locale,
				AudibleApiStorage.AccountsSettingsFile,
				account.GetIdentityTokensJsonPath());
		}
		catch (Exception ex)
		{
			PrintVerbUsage("ERROR", "=====", ex.Message, "", ex.ToString());
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		Console.WriteLine($"Successfully authenticated account '{accountId}' ({locale.Name}).");
	}

	/// <summary>
	/// Resolve by locale name or country code. Prefers modern (non-pre-amazon) locales for country codes.
	/// Works with older AudibleApi builds where <see cref="Localization.Get"/> only matched names.
	/// </summary>
	internal static AudibleApi.Locale ResolveLocale(string localeInput)
	{
		var fromGet = Localization.Get(localeInput);
		if (!IsEmptyLocale(fromGet))
			return fromGet;

		return Localization.Locales
			.Where(l => l.CountryCode.Equals(localeInput, StringComparison.OrdinalIgnoreCase)
				|| l.Name.Equals(localeInput, StringComparison.OrdinalIgnoreCase))
			.OrderBy(l => l.WithUsername)
			.FirstOrDefault()
			?? AudibleApi.Locale.Empty;
	}

	internal static bool IsEmptyLocale(AudibleApi.Locale locale)
		=> string.IsNullOrEmpty(locale.CountryCode);

	private sealed class CliLoginExternal : ILoginExternal
	{
		private readonly string? _presetResponseUrl;

		public CliLoginExternal(string? presetResponseUrl) => _presetResponseUrl = presetResponseUrl;

		public string DeviceName => "Libation";

		public string GetResponseUrl(string loginUrl, CookieCollection signInCookies)
		{
			if (!string.IsNullOrEmpty(_presetResponseUrl))
				return ValidateResponseUrl(_presetResponseUrl);

			Console.WriteLine();
			Console.WriteLine("Open this URL in your web browser and sign in:");
			Console.WriteLine(loginUrl);
			Console.WriteLine();
			Console.WriteLine(
				"After you finish signing in, copy the full URL from your browser's address bar and paste it below.");
			Console.WriteLine("(It is normal if the page says it does not exist.)");
			Console.WriteLine();
			Console.Write("Paste URL: ");

			var line = Console.ReadLine()?.Trim();
			if (string.IsNullOrEmpty(line))
				throw new OperationCanceledException("No response URL was entered.");

			return ValidateResponseUrl(line);
		}

		private static string ValidateResponseUrl(string url)
		{
			if (!Uri.TryCreate(url, UriKind.Absolute, out _))
				throw new ArgumentException("The response URL must be a valid absolute URL.");

			return url;
		}
	}
}
