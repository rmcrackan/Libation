using AAXClean;
using Dinah.Core;
using FileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LibationFileManager
{
	public static class AudioFileLocator
	{
		private static EnumerationOptions enumerationOptions { get; } = new()
		{
			RecurseSubdirectories = true,
			IgnoreInaccessible = true,
			MatchCasing = MatchCasing.CaseInsensitive
		};

		public static async IAsyncEnumerable<FilePathCache.CacheEntry> FindAudiobooks(LongPath searchDirectory, [EnumeratorCancellation] CancellationToken cancellationToken)
		{
			ArgumentValidator.EnsureNotNull(searchDirectory, nameof(searchDirectory));

			foreach (LongPath path in Directory.EnumerateFiles(searchDirectory, "*.M4B", enumerationOptions))
			{
				if (cancellationToken.IsCancellationRequested)
					yield break;

				int generation = 0;
				FilePathCache.CacheEntry audioFile = default;

				try
				{
					using var fileStream = File.OpenRead(path);

					var mp4File = await Task.Run(() => new Mp4File(fileStream), cancellationToken);

					generation = GC.GetGeneration(mp4File);

					if (mp4File?.AppleTags?.Asin is not null)
						audioFile = new FilePathCache.CacheEntry(mp4File.AppleTags.Asin, FileType.Audio, path);

				}
				catch(Exception ex)
				{
					Serilog.Log.Error(ex, "Error checking for asin in {@file}", path);
				}
				finally
				{
					GC.Collect(generation);
				}

				if (audioFile is not null)
					yield return audioFile;
			}
		}
	}
}
