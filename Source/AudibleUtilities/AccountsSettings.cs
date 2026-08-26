using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudibleUtilities;

// 'AccountsSettings' is intentionally NOT IEnumerable<> so that properties can be added/extended
// from newtonsoft (https://www.newtonsoft.com/json/help/html/SerializationGuide.htm):
//   .NET :  IList, IEnumerable, IList<T>, Array
//   JSON :  Array (properties on the collection will not be serialized)
public class AccountsSettings : IUpdatable
{
	public event EventHandler? Updated;
	private void update(object? sender = null, EventArgs? e = null)
	{
		foreach (var account in Accounts)
			validate(account);
		update_no_validate();
	}
	private void update_no_validate() => Updated?.Invoke(this, new EventArgs());

	public AccountsSettings() { }

	// for some reason this will make the json instantiator use _accounts_json.set()
	[JsonConstructor]
	protected AccountsSettings(List<Account> accountsSettings) { }

	#region Accounts
	private readonly List<Account> _accounts_backing = new List<Account>();
	[JsonProperty(PropertyName = nameof(Accounts))]
	private List<Account> _accounts_json
	{
		get => _accounts_backing;
		// 'set' is only used by json deser
		set
		{
			if (value is null)
				return;

			foreach (var account in value)
				_add(account);

			update_no_validate();
		}
	}

	private string? _cdm;
	[JsonProperty]
	public string? Cdm
	{
		get => _cdm;
		set
		{
			if (value is null)
				return;

			_cdm = value;
			update_no_validate();
		}
	}

	[JsonIgnore]
	public IReadOnlyList<Account> Accounts => _accounts_json.AsReadOnly();
	#endregion

	#region de/serialize
	public static AccountsSettings? FromJson(string json)
		=> JsonConvert.DeserializeObject<AccountsSettings>(json, Identity.GetJsonSerializerSettings());

	public string ToJson(Formatting formatting = Formatting.Indented)
		=> JsonConvert.SerializeObject(this, formatting, Identity.GetJsonSerializerSettings());
	#endregion

	// more common naming convention alias for internal collection
	public IReadOnlyList<Account> GetAll() => Accounts;

	public Account Upsert(string accountId, string? locale)
	{
		var acct = GetAccount(accountId, locale);

		if (acct is not null)
			return acct;

		var l = Localization.Get(locale);
		var id = new Identity(l);

		// Match GUI default for new rows (WinForms/Avalonia): include account in library scans.
		var account = new Account(accountId) { IdentityTokens = id, LibraryScan = true };
		Add(account);
		return account;
	}

	public void Add(Account account)
	{
		_add(account);
		update_no_validate();
	}

	public void _add(Account account)
	{
		validate(account);

		_accounts_backing.Add(account);
		account.Updated += update;
	}

	/// <summary>
	/// The account that can speak to <paramref name="locale"/> for this login: the one registered with that
	/// marketplace, or failing that the one carrying it as an extra marketplace. Callers hand this a book's
	/// marketplace and get back the credentials that can license it, which is why extras have to resolve here
	/// as well as the registered marketplace does.
	/// </summary>
	public Account? GetAccount(string accountId, string? locale)
	{
		if (locale is null)
			return null;

		// AccountId is compared case-insensitively: Audible/library data has been observed to differ
		// only by letter case (e.g. a stored id capitalized differently than settings), which caused
		// spurious "No account found" failures that blocked every affected book. See issue #1931.
		var registered = Accounts.SingleOrDefault(a =>
			a.AccountId.EqualsInsensitive(accountId)
			&& a.Locale?.Name == locale);

		if (registered is not null)
			return registered;

		return Accounts.FirstOrDefault(a =>
			a.AccountId.EqualsInsensitive(accountId)
			&& a.HasMarketplace(locale));
	}

	/// <summary>
	/// The account already scanning <paramref name="localeName"/> for this login, if any. Adding a marketplace,
	/// importing an audible-cli file, and probing all need to know whether a marketplace is spoken for - including
	/// by a second row for the same login, which is how multiple marketplaces were handled before one account
	/// could hold several.
	/// </summary>
	public Account? GetAccountClaimingMarketplace(string accountId, string? localeName, Account? excluding = null)
	{
		var name = Localization.Get(localeName).Name;

		return Accounts.FirstOrDefault(a =>
			!ReferenceEquals(a, excluding)
			&& a.AccountId.EqualsInsensitive(accountId)
			&& a.HasMarketplace(name));
	}

	public bool Delete(string accountId, string locale)
	{
		var acct = GetAccount(accountId, locale);
		if (acct is null)
			return false;
		return Delete(acct);
	}

	public bool Delete(Account account)
	{
		if (!_accounts_backing.Contains(account))
			return false;

		account.Updated -= update;
		var result = _accounts_backing.Remove(account);
		update_no_validate();
		return result;
	}

	/// <summary>
	/// No two rows for one login may scan the same marketplace, or a scan would import it twice and a download
	/// would have two accounts to choose from. Extra marketplaces count: claiming 'us' as an extra collides with
	/// another row registered with 'us' just as surely as two 'us' registrations would.
	/// </summary>
	private void validate(Account account)
	{
		ArgumentValidator.EnsureNotNull(account, nameof(account));

		foreach (var locale in account.ScanLocales)
			if (GetAccountClaimingMarketplace(account.AccountId, locale.Name, excluding: account) is not null)
				throw new InvalidOperationException("Cannot add an account with the same account Id and Locale");
	}
}
