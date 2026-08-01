using AudibleApi.Authorization;
using AudibleUtilities;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;

namespace LibationUiBase;

/// <summary>
/// Shared copy and workflows for Important settings token-storage controls.
/// </summary>
public static class TokenStorageSettingsUi
{
	public const string CheckboxText
		= "Store authentication tokens encrypted. Unchecking this option stores tokens in plaintext.";

	public const string PlaintextWarningText
		= "Authentication tokens will be stored as readable plaintext in AccountsSettings.json.";

	public const string ConvertButtonText = "Update existing stored tokens";

	public const string ConvertButtonToolTip
		= "Convert existing tokens in AccountsSettings.json to match the currently selected storage method.";

	public const string ExportButtonText = "Export encryption key...";

	public const string ExportButtonToolTip
		= "Export the OS-bound encryption master key to a file for Docker or another machine. Treat the file like a password.";

	public const string ExportConfirmCaption = "Export encryption key";

	public const string ExportConfirmBody
		= "Export the encryption master key to a file for use in Docker or on another machine?\r\n\r\n"
		+ "Anyone with this file can decrypt encrypted tokens in AccountsSettings.json. Treat it like a password.";

	public const string ExportSucceededCaption = "Encryption key exported";
	public const string ExportFailedCaption = "Export failed";

	public const string SavePromptCaption = "Convert existing tokens?";
	public const string ConvertConfirmCaption = "Confirm token conversion";
	public const string ConversionFailedCaption = "Token conversion failed";
	public const string ConversionSucceededCaption = "Tokens updated";
	public const string OsStoreUnavailableCaption = "Encrypted token storage unavailable";

	public const string SavePromptEncrypted
		= "New and refreshed authentication tokens will now be stored encrypted. Would you also like to encrypt and re-save existing plaintext tokens?";

	public const string SavePromptPlaintext
		= "New and refreshed authentication tokens will now be stored in plaintext. Would you also like to decrypt and re-save existing encrypted tokens? This will make those token values readable in the account settings file.";

	public const string StandaloneConfirmEncrypted
		= "Encrypt and re-save existing plaintext authentication tokens so they match the current setting?";

	public const string StandaloneConfirmPlaintext
		= "Decrypt and re-save existing encrypted authentication tokens as plaintext? This will make those token values readable in the account settings file.";

	public const string ConversionSucceededMessage
		= "Existing authentication tokens were updated to match the selected storage method.";

	public static TokenStorageMethod MethodFromCheckbox(bool storeEncrypted)
		=> storeEncrypted ? TokenStorageMethod.Encrypted : TokenStorageMethod.Plaintext;

	public static bool CheckboxFromMethod(TokenStorageMethod method)
		=> method == TokenStorageMethod.Encrypted;

	/// <summary>
	/// Convert is offered for mismatches and indeterminate state.
	/// Matching / no-token states keep the button disabled.
	/// </summary>
	public static bool IsConvertButtonEnabled(TokenStorageAlignment alignment)
		=> alignment is TokenStorageAlignment.SomeMismatch or TokenStorageAlignment.Indeterminate;

	public static string SavePromptBody(TokenStorageMethod method)
		=> method == TokenStorageMethod.Encrypted ? SavePromptEncrypted : SavePromptPlaintext;

	public static string StandaloneConfirmBody(TokenStorageMethod method)
		=> method == TokenStorageMethod.Encrypted ? StandaloneConfirmEncrypted : StandaloneConfirmPlaintext;

	public static string OsStoreUnavailableMessage(string? unavailableReason)
		=> string.IsNullOrWhiteSpace(unavailableReason)
			? "Encrypted token storage requires an OS secret store, which is not available. Tokens cannot be stored encrypted."
			: $"Encrypted token storage requires an OS secret store, which is not available: {unavailableReason}";

	public static string FormatConversionError(IdentityTokenConversionResult result)
	{
		var error = string.IsNullOrWhiteSpace(result.Error)
			? "Token conversion failed."
			: result.Error!;

		if (result.FailedCategories.Count == 0)
			return error;

		return $"{error}\r\n\r\nFailed categories: {string.Join(", ", result.FailedCategories)}";
	}

