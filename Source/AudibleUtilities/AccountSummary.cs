using Dinah.Core;

namespace AudibleUtilities;

/// <summary>
/// What a failure needs to say about an account, captured so a live <see cref="Account"/> - with its tokens and
/// its activation bytes - never rides along on an exception into a log file.
/// <para>
/// Logs get attached to public issue reports, and Serilog.Exceptions writes every public property of a logged
/// exception into one, following nested objects as it goes. So everything public here is masked, and the label
/// meant for the account's owner is reachable only through a method: reflection reads properties and never calls
/// methods.
/// </para>
/// </summary>
public sealed class AccountSummary
{
	/// <summary>Safe to log. Also what <see cref="ToString"/> returns, so interpolating this cannot leak.</summary>
	public string MaskedLogEntry { get; }

	/// <summary>
	/// True when the account was never fully logged in or its credentials were cleared, rather than merely
	/// holding an expired session.
	/// </summary>
	public bool LooksLikeMissingCredentials { get; }

	/// <summary>
	/// True when the account has been marked as needing a fresh login and device registration.
	/// </summary>
	public bool RequiresReRegistration { get; }

	private readonly string ownerFacingLabel;
	private readonly string logSafeLabel;
	private readonly string? loginExternalHint;

	private AccountSummary(Account account)
	{
		MaskedLogEntry = account.MaskedLogEntry;
		LooksLikeMissingCredentials = AccountCredentialStatus.LooksLikeMissingCredentials(account);
		RequiresReRegistration = account.RequiresReRegistration;
		ownerFacingLabel = AccountCredentialStatus.FormatAccountLabel(account);
		logSafeLabel = FormatLogSafeAccountLabel(account);
		loginExternalHint = !string.IsNullOrWhiteSpace(account.AccountId) && !string.IsNullOrWhiteSpace(account.Locale?.Name)
			? $"libationcli login-external --account {account.AccountId} --locale {account.Locale.Name}"
			: null;
	}

	/// <summary>
	/// The name and address to show the person who owns the account, on their own screen. Never log this: use
	/// <see cref="MaskedLogEntry"/> or <see cref="FormatLogSafeLabel"/>.
	/// </summary>
	public string RevealOwnerFacingLabel() => ownerFacingLabel;

	/// <summary>
	/// Copy-pasteable CLI sign-in command for the account's owner (unmasked). Never log this: write to stderr only.
	/// </summary>
	public string? RevealLoginExternalHint() => loginExternalHint;

	/// <summary>
	/// Log-safe display label for stdout (which is teed to Serilog): uses the user-defined
	/// <see cref="Account.AccountName"/> when distinct, and masks <see cref="Account.AccountId"/>.
	/// </summary>
	public string FormatLogSafeLabel() => logSafeLabel;

	private static string FormatLogSafeAccountLabel(Account account)
	{
		var maskedId = account.AccountId.ToMask();
		var localeSuffix = string.IsNullOrWhiteSpace(account.Locale?.Name) ? "" : $", {account.Locale.Name}";

		return string.IsNullOrWhiteSpace(account.AccountName) || account.AccountName.Equals(account.AccountId, StringComparison.OrdinalIgnoreCase)
			? $"'{maskedId}'{(localeSuffix.Length > 0 ? $" ({account.Locale!.Name})" : "")}"
			: $"'{account.AccountName}' ({maskedId}{localeSuffix})";
	}

	public override string ToString() => MaskedLogEntry;

	public static AccountSummary? From(Account? account)
		=> account is null ? null : new AccountSummary(account);
}
