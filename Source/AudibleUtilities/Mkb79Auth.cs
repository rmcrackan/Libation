using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleUtilities;

public partial class Mkb79Auth : IIdentityMaintainer
{
	[JsonProperty("website_cookies")]
	private JObject? _websiteCookies { get; set; }

	[JsonProperty("adp_token")]
	public string? AdpToken { get; private set; }

	[JsonProperty("access_token")]
	public string? AccessToken { get; private set; }

	[JsonProperty("refresh_token")]
	public string? RefreshToken { get; private set; }

	[JsonProperty("device_private_key")]
	public string? DevicePrivateKey { get; private set; }

	[JsonProperty("store_authentication_cookie")]
	private JObject? _storeAuthenticationCookie { get; set; }

	[JsonProperty("device_info")]
	public DeviceInfo? DeviceInfo { get; private set; }

	[JsonProperty("customer_info")]
	public CustomerInfo? CustomerInfo { get; private set; }

	[JsonProperty("expires")]
	private double _expires { get; set; }

	[JsonProperty("locale_code")]
	public string? LocaleCode { get; private set; }

	[JsonProperty("with_username")]
	public bool WithUsername { get; private set; }

	[JsonProperty("activation_bytes")]
	public string? ActivationBytes { get; private set; }

	[JsonIgnore]
	public Dictionary<string, string?>? WebsiteCookies
	{
		get => _websiteCookies?.ToObject<Dictionary<string, string?>>();
		private set => _websiteCookies = value is null || value.Count == 0
			? null
			: JObject.Parse(JsonConvert.SerializeObject(value, Converter.Settings));
	}

	[JsonIgnore]
	public string? StoreAuthenticationCookie
	{
		get => _storeAuthenticationCookie?.ToObject<Dictionary<string, string>>()?["cookie"];
		private set => _storeAuthenticationCookie = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, string>() { { "cookie", value ?? "" } }, Converter.Settings));
	}

	[JsonIgnore]
	public DateTime AccessTokenExpires
	{
		get => DateTimeOffset.FromUnixTimeMilliseconds((long)(_expires * 1000)).DateTime;
		private set => _expires = new DateTimeOffset(value).ToUnixTimeMilliseconds() / 1000d;
	}

	[JsonIgnore] public ISystemDateTime SystemDateTime { get; } = new SystemDateTime();
	[JsonIgnore]
	public Locale Locale => Localization.Locales.Where(l => l.WithUsername == WithUsername).Single(l => l.CountryCode == LocaleCode);
	[JsonIgnore] public string? DeviceSerialNumber => DeviceInfo?.DeviceSerialNumber;
	[JsonIgnore] public string? DeviceType => DeviceInfo?.DeviceType;
	[JsonIgnore] public string? AmazonAccountId => CustomerInfo?.UserId;

	public Task<AccessToken?> GetAccessTokenAsync()
		=> AccessToken is null ? Task.FromResult((AccessToken?)null) : Task.FromResult((AccessToken?)new AccessToken(AccessToken, AccessTokenExpires));

	public Task<AdpToken?> GetAdpTokenAsync()
		=> AdpToken is null ? Task.FromResult((AdpToken?)null) : Task.FromResult((AdpToken?)new AdpToken(AdpToken));

	public Task<PrivateKey?> GetPrivateKeyAsync()
		=> DevicePrivateKey is null ? Task.FromResult((PrivateKey?)null) : Task.FromResult((PrivateKey?)new PrivateKey(DevicePrivateKey));
}

public partial class CustomerInfo
{
	[JsonProperty("account_pool")]
	public string? AccountPool { get; set; }

	[JsonProperty("user_id")]
	public string? UserId { get; set; }

	[JsonProperty("home_region")]
	public string? HomeRegion { get; set; }

	[JsonProperty("name")]
	public string? Name { get; set; }

	[JsonProperty("given_name")]
	public string? GivenName { get; set; }
}

public partial class DeviceInfo
{
	[JsonProperty("device_name")]
	public string? DeviceName { get; set; }

	[JsonProperty("device_serial_number")]
	public string? DeviceSerialNumber { get; set; }

	[JsonProperty("device_type")]
	public string? DeviceType { get; set; }
}

public partial class Mkb79Auth
{
	public static Mkb79Auth? FromJson(string json)
		=> JsonConvert.DeserializeObject<Mkb79Auth>(json, Converter.Settings);

	public string ToJson()
	{
		var jo = JObject.Parse(JsonConvert.SerializeObject(this, Converter.Settings));
		ApplyAudibleCliExportConventions(jo);
		return jo.ToString(Formatting.Indented);
	}

