using AudibleApi.Authorization;
using Dinah.Core.Security;
using LibationFileManager;
using System.ComponentModel;

namespace AudibleUtilities;

/// <summary>
/// Applies Libation's <see cref="Configuration.TokenStorageMethod"/> to AudibleApi identity persistence.
/// Changing the preference alone does not convert existing tokens.
/// </summary>
public static class IdentityTokenStorageWiring
{
	public const string ApplicationName = "Libation";

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
		var store = OsSecretStore.Create(ApplicationName);
		AesGcmSecretProtector? protector = store.IsAvailable
			? new AesGcmSecretProtector(store)
			: null;

		// Encrypted + unavailable store => protector null => fail-closed on encrypt/decrypt.
		IdentityTokenStorage.Configure(method, protector);
	}

	/// <summary>True when the OS secret store can hold Libation's encryption master key.</summary>
	public static bool IsOsSecretStoreAvailable(out string? unavailableReason)
	{
		var store = OsSecretStore.Create(ApplicationName);
		unavailableReason = store.IsAvailable ? null : store.UnavailableReason;
		return store.IsAvailable;
	}

	private static void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName != nameof(Configuration.TokenStorageMethod))
			return;
		if (sender is not Configuration config)
			return;

		ConfigureFrom(config);
	}
}
