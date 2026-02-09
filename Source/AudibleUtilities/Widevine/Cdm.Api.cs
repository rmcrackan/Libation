using AudibleApi;
using AudibleApi.Cryptography;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AudibleUtilities.Widevine;

public partial class Cdm
{
	/// <summary>
	/// Get a <see cref="Cdm"/> from <see cref="AccountsSettings"/> or from the API.
	/// </summary>
	/// <returns>A <see cref="Cdm"/> if successful, otherwise <see cref="null"/></returns>
	public static async Task<Cdm?> GetCdmAsync()
	{
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();

		//Check if there are any Android accounts. If not, we can't use Widevine.
		if (!persister.Target.Accounts.Any(a => a.IdentityTokens?.DeviceType == Resources.DeviceType))
			return null;

		if (!string.IsNullOrEmpty(persister.Target.Cdm))
		{
			try
			{
				var cdm = Convert.FromBase64String(persister.Target.Cdm);
				return new Cdm(new Device(cdm));
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error loading CDM from account settings.");
				persister.Target.Cdm = string.Empty;
				//Clear the stored Cdm and try getting a fresh one from the server.
			}
		}

		if (string.IsNullOrEmpty(persister.Target.Cdm))
		{
			using var client = new HttpClient();
			if (await GetCdmUris(client) is not Uri[] uris)
				return null;

			//try to get a CDM file for any account that's registered as an android device.
			//CDMs are not account-specific, so it doesn't matter which account we're successful with.
			foreach (var account in persister.Target.Accounts.Where(a => a.IdentityTokens?.DeviceType == Resources.DeviceType))
			{
				try
				{
					var requestMessage = CreateApiRequest(account);

					await TestApiRequest(client, new JsonObject { { "body", requestMessage.ToString() } });

					//Try all CDM URIs until a CDM has been retrieved successfully
					foreach (var uri in uris)
					{
						try
						{
							var resp = await client.PostAsync(uri, ((HttpBody)requestMessage).Content);

							if (!resp.IsSuccessStatusCode)
							{
								var message = await resp.Content.ReadAsStringAsync();
								throw new ApiErrorException(uri, null, message);
							}

							var cdmBts = await resp.Content.ReadAsByteArrayAsync();
							var device = new Device(cdmBts);
							persister.Target.Cdm = Convert.ToBase64String(cdmBts);
							return new Cdm(device);
						}
						catch (Exception ex)
						{
							Serilog.Log.Logger.Error(ex, "Error getting a CDM from URI: " + uri);
							//try the next URI
						}
					}
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error getting a CDM for account: " + account.MaskedLogEntry);
					//try the next Account
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Get a list of CDM API URIs from the main Gitgub repository's .cdmurls.json file.
	/// </summary>
	/// <returns>If successful, an array of URIs to try. Otherwise null</returns>
	private static async Task<Uri[]?> GetCdmUris(HttpClient httpClient)
	{
		const string CdmUrlListFile = "https://raw.githubusercontent.com/rmcrackan/Libation/refs/heads/master/.cdmurls.json";

		try
		{
			var fileContents = await httpClient.GetStringAsync(CdmUrlListFile);
			var releaseIndex = JObject.Parse(fileContents);
			var urlArray = releaseIndex["CdmUrls"] as JArray;
			if (urlArray is null)
				throw new System.IO.InvalidDataException("CDM url list not found in JSON: " + fileContents);

			var uris = urlArray.Select(u => u.Value<string>()).OfType<string>().Select(u => new Uri(u)).ToArray();

			if (uris.Length == 0)
				throw new System.IO.InvalidDataException("No CDM url found in JSON: " + fileContents);

			return uris;
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error getting CDM URLs");
			return null;
		}
	}


	static readonly string[] TLDs = ["com", "co.uk", "com.au", "com.br", "ca", "fr", "de", "in", "it", "co.jp", "es"];

	//Ensure that the request can be made successfully before sending it to the API
	//The API uses System.Text.Json, so perform test with same.
	private static async Task TestApiRequest(HttpClient client, JsonObject input)
	{
		if (input["body"]?.GetValue<string>() is not string body
				|| JsonNode.Parse(body) is not JsonNode bodyJson)
			throw new Exception("Api request doesn't contain a body");

		if (bodyJson?["Url"]?.GetValue<string>() is not string url
			|| !Uri.TryCreate(url, UriKind.Absolute, out var uri))
			throw new Exception("Api request doesn't contain a url");

		if (!TLDs.Select(tld => "api.audible." + tld).Contains(uri.Host.ToLower()))
			throw new Exception($"Unknown Audible Api domain: {uri.Host}");

		if (bodyJson?["Headers"] is not JsonObject headers)
			throw new Exception($"Api request doesn't contain any headers");

		using var request = new HttpRequestMessage(HttpMethod.Get, uri);

		Dictionary<string, string>? headersDict = null;
		try
		{
			headersDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(headers);
		}
		catch (Exception ex)
		{
			throw new Exception("Failed to read Audible Api headers.", ex);
		}

		if (headersDict is null)
			throw new Exception("Failed to read Audible Api headers.");

		foreach (var kvp in headersDict)
			request.Headers.Add(kvp.Key, kvp.Value);

		using var resp = await client.SendAsync(request);
		resp.EnsureSuccessStatusCode();
	}

	/// <summary>
	/// Create a request body to send to the API
	/// </summary>
	/// <param name="account">An authenticated account</param>
	private static JObject CreateApiRequest(Account account)
	{
		const string ACCOUNT_INFO_PATH = "/1.0/account/information";

		if (account?.Locale is null)
			throw new ArgumentException("Account does not have a valid locale.", nameof(account));
		if (account.IdentityTokens?.AdpToken is null || account.IdentityTokens.PrivateKey is null)
			throw new ArgumentException("Account does not have valid identity tokens.", nameof(account));

		var message = new HttpRequestMessage(HttpMethod.Get, ACCOUNT_INFO_PATH);

		message.SignRequest(
					DateTime.UtcNow,
					account.IdentityTokens.AdpToken,
					account.IdentityTokens.PrivateKey);

		return new JObject
		{
			{ "Url", new Uri(account.Locale.AudibleApiUri(), ACCOUNT_INFO_PATH) },
			{ "Headers", JObject.FromObject(message.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Single())) }
		};
	}
}
