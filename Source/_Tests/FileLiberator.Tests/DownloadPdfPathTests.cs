using DataLayer;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace FileLiberator.Tests;

/// <summary>
/// Where a PDF is saved, against a real Books directory on disk. Reported in issue #1947: PDFs landed loose in
/// the Books directory instead of with their book.
/// </summary>
[TestClass]
[DoNotParallelize]
public class DownloadPdfPathTests
{
	private string tempLibationFiles = string.Empty;
	private string booksDir = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-pdf-path-tests-{Guid.NewGuid():N}");
		booksDir = Path.Combine(tempLibationFiles, "Books");
		Directory.CreateDirectory(booksDir);

		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempLibationFiles);
		var config = Configuration.CreateMockInstance();
		config.Books = booksDir;

		// The naming templates read a book's account nickname from here.
		AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();

		// Each test uses its own Books directory, so the cached file list has to be rebuilt against it.
		AudibleFileStorage.Audio.Refresh();
	}

	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);

		try
		{
			Directory.Delete(tempLibationFiles, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	private static LibraryBook BookWithPdf(string title)
	{
		var libraryBook = MockLibraryBook.CreateBook(title: title, subtitle: "", bookStatus: LiberatedStatus.Liberated);
		libraryBook.Book.AddSupplementDownloadUrl("https://example.com/supplement.pdf");
		return libraryBook;
	}

	private string GetPath(LibraryBook libraryBook)
		=> DownloadPdf.Create(Configuration.Instance).GetProposedDownloadFilePath(libraryBook);

	[TestMethod]
	public void A_pdf_goes_beside_the_audio_files_already_on_disk()
	{
		var libraryBook = BookWithPdf("Beside The Audio");
		// Named by an older template, so not where the current folder template would put it.
		var audioDir = Path.Combine(booksDir, "Some Old Folder Name");
		Directory.CreateDirectory(audioDir);
		File.WriteAllText(Path.Combine(audioDir, $"whatever [{libraryBook.Book.AudibleProductId}].m4b"), "audio");
		AudibleFileStorage.Audio.Refresh();

		var path = GetPath(libraryBook);

		Assert.AreEqual(audioDir, Path.GetDirectoryName(path));
	}

	[TestMethod]
	public void A_pdf_for_a_book_with_no_audio_on_disk_goes_in_the_books_own_folder()
	{
		// The bug: this used to return a path directly under the Books directory.
		var libraryBook = BookWithPdf("No Audio On Disk");

		var path = GetPath(libraryBook);
		var directory = Path.GetDirectoryName(path)!;

		Assert.AreNotEqual(booksDir, directory, "the PDF was saved loose in the Books directory");
		Assert.AreEqual(booksDir, Path.GetDirectoryName(directory));
		StringAssert.Contains(Path.GetFileName(directory), "No Audio On Disk");
	}

	[TestMethod]
	public void The_books_own_folder_is_the_one_the_folder_template_names()
	{
		var libraryBook = BookWithPdf("Matches The Folder Template");

		var expected = AudibleFileStorage.Audio.GetDestinationDirectory(libraryBook, Configuration.Instance);

		Assert.AreEqual(expected, Path.GetDirectoryName(GetPath(libraryBook)));
	}

	[TestMethod]
	public void The_folder_is_created_so_the_download_has_somewhere_to_land()
	{
		var libraryBook = BookWithPdf("Folder Gets Created");

		var directory = Path.GetDirectoryName(GetPath(libraryBook))!;

		Assert.IsTrue(Directory.Exists(directory), $"{directory} was not created");
	}

	[TestMethod]
	public void Two_books_with_no_audio_on_disk_get_separate_folders()
	{
		// Loose in the Books directory they shared one namespace, so same-titled books collided.
		var first = BookWithPdf("First Book");
		var second = BookWithPdf("Second Book");

		Assert.AreNotEqual(Path.GetDirectoryName(GetPath(first)), Path.GetDirectoryName(GetPath(second)));
	}

	[TestMethod]
	public void The_file_name_comes_from_the_file_template()
	{
		var libraryBook = BookWithPdf("Named By The Template");

		var path = GetPath(libraryBook);

		// The default file template is "<title> [<id>]"; a library that customises it gets what it asked for.
		StringAssert.Contains(Path.GetFileName(path), "Named By The Template");
		StringAssert.Contains(Path.GetFileName(path), libraryBook.Book.AudibleProductId);
		Assert.AreEqual(".pdf", Path.GetExtension(path));
	}

	[TestMethod]
	public void The_extension_follows_the_supplement_url()
	{
		var libraryBook = MockLibraryBook.CreateBook(title: "Zip Supplement", subtitle: "", bookStatus: LiberatedStatus.Liberated);
		libraryBook.Book.AddSupplementDownloadUrl("https://example.com/supplement.zip");

		Assert.AreEqual(".zip", Path.GetExtension(GetPath(libraryBook)));
	}
}
