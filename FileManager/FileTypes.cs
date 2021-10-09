using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileManager
{
	public enum FileType { Unknown, Audio, AAXC, PDF, Zip, Cue }

	public static class FileTypes
	{
		private static Dictionary<string, FileType> dic => new()
		{
			["aaxc"] = FileType.AAXC,
			["cue"] = FileType.Cue,
			["pdf"] = FileType.PDF,
			["zip"] = FileType.Zip,

			["aac"] = FileType.Audio,
			["flac"] = FileType.Audio,
			["m4a"] = FileType.Audio,
			["m4b"] = FileType.Audio,
			["mp3"] = FileType.Audio,
			["mp4"] = FileType.Audio,
			["ogg"] = FileType.Audio,
		};

		public static FileType GetFileTypeFromPath(string path)
			=> dic.TryGetValue(Path.GetExtension(path).ToLower().Trim('.'), out var fileType)
			? fileType
			: FileType.Unknown;

		public static List<string> GetExtensions(FileType fileType)
			=> dic
			.Where(kvp => kvp.Value == fileType)
			.Select(kvp => kvp.Key)
			.ToList();
	}
}
