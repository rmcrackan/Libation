using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleUtilities
{
	public partial class Mkb79Auth : IIdentityMaintainer
	{
		[JsonProperty("website_cookies")]
		private JObject _websiteCookies { get; set; }
	   
		[JsonProperty("adp_token")]
		public string AdpToken { get; private set; }

		[JsonProperty("access_token")]
		public string AccessToken { get; private set; }

		[JsonProperty("refresh_token")]
		public string RefreshToken { get; private set; }

		[JsonProperty("device_private_key")]
		public string DevicePrivateKey { get; private set; }

		[JsonProperty("store_authentication_cookie")]
		private JObject _storeAuthenticationCookie { get; set; }

		[JsonProperty("device_info")]
		public DeviceInfo DeviceInfo { get; private set; }

		[JsonProperty("customer_info")]
		public CustomerInfo CustomerInfo { get; private set; }

		[JsonProperty("expires")]
		private double _expires { get; set; }

		[JsonProperty("locale_code")]
		public string LocaleCode { get; private set; }

		[JsonProperty("with_username")]
		public bool WithUsername { get; private set; }

		[JsonProperty("activation_bytes")]
		public string ActivationBytes { get; private set; }

		[JsonIgnore]
		public Dictionary<string, string> WebsiteCookies
		{
			get => _websiteCookies.ToObject<Dictionary<string, string>>();
			private set => _websiteCookies = JObject.Parse(JsonConvert.SerializeObject(value, Converter.Settings));
		}

		[JsonIgnore]
		public string StoreAuthenticationCookie
		{
			get => _storeAuthenticationCookie.ToObject<Dictionary<string, string>>()["cookie"];
			private set => _storeAuthenticationCookie = JObject.Parse(JsonConvert.SerializeObject(new Dictionary<string, string>() { { "cookie", value } }, Converter.Settings));
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
		[JsonIgnore] public string DeviceSerialNumber => DeviceInfo.DeviceSerialNumber;
		[JsonIgnore] public string DeviceType => DeviceInfo.DeviceType;
		[JsonIgnore] public string AmazonAccountId => CustomerInfo.UserId;

		public Task<AccessToken> GetAccessTokenAsync()
			=> Task.FromResult(new AccessToken(AccessToken, AccessTokenExpires));

		public Task<AdpToken> GetAdpTokenAsync() 
			=> Task.FromResult(new AdpToken(AdpToken));

		public Task<PrivateKey> GetPrivateKeyAsync() 
			=> Task.FromResult(new PrivateKey(DevicePrivateKey));
	}

	public partial class CustomerInfo
	{
		[JsonProperty("account_pool")]
		public string AccountPool { get; set; }

		[JsonProperty("user_id")]
		public string UserId { get; set; }

		[JsonProperty("home_region")]
		public string HomeRegion { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("given_name")]
		public string GivenName { get; set; }
	}

	public partial class DeviceInfo
	{
		[JsonProperty("device_name")]
		public string DeviceName { get; set; }

		[JsonProperty("device_serial_number")]
		public string DeviceSerialNumber { get; set; }

		[JsonProperty("device_type")]
		public string DeviceType { get; set; }
	}

	public partial class Mkb79Auth
	{
		public static Mkb79Auth FromJson(string json)
			=> JsonConvert.DeserializeObject<Mkb79Auth>(json, Converter.Settings);

		public string ToJson()
			=> JObject.Parse(JsonConvert.SerializeObject(this, Converter.Settings)).ToString(Formatting.Indented);

		public async Task<Account> ToAccountAsync()
		{
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
				await GetPrivateKeyAsync(),
				await GetAdpTokenAsync(),
				await GetAccessTokenAsync(),
				refreshToken,
				WebsiteCookies.Select(c => new KeyValuePair<string, string>(c.Key, c.Value)),
				DeviceSerialNumber,
				DeviceType,
				AmazonAccountId,
				DeviceInfo.DeviceName,
				StoreAuthenticationCookie);

			return account;
		}

		public static Mkb79Auth FromAccount(Account account)
			=> new()
			{
				AccessToken = account.IdentityTokens.ExistingAccessToken.TokenValue,
				ActivationBytes = string.IsNullOrEmpty(account.DecryptKey) ? null : account.DecryptKey,
				AdpToken = account.IdentityTokens.AdpToken.Value,
				CustomerInfo = new CustomerInfo
				{
					AccountPool = "Amazon",
					GivenName = string.Empty,
					HomeRegion = "NA",
					Name = string.Empty,
					UserId = account.IdentityTokens.AmazonAccountId
				},
				DeviceInfo = new DeviceInfo
				{
					DeviceName = account.IdentityTokens.DeviceName,
					DeviceSerialNumber = account.IdentityTokens.DeviceSerialNumber,
					DeviceType = account.IdentityTokens.DeviceType,
				},
				DevicePrivateKey = account.IdentityTokens.PrivateKey,
				AccessTokenExpires = account.IdentityTokens.ExistingAccessToken.Expires,
				LocaleCode = account.Locale.CountryCode,
				WithUsername = account.Locale.WithUsername,
				RefreshToken = account.IdentityTokens.RefreshToken.Value,				
				StoreAuthenticationCookie = account.IdentityTokens.StoreAuthenticationCookie,
				WebsiteCookies = new(account.IdentityTokens.Cookies),
			};
	}

	public static class Serialize
	{
		public static string ToJson(this Mkb79Auth self) 
			=> JObject.Parse(JsonConvert.SerializeObject(self, Converter.Settings)).ToString(Formatting.Indented);
	}

	internal static class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
		};
	}
}
