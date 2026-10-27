using ApplicationServices;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileManager;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FileLiberator;

public class UploadToAudiobookshelf : Processable, IProcessable<UploadToAudiobookshelf>
{
	internal enum UploadFailureKind { Cancellation, Network, Other }

	public override string Name => "Upload to Audiobookshelf";

	public enum UploadOutcome { Uploaded, AlreadyExists, NoFilesFound, Failed }

	public sealed class UploadOutcomeEventArgs(UploadOutcome outcome, string message) : EventArgs
	{
		public UploadOutcome Outcome { get; } = outcome;
		public string Message { get; } = message;
	}

	/// <summary>
	/// Force ASIN duplicate-checking on for this uploader instance regardless of the
	/// <see cref="Configuration.AudiobookshelfCheckAsin"/> setting. Used by the CLI
	/// <c>--check-asin</c> flag. GUI uploads instead read the persistent setting.
	/// </summary>
	public bool CheckAsin { get; set; }

	/// <summary>
	/// Raised exactly once per processed book, classifying what happened and why.
	/// <para/>
	/// Upload failures are reported here rather than through the returned <see cref="StatusHandler"/>.
	/// A non-success StatusHandler marks the book as a bad book: the GUI process queue breaks its step
	/// loop and raises the Abort/Retry/Ignore dialog. Uploading is a courtesy step layered onto
	/// liberation and must never fail the book, so <see cref="ProcessAsync"/> always reports success.
	/// </summary>
	public event EventHandler<UploadOutcomeEventArgs>? OutcomeDetermined;

	private void OnOutcomeDetermined(UploadOutcome outcome, string message)
		=> OutcomeDetermined?.Invoke(this, new UploadOutcomeEventArgs(outcome, message));

	public override bool Validate(LibraryBook libraryBook)
	{
		if (!Configuration.AudiobookshelfEnabled)
			return false;

		if (string.IsNullOrWhiteSpace(Configuration.AudiobookshelfServerUrl)
			|| string.IsNullOrWhiteSpace(Configuration.AudiobookshelfApiToken)
			|| string.IsNullOrWhiteSpace(Configuration.AudiobookshelfLibraryId)
			|| string.IsNullOrWhiteSpace(Configuration.AudiobookshelfFolderId))
			return false;

		// Deliberately narrower than Book.AudioExists, which also accepts LiberatedStatus.Error.
		// An errored liberation may have left partial files; uploading those is worse than skipping.
		return libraryBook.Book.UserDefinedItem.BookStatus == LiberatedStatus.Liberated;
	}

