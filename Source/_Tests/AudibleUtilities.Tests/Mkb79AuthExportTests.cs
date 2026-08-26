using AssertionHelper;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AudibleUtilities.Tests;

[TestClass]
public class Mkb79AuthExportTests
{
	static string MinimalMkb79Json(Action<JObject>? tweak = null)
	{
		var jo = new JObject
		{
			["website_cookies"] = new JObject(),
			["adp_token"] = "a",
			["access_token"] = "b",
			["refresh_token"] = "c",
			["device_private_key"] = "d",
			["store_authentication_cookie"] = new JObject { ["cookie"] = "" },
			["device_info"] = new JObject(),
			["customer_info"] = new JObject(),
			["expires"] = 0,
			["locale_code"] = "us",
			["with_username"] = false,
		};
		tweak?.Invoke(jo);
		return jo.ToString(Newtonsoft.Json.Formatting.None);
	}

	[TestMethod]
	public void ToJson_empty_website_cookies_is_null_not_object()
	{
		var auth = Mkb79Auth.FromJson(MinimalMkb79Json());
		auth.BeNotNull();
		var jo = JObject.Parse(auth.ToJson());
		Assert.AreEqual(JTokenType.Null, jo["website_cookies"]!.Type);
	}

	[TestMethod]
	public void ToJson_device_private_key_is_pem_with_64_char_lines()
	{
		var keyMaterial = Convert.ToBase64String(new byte[100]);
		var singleLine = PrivateKey.REQUIRED_BEGINNING + keyMaterial + PrivateKey.REQUIRED_ENDING;
		var auth = Mkb79Auth.FromJson(MinimalMkb79Json(j =>
		{
			j["website_cookies"] = JValue.CreateNull();
			j["device_private_key"] = singleLine;
		}));
		auth.BeNotNull();
		var pem = JObject.Parse(auth.ToJson())["device_private_key"]!.Value<string>()!;
		var lines = pem.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
		lines[0].Should().Be(PrivateKey.REQUIRED_BEGINNING);
		lines[^1].Should().Be(PrivateKey.REQUIRED_ENDING);
		foreach (var body in lines.Skip(1).Take(lines.Length - 2))
			Assert.IsTrue(body.Length <= 64);
	}

	[TestMethod]
	public void Serialize_ToJson_matches_instance_ToJson()
	{
		var auth = Mkb79Auth.FromJson(MinimalMkb79Json(j => j["device_private_key"] = "AAAA"));
		auth.BeNotNull();
		auth.ToJson().Should().Be(Serialize.ToJson(auth));
	}

	/// <summary>
	/// The file carries one marketplace because that is all the format has room for: one locale_code beside one
	/// device registration. Exporting an account that reads several must still name the one it is registered
	/// with, so the file stays valid for audible-cli - which switches marketplaces from those same tokens anyway.
	/// </summary>
	[TestMethod]
	public void export_names_the_registered_marketplace_and_leaves_the_additional_ones_out()
	{
		var account = new Account("user@example.com")
		{
			IdentityTokens = new Identity(Localization.Get("ca"))
		};
		account.AddMarketplace("us");

		var jo = JObject.Parse(Mkb79Auth.FromAccount(account).ToJson());

		jo["locale_code"]!.Value<string>().Should().Be("ca");
		jo["with_username"]!.Value<bool>().Should().BeFalse();

		// nothing in the format records the extra marketplace, so a re-import will not restore it
		jo.ContainsKey("AdditionalLocaleNames").Should().BeFalse();
		jo.Properties().Select(p => p.Name).Contains("additional_locale_codes").Should().BeFalse();
	}
}
