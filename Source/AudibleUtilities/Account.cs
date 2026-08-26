using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.Security;
using LibationFileManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AudibleUtilities;

[DebuggerDisplay("{AccountId,nq} - {Locale}")]
public class Account : IUpdatable, ILogMasked
{
	public event EventHandler? Updated;
	private void update(object? sender = null, EventArgs? e = null)
		=> Updated?.Invoke(this, EventArgs.Empty);

	// canonical. immutable. email or phone number
	public string AccountId { get; }

	// user-friendly, non-canonical name. mutable
	public string? AccountName
	{
		get => field;
		set
		{
			if (string.IsNullOrWhiteSpace(value))
				return;
			var v = value.Trim();
			if (v == field)
				return;
			field = v;
			update();
		}
	}

	// whether to include this account when scanning libraries.
	// technically this is an app setting; not an attribute of account. but since it's managed with accounts, it makes sense to put this exception-to-the-rule here
	public bool LibraryScan
	{
		get => field;
		set
		{
			if (value == field)
				return;
			field = value;
			update();
		}
	}

	/// <summary>
	/// aka: activation bytes. A <see cref="SecretString"/> so that no reflective dump - Serilog's structured
	/// logging, or Serilog.Exceptions walking a logged exception - can reach the value. Persists as the same
	/// bare JSON string it always did.
	/// </summary>
	public SecretString DecryptKey
	{
		get => field;
		set
		{
			var v = (value.Reveal() ?? "").Trim();
			if (v == field.Reveal())
				return;
			field = v;
			update();
		}
	}

	public Identity? IdentityTokens
	{
		get => field;
		set
		{
			if (field is null && value is null)
				return;

			if (field is not null)
				field.Updated -= update;

			if (value is not null)
				value.Updated += update;

			field = value;
			update();
		}
	}

	/// <summary>
	/// The marketplace this account is registered with: where it logs in, and the only place its tokens can be
	/// refreshed. Every account has exactly one.
	/// </summary>
	[JsonIgnore]
	public Locale? Locale => IdentityTokens?.Locale;

	private readonly List<string> _additionalLocaleNames = new();

	/// <summary>
	/// <para>
	/// Further marketplaces this same login holds a library in, beyond <see cref="Locale"/>. A title bought while
	/// an Amazon address was temporarily set to another country stays in that country's library forever, and only
	/// a scan of that marketplace will ever see it.
	/// </para>
	/// <para>
	/// Only marketplace names live here - no credentials. Audible honors one device registration across every
	/// marketplace, so these are read with the very tokens <see cref="Locale"/> registered.
	/// </para>
	/// <para>
	/// Written as null rather than <c>[]</c> when empty, so that the settings file of an account with one
	/// marketplace - which is nearly all of them - is exactly what it was before this property existed.
	/// </para>
	/// </summary>
	[JsonProperty(PropertyName = "AdditionalLocaleNames", NullValueHandling = NullValueHandling.Ignore)]
	private List<string>? _additionalLocaleNames_json
	{
		get => _additionalLocaleNames.Count == 0 ? null : _additionalLocaleNames;
		// 'set' is only used by json deser
		set
		{
			if (value is null)
				return;

			_additionalLocaleNames.Clear();
			foreach (var name in value.Select(canonicalize).OfType<string>())
				if (!_additionalLocaleNames.Contains(name) && name != Locale?.Name)
					_additionalLocaleNames.Add(name);
		}
	}

	/// <summary>
	/// The additional marketplaces, resolved. Names that no longer match a known locale are dropped, as is the
	/// registered marketplace: json is applied in document order, so a file listing these before its
	/// IdentityTokens would slip a duplicate past the check on the way in, and this marketplace would then be
	/// scanned twice.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<Locale> AdditionalLocales
		=> _additionalLocaleNames
		.Select(Localization.Get)
		.Where(l => !string.IsNullOrEmpty(l.CountryCode) && l.Name != Locale?.Name)
		.ToList();

	/// <summary>
	/// Every marketplace a scan of this account should read: its own, then any extras. One account is scanned as
	/// a unit, so that a title found under one marketplace is never counted absent because another was scanned.
	/// </summary>
	[JsonIgnore]
	public IReadOnlyList<Locale> ScanLocales
		=> Locale is null ? AdditionalLocales : new[] { Locale }.Concat(AdditionalLocales).ToList();

	/// <summary>True if <paramref name="localeName"/> is this account's own marketplace or one of its extras.</summary>
	public bool HasMarketplace(string? localeName)
		=> canonicalize(localeName) is string name
		&& (name == Locale?.Name || _additionalLocaleNames.Contains(name));

	/// <summary>Adds an extra marketplace. No-op if it is already this account's own, or already added.</summary>
	public bool AddMarketplace(string? localeName)
	{
		if (canonicalize(localeName) is not string name || HasMarketplace(name))
			return false;

		_additionalLocaleNames.Add(name);
		update();
		return true;
	}

	public bool RemoveMarketplace(string? localeName)
	{
		if (canonicalize(localeName) is not string name || !_additionalLocaleNames.Remove(name))
			return false;

		update();
		return true;
	}

	/// <summary>Replaces the extra marketplaces wholesale. Used by the accounts dialog, which edits a copy.</summary>
	public void SetAdditionalMarketplaces(IEnumerable<string?> localeNames)
	{
		var replacement = (localeNames ?? [])
			.Select(canonicalize)
			.OfType<string>()
			.Where(n => n != Locale?.Name)
			.Distinct()
			.ToList();

		if (replacement.SequenceEqual(_additionalLocaleNames))
			return;

		_additionalLocaleNames.Clear();
		_additionalLocaleNames.AddRange(replacement);
		update();
	}

	/// <summary>
	/// Store the locale's canonical name, so that a country code ('de') and an internal name ('germany') cannot
	/// end up in the list as two separate marketplaces.
	/// </summary>
	private static string? canonicalize(string? localeName)
	{
		var locale = Localization.Get(localeName);
		return string.IsNullOrEmpty(locale.CountryCode) ? null : locale.Name;
	}

	public Account(string accountId)
	{
		AccountId = ArgumentValidator.EnsureNotNullOrWhiteSpace(accountId, nameof(accountId)).Trim();
	}

	/// <summary>
	/// Masked, because this is what interpolation and non-destructured logging reach for. Use
	/// <see cref="AccountCredentialStatus.FormatAccountLabel"/> for dialogs shown to the account's owner, and see
	/// the DebuggerDisplay above for the unmasked form while debugging.
	/// </summary>
	public override string ToString() => MaskedLogEntry;

	/// <summary>Derived from the fields above, so persisting it would only add a stale copy of them.</summary>
	[JsonIgnore]
	public string MaskedLogEntry => @$"AccountId={mask(AccountId)}|AccountName={mask(AccountName)}|Locale={Locale?.Name ?? "[empty]"}";
	private static string mask(string? str)
		=> str is null ? "[null]"
		: str == string.Empty ? "[empty]"
		: str.ToMask();
}
