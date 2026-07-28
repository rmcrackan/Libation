using AudibleApi.Authorization;
using LibationFileManager;

namespace ApplicationServices;

public static class AudiobookshelfTokenStorage
{
	private const string EncryptedPrefix = "enc:";
	private const string AssociatedData = "audiobookshelf-api-token";

	public static string? EncryptToken(string? plaintextToken)
	{
		if (string.IsNullOrEmpty(plaintextToken))
			return plaintextToken;

		// Guard against double-encrypting an already-encrypted value
		if (plaintextToken.StartsWith(EncryptedPrefix))
			return plaintextToken;

		if (Configuration.Instance.TokenStorageMethod != TokenStorageMethod.Encrypted)
			return plaintextToken;

		var protector = IdentityTokenStorage.Protector;
		if (protector is null)
		{
			Serilog.Log.Logger.Warning("Audiobookshelf API token encryption requested but no protector is available; storing plaintext");
			return plaintextToken;
		}

		try
		{
			var encrypted = protector.Protect(plaintextToken, AssociatedData);
			return EncryptedPrefix + encrypted;
		}
		catch (System.Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to encrypt Audiobookshelf API token; storing plaintext");
			return plaintextToken;
		}
	}

	public static string? DecryptToken(string? storedToken)
	{
		if (string.IsNullOrEmpty(storedToken))
			return storedToken;

		if (!storedToken.StartsWith(EncryptedPrefix))
			return storedToken;

		var payload = storedToken[EncryptedPrefix.Length..];
		var protector = IdentityTokenStorage.Protector;
		if (protector is null)
		{
			Serilog.Log.Logger.Warning("Audiobookshelf API token decryption requested but no protector is available; returning stored value");
			return storedToken;
		}

		try
		{
			return protector.Unprotect(payload, AssociatedData);
		}
		catch (System.Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to decrypt Audiobookshelf API token; returning stored value");
			return storedToken;
		}
	}
}
