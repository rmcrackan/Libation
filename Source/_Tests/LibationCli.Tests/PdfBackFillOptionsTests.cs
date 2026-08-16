using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LibationCli.Tests;

/// <summary>
/// Which liberate runs also pick up titles that need nothing but their PDF. Reported in issue #1947: the CLI
/// had never downloaded PDFs for titles it had already downloaded the audio of.
/// </summary>
[TestClass]
public class PdfBackFillOptionsTests
{
	private static LiberateOptions Parse(params string[] args)
	{
		using var error = new StringWriter();
		var options = Program.ParseInvocation(args, error).Result?.Value as LiberateOptions;
		Assert.IsNotNull(options);
		return options;
	}

	[TestMethod]
	public void A_plain_run_picks_them_up()
		=> Assert.IsTrue(Parse("liberate").BackFillsPdfs);

	[TestMethod]
	public void A_forced_run_picks_them_up()
		=> Assert.IsTrue(Parse("liberate", "--force").BackFillsPdfs);

	[TestMethod]
	public void A_run_with_a_download_limit_picks_them_up()
		=> Assert.IsTrue(Parse("liberate", "--limit-books", "5").BackFillsPdfs);

	[TestMethod]
	[DataRow("--pdf")]
	[DataRow("-p")]
	public void A_pdf_only_run_needs_no_second_pass(string pdfOnly)
	{
		// It selects those titles to begin with.
		Assert.IsFalse(Parse("liberate", pdfOnly).BackFillsPdfs);
	}

	[TestMethod]
	[DataRow("liberate", "B017V4IM1G")]
	[DataRow("liberate", "--id", "B017V4IM1G")]
	public void A_run_that_names_its_titles_needs_no_second_pass(params string[] args)
	{
		// Naming a title re-downloads it, so its PDF follows from the main pass.
		Assert.IsFalse(Parse(args).BackFillsPdfs);
	}
}
