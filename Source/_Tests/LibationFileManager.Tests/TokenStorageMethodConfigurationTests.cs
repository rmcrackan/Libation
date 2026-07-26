using AssertionHelper;
using AudibleApi.Authorization;
using FileManager;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace TokenStorageMethodConfigurationTests;

[TestClass]
[DoNotParallelize]
public class TokenStorageMethodConfigurationTests
{
	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
	}

	[TestMethod]
	public void Default_when_missing_is_Encrypted()
	{
		var config = Configuration.CreateMockInstance();

		config.Exists(nameof(Configuration.TokenStorageMethod)).Should().BeFalse();
		Assert.AreEqual(TokenStorageMethod.Encrypted, config.TokenStorageMethod);
	}

	[TestMethod]
	public void Round_trips_Plaintext_and_Encrypted()
	{
		var config = Configuration.CreateMockInstance();

		config.TokenStorageMethod = TokenStorageMethod.Plaintext;
		Assert.AreEqual(TokenStorageMethod.Plaintext, config.TokenStorageMethod);
		Assert.AreEqual(TokenStorageMethod.Plaintext, config.CreateEphemeralCopy().TokenStorageMethod);

		config.TokenStorageMethod = TokenStorageMethod.Encrypted;
		Assert.AreEqual(TokenStorageMethod.Encrypted, config.TokenStorageMethod);
		Assert.AreEqual(TokenStorageMethod.Encrypted, config.CreateEphemeralCopy().TokenStorageMethod);
	}

	[TestMethod]
	public void Unknown_enum_value_resolves_to_Encrypted_not_Plaintext()
	{
		// Same UpCast path used by Settings.json deserialization for enum settings.
		var parsed = IJsonBackedDictionary.UpCast<TokenStorageMethod>(new JValue("NotARealMethod"));

		Assert.AreEqual(TokenStorageMethod.Encrypted, parsed);
		Assert.AreNotEqual(TokenStorageMethod.Plaintext, parsed);
	}

	[TestMethod]
	public void String_enum_names_round_trip_via_UpCast()
	{
		Assert.AreEqual(
			TokenStorageMethod.Plaintext,
			IJsonBackedDictionary.UpCast<TokenStorageMethod>(new JValue("Plaintext")));
		Assert.AreEqual(
			TokenStorageMethod.Encrypted,
			IJsonBackedDictionary.UpCast<TokenStorageMethod>(new JValue("Encrypted")));
	}
}
