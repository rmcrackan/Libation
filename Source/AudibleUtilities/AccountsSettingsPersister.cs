using System;
using AudibleApi.Authorization;
using Dinah.Core.IO;
using Newtonsoft.Json;

namespace AudibleUtilities
{
	public class AccountsSettingsPersister : JsonFilePersister<AccountsSettings>
	{
		/// <summary>Alias for Target </summary>
		public AccountsSettings AccountsSettings => Target;

		/// <summary>uses path. create file if doesn't yet exist</summary>
		public AccountsSettingsPersister(AccountsSettings target, string path, string jsonPath = null)
			: base(target, path, jsonPath) { }

		/// <summary>load from existing file</summary>
		public AccountsSettingsPersister(string path, string jsonPath = null)
			: base(path, jsonPath) { }

		protected override JsonSerializerSettings GetSerializerSettings()
			=> Identity.GetJsonSerializerSettings();
	}
}
