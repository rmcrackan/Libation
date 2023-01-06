using AaxDecrypter;
using AAXClean;
using Dinah.Core;
using DataLayer;
using LibationFileManager;
using FileManager;
using System.Threading.Tasks;
using System.ComponentModel;
using System;
using System.IO;
using ApplicationServices;

namespace FileLiberator
{
    public class DownloadOptions : IDownloadOptions, IDisposable
    {

        public event PropertyChangedEventHandler PropertyChanged;
		private readonly IDisposable cancellation;
		public LibraryBook LibraryBook { get; }
        public LibraryBookDto LibraryBookDto { get; }
        public string DownloadUrl { get; }
        public string UserAgent { get; }
        public string AudibleKey { get; init; }
        public string AudibleIV { get; init; }
        public AaxDecrypter.OutputFormat OutputFormat { get; init; }
        public bool TrimOutputToChapterLength { get; init; }
        public bool RetainEncryptedFile { get; init; }
        public bool StripUnabridged { get; init; }
        public bool CreateCueSheet { get; init; }
        public bool DownloadClipsBookmarks { get; init; }
        public long DownloadSpeedBps { get; set; }
		public ChapterInfo ChapterInfo { get; init; }
        public bool FixupFile { get; init; }
        public NAudio.Lame.LameConfig LameConfig { get; init; }
        public bool Downsample { get; init; }
        public bool MatchSourceBitrate { get; init; }
        public ReplacementCharacters ReplacementCharacters => Configuration.Instance.ReplacementCharacters;

        public string GetMultipartFileName(MultiConvertFileProperties props)
            => Templates.ChapterFile.GetFilename(LibraryBookDto, props);

        public string GetMultipartTitleName(MultiConvertFileProperties props)
            => Templates.ChapterTitle.GetTitle(LibraryBookDto, props);

        public async Task<string> SaveClipsAndBookmarks(string fileName)
        {
            if (DownloadClipsBookmarks)
            {
                var format = Configuration.Instance.ClipsBookmarksFileFormat;

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

        private void DownloadSpeedChanged(string propertyName, long speed)
        {
			DownloadSpeedBps = speed;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DownloadSpeedBps)));
        }

		public void Dispose()
		{
			cancellation?.Dispose();
		}

		public DownloadOptions(LibraryBook libraryBook, string downloadUrl, string userAgent)
		{
            LibraryBook = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook));
            DownloadUrl = ArgumentValidator.EnsureNotNullOrEmpty(downloadUrl, nameof(downloadUrl));
            UserAgent = ArgumentValidator.EnsureNotNullOrEmpty(userAgent, nameof(userAgent));

			LibraryBookDto = LibraryBook.ToDto();

			cancellation = Configuration.Instance.SubscribeToPropertyChanged<long>(nameof(Configuration.DownloadSpeedLimit), DownloadSpeedChanged);
			// no null/empty check for key/iv. unencrypted files do not have them
		}
    }
}
