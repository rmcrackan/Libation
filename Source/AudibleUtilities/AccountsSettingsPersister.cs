using AudibleApi.Authorization;
using Dinah.Core.IO;
using Newtonsoft.Json;
using System;

namespace AudibleUtilities;

public class AccountsSettingsPersister : JsonFilePersister<AccountsSettings>
{
	public static event EventHandler? Saving;
	public static event EventHandler? Saved;

	protected override void OnSaving() => Saving?.Invoke(null, EventArgs.Empty);
	protected override void OnSaved() => Saved?.Invoke(null, EventArgs.Empty);

	/// <summary>Alias for Target </summary>
	public AccountsSettings AccountsSettings => Target;

	/// <summary>uses path. create file if doesn't yet exist</summary>
	public AccountsSettingsPersister(AccountsSettings target, string path, string? jsonPath = null)
		: base(target, path, jsonPath) { }

	/// <summary>load from existing file</summary>
	public AccountsSettingsPersister(string path, string? jsonPath = null)
		: base(path, jsonPath) { }

	protected override JsonSerializerSettings GetSerializerSettings()
		=> Identity.GetJsonSerializerSettings();
}
