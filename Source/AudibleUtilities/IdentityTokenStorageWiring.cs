using AudibleApi.Authorization;
using Dinah.Core.IO;
using Dinah.Core.Security;
using LibationFileManager;
using System.ComponentModel;
using System.Security.Cryptography;

namespace AudibleUtilities;

/// <summary>
/// Applies Libation's <see cref="Configuration.TokenStorageMethod"/> to AudibleApi identity persistence.
/// Changing the preference alone does not convert existing tokens.
/// </summary>
public static class IdentityTokenStorageWiring
{
	public const string ApplicationName = "Libation";

	/// <summary>Env var: path to a raw 32-byte master key file (from <c>export-master-key</c>).</summary>
	public const string MasterKeyFileEnvVar = "LIBATION_MASTER_KEY_FILE";

	/// <summary>Env var: Base64-encoded 32-byte master key.</summary>
	public const string MasterKeyEnvVar = "LIBATION_MASTER_KEY";

	/// <summary>Default master key file name under the Libation files directory.</summary>
	public const string DefaultMasterKeyFileName = "libation-master.key";

	private static Lock Gate { get; } = new();
	private static Configuration? _wiredConfig;

	/// <summary>
	/// Configure AudibleApi token persistence from <paramref name="config"/> and keep it in sync when the preference changes.
	/// </summary>
	public static void Apply(Configuration config)
	{
		ArgumentNullException.ThrowIfNull(config);

		ConfigureFrom(config);

		lock (Gate)
		{
			if (ReferenceEquals(_wiredConfig, config))
				return;

			if (_wiredConfig is not null)
				_wiredConfig.PropertyChanged -= OnPropertyChanged;

			_wiredConfig = config;
			_wiredConfig.PropertyChanged += OnPropertyChanged;
		}
	}

	/// <summary>
	/// Configure AudibleApi from the current preference without attaching change listeners.
	/// Prefer <see cref="Apply"/> at application startup.
	/// </summary>
	public static void ConfigureFrom(Configuration config)
	{
		ArgumentNullException.ThrowIfNull(config);

		var method = config.TokenStorageMethod;
		var store = ResolveSecretStore(config);
		AesGcmSecretProtector? protector = store.IsAvailable
			? new AesGcmSecretProtector(store)
			: null;

		// Encrypted + unavailable store => protector null => fail-closed on encrypt/decrypt.
		IdentityTokenStorage.Configure(method, protector);
	}

	/// <summary>
	/// Resolve the secret store used for the AES-GCM master key.
	/// Supported priority: <see cref="MasterKeyFileEnvVar"/> -> existing default <see cref="DefaultMasterKeyFileName"/>
	/// under Libation files -> <see cref="MasterKeyEnvVar"/> -> OS-bound <see cref="OsSecretStore"/>.
	/// If the OS store is unavailable, falls through to
	/// <see cref="TryCreateLastResortPortableMasterKeyStore"/> (headless compatibility path; not the preferred setup).
	/// </summary>
	public static IOsSecretStore ResolveSecretStore(Configuration config)
	{
		ArgumentNullException.ThrowIfNull(config);

		var keyFileEnv = Environment.GetEnvironmentVariable(MasterKeyFileEnvVar);
		if (!string.IsNullOrWhiteSpace(keyFileEnv))
			return LoadMasterKeyFileOrUnavailable(keyFileEnv.Trim());

		var defaultKeyPath = Path.Combine(config.LibationFiles.Location, DefaultMasterKeyFileName);
		if (File.Exists(defaultKeyPath))
			return LoadMasterKeyFileOrUnavailable(defaultKeyPath);

		var keyEnv = Environment.GetEnvironmentVariable(MasterKeyEnvVar);
		if (!string.IsNullOrWhiteSpace(keyEnv))
			return LoadMasterKeyBase64OrUnavailable(keyEnv.Trim());

		var osStore = OsSecretStore.Create(ApplicationName);
		if (osStore.IsAvailable)
			return osStore;

		return TryCreateLastResortPortableMasterKeyStore(config);
	}

	/// <summary>True when the OS secret store can hold Libation's encryption master key.</summary>
	public static bool IsOsSecretStoreAvailable(out string? unavailableReason)
	{
		var store = OsSecretStore.Create(ApplicationName);
		unavailableReason = store.IsAvailable ? null : store.UnavailableReason;
		return store.IsAvailable;
	}

