using Dinah.Core.Security;

namespace AudibleUtilities;

/// <summary>
/// Export Libation's OS-bound AES-GCM master key for portable use (e.g. Docker).
/// Never creates a new key; the desktop OS store must already hold one.
/// </summary>
public static class MasterKeyExport
{
	/// <summary>
	/// Write the existing Libation master key as raw bytes to <paramref name="filePath"/>.
	/// </summary>
	/// <exception cref="InvalidOperationException">OS secret store unavailable or master key missing.</exception>
	public static void ExportToFile(string filePath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

		if (!IdentityTokenStorageWiring.IsOsSecretStoreAvailable(out var unavailableReason))
		{
			throw new InvalidOperationException(
				"Cannot export the encryption master key because the OS secret store is unavailable."
				+ (string.IsNullOrWhiteSpace(unavailableReason) ? "" : " " + unavailableReason));
		}

		var store = OsSecretStore.Create(IdentityTokenStorageWiring.ApplicationName);
		try
		{
			MasterKeyPortability.ExportToFile(store, filePath);
		}
		catch (SecretProtectionException ex)
		{
			throw new InvalidOperationException(
				"Cannot export the encryption master key because it was not found. "
				+ "Encrypt tokens on this machine at least once (Settings -> Important, store tokens encrypted) so a key exists, then try again.",
				ex);
		}
	}
}
