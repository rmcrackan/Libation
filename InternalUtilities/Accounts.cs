using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.IO;
using Newtonsoft.Json;

namespace InternalUtilities
{
	public class AccountsPersister : JsonFilePersister<Accounts>
	{
		/// <summary>Alias for Target </summary>
		public Accounts Accounts => Target;

		/// <summary>uses path. create file if doesn't yet exist</summary>
		public AccountsPersister(Accounts target, string path, string jsonPath = null)
			: base(target, path, jsonPath) { }

		/// <summary>load from existing file</summary>
		public AccountsPersister(string path, string jsonPath = null)
			: base(path, jsonPath) { }

		protected override JsonSerializerSettings GetSerializerSettings()
			=> Identity.GetJsonSerializerSettings();
	}
	public class Accounts : Updatable
	{
		public event EventHandler Updated;
		private void update(object sender = null, EventArgs e = null)
		{
			foreach (var account in AccountsSettings)
				validate(account);
			update_no_validate();
		}
		private void update_no_validate() => Updated?.Invoke(this, new EventArgs());

		public Accounts() { }

		// for some reason this will make the json instantiator use _accountsSettings_json.set()
		[JsonConstructor]
		protected Accounts(List<Account> accounts) { }

		#region AccountsSettings
		private List<Account> _accountsSettings_backing = new List<Account>();
		[JsonProperty(PropertyName = "AccountsSettings")]
		private List<Account> _accountsSettings_json
		{
			get => _accountsSettings_backing;
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
		[JsonIgnore]
		public IReadOnlyList<Account> AccountsSettings => _accountsSettings_json.AsReadOnly();
		#endregion

		public static Accounts FromJson(string json)
			=> JsonConvert.DeserializeObject<Accounts>(json, Identity.GetJsonSerializerSettings());

		public string ToJson(Formatting formatting = Formatting.Indented)
			=> JsonConvert.SerializeObject(this, formatting, Identity.GetJsonSerializerSettings());

		public void Add(Account account)
		{
			_add(account);
			update_no_validate();
		}

		public void _add(Account account)
		{
			validate(account);

			_accountsSettings_backing.Add(account);
			account.Updated += update;
		}

		// more common naming convention alias for internal collection
		public IReadOnlyList<Account> GetAll() => AccountsSettings;

		public Account GetAccount(string accountId, string locale)
		{
			if (locale is null)
				return null;

			return AccountsSettings.SingleOrDefault(a => a.AccountId == accountId && a.IdentityTokens.Locale.Name == locale);
		}

		public Account Upsert(string accountId, string locale)
		{
			var acct = GetAccount(accountId, locale);

			if (acct != null)
				return acct;

			var l = Localization.Locales.Single(l => l.Name == locale);
			var id = new Identity(l);

			var account = new Account(accountId) { IdentityTokens = id };
			Add(account);
			return account;
		}

		public bool Delete(Account account)
		{
			if (!_accountsSettings_backing.Contains(account))
				return false;

			account.Updated -= update;
			return _accountsSettings_backing.Remove(account);
		}

		public bool Delete(string accountId, string locale)
		{
			var acct = GetAccount(accountId, locale);
			if (acct is null)
				return false;
			return Delete(acct);
		}

		private void validate(Account account)
		{
			ArgumentValidator.EnsureNotNull(account, nameof(account));

			var accountId = account.AccountId;
			var locale = account?.IdentityTokens?.Locale?.Name;

			var acct = GetAccount(accountId, locale);

			if (acct is null || account is null)
				return;

			if (acct != account)
				throw new InvalidOperationException("Cannot add an account with the same account Id and Locale");
		}
	}
	public class Account : Updatable
	{
		public event EventHandler Updated;
		private void update(object sender = null, EventArgs e = null)
			=> Updated?.Invoke(this, new EventArgs());

		// canonical. immutable. email or phone number
		public string AccountId { get; }

		// user-friendly, non-canonical name. mutable
		private string _accountName;
		public string AccountName
		{
			get => _accountName;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					return;
				var v = value.Trim();
				if (v == _accountName)
					return;
				_accountName = v;
				update();
			}
		}

		private string _decryptKey = "";
		public string DecryptKey
		{
			get => _decryptKey;
			set
			{
				var v = (value ?? "").Trim();
				if (v == _decryptKey)
					return;
				_decryptKey = v;
				update();
			}
		}

		private Identity _identity;
		public Identity IdentityTokens
		{
			get => _identity;
			set
			{
				if (_identity is null && value is null)
					return;

				if (_identity != null)
					_identity.Updated -= update;

				if (value != null)
					value.Updated += update;

				_identity = value;
				update();
			}
		}

		[JsonIgnore]
		public Locale Locale => IdentityTokens?.Locale;

		public Account(string accountId)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(accountId, nameof(accountId));
			AccountId = accountId.Trim();
		}
	}
}