	/// <summary>True when any resolved secret store (portable or OS) can supply an encryption master key.</summary>
	public static bool IsEncryptionKeyAvailable(Configuration config, out string? unavailableReason)
	{
		ArgumentNullException.ThrowIfNull(config);
		var store = ResolveSecretStore(config);
		unavailableReason = store.IsAvailable ? null : store.UnavailableReason;
		return store.IsAvailable;
	}

	/// <summary>
	/// Headless / Docker compatibility path when encryption is enabled but there is no OS secret store
	/// and the user did not supply a master key.
	/// Prefer setting <see cref="Configuration.TokenStorageMethod"/> to plaintext, or supplying an exported
	/// <see cref="DefaultMasterKeyFileName"/> / env key, instead of relying on this path.
	/// Creates <see cref="DefaultMasterKeyFileName"/> under Libation files if missing, then loads it.
	/// </summary>
	internal static IOsSecretStore TryCreateLastResortPortableMasterKeyStore(Configuration config)
	{
		ArgumentNullException.ThrowIfNull(config);

		var keyPath = Path.Combine(config.LibationFiles.Location, DefaultMasterKeyFileName);
		try
		{
			if (!File.Exists(keyPath))
				MintRawMasterKeyFile(keyPath);

			return LoadMasterKeyFileOrUnavailable(keyPath);
		}
		catch (Exception ex)
		{
			return new UnavailablePortableSecretStore(
				"Last-resort portable master key",
				$"Could not create or load last-resort portable master key ({keyPath}): {SafeMessage(ex)}");
		}
	}

	/// <summary>Write a new raw 32-byte master key file (same format as <c>export-master-key</c>).</summary>
	private static void MintRawMasterKeyFile(string keyPath)
	{
		var key = new byte[AesGcmSecretProtector.KeySizeBytes];
		RandomNumberGenerator.Fill(key);
		try
		{
			var directory = Path.GetDirectoryName(Path.GetFullPath(keyPath));
			if (!string.IsNullOrEmpty(directory))
				Directory.CreateDirectory(directory);

			AtomicFileWriter.WriteAllBytes(keyPath, key);
		}
		finally
		{
			CryptographicOperations.ZeroMemory(key);
		}
	}

	private static IOsSecretStore LoadMasterKeyFileOrUnavailable(string path)
	{
		var store = new MemoryOsSecretStore();
		try
		{
			MasterKeyPortability.ImportFromFile(store, path);
			return store;
		}
		catch (Exception ex)
		{
			return new UnavailablePortableSecretStore(
				"Portable master key file",
				$"Portable master key file is unusable ({path}): {SafeMessage(ex)}");
		}
	}

	private static IOsSecretStore LoadMasterKeyBase64OrUnavailable(string base64)
	{
		byte[] key;
		try
		{
			key = Convert.FromBase64String(base64);
		}
		catch (FormatException ex)
		{
			return new UnavailablePortableSecretStore(
				"Portable master key env",
				$"{MasterKeyEnvVar} is not valid Base64: {SafeMessage(ex)}");
		}

		try
		{
			if (key.Length != AesGcmSecretProtector.KeySizeBytes)
			{
				return new UnavailablePortableSecretStore(
					"Portable master key env",
					$"{MasterKeyEnvVar} must decode to {AesGcmSecretProtector.KeySizeBytes} bytes.");
			}

			var store = new MemoryOsSecretStore();
			store.Set(AesGcmSecretProtector.DefaultMasterKeyName, key);
			return store;
		}
		finally
		{
			CryptographicOperations.ZeroMemory(key);
		}
	}

	private static string SafeMessage(Exception ex)
		=> string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;

	private static void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(Configuration.TokenStorageMethod))
			return;
		if (sender is not Configuration config)
			return;

		ConfigureFrom(config);
	}

	private sealed class UnavailablePortableSecretStore : IOsSecretStore
	{
		public UnavailablePortableSecretStore(string name, string reason)
		{
			Name = name;
			UnavailableReason = reason;
		}

		public string Name { get; }
		public bool IsAvailable => false;
		public string? UnavailableReason { get; }

		public void Set(string key, ReadOnlySpan<byte> value)
			=> throw new OsSecretStoreUnavailableException(Name, UnavailableReason!);

		public bool TryGet(string key, out byte[] value)
			=> throw new OsSecretStoreUnavailableException(Name, UnavailableReason!);

		public void Delete(string key)
			=> throw new OsSecretStoreUnavailableException(Name, UnavailableReason!);
	}
}
