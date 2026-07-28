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

	public override bool Validate(LibraryBook libraryBook)
	{
		if (!Configuration.AudiobookshelfEnabled)
			return false;

		if (string.IsNullOrWhiteSpace(Configuration.AudiobookshelfServerUrl)
			|| string.IsNullOrWhiteSpace(Configuration.AudiobookshelfApiToken)
			|| string.IsNullOrWhiteSpace(Configuration.AudiobookshelfLibraryId)
			|| string.IsNullOrWhiteSpace(Configuration.AudiobookshelfFolderId))
			return false;

		return libraryBook.Book.AudioExists;
	}

	public override async Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
	{
		OnBegin(libraryBook);
		try
		{
			var files = GetFilesToUpload(libraryBook);
			if (files.Count == 0)
			{
				OnStatusUpdate("No audio files found to upload");
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
				OnStatusUpdate("Upload to Audiobookshelf completed successfully");
				return new StatusHandler();
			}
			else if (result == AudiobookshelfApiService.UploadResult.AlreadyExists)
			{
				OnStatusUpdate("Book already exists on Audiobookshelf; skipping upload");
				Serilog.Log.Logger.Information("Book already exists on Audiobookshelf, skipping upload: {Book}", libraryBook.LogFriendly());
				return new StatusHandler();
			}
			else
			{
				OnStatusUpdate("Upload to Audiobookshelf failed; book remains liberated on disk");
				Serilog.Log.Logger.Error("Audiobookshelf upload failed for {Book}, but continuing as soft-failure", libraryBook.LogFriendly());
				// Soft-fail: log the error but do not mark the book as failed
				return new StatusHandler();
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error uploading {Book} to Audiobookshelf; continuing as soft-failure", libraryBook.LogFriendly());
			OnStatusUpdate($"Audiobookshelf upload error: {ex.Message}");
			// Soft-fail: log the error but do not mark the book as failed
			return new StatusHandler();
		}
		finally
		{
			OnCompleted(libraryBook);
		}
	}

	private static List<string> GetFilesToUpload(LibraryBook libraryBook)
	{
		var files = new List<string>();
		var productId = libraryBook.Book.AudibleProductId;

		// Get audio files from cache
		var audioFiles = FilePathCache.GetFiles(productId)
			.Where(f => f.fileType == FileType.Audio && File.Exists(f.path))
			.Select(f => (string)f.path)
			.Distinct()
			.ToList();

		files.AddRange(audioFiles);

		// Use Libation's known cover art output path (same logic as DownloadDecryptBook.DownloadCoverArt)
		if (audioFiles.FirstOrDefault() is { } firstAudioFile)
		{
			var dir = Path.GetDirectoryName(firstAudioFile);
			if (dir is not null)
			{
				var coverPath = AudibleFileStorage.Audio.GetCustomDirFilename(
					libraryBook,
					dir,
					".jpg",
					returnFirstExisting: false);

				if (File.Exists(coverPath))
					files.Add(coverPath);
			}
		}

		return files;
	}

	public static UploadToAudiobookshelf Create(Configuration config) => new() { Configuration = config };
	private UploadToAudiobookshelf() { }
}
