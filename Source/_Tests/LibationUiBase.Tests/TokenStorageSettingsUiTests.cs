using AudibleApi.Authorization;
using LibationUiBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TokenStorageSettingsUiTests;

[TestClass]
public class TokenStorageSettingsUiTests
{
	[TestMethod]
	public void Checkbox_maps_to_enum_values()
	{
		Assert.AreEqual(TokenStorageMethod.Encrypted, TokenStorageSettingsUi.MethodFromCheckbox(true));
		Assert.AreEqual(TokenStorageMethod.Plaintext, TokenStorageSettingsUi.MethodFromCheckbox(false));
		Assert.IsTrue(TokenStorageSettingsUi.CheckboxFromMethod(TokenStorageMethod.Encrypted));
		Assert.IsFalse(TokenStorageSettingsUi.CheckboxFromMethod(TokenStorageMethod.Plaintext));
	}

	[TestMethod]
	public void Convert_button_enabled_only_for_mismatch_or_indeterminate()
	{
		Assert.IsTrue(TokenStorageSettingsUi.IsConvertButtonEnabled(TokenStorageAlignment.SomeMismatch));
		Assert.IsTrue(TokenStorageSettingsUi.IsConvertButtonEnabled(TokenStorageAlignment.Indeterminate));
		Assert.IsFalse(TokenStorageSettingsUi.IsConvertButtonEnabled(TokenStorageAlignment.AllMatch));
		Assert.IsFalse(TokenStorageSettingsUi.IsConvertButtonEnabled(TokenStorageAlignment.NoApplicableTokens));
	}

	[TestMethod]
	public void Prompt_bodies_warn_clearly()
	{
		Assert.AreEqual(
			TokenStorageSettingsUi.SavePromptEncrypted,
			TokenStorageSettingsUi.SavePromptBody(TokenStorageMethod.Encrypted));
		Assert.AreEqual(
			TokenStorageSettingsUi.SavePromptPlaintext,
			TokenStorageSettingsUi.SavePromptBody(TokenStorageMethod.Plaintext));

		StringAssert.Contains(TokenStorageSettingsUi.SavePromptPlaintext, "readable");
		StringAssert.Contains(TokenStorageSettingsUi.StandaloneConfirmPlaintext, "readable");
	}

	[TestMethod]
	public void FormatConversionError_includes_categories_without_secret_values()
	{
		var result = IdentityTokenConversionResult.Failure(
			TokenStorageAlignment.SomeMismatch,
			"Conversion failed.",
			"RefreshToken",
			"AccessToken");

		var message = TokenStorageSettingsUi.FormatConversionError(result);
		StringAssert.Contains(message, "RefreshToken");
		StringAssert.Contains(message, "AccessToken");
		Assert.IsFalse(message.Contains("Atna|"));
	}

	[TestMethod]
	public void Export_copy_warns_about_secret_file()
	{
		StringAssert.Contains(TokenStorageSettingsUi.ExportConfirmBody, "password");
		StringAssert.Contains(TokenStorageSettingsUi.ExportButtonToolTip, "Docker");
		Assert.AreEqual("Export encryption key...", TokenStorageSettingsUi.ExportButtonText);
	}
}