	public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
	{
		OnBegin(libraryBook);
		try
		{
			var files = GetFilesToUpload(libraryBook);
			if (files.Count == 0)
			{
				const string message = "No audio files found on disk to upload";
				OnStatusUpdate(message);
				Serilog.Log.Logger.Warning("No audio files found on disk to upload for {Book}", libraryBook.LogFriendly());
				OnOutcomeDetermined(UploadOutcome.NoFilesFound, message);
				return new StatusHandler();
			}

			OnStatusUpdate($"Uploading {files.Count} file(s) to Audiobookshelf...");
			var progress = new SynchronousProgress<(long bytesSent, long totalBytes)>(p =>
			{
				var percent = p.totalBytes > 0 ? 100.0 * p.bytesSent / p.totalBytes : 100.0;
				OnStreamingProgressChanged(new Dinah.Core.Net.Http.DownloadProgress { ProgressPercentage = percent, BytesReceived = p.bytesSent, TotalBytesToReceive = p.totalBytes });
			});

			var title = libraryBook.Book.TitleWithSubtitle;
			var author = libraryBook.Book.AuthorNames;
			var series = libraryBook.Book.SeriesNames();

			var checkAsin = CheckAsin || Configuration.AudiobookshelfCheckAsin;

			var result = await AudiobookshelfApiService.UploadBookAsync(
				Configuration.AudiobookshelfServerUrl!,
				Configuration.AudiobookshelfApiToken!,
				Configuration.AudiobookshelfLibraryId!,
				Configuration.AudiobookshelfFolderId!,
				title,
				author,
				series,
				files,
				asin: checkAsin ? libraryBook.Book.AudibleProductId : null, progress: progress);

			if (result == AudiobookshelfApiService.UploadResult.Success)
			{
				const string message = "Upload to Audiobookshelf completed successfully";
				OnStatusUpdate(message);
				OnOutcomeDetermined(UploadOutcome.Uploaded, message);
				return new StatusHandler();
			}
			else if (result == AudiobookshelfApiService.UploadResult.AlreadyExists)
			{
				const string message = "Book already exists on Audiobookshelf; skipping upload";
				OnStatusUpdate(message);
				Serilog.Log.Logger.Information("Book already exists on Audiobookshelf, skipping upload: {Book}", libraryBook.LogFriendly());
				OnOutcomeDetermined(UploadOutcome.AlreadyExists, message);
				return new StatusHandler();
			}
			else
			{
				const string message = "Upload to Audiobookshelf failed; book remains liberated on disk";
				OnStatusUpdate(message);
				Serilog.Log.Logger.Error("Audiobookshelf upload failed for {Book}, but continuing as soft-failure", libraryBook.LogFriendly());
				// Soft-fail: log the error but do not mark the book as failed
				OnOutcomeDetermined(UploadOutcome.Failed, message + ". See log for details.");
				return new StatusHandler();
			}
		}
		catch (Exception ex)
		{
			var message = FormatUploadFailure(ex);
			Serilog.Log.Logger.Error(ex, "Audiobookshelf upload failed; continuing as soft-failure. See log for details.");
			OnStatusUpdate(message);
			// Soft-fail: log the error but do not mark the book as failed
			OnOutcomeDetermined(UploadOutcome.Failed, message);
			return new StatusHandler();
		}
		finally
		{
			OnCompleted(libraryBook);
		}
	}

	internal static UploadFailureKind ClassifyUploadFailure(Exception ex)
	{
		// Cancellation must win over a SocketException/HttpRequestException nested in a wrapper.
		if (ContainsException<OperationCanceledException>(ex))
			return UploadFailureKind.Cancellation;

		for (var current = ex; current is not null; current = current.InnerException)
		{
			if (current is SocketException socket
				&& IsConnectionSocketError(socket.SocketErrorCode))
				return UploadFailureKind.Network;

			if (current is HttpRequestException request
				&& IsConnectionHttpError(request.HttpRequestError))
				return UploadFailureKind.Network;

			if (current is HttpIOException http
				&& IsConnectionHttpError(http.HttpRequestError))
				return UploadFailureKind.Network;
		}

		return UploadFailureKind.Other;
	}

	internal static string FormatUploadFailure(Exception ex)
	{
		var kind = ClassifyUploadFailure(ex);
		var category = kind switch
		{
			UploadFailureKind.Cancellation => "cancelled or timed out",
			UploadFailureKind.Network => "network failure",
			_ => "failure"
		};

		var detail = ex.GetBaseException().Message.ReplaceLineEndings(" ").Trim();
		return $"Audiobookshelf upload {category}: {detail}. See log for details.";
	}

	private static bool ContainsException<T>(Exception ex) where T : Exception
	{
		for (var current = ex; current is not null; current = current.InnerException)
			if (current is T)
				return true;
		return false;
	}

	private static bool IsConnectionSocketError(SocketError error)
		=> error is SocketError.ConnectionAborted
			or SocketError.ConnectionRefused
			or SocketError.ConnectionReset
			or SocketError.HostDown
			or SocketError.HostNotFound
			or SocketError.HostUnreachable
			or SocketError.NetworkDown
			or SocketError.NetworkReset
			or SocketError.NetworkUnreachable
			or SocketError.NoData
			or SocketError.NotInitialized
			or SocketError.Shutdown
			or SocketError.TimedOut
			or SocketError.TryAgain;