	/// <summary>
	/// Decide whether saving a changed preference should also convert existing tokens.
	/// Cancel / dismiss must never be treated as approval.
	/// </summary>
	public static async Task<TokenStorageSaveDecision> PromptOnPreferenceSaveAsync(
		object? owner,
		TokenStorageMethod originalMethod,
		TokenStorageMethod selectedMethod)
	{
		if (originalMethod == selectedMethod)
			return TokenStorageSaveDecision.SavePreferenceOnly;

		if (selectedMethod == TokenStorageMethod.Encrypted
			&& !IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out var unavailableReason))
		{
			await MessageBoxBase.Show(
				owner,
				OsStoreUnavailableMessage(unavailableReason),
				OsStoreUnavailableCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return TokenStorageSaveDecision.Abort;
		}

		var alignment = AccountTokenStorage.GetAccountsAlignment(selectedMethod);
		if (alignment is TokenStorageAlignment.AllMatch or TokenStorageAlignment.NoApplicableTokens)
			return TokenStorageSaveDecision.SavePreferenceOnly;

		var result = await MessageBoxBase.Show(
			owner,
			SavePromptBody(selectedMethod),
			SavePromptCaption,
			MessageBoxButtons.YesNoCancel,
			MessageBoxIcon.Warning,
			defaultButton: selectedMethod == TokenStorageMethod.Plaintext
				? MessageBoxDefaultButton.Button2
				: MessageBoxDefaultButton.Button1);

		return result switch
		{
			DialogResult.Yes => TokenStorageSaveDecision.SavePreferenceAndConvert,
			DialogResult.No => TokenStorageSaveDecision.SavePreferenceOnly,
			_ => TokenStorageSaveDecision.Abort
		};
	}

	public static async Task<bool> ConfirmAndConvertAsync(object? owner, TokenStorageMethod method)
	{
		if (method == TokenStorageMethod.Encrypted
			&& !IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out var unavailableReason))
		{
			await MessageBoxBase.Show(
				owner,
				OsStoreUnavailableMessage(unavailableReason),
				OsStoreUnavailableCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return false;
		}

		var alignment = AccountTokenStorage.GetAccountsAlignment(method);
		if (!IsConvertButtonEnabled(alignment))
			return false;

		var confirm = await MessageBoxBase.Show(
			owner,
			StandaloneConfirmBody(method),
			ConvertConfirmCaption,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Warning,
			defaultButton: method == TokenStorageMethod.Plaintext
				? MessageBoxDefaultButton.Button2
				: MessageBoxDefaultButton.Button1);

		if (confirm != DialogResult.Yes)
			return false;

		return await RunConversionAsync(owner, method);
	}

	public static async Task<bool> RunConversionAsync(object? owner, TokenStorageMethod method)
	{
		IdentityTokenConversionResult result;
		try
		{
			result = AccountTokenStorage.ConvertAllAccounts(method);
		}
		catch (System.Exception ex)
		{
			Serilog.Log.Logger.Warning(ex, "Token conversion failed");
			await MessageBoxBase.Show(
				owner,
				"Token conversion failed. The original AccountsSettings.json file was left unchanged.",
				ConversionFailedCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return false;
		}

		if (!result.Succeeded)
		{
			await MessageBoxBase.Show(
				owner,
				FormatConversionError(result),
				ConversionFailedCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return false;
		}

		if (result.Changed)
		{
			await MessageBoxBase.Show(
				owner,
				ConversionSucceededMessage,
				ConversionSucceededCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		return true;
	}

	/// <summary>Ask the user to confirm exporting the master key. Returns true when they choose Yes.</summary>
	public static async Task<bool> ConfirmExportMasterKeyAsync(object? owner)
	{
		var confirm = await MessageBoxBase.Show(
			owner,
			ExportConfirmBody,
			ExportConfirmCaption,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Warning,
			defaultButton: MessageBoxDefaultButton.Button2);

		return confirm == DialogResult.Yes;
	}

	/// <summary>
	/// Export the OS-bound master key to <paramref name="filePath"/> and show success/error.
	/// Caller should confirm first and obtain <paramref name="filePath"/> via a save-file dialog.
	/// </summary>
	public static async Task ExportMasterKeyToFileAsync(object? owner, string filePath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		try
		{
			MasterKeyExport.ExportToFile(filePath);
		}
		catch (System.Exception ex)
		{
			Serilog.Log.Logger.Warning(ex, "Master key export failed");
			await MessageBoxBase.Show(
				owner,
				ex.Message,
				ExportFailedCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			return;
		}

		await MessageBoxBase.Show(
			owner,
			$"Wrote master key to:\r\n{filePath}\r\n\r\n"
			+ "For Docker, copy it next to AccountsSettings.json as libation-master.key, or set LIBATION_MASTER_KEY_FILE.",
			ExportSucceededCaption,
			MessageBoxButtons.OK,
			MessageBoxIcon.Information);
	}
}

public enum TokenStorageSaveDecision
{
	Abort,
	SavePreferenceOnly,
	SavePreferenceAndConvert
}
