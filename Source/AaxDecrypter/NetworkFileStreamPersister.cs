using Dinah.Core.IO;

namespace AaxDecrypter;

internal class NetworkFileStreamPersister : JsonFilePersister<NetworkFileStream>
{
	/// <summary>Alias for Target </summary>
	public NetworkFileStream NetworkFileStream => Target;

	/// <summary>uses path. create file if doesn't yet exist</summary>
	public NetworkFileStreamPersister(NetworkFileStream networkFileStream, string path, string? jsonPath = null)
		: base(networkFileStream, path, jsonPath) { }

	/// <summary>load from existing file</summary>
	public NetworkFileStreamPersister(string path, string? jsonPath = null)
		: base(path, jsonPath) { }

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			NetworkFileStream?.Dispose();
		base.Dispose(disposing);
	}
}
