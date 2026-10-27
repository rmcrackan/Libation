using AssertionHelper;
using DataLayer;
using FileManager;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FileLiberator.Tests;

/// <summary>
	/// Configuration uses process-wide test state, so these tests must not run alongside others.
/// </summary>
[TestClass]
[DoNotParallelize]
public class UploadToAudiobookshelfTests
{
	private string testFilesDirectory = string.Empty;
	private string? previousFilesDirectory;

	[TestInitialize]
	public void IsolateFileCache()
	{
		previousFilesDirectory = Environment.GetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR);
		testFilesDirectory = CreateEmptyBooksDirectory();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, testFilesDirectory);
	}

	[TestCleanup]
	public void RestoreConfiguration()
	{
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, previousFilesDirectory);
		Directory.Delete(testFilesDirectory, recursive: true);
	}

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

	[TestMethod]
	public void Pdf_setting_defaults_off_and_round_trips()
	{
		var config = ConfiguredForAudiobookshelf();
		Assert.IsFalse(config.AudiobookshelfIncludePdfs);
		foreach (var enabled in new[] { true, false })
		{
			config.AudiobookshelfIncludePdfs = enabled;
			Assert.AreEqual(enabled, config.CreateEphemeralCopy().AudiobookshelfIncludePdfs);
		}
	}

	[TestMethod]
	public void BuildUploadFileList_appends_distinct_pdfs_after_cover_and_excludes_zip()
	{
		var files = UploadToAudiobookshelf.BuildUploadFileList(
			["book.m4b"], "cover.jpg", ["b.pdf", "a.PDF", "b.pdf", "supplement.zip"]);
		CollectionAssert.AreEqual(new[] { "book.m4b", "cover.jpg", "a.PDF", "b.pdf" }, files);
	}

	[TestMethod]
	public void BuildUploadFileList_requires_audio_even_with_attachments()
	{
		Assert.AreEqual(0, UploadToAudiobookshelf.BuildUploadFileList(
			["cover.jpg", "book.pdf"], "cover.jpg", ["book.pdf"]).Count);
	}

	[TestMethod]
	public void ClassifyUploadFailure_classifies_connection_socket_errors()
	{
		Assert.AreEqual(UploadToAudiobookshelf.UploadFailureKind.Network,
			UploadToAudiobookshelf.ClassifyUploadFailure(new SocketException((int)SocketError.ConnectionReset)));
	}

	[TestMethod]
	public void ClassifyUploadFailure_classifies_connection_http_errors()
	{
		var error = new HttpRequestException(HttpRequestError.NameResolutionError, "not a classification input");

		Assert.AreEqual(UploadToAudiobookshelf.UploadFailureKind.Network,
			UploadToAudiobookshelf.ClassifyUploadFailure(error));
	}

	[TestMethod]
	public void ClassifyUploadFailure_does_not_treat_local_stream_io_as_connection_loss()
	{
		var error = new HttpRequestException("upload stream failed", new IOException("disk read failed"));

		Assert.AreEqual(UploadToAudiobookshelf.UploadFailureKind.Other,
			UploadToAudiobookshelf.ClassifyUploadFailure(error));
	}

	[TestMethod]
	public void ClassifyUploadFailure_cancellation_wrapping_socket_reset_is_cancellation()
	{
		var error = new IOException("wrapper", new OperationCanceledException(
			"request cancelled", new SocketException((int)SocketError.ConnectionReset)));

		Assert.AreEqual(UploadToAudiobookshelf.UploadFailureKind.Cancellation,
			UploadToAudiobookshelf.ClassifyUploadFailure(error));
	}

	[TestMethod]
	public void FormatUploadFailure_is_single_line_and_does_not_require_configuration()
	{
		var message = UploadToAudiobookshelf.FormatUploadFailure(
			new SocketException((int)SocketError.ConnectionRefused));

		StringAssert.Contains(message, "network failure");
		StringAssert.Contains(message, "See log for details.");
		Assert.IsFalse(message.Contains('\n'));
		Assert.IsFalse(message.Contains("localhost", StringComparison.OrdinalIgnoreCase));
	}

	[TestMethod]
	public void GetFilesToUpload_includes_only_existing_pdfs_for_this_book_when_enabled()
	{
		var directory = CreateEmptyBooksDirectory();
		try
		{
			var config = ConfiguredForAudiobookshelf();
			config.Books = directory;
			// Cover-art naming reads account nicknames, even when no cover exists.
			AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();
			var book = LibraryBookWith(LiberatedStatus.Liberated);
			var audio = Path.Combine(directory, "B0TEST0001.m4b");
			var pdf = Path.Combine(directory, "supplement.pdf");
			var secondPdf = Path.Combine(directory, "second.PDF");
			var otherPdf = Path.Combine(directory, "other.pdf");
			var untrackedPdf = Path.Combine(directory, "untracked.pdf");
			var zip = Path.Combine(directory, "supplement.zip");
			foreach (var path in new[] { audio, pdf, secondPdf, otherPdf, untrackedPdf, zip })
				File.WriteAllText(path, "test content");
			FilePathCache.Insert(book.Book.AudibleProductId, audio, pdf, secondPdf, zip, Path.Combine(directory, "missing.pdf"));
			FilePathCache.Insert("B0OTHER001", otherPdf);
			var sut = UploadToAudiobookshelf.Create(config);
			string[] Paths() => sut.GetFilesToUpload(book)
				.Select(p => ((LongPath)p).PathWithoutPrefix).ToArray();

			CollectionAssert.AreEquivalent(new[] { audio }, Paths());
			config.AudiobookshelfIncludePdfs = true;
			CollectionAssert.AreEquivalent(new[] { audio, pdf, secondPdf }, Paths());
			File.Delete(pdf);
			CollectionAssert.AreEquivalent(new[] { audio, secondPdf }, Paths());
			File.Delete(audio);
			Assert.AreEqual(0, Paths().Length);
		}
		finally
		{
			Directory.Delete(directory, recursive: true);
		}
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

			UploadToAudiobookshelf.GetAudioFilesOnDisk(productId)
				.Select(path => ((LongPath)path).PathWithoutPrefix)
				.Should().BeEquivalentTo([audioFile]);
		}
		finally
		{
			Directory.Delete(booksDirectory, recursive: true);
		}
	}
}
