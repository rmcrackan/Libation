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

	private readonly string ownerFacingLabel;

	private AccountSummary(Account account)
	{
		MaskedLogEntry = account.MaskedLogEntry;
		LooksLikeMissingCredentials = AccountCredentialStatus.LooksLikeMissingCredentials(account);
		ownerFacingLabel = AccountCredentialStatus.FormatAccountLabel(account);
	}

	/// <summary>
	/// The name and address to show the person who owns the account, on their own screen. Never log this: use
	/// <see cref="MaskedLogEntry"/>.
	/// </summary>
	public string RevealOwnerFacingLabel() => ownerFacingLabel;

	public override string ToString() => MaskedLogEntry;

	public static AccountSummary? From(Account? account)
		=> account is null ? null : new AccountSummary(account);
}
