using ApplicationServices;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileManager;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileLiberator;

public class UploadToAudiobookshelf : Processable, IProcessable<UploadToAudiobookshelf>
{
	public override string Name => "Upload to Audiobookshelf";

	public enum UploadOutcome { Uploaded, AlreadyExists, NoFilesFound, Failed }

	public sealed class UploadOutcomeEventArgs(UploadOutcome outcome, string message) : EventArgs
	{
		public UploadOutcome Outcome { get; } = outcome;
		public string Message { get; } = message;
	}

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
				Serilog.Log.Logger.Error("No audio files found on disk to upload for {Book}", libraryBook.LogFriendly());
				OnOutcomeDetermined(UploadOutcome.NoFilesFound, message);
				return new StatusHandler();
			}

			OnStatusUpdate($"Uploading {files.Count} file(s) to Audiobookshelf...");

			var title = libraryBook.Book.TitleWithSubtitle;
			var author = libraryBook.Book.AuthorNames;
			var series = libraryBook.Book.SeriesNames();

			var result = await AudiobookshelfApiService.UploadBookAsync(
				Configuration.AudiobookshelfServerUrl!,
				Configuration.AudiobookshelfApiToken!,
				Configuration.AudiobookshelfLibraryId!,
				Configuration.AudiobookshelfFolderId!,
				title,
				author,
				series,
				files);

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
			Serilog.Log.Logger.Error(ex, "Error uploading {Book} to Audiobookshelf; continuing as soft-failure", libraryBook.LogFriendly());
			OnStatusUpdate($"Audiobookshelf upload error: {ex.Message}");
			// Soft-fail: log the error but do not mark the book as failed
			OnOutcomeDetermined(UploadOutcome.Failed, $"Audiobookshelf upload error: {ex.Message}");
			return new StatusHandler();
		}
		finally
		{
			OnCompleted(libraryBook);
		}
	}

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
	/// Composes the final upload payload: de-duplicated audio files, followed by cover art at most once.
	/// </summary>
	internal static List<string> BuildUploadFileList(IEnumerable<string> audioPaths, string? coverPath)
	{
		var files = audioPaths
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Distinct()
			.Where(p => !string.Equals(p, coverPath, StringComparison.Ordinal))
			.ToList();

		if (!string.IsNullOrWhiteSpace(coverPath))
			files.Add(coverPath);

		return files;
	}

	internal static List<string> GetFilesToUpload(LibraryBook libraryBook)
	{
		var audioFiles = GetAudioFilesOnDisk(libraryBook.Book.AudibleProductId);

		return BuildUploadFileList(audioFiles, GetCoverArtPath(libraryBook, audioFiles.FirstOrDefault()));
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
