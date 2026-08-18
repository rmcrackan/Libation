#:package AudibleApi@11.0.3.1

using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Adds fake Audible accounts to AccountsSettings.json, so the parts of Libation that only appear once an
// account exists can be tested without signing in to Audible.
//
//   dotnet run Scripts/seed-demo-accounts.cs [--count 1] [--clean] [path-to-Libation-files-folder]
//
// Run Libation at least once first so AccountsSettings.json exists, and close it before seeding: the file is
// read at startup and written back on save, so a running app would overwrite whatever this wrote.
//
// The accounts carry structurally valid but meaningless tokens. Libation will count them, list them and
// enable everything that is gated on having an account, but any scan or download attempt fails at Audible.
//
// Real accounts in the file are left alone. --clean removes only the accounts this script added, which are
// the ones whose AccountId matches demo*@example.com.
//
// Keep the package version above in step with AudibleApi in Source/AudibleUtilities/AudibleUtilities.csproj,
// so the JSON this writes is the shape the app expects to read.

// Fixture material. A syntactically valid RSA key and ADP token so the identity parses; neither authenticates.
const string SamplePrivateKey = @"
-----BEGIN RSA PRIVATE KEY-----
MIIEpgIBAAKCAQEA5nPbGSVDmlEH2tJa6kz/P2HI8IeirhfPHdmi+X/nsb9i3WNf
tmEdZxfK26IValQDXvBH17a1gr0HD6pYse1XsV2w0HxiW1RW+ZnjL8/fzPdkSOb+
4xKlqRopCueBSdDGgAF06spZ3IeHLfEFOJX4dO1Y73pFBUkA0k53LT12L2Tjay/r
buZHJqIzxmwja7/nkiWL0Xo7UySHtQACYsKEatu6yHBS+cPTlGR/qeUpeJTHwDLP
7ZQ7kWzJGY1mfInYekjlZLsMsWswso3pg1vPyHgxzM2BWhY8m6mlXQ9G/USxBTib
MNuMtpR73XsgamneFCc+Uv1cxw7ofZ41YOOAbQIDAQABAoIBAQDIre8HkKm0Aggj
B7df/TjxCsgenR6PF/Cmf9UqC7XJ1W3UeCrq+NrP4aonZJfdhdeBnyAQuuyJMu6p
N6ARISuSKpJEm2xTN7idluJ9yjmLlYtg6LbhKmXUQhGniz3M999DrQERTLDAF80h
tpbjVcWMnPsrX4AnQBFVEjs5zCHU1hD+X463EmUHBWyT975jbZ8Fy7/fTzkdzLnn
qE5lROALr2MCAAwQRFbRE6dd52vnXaBrVcAtRzjATts3WG3+SNi2Fm/OrYqQcY9e
lBexNviT8VcldOAMrO10E2u0d+tvxFzwB3ABMvaVamrEZky4XSfB6aLzpD0JJj1s
UHnIiVwJAoGBAPl8nLll/J9rud/N2HiAX2YkP0MC0HW4yM3KxLtXKyXrP5qBpaci
wTDUmSWEEE3GUJMM1Z4d9tl9Lz2MhU2KqkEvLI3kQ7aUu33PYUBGMVcUzhFQ49lU
Nzz8YB183iqo31o/DKk2Cr5gI7SykQZ0gn/urZkEJeErLzlhPXcyeY5jAoGBAOx4
CGucVdv5MbdXZP8jVzxuvUlSp7BIQJ2phQXDFBNApFKnZn7yBYBx7dqzleymGm+R
INZAurg3SNw4nvbQc3Z2dJ8I+n5ErjFCKp1IedVxx1eMEfecTwrQZuUwLISIyjqF
czSJNwcNqzCx67z397/Cg5K/0pu6uIe0r7xozcbvAoGBAOOvZ9CDVPOg+rdXQvFm
Jqou9lUPonNtOkUlgjl+qfAnK5q0KxvHSgxoWYO1bLOuAybQlbuBmSCPcKd5MMa9
f/eRN9YetfVQ83Mz6YshBDJ22EFRUz+p7eeIY6dFp/PCvmO8Gq/qlA996dglBtmf
RuG+T0vQT0mZgbWaGuBHfkwFAoGBAMOLg1MRxgKRMKavk6pU3EfyP3+J5XemWCDI
1WLtbgV5uClNmzmxBBGypQHs7jbzKPtHpULn5kB+HzdVb0clG8ZDsK7u6s5OF0pO
sBS+oVl7rF/eSeFcFhUYP26ZhsbWo3z/bERuj926VO2AxDPRTsP5o3pQPGZhY0V9
irGgbUJrAoGBAOseS3J4BqYM4R3Hr7cRAhvzSjIkeTcDF1zTOa4FZDHBxZ6g2PNq
8ekhtfn1zPczsPTF1vNuqEISKLxaPkVPiw0mtaZQjVwpF/IOxMNjWVLp6oJf8Mm2
BxlXqPnQ4mG66oqSFQgDEmFdMhRb2of6xL1gYYL62C80G2T7QtmPfSab
-----END RSA PRIVATE KEY-----
";

