using AudibleApi.Authorization;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;

namespace AccountSettingsDecryptFailureTests;

[TestClass]
public class AccountSettingsDecryptFailureTests
{
	[TestMethod]
	public void TryFindInTree_matches_IdentityTokenDecryptException()
	{
		var inner = CreateDecryptException("ExistingAccessToken");
		var outer = new InvalidOperationException("load failed", inner);

		Assert.IsTrue(AccountSettingsDecryptFailure.TryFindInTree(outer, out var match));
		Assert.AreSame(inner, match);
	}

	[TestMethod]
	public void TryFindInTree_matches_inside_AggregateException()
	{
		var decrypt = CreateDecryptException("RefreshToken");
		var agg = new AggregateException(new Exception("other"), decrypt);

		Assert.IsTrue(AccountSettingsDecryptFailure.TryFindInTree(agg, out var match));
		Assert.AreSame(decrypt, match);
	}

	[TestMethod]
	public void TryFindInTree_ignores_unrelated_JsonReaderException()
	{
		var ex = new JsonReaderException("Unexpected character encountered while parsing value.");
		Assert.IsFalse(AccountSettingsDecryptFailure.TryFindInTree(ex, out var match));
		Assert.IsNull(match);
	}

	[TestMethod]
	public void TryFindInTree_ignores_JsonReaderException_with_old_message_prefix()
	{
		var ex = new JsonReaderException("Failed to decrypt ExistingAccessToken.");
		Assert.IsFalse(AccountSettingsDecryptFailure.TryFindInTree(ex, out var match));
		Assert.IsNull(match);
	}

	[TestMethod]
	public void Explainer_mentions_portable_key_plaintext_cli_and_faq()
	{
		var ex = CreateDecryptException("ExistingAccessToken");
		var body = AccountSettingsDecryptFailure.GetExplainerBody(ex);

		StringAssert.Contains(body, "could not be decrypted");
		StringAssert.Contains(body, "export-master-key");
		StringAssert.Contains(body, "libation-master.key");
		StringAssert.Contains(body, "LIBATION_MASTER_KEY_FILE");
		StringAssert.Contains(body, "Store authentication tokens encrypted");
		StringAssert.Contains(body, "login-external");
		StringAssert.Contains(body, "import-account");
		StringAssert.Contains(body, AccountSettingsDecryptFailure.FaqUrl);
		StringAssert.Contains(body, "ExistingAccessToken");
	}

	[TestMethod]
	public void Recovered_dialog_includes_backup_path_and_explainer()
	{
		var ex = CreateDecryptException("ExistingAccessToken");
		var body = AccountSettingsDecryptFailure.GetRecoveredDialogBody(ex, @"C:\tmp\AccountsSettings.json.bak");

		StringAssert.Contains(body, "empty account settings file");
		StringAssert.Contains(body, @"C:\tmp\AccountsSettings.json.bak");
		StringAssert.Contains(body, AccountSettingsDecryptFailure.FaqUrl);
	}

	private static IdentityTokenDecryptException CreateDecryptException(string fieldName)
		=> new(fieldName, new InvalidOperationException("crypto failed"));
}
