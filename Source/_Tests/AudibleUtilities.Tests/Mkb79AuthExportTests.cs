using AssertionHelper;
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
}
