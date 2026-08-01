using Newtonsoft.Json;

namespace AudibleUtilities;

/// <summary>
/// Libation-facing helpers when AccountsSettings.json contains encrypted tokens
/// that cannot be unlocked on this OS/machine (common when copying Windows config into Docker).
/// </summary>
public static class AccountSettingsDecryptFailure
{
	public const string LoadErrorCaption = "Error Loading Account Settings";

	public const string FaqUrl
		= "https://getlibation.com/docs/frequently-asked-questions#docker-finds-no-new-books-failed-to-decrypt-existingaccesstoken";

	private const string MessagePrefix = "Failed to decrypt";

	private static readonly string[] ThingsToTryBullets =
	[
		"On the machine that created the encrypted file (usually Windows): Settings -> Important, uncheck \"Store authentication tokens encrypted\", convert existing tokens to plaintext when prompted, then copy the updated AccountsSettings.json again.",
		"Or re-authenticate on this machine/container with LibationCli login-external or import-account (do not rely on the encrypted Windows copy).",
		$"Details: {FaqUrl}",
	];

	/// <summary>
	/// True when <paramref name="ex"/> (or an inner exception) is a decrypt failure from identity token JSON.
	/// </summary>
	public static bool TryFindInTree(Exception ex, out JsonReaderException? match)
	{
		ArgumentNullException.ThrowIfNull(ex);
		JsonReaderException? found = null;
		walk(ex);
		match = found;
		return found is not null;

		void walk(Exception? e)
		{
			if (e is null || found is not null)
				return;

			if (e is JsonReaderException jsonEx
				&& jsonEx.Message.StartsWith(MessagePrefix, StringComparison.Ordinal))
			{
				found = jsonEx;
				return;
			}

			if (e is AggregateException agg)
			{
				foreach (var inner in agg.InnerExceptions)
					walk(inner);
			}

			walk(e.InnerException);
		}
	}

	public static IEnumerable<string> GetExplainerLines(Exception ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		yield return "Encrypted authentication tokens in AccountsSettings.json could not be decrypted on this machine.";
		yield return "The encryption key is stored in the OS secret store where the tokens were encrypted (for example Windows DPAPI) and does not travel when you copy the file to Docker or another computer.";
		if (TryFindInTree(ex, out var decryptEx) && decryptEx is not null)
			yield return $"Underlying error: {decryptEx.Message.TrimEnd('.')}.";
		yield return string.Empty;
		yield return "Things to try:";
		foreach (var bullet in ThingsToTryBullets)
			yield return "• " + bullet;
	}

	public static string GetExplainerBody(Exception ex)
		=> string.Join("\r\n", GetExplainerLines(ex));

	/// <summary>
	/// Dialog body after backup-and-empty recovery when load failed due to token decrypt.
	/// </summary>
	public static string GetRecoveredDialogBody(Exception ex, string backupPath)
	{
		ArgumentNullException.ThrowIfNull(ex);
		ArgumentException.ThrowIfNullOrWhiteSpace(backupPath);

		return $"""
			{GetExplainerBody(ex)}

			Libation created a new, empty account settings file so the app can start. You will need to re-add your Audible account(s) (or copy a plaintext AccountsSettings.json) before scanning or downloading.

			The previous account settings file was archived at '{backupPath}'
			""";
	}
}
