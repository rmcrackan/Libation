using AssertionHelper;
using DataLayer;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileLiberator.Tests;

/// <summary>
/// <see cref="Configuration.CreateMockInstance"/> replaces the process-wide
/// <see cref="Configuration.Instance"/>, so these tests must not run alongside others.
/// </summary>
[TestClass]
[DoNotParallelize]
public class UploadToAudiobookshelfTests
{
	[TestCleanup]
	public void RestoreConfiguration() => Configuration.RestoreSingletonInstance();

	private static Configuration ConfiguredForAudiobookshelf()
	{
		var config = Configuration.CreateMockInstance();
		config.AudiobookshelfEnabled = true;
		config.AudiobookshelfServerUrl = "http://localhost:13378";
		config.AudiobookshelfApiToken = "test-token";
		config.AudiobookshelfLibraryId = "test-library-id";
		config.AudiobookshelfFolderId = "test-folder-id";
		return config;
	}

	private static LibraryBook LibraryBookWith(LiberatedStatus bookStatus)
	{
		var book = new Book(
			new AudibleProductId("B0TEST0001"),
			"Test Title",
			"Test Subtitle",
			"Test Description",
			600,
			ContentType.Product,
			[new Contributor("Test Author")],
			[new Contributor("Test Narrator")],
			"us");

		book.UserDefinedItem.BookStatus = bookStatus;

		return new LibraryBook(book, new DateTime(2020, 1, 1), "test-account");
	}

	[TestMethod]
	public void Validate_rejects_book_whose_liberation_errored()
	{
		var sut = UploadToAudiobookshelf.Create(ConfiguredForAudiobookshelf());

		sut.Validate(LibraryBookWith(LiberatedStatus.Error)).Should().BeFalse();
	}

	[TestMethod]
	public void Validate_accepts_liberated_book()
	{
		var sut = UploadToAudiobookshelf.Create(ConfiguredForAudiobookshelf());

		sut.Validate(LibraryBookWith(LiberatedStatus.Liberated)).Should().BeTrue();
	}

	[TestMethod]
	public void Validate_rejects_book_that_was_never_liberated()
	{
		var sut = UploadToAudiobookshelf.Create(ConfiguredForAudiobookshelf());

		sut.Validate(LibraryBookWith(LiberatedStatus.NotLiberated)).Should().BeFalse();
	}

	[TestMethod]
	public void Validate_rejects_when_audiobookshelf_is_disabled()
	{
		var config = ConfiguredForAudiobookshelf();
		config.AudiobookshelfEnabled = false;
		var sut = UploadToAudiobookshelf.Create(config);

		sut.Validate(LibraryBookWith(LiberatedStatus.Liberated)).Should().BeFalse();
	}

	[TestMethod]
	public void Validate_rejects_when_folder_id_is_not_configured()
	{
		var config = ConfiguredForAudiobookshelf();
		config.AudiobookshelfFolderId = null;
		var sut = UploadToAudiobookshelf.Create(config);

		sut.Validate(LibraryBookWith(LiberatedStatus.Liberated)).Should().BeFalse();
	}

	[TestMethod]
	public void BuildUploadFileList_removes_duplicate_audio_paths()
	{
		var result = UploadToAudiobookshelf.BuildUploadFileList(
			["/books/part1.m4b", "/books/part1.m4b"],
			coverPath: null);

		result.Should().HaveCount(1);
		result[0].Should().Be("/books/part1.m4b");
	}

	[TestMethod]
	public void BuildUploadFileList_sorts_multipart_audio_paths()
	{
		var result = UploadToAudiobookshelf.BuildUploadFileList(
			["/books/part3.m4b", "/books/part1.m4b", "/books/part2.m4b"],
			coverPath: null);

		result.Should().HaveCount(3);
		result[0].Should().Be("/books/part1.m4b");
		result[1].Should().Be("/books/part2.m4b");
		result[2].Should().Be("/books/part3.m4b");
	}

	[TestMethod]
	public void BuildUploadFileList_prefers_m4b_when_m4b_and_mp3_are_available()
	{
		var result = UploadToAudiobookshelf.BuildUploadFileList(
			["/books/part1.mp3", "/books/part2.m4b", "/books/part1.m4b", "/books/part2.mp3"],
			coverPath: null);

		result.Should().HaveCount(2);
		result[0].Should().Be("/books/part1.m4b");
		result[1].Should().Be("/books/part2.m4b");
	}

	[TestMethod]
	public void BuildUploadFileList_appends_cover_art_after_the_audio_files()
	{
		var result = UploadToAudiobookshelf.BuildUploadFileList(
			["/books/part1.m4b"],
			coverPath: "/books/cover.jpg");

		result.Should().HaveCount(2);
		result[0].Should().Be("/books/part1.m4b");
		result[1].Should().Be("/books/cover.jpg");
	}

