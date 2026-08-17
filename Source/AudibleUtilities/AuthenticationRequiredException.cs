namespace AudibleUtilities;

/// <summary>
/// Stored Audible credentials are missing or invalid and interactive login is required.
/// Thrown instead of opening login UI when the caller disallows interactive login (e.g. auto-scan).
/// </summary>
public sealed class AuthenticationRequiredException : Exception
{
	/// <summary>
	/// A log-safe summary rather than the <see cref="Account"/> itself. Serilog.Exceptions reflects over every
	/// public property of a logged exception, so holding the live account published its address - and would have
	/// published its activation bytes - into logs people attach to public issue reports.
	/// </summary>
	public AccountSummary? AccountInfo { get; }

	public AuthenticationRequiredException(Account? account, string? message = null, Exception? innerException = null)
		: base(message ?? "Audible authentication is required.", innerException)
		=> AccountInfo = AccountSummary.From(account);
}
