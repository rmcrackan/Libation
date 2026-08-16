using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace LibationCli.Tests;

/// <summary>
/// Which liberate runs leave alone the titles Audible recently refused. A run that says what it wants
/// downloaded must always attempt it.
/// </summary>
[TestClass]
public class DeferredRetryOptionsTests
{
	private static OptionsBase? Parse(params string[] args)
	{
		using var error = new StringWriter();
		return Program.ParseInvocation(args, error).Result?.Value as OptionsBase;
	}

	[TestMethod]
	public void A_plain_liberate_run_waits_on_the_titles_Audible_refused()
		=> Assert.IsTrue(((ProcessableOptionsBase)Parse("liberate")!).HonorsDeferredRetries);

	[TestMethod]
	[DataRow("--force")]
	[DataRow("-f")]
	public void Force_attempts_them_anyway(string force)
		=> Assert.IsFalse(((ProcessableOptionsBase)Parse("liberate", force)!).HonorsDeferredRetries);

	[TestMethod]
	[DataRow("--pdf")]
	[DataRow("-p")]
	public void A_pdf_only_run_is_never_held_back(string pdfOnly)
	{
		// The refusal recorded against a title is about its audiobook; a PDF is a different request.
		Assert.IsFalse(((ProcessableOptionsBase)Parse("liberate", pdfOnly)!).HonorsDeferredRetries);
	}

	[TestMethod]
	public void Other_processable_verbs_do_not_consult_the_record()
	{
		// convert-to-mp3 and the like never request a license, so there is nothing for them to wait on.
		Assert.IsFalse(((ProcessableOptionsBase)Parse("convert")!).HonorsDeferredRetries);
	}
}