	/// <summary>
	/// audible-cli expects <c>website_cookies</c> as JSON null when empty (not <c>{}</c>) and a PEM
	/// <c>device_private_key</c> with standard 64-character base64 lines and newline separators.
	/// </summary>
	internal static void ApplyAudibleCliExportConventions(JObject jo)
	{
		if (jo["website_cookies"] is JObject wc && !wc.Properties().Any())
			jo["website_cookies"] = JValue.CreateNull();

		if (jo["device_private_key"]?.Type == JTokenType.String)
		{
			var s = jo["device_private_key"]!.Value<string>();
			var formatted = FormatDevicePrivateKeyForAudibleCliExport(s);
			if (formatted is not null)
				jo["device_private_key"] = formatted;
		}
	}

	private static string? FormatDevicePrivateKeyForAudibleCliExport(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return value;

		var trimmed = value.Trim();
		string payload;
		if (trimmed.StartsWith(PrivateKey.REQUIRED_BEGINNING, StringComparison.Ordinal))
		{
			var endIdx = trimmed.LastIndexOf(PrivateKey.REQUIRED_ENDING, StringComparison.Ordinal);
			if (endIdx < PrivateKey.REQUIRED_BEGINNING.Length)
				return value;

			payload = trimmed
				.Substring(PrivateKey.REQUIRED_BEGINNING.Length, endIdx - PrivateKey.REQUIRED_BEGINNING.Length)
				.Replace("\r", "")
				.Replace("\n", "")
				.Replace("\\n", "", StringComparison.Ordinal)
				.Trim();
		}
		else
			payload = trimmed;

		if (payload.Length == 0)
			return value;

		try
		{
			Convert.FromBase64String(payload);
		}
		catch (FormatException)
		{
			return value;
		}

		var sb = new StringBuilder();
		sb.Append(PrivateKey.REQUIRED_BEGINNING).Append('\n');
		for (var i = 0; i < payload.Length; i += 64)
		{
			var len = Math.Min(64, payload.Length - i);
			sb.Append(payload, i, len).Append('\n');
		}
		sb.Append(PrivateKey.REQUIRED_ENDING).Append('\n');
		return sb.ToString();
	}

	public async Task<Account> ToAccountAsync()
	{
		if (RefreshToken is null)
			throw new InvalidOperationException("Cannot create Account from Mkb79Auth without a Refresh Token.");
		if (await GetAdpTokenAsync() is not { } adpToken)
			throw new InvalidOperationException("Cannot create Account from Mkb79Auth without an ADP Token.");
		if (await GetPrivateKeyAsync() is not { } privateKey)
			throw new InvalidOperationException("Cannot create Account from Mkb79Auth without a Private Key.");
		var refreshToken = new RefreshToken(RefreshToken);

		var authorize = new Authorize(Locale);
		var newToken = await authorize.RefreshAccessTokenAsync(refreshToken);
		AccessToken = newToken.TokenValue;
		AccessTokenExpires = newToken.Expires;

		var api = new Api(this);
		var email = await api.GetEmailAsync();
		var account = new Account(email)
		{
			DecryptKey = ActivationBytes,
			AccountName = $"{email} - {Locale.Name}",
			IdentityTokens = new Identity(Locale)
		};

		account.IdentityTokens.Update(
			privateKey,
			adpToken,
			newToken,
			refreshToken,
			WebsiteCookies?.Select(c => new KeyValuePair<string, string?>(c.Key, c.Value)),
			DeviceSerialNumber,
			DeviceType,
			AmazonAccountId,
			DeviceInfo?.DeviceName,
			StoreAuthenticationCookie);

		return account;
	}

	public static Mkb79Auth FromAccount(Account account)
		=> new()
		{
			AccessToken = account.IdentityTokens?.ExistingAccessToken.TokenValue,
			ActivationBytes = string.IsNullOrEmpty(account.DecryptKey) ? null : account.DecryptKey,
			AdpToken = account.IdentityTokens?.AdpToken?.Value,
			CustomerInfo = new CustomerInfo
			{
				AccountPool = "Amazon",
				GivenName = string.Empty,
				HomeRegion = "NA",
				Name = string.Empty,
				UserId = account.IdentityTokens?.AmazonAccountId
			},
			DeviceInfo = new DeviceInfo
			{
				DeviceName = account.IdentityTokens?.DeviceName,
				DeviceSerialNumber = account.IdentityTokens?.DeviceSerialNumber,
				DeviceType = account.IdentityTokens?.DeviceType,
			},
			DevicePrivateKey = account.IdentityTokens?.PrivateKey?.Value,
			AccessTokenExpires = account.IdentityTokens?.ExistingAccessToken.Expires ?? default,
			LocaleCode = account.Locale?.CountryCode,
			WithUsername = account.Locale?.WithUsername ?? false,
			RefreshToken = account.IdentityTokens?.RefreshToken?.Value,
			StoreAuthenticationCookie = account.IdentityTokens?.StoreAuthenticationCookie,
			WebsiteCookies = new(account.IdentityTokens?.Cookies ?? []),
		};
}

public static class Serialize
{
	public static string ToJson(this Mkb79Auth self) => self.ToJson();
}

internal static class Converter
{
	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
		DateParseHandling = DateParseHandling.None,
	};
}
