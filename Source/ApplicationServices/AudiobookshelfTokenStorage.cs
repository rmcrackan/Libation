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

		if (Configuration.Instance.TokenStorageMethod != TokenStorageMethod.Encrypted)
			return plaintextToken;

		var protector = IdentityTokenStorage.Protector;
		if (protector is null)
			return plaintextToken;

		try
		{
			var encrypted = protector.Protect(plaintextToken, AssociatedData);
			return EncryptedPrefix + encrypted;
		}
		catch
		{
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
			return storedToken;

		try
		{
			return protector.Unprotect(payload, AssociatedData);
		}
		catch
		{
			return storedToken;
		}
	}
}
