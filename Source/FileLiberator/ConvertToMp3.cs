using System;
using System.IO;
using System.Linq;
using System.Threading;
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
	public class ConvertToMp3 : AudioDecodable, IProcessable<ConvertToMp3>
	{
		public override string Name => "Convert to Mp3";
		private Mp4Operation Mp4Operation;
		private readonly AaxDecrypter.AverageSpeed averageSpeed = new();
		private static string Mp3FileName(string m4bPath) => Path.ChangeExtension(m4bPath ?? "", ".mp3");

		private CancellationTokenSource CancellationTokenSource { get; set; }
		public override async Task CancelAsync()
		{
			await CancellationTokenSource.CancelAsync();
			if (Mp4Operation is not null)
				await Mp4Operation.CancelAsync();
		}

		public static bool ValidateMp3(LibraryBook libraryBook)
		{
			var paths = AudibleFileStorage.Audio.GetPaths(libraryBook.Book.AudibleProductId);
			return paths.Any(path => path?.ToString()?.ToLower()?.EndsWith(".m4b") == true && !File.Exists(Mp3FileName(path)));
		}

		public override bool Validate(LibraryBook libraryBook) => ValidateMp3(libraryBook);

		public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
		{
			OnBegin(libraryBook);
			var cancellationToken = (CancellationTokenSource = new()).Token;

			try
			{
				var m4bPaths = AudibleFileStorage.Audio.GetPaths(libraryBook.Book.AudibleProductId)
					.Where(m4bPath => File.Exists(m4bPath))
					.Select(m4bPath => new { m4bPath, proposedMp3Path = Mp3FileName(m4bPath), m4bSize = new FileInfo(m4bPath).Length })
					.Where(p => !File.Exists(p.proposedMp3Path))
					.ToArray();

				long totalInputSize = m4bPaths.Sum(p => p.m4bSize);
				long sizeOfCompletedFiles = 0L;
				foreach (var entry in m4bPaths)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (File.Exists(entry.proposedMp3Path) || !File.Exists(entry.m4bPath))
					{
						sizeOfCompletedFiles += entry.m4bSize;
						continue;
					}

					using var m4bFileStream = File.Open(entry.m4bPath, FileMode.Open, FileAccess.Read, FileShare.Read);
					var m4bBook = new Mp4File(m4bFileStream);

					//AAXClean.Codecs only supports decoding AAC and E-AC-3 audio.
					if (m4bBook.AudioSampleEntry.Esds is null && m4bBook.AudioSampleEntry.Dec3 is null)
						continue;

					OnTitleDiscovered(m4bBook.AppleTags.Title);
					OnAuthorsDiscovered(m4bBook.AppleTags.FirstAuthor);
					OnNarratorsDiscovered(m4bBook.AppleTags.Narrator);
					OnCoverImageDiscovered(m4bBook.AppleTags.Cover);

					var lameConfig = DownloadOptions.GetLameOptions(Configuration);
					var chapters = m4bBook.GetChaptersFromMetadata();
					//Finishing configuring lame encoder.
					AaxDecrypter.MpegUtil.ConfigureLameOptions(
						m4bBook,
						lameConfig,
						Configuration.LameDownsampleMono,
						Configuration.LameMatchSourceBR,
						chapters);

					if (m4bBook.AppleTags.Tracks is (int trackNum, int trackCount))
					{
						lameConfig.ID3.Track = trackCount > 0 ? $"{trackNum}/{trackCount}" : trackNum.ToString();
					}

					long currentFileNumBytesProcessed = 0;
					try
					{
						var tempPath = Path.GetTempFileName();
						using (var mp3File = File.Open(tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
						{
							Mp4Operation = m4bBook.ConvertToMp3Async(mp3File, lameConfig, chapters);
							Mp4Operation.ConversionProgressUpdate += m4bBook_ConversionProgressUpdate;
							await Mp4Operation;
						}

						if (cancellationToken.IsCancellationRequested)
							FileUtility.SaferDelete(tempPath);

						cancellationToken.ThrowIfCancellationRequested();

						var realMp3Path
								= FileUtility.SaferMoveToValidPath(
									tempPath,
									entry.proposedMp3Path,
									Configuration.ReplacementCharacters,
									extension: "mp3",
									Configuration.OverwriteExisting);

						SetFileTime(libraryBook, realMp3Path);
						SetDirectoryTime(libraryBook, Path.GetDirectoryName(realMp3Path));
						OnFileCreated(libraryBook, realMp3Path);
					}
					finally
					{
						if (Mp4Operation is not null)
							Mp4Operation.ConversionProgressUpdate -= m4bBook_ConversionProgressUpdate;

						sizeOfCompletedFiles += entry.m4bSize;
					}
					void m4bBook_ConversionProgressUpdate(object sender, ConversionProgressEventArgs e)
					{
						currentFileNumBytesProcessed = (long)(e.FractionCompleted * entry.m4bSize);
						var bytesCompleted = sizeOfCompletedFiles + currentFileNumBytesProcessed;
						ConversionProgressUpdate(totalInputSize, bytesCompleted);
					}
				}
				return new StatusHandler();
			}
			catch (Exception ex)
			{
				if (!cancellationToken.IsCancellationRequested)
				{
					Serilog.Log.Error(ex, "AAXClean error");
					return new StatusHandler { "Conversion failed" };
				}
				return new StatusHandler { "Cancelled" };
			}
			finally
			{
				OnCompleted(libraryBook);
				CancellationTokenSource.Dispose();
				CancellationTokenSource = null;
			}
		}

		private void ConversionProgressUpdate(long totalInputSize, long bytesCompleted)
		{
			averageSpeed.AddPosition(bytesCompleted);

			var remainingBytes = (totalInputSize - bytesCompleted);
			var estTimeRemaining = remainingBytes / averageSpeed.Average;

			if (double.IsNormal(estTimeRemaining))
				OnStreamingTimeRemaining(TimeSpan.FromSeconds(estTimeRemaining));

			double progressPercent = 100 * bytesCompleted / totalInputSize;

			OnStreamingProgressChanged(
				new DownloadProgress
				{
					ProgressPercentage = progressPercent,
					BytesReceived = bytesCompleted,
					TotalBytesToReceive = totalInputSize
				});
		}
		public static ConvertToMp3 Create(Configuration config) => new() { Configuration = config };
		private ConvertToMp3() { }
	}
}
