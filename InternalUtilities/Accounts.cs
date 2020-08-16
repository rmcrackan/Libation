using System;
using System.Collections.Generic;
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
			=> Updated?.Invoke(this, new EventArgs());

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
				_accountsSettings_backing = value;

				if (_accountsSettings_backing is null)
					return;

				foreach (var acct in _accountsSettings_backing)
					acct.Updated += update;

				update();
			}
		}
		[JsonIgnore]
		public IReadOnlyList<Account> AccountsSettings => _accountsSettings_json.AsReadOnly();
		#endregion

		public static Accounts FromJson(string json)
			=> JsonConvert.DeserializeObject<Accounts>(json, Identity.GetJsonSerializerSettings());

		public string ToJson(Formatting formatting = Formatting.Indented)
			=> JsonConvert.SerializeObject(this, formatting, Identity.GetJsonSerializerSettings());

public void UNITTEST_Seed(Account account)
		{
			_accountsSettings_backing.Add(account);
			update();
		}

		// replace UNITTEST_Seed

		// when creating Account object (get, update, insert), subscribe to it's update. including all new ones on initial load
		// removing: unsubscribe

		// IEnumerable<Account> GetAllAccounts

		// void UpsertAccount (id, locale)
		//   if not exists
		//     create account w/null identity
		//     save in file
		//   return Account?
		//   return bool/enum of whether is newly created?

		// how to persist edits to [Account] obj?
		//   account name, decryptkey, id tokens, ...
		//   persistence happens in [Accounts], not [Account]. an [Account] accidentally created directly shouldn't mess up expected workflow ???
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