	[TestMethod]
	public void BuildUploadFileList_includes_cover_art_once_when_it_is_also_in_the_audio_list()
	{
		var result = UploadToAudiobookshelf.BuildUploadFileList(
			["/books/part1.m4b", "/books/cover.jpg"],
			coverPath: "/books/cover.jpg");

		result.Should().HaveCount(2);
		result[0].Should().Be("/books/part1.m4b");
		result[1].Should().Be("/books/cover.jpg");
	}

	[TestMethod]
	public void BuildUploadFileList_omits_cover_art_when_none_was_resolved()
	{
		var result = UploadToAudiobookshelf.BuildUploadFileList(
			["/books/part1.m4b"],
			coverPath: null);

		result.Should().HaveCount(1);
		result[0].Should().Be("/books/part1.m4b");
	}

	[TestMethod]
	public void BuildUploadFileList_returns_empty_when_there_are_no_audio_files()
	{
		var result = UploadToAudiobookshelf.BuildUploadFileList([], coverPath: null);

		result.Should().HaveCount(0);
	}

	/// <summary>
	/// An Audiobookshelf problem must never fail the book. The GUI process queue treats a
	/// non-success <see cref="Dinah.Core.ErrorHandling.StatusHandler"/> as a bad book: it breaks the
	/// step loop, raises the Abort/Retry/Ignore dialog, and marks the book Failed. Uploading is a
	/// courtesy step layered onto liberation, so it reports through
	/// <see cref="UploadToAudiobookshelf.OutcomeDetermined"/> instead.
	/// </summary>
	[TestMethod]
	public async Task ProcessAsync_does_not_fail_the_book_when_no_audio_files_are_found()
	{
		var booksDirectory = CreateEmptyBooksDirectory();
		try
		{
			var config = ConfiguredForAudiobookshelf();
			config.Books = booksDirectory;
			var sut = UploadToAudiobookshelf.Create(config);

			var status = await sut.ProcessAsync(LibraryBookWith(LiberatedStatus.Liberated));

			status.IsSuccess.Should().BeTrue();
		}
		finally
		{
			Directory.Delete(booksDirectory, recursive: true);
		}
	}

	[TestMethod]
	public async Task ProcessAsync_raises_the_no_files_found_outcome()
	{
		var booksDirectory = CreateEmptyBooksDirectory();
		try
		{
			var config = ConfiguredForAudiobookshelf();
			config.Books = booksDirectory;
			var sut = UploadToAudiobookshelf.Create(config);

			UploadToAudiobookshelf.UploadOutcome? outcome = null;
			sut.OutcomeDetermined += (_, e) => outcome = e.Outcome;

			await sut.ProcessAsync(LibraryBookWith(LiberatedStatus.Liberated));

			Assert.AreEqual(UploadToAudiobookshelf.UploadOutcome.NoFilesFound, outcome);
		}
		finally
		{
			Directory.Delete(booksDirectory, recursive: true);
		}
	}

	[TestMethod]
	public async Task ProcessAsync_explains_the_no_files_found_outcome()
	{
		var booksDirectory = CreateEmptyBooksDirectory();
		try
		{
			var config = ConfiguredForAudiobookshelf();
			config.Books = booksDirectory;
			var sut = UploadToAudiobookshelf.Create(config);

			string? message = null;
			sut.OutcomeDetermined += (_, e) => message = e.Message;

			await sut.ProcessAsync(LibraryBookWith(LiberatedStatus.Liberated));

			message.Should().BeNotNull();
		}
		finally
		{
			Directory.Delete(booksDirectory, recursive: true);
		}
	}

	private static string CreateEmptyBooksDirectory()
	{
		var booksDirectory = Path.Combine(Path.GetTempPath(), $"libation-test-{Guid.NewGuid():N}");
		Directory.CreateDirectory(booksDirectory);
		return booksDirectory;
	}

	/// <summary>
	/// Books liberated before the path cache existed - or whose cache was lost - have no
	/// <see cref="FilePathCache"/> entry. Backfill must still find them by scanning the Books
	/// directory, otherwise the upload silently reports success having sent nothing.
	/// </summary>
	[TestMethod]
	public void GetAudioFilesOnDisk_finds_audio_that_is_absent_from_the_file_path_cache()
	{
		const string productId = "B0DISKSCAN1";
		var booksDirectory = Path.Combine(Path.GetTempPath(), $"libation-test-{Guid.NewGuid():N}");
		Directory.CreateDirectory(booksDirectory);

		try
		{
			var audioFile = Path.Combine(booksDirectory, $"{productId}.m4b");
			File.WriteAllText(audioFile, "not really audio");

			var config = Configuration.CreateMockInstance();
			config.Books = booksDirectory;

			// Precondition: the cache knows nothing about this book.
			FilePathCache.GetFiles(productId).Should().HaveCount(0);

			UploadToAudiobookshelf.GetAudioFilesOnDisk(productId).Should().BeEquivalentTo([audioFile]);
		}
		finally
		{
			Directory.Delete(booksDirectory, recursive: true);
		}
	}
}
