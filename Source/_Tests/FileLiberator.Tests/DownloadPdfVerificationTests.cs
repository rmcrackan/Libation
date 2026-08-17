using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace FileLiberator.Tests;

/// <summary>
/// What counts as a supplement having been downloaded. Reported in issue #1947: an Audible response that was
/// not the PDF was saved into the book's folder and the title recorded as having its PDF.
/// </summary>
[TestClass]
public class DownloadPdfVerificationTests
{
	private string directory = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		directory = Path.Combine(Path.GetTempPath(), $"libation-pdf-verify-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(directory);
	}

	[TestCleanup]
	public void Cleanup()
	{
		try
		{
			Directory.Delete(directory, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	private string Write(string fileName, byte[] contents)
	{
		var path = Path.Combine(directory, fileName);
		File.WriteAllBytes(path, contents);
		return path;
	}

	private string Write(string fileName, string contents) => Write(fileName, Encoding.UTF8.GetBytes(contents));

	private static byte[] Pdf => Encoding.ASCII.GetBytes("%PDF-1.4\n1 0 obj\n<< >>\nendobj\n");

	[TestMethod]
	public void A_real_pdf_passes()
		=> Assert.IsTrue(DownloadPdf.verifyDownload(Write("book.pdf", Pdf)).IsSuccess);

	[TestMethod]
	public void A_file_that_is_not_there_fails()
		=> Assert.IsFalse(DownloadPdf.verifyDownload(Path.Combine(directory, "never-written.pdf")).IsSuccess);

	[TestMethod]
	public void An_empty_file_fails()
		=> Assert.IsFalse(DownloadPdf.verifyDownload(Write("book.pdf", Array.Empty<byte>())).IsSuccess);

	[TestMethod]
	public void A_file_named_pdf_that_is_not_a_pdf_fails()
		=> Assert.IsFalse(DownloadPdf.verifyDownload(Write("book.pdf", "Not a PDF at all")).IsSuccess);

	/// <summary>
	/// The reported failure. Dinah's downloader renames by Content-Disposition, so an Audible JSON body does
	/// not even arrive named .pdf and an extension check alone would wave it through.
	/// </summary>
	[TestMethod]
	public void An_audible_json_body_fails_whatever_it_is_named()
	{
		const string body = """{"asin":"B089T8FSK6","asset_details":[],"is_preview_enabled":false,"is_vvab":false}""";

		Assert.IsFalse(DownloadPdf.verifyDownload(Write("book.json", body)).IsSuccess);
		Assert.IsFalse(DownloadPdf.verifyDownload(Write("book.pdf", body)).IsSuccess);
	}

	[TestMethod]
	public void A_json_body_behind_a_byte_order_mark_fails()
		=> Assert.IsFalse(DownloadPdf.verifyDownload(Write("book.json", Encoding.UTF8.GetPreamble().Concat("""{"asin":"B089T8FSK6"}"""))).IsSuccess);

	[TestMethod]
	public void A_login_page_fails()
		=> Assert.IsFalse(DownloadPdf.verifyDownload(Write("book.html", "<!DOCTYPE html><html><body>Sign in</body></html>")).IsSuccess);

	/// <summary>
	/// A supplement is whatever its URL says it is, so a non-PDF one is only rejected when it looks like a
	/// document Audible sent instead of the file. This is a zip's signature.
	/// </summary>
	[TestMethod]
	public void A_non_pdf_supplement_passes()
		=> Assert.IsTrue(DownloadPdf.verifyDownload(Write("book.zip", [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00])).IsSuccess);
}

file static class ByteExtensions
{
	public static byte[] Concat(this byte[] first, string second)
		=> [.. first, .. Encoding.UTF8.GetBytes(second)];
}
