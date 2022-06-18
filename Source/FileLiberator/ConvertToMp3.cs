using System;
using System.IO;
using System.Threading.Tasks;
using AAXClean;
using AAXClean.Codecs;
using DataLayer;
using Dinah.Core.ErrorHandling;
using Dinah.Core.Net.Http;
using FileManager;
using LibationFileManager;

namespace FileLiberator
{
	public class ConvertToMp3 : AudioDecodable
	{
		public override string Name => "Convert to Mp3";
		private Mp4File m4bBook;

		private long fileSize;
		private static string Mp3FileName(string m4bPath) => Path.ChangeExtension(m4bPath ?? "", ".mp3");

		public override void Cancel()
		{            
			m4bBook?.Cancel();
		}

		public static bool ValidateMp3(LibraryBook libraryBook)
		{
			var path = AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId);
			return path?.ToString()?.ToLower()?.EndsWith(".m4b") == true && !File.Exists(Mp3FileName(path));
		}

		public override bool Validate(LibraryBook libraryBook) => ValidateMp3(libraryBook);

		public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
		{
			OnBegin(libraryBook);

			try
			{
				var m4bPath = AudibleFileStorage.Audio.GetPath(libraryBook.Book.AudibleProductId);
				m4bBook = new Mp4File(m4bPath, FileAccess.Read);
				m4bBook.ConversionProgressUpdate += M4bBook_ConversionProgressUpdate;

				fileSize = m4bBook.InputStream.Length;

				OnTitleDiscovered(m4bBook.AppleTags.Title);
				OnAuthorsDiscovered(m4bBook.AppleTags.FirstAuthor);
				OnNarratorsDiscovered(m4bBook.AppleTags.Narrator);
				OnCoverImageDiscovered(m4bBook.AppleTags.Cover);

				using var mp3File = File.OpenWrite(Path.GetTempFileName());
				var lameConfig = GetLameOptions(Configuration.Instance);
				var result = await Task.Run(() => m4bBook.ConvertToMp3(mp3File, lameConfig));
				m4bBook.InputStream.Close();
				mp3File.Close();

				var proposedMp3Path = Mp3FileName(m4bPath);
				var realMp3Path = FileUtility.SaferMoveToValidPath(mp3File.Name, proposedMp3Path);
				OnFileCreated(libraryBook, realMp3Path);

				if (result == ConversionResult.Failed)
					return new StatusHandler { "Conversion failed" };
				else if (result == ConversionResult.Cancelled)
					return new StatusHandler { "Cancelled" };
				else
					return new StatusHandler();
			}
			finally
			{
				OnCompleted(libraryBook);
			}
		}

		private void M4bBook_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
		{
			var duration = m4bBook.Duration;
			var remainingSecsToProcess = (duration - e.ProcessPosition).TotalSeconds;
			var estTimeRemaining = remainingSecsToProcess / e.ProcessSpeed;

			if (double.IsNormal(estTimeRemaining))
				OnStreamingTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

			double progressPercent = 100 * e.ProcessPosition.TotalSeconds / duration.TotalSeconds;

			OnStreamingProgressChanged(
				new DownloadProgress
				{
					ProgressPercentage = progressPercent,
					BytesReceived = (long)(fileSize * progressPercent),
					TotalBytesToReceive = fileSize
				});
		}
	}
}
