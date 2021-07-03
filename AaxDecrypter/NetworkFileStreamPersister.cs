using Dinah.Core.IO;
using Newtonsoft.Json;

namespace AaxDecrypter
{
    internal class NetworkFileStreamPersister : JsonFilePersister<NetworkFileStream>
    {

		/// <summary>Alias for Target </summary>
		public NetworkFileStream Identity => Target;

		/// <summary>uses path. create file if doesn't yet exist</summary>
		public NetworkFileStreamPersister(NetworkFileStream networkFileStream, string path, string jsonPath = null)
			: base(networkFileStream, path, jsonPath) { }

		/// <summary>load from existing file</summary>
		public NetworkFileStreamPersister(string path, string jsonPath = null)
			: base(path, jsonPath) { }

		protected override JsonSerializerSettings GetSerializerSettings() => NetworkFileStream.GetJsonSerializerSettings();

    }
}