const string SampleAdpToken = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";

const string DemoAccountSuffix = "@example.com";
const string DemoAccountPrefix = "demo";

var clean = args.Contains("--clean", StringComparer.OrdinalIgnoreCase);
var count = IntArg("--count") ?? 1;
var pathArg = args.FirstOrDefault(a => !a.StartsWith("--") && !int.TryParse(a, out _));

if (count < 1)
{
	Console.Error.WriteLine("--count must be at least 1.");
	return 1;
}

if (FindLibationFiles(pathArg) is not string libationFiles)
{
	Console.Error.WriteLine("Could not find a Libation accounts file.");
	Console.Error.WriteLine("Run Libation once to create it, then pass its folder, e.g.:");
	Console.Error.WriteLine(@"  dotnet run Scripts/seed-demo-accounts.cs C:\Users\me\AppData\Local\Libation");
	return 1;
}

var accountsPath = Path.Combine(libationFiles, "AccountsSettings.json");
Console.WriteLine($"Accounts file: {accountsPath}");

var root = JObject.Parse(File.ReadAllText(accountsPath));
if (root["Accounts"] is not JArray accounts)
{
	accounts = [];
	root["Accounts"] = accounts;
}

static bool IsDemo(JToken account)
	=> account["AccountId"]?.Value<string>() is string id
	&& id.StartsWith(DemoAccountPrefix, StringComparison.OrdinalIgnoreCase)
	&& id.EndsWith(DemoAccountSuffix, StringComparison.OrdinalIgnoreCase);

if (clean)
{
	var removed = accounts.Where(IsDemo).ToList();
	foreach (var account in removed)
		account.Remove();

	File.WriteAllText(accountsPath, root.ToString(Formatting.Indented));
	Console.WriteLine($"Removed {removed.Count} demo account(s). {accounts.Count} account(s) remain.");
	return 0;
}

var realAccounts = accounts.Count(a => !IsDemo(a));
if (realAccounts > 0)
	Console.WriteLine($"Leaving {realAccounts} real account(s) untouched.");

var added = 0;
foreach (var accountId in DemoAccountIds(count))
{
	if (accounts.Any(a => string.Equals(a["AccountId"]?.Value<string>(), accountId, StringComparison.OrdinalIgnoreCase)))
	{
		Console.WriteLine($"  {accountId} already present, skipping");
		continue;
	}

	accounts.Add(BuildAccount(accountId));
	added++;
}

File.WriteAllText(accountsPath, root.ToString(Formatting.Indented));

