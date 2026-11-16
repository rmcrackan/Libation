using AudibleApi.Authorization;
using AudibleUtilities;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;
using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>
/// Shared copy and workflows for Important settings token-storage controls.
/// </summary>
public static class TokenStorageSettingsUi
{
	public static string CheckboxText
		=> Strings.TokenStorageSettingsUi_StoreAuthenticationTokensEncryptedUncheckingThis;

	public static string PlaintextWarningText
		=> Strings.TokenStorageSettingsUi_AuthenticationTokensWillBeStoredAs;

	public static string ConvertButtonText => Strings.TokenStorageSettingsUi_UpdateExistingStoredTokens;

	public static string ConvertButtonToolTip
		=> Strings.TokenStorageSettingsUi_ConvertExistingTokensInAccountsSettingsJson;

	public static string ExportButtonText => Strings.TokenStorageSettingsUi_ExportEncryptionKey;

	public static string ExportButtonToolTip
		=> Strings.TokenStorageSettingsUi_ExportTheOSBoundEncryptionMaster;

	public static string ExportConfirmCaption => Strings.TokenStorageSettingsUi_ExportEncryptionKey2;

	public static string ExportConfirmBody
		=> Strings.TokenStorageSettingsUi_ExportTheEncryptionMasterKeyTo;

	public static string ExportSucceededCaption => Strings.TokenStorageSettingsUi_EncryptionKeyExported;
	public static string ExportFailedCaption => Strings.TokenStorageSettingsUi_ExportFailed;

	public static string SavePromptCaption => Strings.TokenStorageSettingsUi_ConvertExistingTokens;
	public static string ConvertConfirmCaption => Strings.TokenStorageSettingsUi_ConfirmTokenConversion;
	public static string ConversionFailedCaption => Strings.TokenStorageSettingsUi_TokenConversionFailed;
	public static string ConversionSucceededCaption => Strings.TokenStorageSettingsUi_TokensUpdated;
	public static string OsStoreUnavailableCaption => Strings.TokenStorageSettingsUi_EncryptedTokenStorageUnavailable;

	public static string SavePromptEncrypted
		=> Strings.TokenStorageSettingsUi_NewAndRefreshedAuthenticationTokensWill;

	public static string SavePromptPlaintext
		=> Strings.TokenStorageSettingsUi_NewAndRefreshedAuthenticationTokensWill2;

	public static string StandaloneConfirmEncrypted
		=> Strings.TokenStorageSettingsUi_EncryptAndReSaveExistingPlaintext;

	public static string StandaloneConfirmPlaintext
		=> Strings.TokenStorageSettingsUi_DecryptAndReSaveExistingEncrypted;

	public static string ConversionSucceededMessage
		=> Strings.TokenStorageSettingsUi_ExistingAuthenticationTokensWereUpdatedTo;

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
			? Strings.TokenStorageSettingsUi_EncryptedTokenStorageRequiresAnOS
			: string.Format(Strings.TokenStorageSettingsUi_EncryptedTokenStorageRequiresAnOS2, unavailableReason);

	public static string FormatConversionError(IdentityTokenConversionResult result)
	{
		var error = string.IsNullOrWhiteSpace(result.Error)
			? Strings.TokenStorageSettingsUi_TokenConversionFailed2
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
				Strings.TokenStorageSettingsUi_TokenConversionFailedTheOriginalAccountsSettings,
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
			string.Format(Strings.TokenStorageSettingsUi_WroteMasterKeyToForDocker, filePath),
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
