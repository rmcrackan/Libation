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
	public void Unknown_enum_value_throws_InvalidConfigurationValueException()
	{
		var ex = Assert.ThrowsExactly<InvalidConfigurationValueException>(
			() => IJsonBackedDictionary.UpCast<TokenStorageMethod>(new JValue("NotARealMethod"), nameof(Configuration.TokenStorageMethod)));

		StringAssert.Contains(ex.Message, "TokenStorageMethod");
		StringAssert.Contains(ex.Message, "NotARealMethod");
		StringAssert.Contains(ex.Message, "Plaintext");
		StringAssert.Contains(ex.Message, "Encrypted");
	}

	[TestMethod]
	public void PlainText_casing_resolves_to_Plaintext()
	{
		Assert.AreEqual(
			TokenStorageMethod.Plaintext,
			IJsonBackedDictionary.UpCast<TokenStorageMethod>(new JValue("PlainText")));
		Assert.AreEqual(
			TokenStorageMethod.Plaintext,
			IJsonBackedDictionary.UpCast<TokenStorageMethod>(new JValue("plaintext")));
	}

	[TestMethod]
	public void Undefined_numeric_enum_value_throws()
	{
		var ex = Assert.ThrowsExactly<InvalidConfigurationValueException>(
			() => IJsonBackedDictionary.UpCast<TokenStorageMethod>(new JValue(99), nameof(Configuration.TokenStorageMethod)));

		StringAssert.Contains(ex.Message, "TokenStorageMethod");
		StringAssert.Contains(ex.Message, "99");
	}

	[TestMethod]
	public void Config_property_throws_for_invalid_TokenStorageMethod()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString("PlainTextTypo", nameof(Configuration.TokenStorageMethod));

		var ex = Assert.ThrowsExactly<InvalidConfigurationValueException>(() => _ = config.TokenStorageMethod);
		StringAssert.Contains(ex.Message, "TokenStorageMethod");
		StringAssert.Contains(ex.Message, "PlainTextTypo");
	}

	[TestMethod]
	public void ValidateEnumSettings_throws_for_invalid_TokenStorageMethod()
	{
		var config = Configuration.CreateMockInstance();
		config.SetNonString("NotARealMethod", nameof(Configuration.TokenStorageMethod));

		Assert.ThrowsExactly<InvalidConfigurationValueException>(config.ValidateEnumSettings);
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
