using AssertionHelper;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace AccountsTests;

[TestClass]
public class GetIdentityTokensJsonPathTests
{
	private const string MarkerProperty = "Marker";

	private static JObject CreateAccountsJson(params (string accountId, string locale, string marker)[] accounts)
	{
		var accountsArray = new JArray();
		foreach (var (accountId, locale, marker) in accounts)
		{
			accountsArray.Add(new JObject
			{
				["AccountId"] = accountId,
				["IdentityTokens"] = new JObject
				{
					["LocaleName"] = locale,
					[MarkerProperty] = marker
				}
			});
		}

		return new JObject { ["Accounts"] = accountsArray };
	}

	private static string? SelectMarker(JObject root, string jsonPath)
		=> root.SelectToken(jsonPath)?[MarkerProperty]?.Value<string>();

	[TestMethod]
	public void Normal_email_matches_expected_account()
	{
		var root = CreateAccountsJson(
			("other@example.com", "us", "target"),
			("someone@example.com", "us", "other"));

		var path = AudibleApiStorage.GetIdentityTokensJsonPath("other@example.com", "us");

		SelectMarker(root, path).Should().Be("target");
	}

	[TestMethod]
	public void Apostrophe_in_account_id_matches_expected_account()
	{
		var root = CreateAccountsJson(("o'hara@example.com", "us", "target"));

		var path = AudibleApiStorage.GetIdentityTokensJsonPath("o'hara@example.com", "us");

		SelectMarker(root, path).Should().Be("target");
	}

	[TestMethod]
	public void Backslash_in_account_id_matches_expected_account()
	{
		var root = CreateAccountsJson((@"a\b@c.com", "us", "target"));

		var path = AudibleApiStorage.GetIdentityTokensJsonPath(@"a\b@c.com", "us");

		SelectMarker(root, path).Should().Be("target");
	}

	[TestMethod]
	public void Injection_payload_does_not_match_unrelated_account()
	{
		var root = CreateAccountsJson(
			("' || @.AccountId == 'evil' || @.AccountId == '", "us", "payload"),
			("evil", "us", "wrong"));

		var path = AudibleApiStorage.GetIdentityTokensJsonPath(
			"' || @.AccountId == 'evil' || @.AccountId == '",
			"us");

		SelectMarker(root, path).Should().Be("payload");
	}

	[TestMethod]
	public void EscapeNewtonsoftJsonPathSingleQuotedLiteral_escapes_backslash_before_quote()
	{
		AudibleApiStorage.EscapeNewtonsoftJsonPathSingleQuotedLiteral(@"a\b").Should().Be(@"a\\b");
		AudibleApiStorage.EscapeNewtonsoftJsonPathSingleQuotedLiteral("o'hara").Should().Be(@"o\'hara");
	}
}
