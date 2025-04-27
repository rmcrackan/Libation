using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using Newtonsoft.Json;

namespace AudibleUtilities
{
	// 'AccountsSettings' is intentionally NOT IEnumerable<> so that properties can be added/extended
	// from newtonsoft (https://www.newtonsoft.com/json/help/html/SerializationGuide.htm):
	//   .NET :  IList, IEnumerable, IList<T>, Array
	//   JSON :  Array (properties on the collection will not be serialized)
	public class AccountsSettings : IUpdatable
	{
		public event EventHandler Updated;
		private void update(object sender = null, EventArgs e = null)
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
		private List<Account> _accounts_backing = new List<Account>();
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

		private string _cdm;
		[JsonProperty]
		public string Cdm
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
		public static AccountsSettings FromJson(string json)
			=> JsonConvert.DeserializeObject<AccountsSettings>(json, Identity.GetJsonSerializerSettings());

		public string ToJson(Formatting formatting = Formatting.Indented)
			=> JsonConvert.SerializeObject(this, formatting, Identity.GetJsonSerializerSettings());
		#endregion

		// more common naming convention alias for internal collection
		public IReadOnlyList<Account> GetAll() => Accounts;

		public Account Upsert(string accountId, string locale)
		{
			var acct = GetAccount(accountId, locale);

			if (acct is not null)
				return acct;

			var l = Localization.Get(locale);
			var id = new Identity(l);

			var account = new Account(accountId) { IdentityTokens = id };
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

		public Account GetAccount(string accountId, string locale)
		{
			if (locale is null)
				return null;

			return Accounts.SingleOrDefault(a => a.AccountId == accountId && a.IdentityTokens.Locale.Name == locale);
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

		private void validate(Account account)
		{
			ArgumentValidator.EnsureNotNull(account, nameof(account));

			var accountId = account.AccountId;
			var locale = account?.IdentityTokens?.Locale?.Name;

			var acct = GetAccount(accountId, locale);

			// new: ok
			if (acct is null)
				return;

			// same account instance: ok
			if (acct == account)
				return;
			
			// same account id + locale, different instance: bad
			throw new InvalidOperationException("Cannot add an account with the same account Id and Locale");
		}
	}
}
