using AaxDecrypter;
using AAXClean;
using Dinah.Core;
using DataLayer;
using LibationFileManager;
using System.Threading.Tasks;
using System;
using System.IO;
using ApplicationServices;
using LibationFileManager.Templates;

namespace FileLiberator
{
	public partial class DownloadOptions : IDownloadOptions, IDisposable
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
		public string Title => LibraryBook.Book.Title;
		public string Subtitle => LibraryBook.Book.Subtitle;
		public string Publisher => LibraryBook.Book.Publisher;
		public string Language => LibraryBook.Book.Language;
		public string AudibleProductId => LibraryBookDto.AudibleProductId;
		public string SeriesName => LibraryBookDto.FirstSeries?.Name;
		public float? SeriesNumber => LibraryBookDto.FirstSeries?.Number;
		public NAudio.Lame.LameConfig LameConfig { get; init; }
		public string UserAgent => AudibleApi.Resources.Download_User_Agent;
		public bool TrimOutputToChapterLength => Config.AllowLibationFixup && Config.StripAudibleBrandAudio;
		public bool StripUnabridged => Config.AllowLibationFixup && Config.StripUnabridged;
		public bool CreateCueSheet => Config.CreateCueSheet;
		public bool DownloadClipsBookmarks => Config.DownloadClipsBookmarks;
		public long DownloadSpeedBps => Config.DownloadSpeedLimit;
		public bool RetainEncryptedFile => Config.RetainAaxFile;
		public bool FixupFile => Config.AllowLibationFixup;
		public bool Downsample => Config.AllowLibationFixup && Config.LameDownsampleMono;
		public bool MatchSourceBitrate => Config.AllowLibationFixup && Config.LameMatchSourceBR && Config.LameTargetBitrate;
		public bool MoveMoovToBeginning => Config.MoveMoovToBeginning;
		public AAXClean.FileType? InputType { get; init; }
		public AudibleApi.Common.DrmType DrmType { get; init; }
		public AudibleApi.Common.ContentMetadata ContentMetadata { get; init; }

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
				var format = Config.ClipsBookmarksFileFormat;

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

		public Configuration Config { get; }
		private readonly IDisposable cancellation;
		public void Dispose()
		{
			cancellation?.Dispose();
			GC.SuppressFinalize(this);
		}

		private DownloadOptions(Configuration config, LibraryBook libraryBook, [System.Diagnostics.CodeAnalysis.NotNull] string downloadUrl)
		{
			Config = ArgumentValidator.EnsureNotNull(config, nameof(config));
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
