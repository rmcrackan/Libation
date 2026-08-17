using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.Security;
using LibationFileManager;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

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

	[JsonIgnore]
	public Locale? Locale => IdentityTokens?.Locale;

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

	public string MaskedLogEntry => @$"AccountId={mask(AccountId)}|AccountName={mask(AccountName)}|Locale={Locale?.Name ?? "[empty]"}";
	private static string mask(string? str)
		=> str is null ? "[null]"
		: str == string.Empty ? "[empty]"
		: str.ToMask();
}
