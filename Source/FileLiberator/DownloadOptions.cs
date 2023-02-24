using AaxDecrypter;
using AAXClean;
using Dinah.Core;
using DataLayer;
using LibationFileManager;
using System.Threading.Tasks;
using System;
using System.IO;
using ApplicationServices;

namespace FileLiberator
{
	public class DownloadOptions : IDownloadOptions, IDisposable
	{
		public event EventHandler<long> DownloadSpeedChanged;
		public LibraryBook LibraryBook { get; }
		public LibraryBookDto LibraryBookDto { get; }
		public string DownloadUrl { get; }
		public string AudibleKey { get; init; }
		public string AudibleIV { get; init; }
		public TimeSpan RuntimeLength { get; init; }
		public OutputFormat OutputFormat { get; init; }
		public ChapterInfo ChapterInfo { get; init; }
		public NAudio.Lame.LameConfig LameConfig { get; init; }
		public string UserAgent => AudibleApi.Resources.Download_User_Agent;
		public bool TrimOutputToChapterLength => config.AllowLibationFixup && config.StripAudibleBrandAudio;
		public bool StripUnabridged => config.AllowLibationFixup && config.StripUnabridged;
		public bool CreateCueSheet => config.CreateCueSheet;
		public bool DownloadClipsBookmarks => config.DownloadClipsBookmarks;
		public long DownloadSpeedBps => config.DownloadSpeedLimit;
		public bool RetainEncryptedFile => config.RetainAaxFile;
		public bool FixupFile => config.AllowLibationFixup;
		public bool Downsample => config.AllowLibationFixup && config.LameDownsampleMono;
		public bool MatchSourceBitrate => config.AllowLibationFixup && config.LameMatchSourceBR && config.LameTargetBitrate;
		public bool MoveMoovToBeginning => config.MoveMoovToBeginning;

		public string GetMultipartFileName(MultiConvertFileProperties props)
		{
			var baseDir = Path.GetDirectoryName(props.OutputFileName);
			var extension = Path.GetExtension(props.OutputFileName);
			return Templates.ChapterFile.GetFilename(LibraryBookDto, props, baseDir, extension);
		}

		public string GetMultipartTitle(MultiConvertFileProperties props)
			=> Templates.ChapterTitle.GetName(LibraryBookDto, props);

		public async Task<string> SaveClipsAndBookmarksAsync(string fileName)
		{
			if (DownloadClipsBookmarks)
			{
				var format = config.ClipsBookmarksFileFormat;

				var formatExtension = format.ToString().ToLowerInvariant();
				var filePath = Path.ChangeExtension(fileName, formatExtension);

				var api = await LibraryBook.GetApiAsync();
				var records = await api.GetRecordsAsync(LibraryBook.Book.AudibleProductId);

				switch(format)
				{
					case Configuration.ClipBookmarkFormat.CSV:
						RecordExporter.ToCsv(filePath, records);
						break;
					case Configuration.ClipBookmarkFormat.Xlsx:
						RecordExporter.ToXlsx(filePath, records);
						break;
					case Configuration.ClipBookmarkFormat.Json:
						RecordExporter.ToJson(filePath, LibraryBook, records);
						break;
				}
				return filePath;
			}
			return string.Empty;
		}

		private readonly Configuration config;
		private readonly IDisposable cancellation;
		public void Dispose() => cancellation?.Dispose();

		public DownloadOptions(Configuration config, LibraryBook libraryBook, string downloadUrl)
		{
			this.config = ArgumentValidator.EnsureNotNull(config, nameof(config));
			LibraryBook = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook));
			DownloadUrl = ArgumentValidator.EnsureNotNullOrEmpty(downloadUrl, nameof(downloadUrl));
			// no null/empty check for key/iv. unencrypted files do not have them

			LibraryBookDto = LibraryBook.ToDto();

			cancellation =
				config
				.ObservePropertyChanged<long>(
					nameof(Configuration.DownloadSpeedLimit),
					newVal => DownloadSpeedChanged?.Invoke(this, newVal));
		}
	}
}
