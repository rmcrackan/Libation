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
	public void A_pdf_only_run_waits_on_them_as_well(string pdfOnly)
	{
		// A PDF comes from the same license request as the audiobook, so a title Libation is waiting on would be
		// refused for its PDF exactly as it was for its audio. This run used to be exempt, which is how issue
		// #1973's scheduled run kept asking.
		Assert.IsTrue(((ProcessableOptionsBase)Parse("liberate", pdfOnly)!).HonorsDeferredRetries);
	}

	[TestMethod]
	public void Other_processable_verbs_do_not_consult_the_record()
	{
		// convert-to-mp3 and the like never request a license, so there is nothing for them to wait on.
		Assert.IsFalse(((ProcessableOptionsBase)Parse("convert")!).HonorsDeferredRetries);
	}
}
