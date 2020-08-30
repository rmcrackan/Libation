using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using Newtonsoft.Json;

namespace InternalUtilities
{
	public class Account : IUpdatable
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

		// whether to include this account when scanning libraries.
		// technically this is an app setting; not an attribute of account. but since it's managed with accounts, it makes sense to put this exception-to-the-rule here
		private bool _libraryScan = true;
		public bool LibraryScan
		{
			get => _libraryScan;
			set
			{
				if (value == _libraryScan)
					return;
				_libraryScan = value;
				update();
			}
		}

		private string _decryptKey = "";
		/// <summary>aka: activation bytes</summary>
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
			AccountId = ArgumentValidator.EnsureNotNullOrWhiteSpace(accountId, nameof(accountId)).Trim();
		}

		public override string ToString() => $"{AccountId} - {Locale?.Name ?? "[empty]"}";
	}
}