	private static bool IsConnectionHttpError(HttpRequestError error)
		=> error is HttpRequestError.ConnectionError
			or HttpRequestError.NameResolutionError
			or HttpRequestError.SecureConnectionError
			or HttpRequestError.ProxyTunnelError
			or HttpRequestError.ResponseEnded
			or HttpRequestError.HttpProtocolError
			or HttpRequestError.InvalidResponse;

	/// <summary>
	/// Resolves a book's audio files by both the path cache and a live scan of the Books directory.
	/// <para/>
	/// Must not use <see cref="FilePathCache"/> alone. A book can be liberated - and so pass
	/// <see cref="Validate"/>, which reads a database status - while having no cache entry at all.
	/// </summary>
	internal static List<string> GetAudioFilesOnDisk(string productId)
		=> AudibleFileStorage.Audio.GetPaths(productId)
		.Select(p => (string)p)
		.Where(File.Exists)
		.ToList();

	/// <summary>
	/// Composes the final upload payload from one preferred audio format, in deterministic order,
	/// followed by cover art and optional PDFs at most once.
	/// </summary>
	internal static List<string> BuildUploadFileList(IEnumerable<string> audioPaths, string? coverPath, IEnumerable<string>? pdfPaths = null)
	{
		var audioFiles = audioPaths
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Where(p => FileTypes.GetFileTypeFromPath(p) == FileType.Audio)
			.Distinct(StringComparer.Ordinal)
			.ToList();

		if (audioFiles.Count == 0)
			return [];

		var m4bFiles = audioFiles
			.Where(p => p.EndsWith(".m4b", StringComparison.OrdinalIgnoreCase))
			.ToList();
		var preferredAudioFiles = m4bFiles.Count > 0
			? m4bFiles
			: audioFiles.Any(p => p.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
				? audioFiles.Where(p => p.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)).ToList()
				: audioFiles;

		var files = preferredAudioFiles
			.OrderBy(p => p, StringComparer.Ordinal)
			.Where(p => !string.Equals(p, coverPath, StringComparison.Ordinal))
			.ToList();

		if (!string.IsNullOrWhiteSpace(coverPath))
			files.Add(coverPath);

		files.AddRange((pdfPaths ?? [])
			.Where(p => !string.IsNullOrWhiteSpace(p) && FileTypes.GetFileTypeFromPath(p) == FileType.PDF)
			.OrderBy(p => p, StringComparer.Ordinal));

		return files.Distinct(StringComparer.Ordinal).ToList();
	}

	internal List<string> GetFilesToUpload(LibraryBook libraryBook)
	{
		var audioFiles = GetAudioFilesOnDisk(libraryBook.Book.AudibleProductId);
		if (audioFiles.Count == 0)
			return [];

		var pdfFiles = Configuration.AudiobookshelfIncludePdfs
			? FilePathCache.GetFiles(libraryBook.Book.AudibleProductId)
				.Where(f => f.fileType == FileType.PDF)
				.Select(f => (string)f.path)
				.Where(File.Exists)
			: Enumerable.Empty<string>();

		return BuildUploadFileList(audioFiles, GetCoverArtPath(libraryBook, audioFiles.FirstOrDefault()), pdfFiles);
	}

	/// <summary>Libation's known cover art output path. Same logic as DownloadDecryptBook.DownloadCoverArt.</summary>
	private static string? GetCoverArtPath(LibraryBook libraryBook, string? firstAudioFile)
	{
		if (firstAudioFile is null || Path.GetDirectoryName(firstAudioFile) is not string dir)
			return null;

		var coverPath = AudibleFileStorage.Audio.GetCustomDirFilename(
			libraryBook,
			dir,
			".jpg",
			returnFirstExisting: false);

		return File.Exists(coverPath) ? coverPath : null;
	}

	public static UploadToAudiobookshelf Create(Configuration config) => new() { Configuration = config };
	private UploadToAudiobookshelf() { }
}
