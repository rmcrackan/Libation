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
				return new StatusHandler { "No audio files found to upload" };
			}

			OnStatusUpdate($"Uploading {files.Count} file(s) to Audiobookshelf...");

			var title = libraryBook.Book.TitleWithSubtitle;
			var author = libraryBook.Book.AuthorNames;
			var series = libraryBook.Book.SeriesNames();

			var success = await AudiobookshelfApiService.UploadBookAsync(
				Configuration.AudiobookshelfServerUrl!,
				Configuration.AudiobookshelfApiToken!,
				Configuration.AudiobookshelfLibraryId!,
				Configuration.AudiobookshelfFolderId!,
				title,
				author,
				series,
				files);

			if (success)
			{
				OnStatusUpdate("Upload to Audiobookshelf completed successfully");
				return new StatusHandler();
			}
			else
			{
				OnStatusUpdate("Upload to Audiobookshelf failed");
				return new StatusHandler { "Upload to Audiobookshelf failed" };
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error uploading {Book} to Audiobookshelf", libraryBook.LogFriendly());
			return new StatusHandler { $"Audiobookshelf upload error: {ex.Message}" };
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

		// Also look for cover art in the same directory as the first audio file
		if (audioFiles.FirstOrDefault() is { } firstAudioFile)
		{
			var dir = Path.GetDirectoryName(firstAudioFile);
			if (dir is not null)
			{
				var coverFile = Directory.EnumerateFiles(dir, "*.jpg", SearchOption.TopDirectoryOnly)
					.Concat(Directory.EnumerateFiles(dir, "*.jpeg", SearchOption.TopDirectoryOnly))
					.Concat(Directory.EnumerateFiles(dir, "*.png", SearchOption.TopDirectoryOnly))
					.FirstOrDefault();

				if (coverFile is not null)
					files.Add(coverFile);
			}
		}

		return files;
	}

	public static UploadToAudiobookshelf Create(Configuration config) => new() { Configuration = config };
	private UploadToAudiobookshelf() { }
}
