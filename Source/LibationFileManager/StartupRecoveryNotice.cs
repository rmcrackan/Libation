namespace LibationFileManager;

/// <summary>
/// What startup found and repaired before the app came up, and whether restarting into the result is worth
/// offering the user.
/// </summary>
/// <param name="Message">Title and body to show.</param>
/// <param name="OfferRestart">
/// True when the install is worth going back into, so the user can be asked whether to restart now. False
/// when the rollback left files behind, where the honest answer is to install a fresh copy instead, and a
/// restart prompt would only invite the user into a broken install.
/// </param>
public sealed record StartupRecoveryNotice(FatalStartupMessage Message, bool OfferRestart)
{
	public string Title => Message.Title;
	public string Body => Message.Body;
}
