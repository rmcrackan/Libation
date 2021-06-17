using System;
using System.IO;

namespace ffmpeg_decrypt
{
	public static class Resources
	{
		public static string resdir { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res");

		/// <summary>extract embedded resource to file if it doesn't already exist</summary>
		public static void Extract(string resourceName)
		{
			// first determine whether files exist already in res dir
			if (File.Exists(Path.Combine(resdir, resourceName)))
				return;

			// extract embedded resource
			// this technique works but there are easier ways:
			// https://stackoverflow.com/questions/13031778/how-can-i-extract-a-file-from-an-embedded-resource-and-save-it-to-disk
			Directory.CreateDirectory(resdir);
			using var resource = System.Reflection.Assembly.GetCallingAssembly().GetManifestResourceStream($"{nameof(ffmpeg_decrypt)}.res." + resourceName);
			using var reader = new BinaryReader(resource);
			using var file = new FileStream(Path.Combine(resdir, resourceName), FileMode.OpenOrCreate);
			using var writer = new BinaryWriter(file);
			writer.Write(reader.ReadBytes((int)resource.Length));
		}
	}
}
