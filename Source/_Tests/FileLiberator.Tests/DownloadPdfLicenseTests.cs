using ApplicationServices;
using AudibleApi.Common;
using DataLayer;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileLiberator.Tests;

/// <summary>
/// How the supplement download treats the content license it is given. Reported in issue #1973: a scheduled run
/// asked Audible for a license for the same titles every 15 minutes, because the step asked for its own license
/// every time and remembered nothing about what came back.
/// <para>
/// No Audible account exists in these tests, so <c>GetApiAsync</c> throws. That is the point: a test that
/// completes proves the step never went to Audible for a license of its own.
/// </para>
/// </summary>
[TestClass]
[DoNotParallelize]
public class DownloadPdfLicenseTests
{
	private string tempLibationFiles = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-pdf-license-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempLibationFiles);

		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempLibationFiles);
		Configuration.CreateMockInstance();
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

	private static int seeded;

	/// <summary>
	/// A title Audible lists a supplement for, as a real row: writing off a PDF is a change to the database, so
	/// there has to be something there to change.
	/// </summary>
	private static LibraryBook BookWithPdf(LiberatedStatus pdfStatus = LiberatedStatus.NotLiberated)
	{
		var book = new Book(
			new AudibleProductId($"B0PDFLIC{++seeded:0000}"),
			"Supplemented Title",
			"",
			"",
			600,
			DataLayer.ContentType.Product,
			[new Contributor("Test Author")],
			[new Contributor("Test Narrator")],
			"us");

		book.UserDefinedItem.BookStatus = LiberatedStatus.Liberated;
		book.SetSupplementDownloadUrl("https://example.com/supplement.pdf");
		book.UserDefinedItem.SetPdfStatus(pdfStatus);

		var libraryBook = new LibraryBook(book, new DateTime(2026, 1, 1), "test-account");

		using var context = DbContexts.GetContext();
		context.LibraryBooks.Add(libraryBook);
		context.SaveChanges();

		return libraryBook;
	}

	/// <summary>
	/// A license Audible granted, carrying a supplement link or not. No content metadata: a supplement needs
	/// nothing from a license but its pdf_url, which is why requiring metadata belongs to the audiobook step.
	/// </summary>
	private static DownloadOptions.LicenseInfo License(string? pdfUrl)
		=> DownloadOptions.LicenseInfo.Create(new ContentLicense
		{
			Asin = "B002V5B8OY",
			StatusCode = "Granted",
			PdfUrl = pdfUrl
		});

	private static DownloadPdf NewStep(DownloadOptions.LicenseInfo? carried = null)
	{
		var step = DownloadPdf.Create(Configuration.Instance);
		step.LicenseInfo = carried;
		return step;
	}

	[TestMethod]
	public async Task A_license_that_carries_no_supplement_writes_the_pdf_off()
	{
		// Audible granting a license and returning no pdf_url is Audible saying the title has no PDF. Error is
		// what the audiobook download has always used to mean "don't attempt this again", and until now the
		// supplement had no way of saying it - so every run asked, and every run got the same answer.
		var libraryBook = BookWithPdf();

		var status = await NewStep(License(pdfUrl: null)).ProcessAsync(libraryBook);

		Assert.IsTrue(status.IsSuccess, string.Join(", ", status.Errors));
		Assert.AreEqual(LiberatedStatus.Error, libraryBook.Book.UserDefinedItem.PdfStatus);
	}

	[TestMethod]
	public async Task A_written_off_pdf_is_not_selected_again()
	{
		var libraryBook = BookWithPdf();

		await NewStep(License(pdfUrl: null)).ProcessAsync(libraryBook);

		Assert.IsFalse(NewStep().Validate(libraryBook));
	}

	[TestMethod]
	public void A_pdf_marked_as_an_error_is_skipped_the_way_an_errored_audiobook_is()
	{
		// DownloadDecryptBook.Validate goes through AudioExists, which counts Error, so the audiobook step has
		// always left an errored title alone. The supplement step selected on PdfExists, which does not.
		Assert.IsFalse(NewStep().Validate(BookWithPdf(LiberatedStatus.Error)));
		Assert.IsTrue(NewStep().Validate(BookWithPdf(LiberatedStatus.NotLiberated)));
		Assert.IsFalse(NewStep().Validate(BookWithPdf(LiberatedStatus.Liberated)));
	}

	[TestMethod]
	public async Task The_license_in_hand_is_published_for_the_next_step()
	{
		var carried = License(pdfUrl: null);
		var step = NewStep(carried);

		await step.ProcessAsync(BookWithPdf());

		Assert.AreSame(carried, step.ObtainedLicense);
	}

	[TestMethod]
	public async Task A_step_with_no_license_asks_Audible_for_one()
	{
		// The other half of the arrangement: a PDF-only run has no license to be handed, so it requests one.
		// With no account configured that request cannot be made, which is how this test can tell it was tried.
		var status = await NewStep().ProcessAsync(BookWithPdf());

		Assert.IsFalse(status.IsSuccess);
		StringAssert.Contains(string.Join(", ", status.Errors), "Error downloading PDF");
	}

	[TestMethod]
	public async Task A_license_left_over_from_another_title_is_not_reused()
	{
		// Processable instances are reused across books, so a step must not carry a license into a second
		// attempt of its own accord.
		var step = NewStep(License(pdfUrl: null));

		await step.ProcessAsync(BookWithPdf());
		step.LicenseInfo = null;
		var second = await step.ProcessAsync(BookWithPdf());

		Assert.IsFalse(second.IsSuccess);
		Assert.IsNull(step.ObtainedLicense);
	}
}