Console.WriteLine($"Added {added} demo account(s). Libation will now see {accounts.Count} account(s).");
Console.WriteLine();
Console.WriteLine("Start Libation. What the account count changes:");
Console.WriteLine("  1 account   Import > Scan Library, Import > Remove Library Books");
Console.WriteLine("  2 or more   Import > Scan Library of All / Some Accounts, and the matching Remove items");
Console.WriteLine("  any         an empty library offers 'Scan Library' rather than 'Add Account'");
Console.WriteLine();
Console.WriteLine("Do not scan or download: these accounts have meaningless tokens and Audible will refuse them.");
return 0;

/// <summary>
/// The first account matches the one seed-demo-library.cs assigns its books to, so seeded books and a
/// seeded account describe the same library rather than two unrelated ones.
/// </summary>
static IEnumerable<string> DemoAccountIds(int count)
{
	yield return $"{DemoAccountPrefix}{DemoAccountSuffix}";

	for (var n = 2; n <= count; n++)
		yield return $"{DemoAccountPrefix}{n}{DemoAccountSuffix}";
}

/// <summary>
/// An account entry carrying a registered-looking identity. The values are fixture material, the same shape
/// AudibleApi writes after a real registration, so the file parses and the account loads.
/// </summary>
static JObject BuildAccount(string accountId)
{
	var identity = new Identity(Localization.Get("us"));
	identity.Update(
		new PrivateKey(SamplePrivateKey),
		new AdpToken(SampleAdpToken),
		new AccessToken($"Atna|_DEMO_ACCESS_{accountId}", new DateTime(2200, 1, 1, 12, 0, 0, DateTimeKind.Utc)),
		new RefreshToken($"Atnr|_DEMO_REFRESH_{accountId}"),
		[new KeyValuePair<string, Dinah.Core.Security.SecretString>("session-id", "demo-cookie-value")],
		deviceSerialNumber: "demo-device-serial",
		deviceType: "demo-device-type",
		amazonAccountId: "demo-amazon-account",
		deviceName: "Demo Device",
		storeAuthenticationCookie: "demo-store-auth-cookie");

	// Write the tokens as plaintext rather than reaching for the OS secret store, which this script has no
	// business touching. Libation reads either, whatever its own Token storage preference is set to.
	IdentityTokenStorage.Configure(TokenStorageMethod.Plaintext, protector: null);

	return new JObject
	{
		["AccountId"] = accountId,
		["AccountName"] = $"Demo ({accountId})",
		["LibraryScan"] = true,
		["DecryptKey"] = "",
		["IdentityTokens"] = JObject.Parse(JsonConvert.SerializeObject(identity, Identity.GetJsonSerializerSettings()))
	};
}

int? IntArg(string name)
{
	var i = Array.FindIndex(args, a => a.Equals(name, StringComparison.OrdinalIgnoreCase));
	return i >= 0 && i + 1 < args.Length && int.TryParse(args[i + 1], out var value) ? value : null;
}

/// <summary>Locate the Libation files folder the same way Libation itself does.</summary>
static string? FindLibationFiles(string? explicitPath)
{
	var candidates = new List<string>();

	if (explicitPath is not null)
		candidates.Add(explicitPath);

	foreach (var folder in KnownLibationFolders())
	{
		candidates.Add(folder);

		// appsettings.json may redirect the files folder somewhere else entirely.
		var appSettings = Path.Combine(folder, "appsettings.json");
		if (!File.Exists(appSettings))
			continue;

		try
		{
			if (System.Text.Json.JsonDocument.Parse(File.ReadAllText(appSettings)).RootElement
				.TryGetProperty("LibationFiles", out var redirect) && redirect.GetString() is string path)
				candidates.Add(path);
		}
		catch (System.Text.Json.JsonException) { }
	}

	return candidates.FirstOrDefault(c => File.Exists(Path.Combine(c, "AccountsSettings.json")));
}

static IEnumerable<string> KnownLibationFolders()
{
	yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Libation");
	yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Libation");
	yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Libation");
	yield return Path.Combine(Path.GetTempPath(), $"Libation-{Environment.UserName}");
	yield return AppContext.BaseDirectory;
}
