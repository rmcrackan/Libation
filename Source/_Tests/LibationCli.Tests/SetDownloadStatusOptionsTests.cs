using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace LibationCli.Tests;

/// <summary>
/// The 'Not Downloaded' status was renamed 'Download Pending', so the flag that asks for it is now
/// --download-pending. The old --not-downloaded has to keep working: it is what years of scripts, forum
/// answers and issue comments tell people to run.
/// </summary>
[TestClass]
public class SetDownloadStatusOptionsTests
{
	private static SetDownloadStatusOptions Parse(params string[] args)
	{
		using var error = new StringWriter();
		var options = Program.ParseInvocation(args, error).Result?.Value as SetDownloadStatusOptions;
		Assert.IsNotNull(options);
		return options;
	}

	[TestMethod]
	[DataRow("--download-pending")]
	[DataRow("-p")]
	public void The_new_flag_asks_for_download_pending(string flag)
	{
		var options = Parse("set-status", flag);
		Assert.IsTrue(options.SetPending);
		Assert.IsFalse(options.SetDownloaded);
	}

	[TestMethod]
	[DataRow("--not-downloaded")]
	[DataRow("-n")]
	public void The_legacy_flag_still_asks_for_the_same_thing(string flag)
	{
		var options = Parse("set-status", flag);
		Assert.IsTrue(options.SetPending);
		Assert.IsFalse(options.SetDownloaded);
	}

	[TestMethod]
	public void Downloaded_on_its_own_asks_for_nothing_pending()
	{
		var options = Parse("set-status", "--downloaded");
		Assert.IsTrue(options.SetDownloaded);
		Assert.IsFalse(options.SetPending);
	}

	[TestMethod]
	[DataRow("--download-pending")]
	[DataRow("--not-downloaded")]
	public void Both_statuses_can_still_be_set_in_one_run(string pendingFlag)
	{
		var options = Parse("set-status", "--downloaded", pendingFlag);
		Assert.IsTrue(options.SetDownloaded);
		Assert.IsTrue(options.SetPending);
	}

	[TestMethod]
	public void A_run_naming_no_status_is_still_rejected()
	{
		// The two names live in one option group, so adding the legacy alias must not make the group
		// satisfiable by nothing at all.
		using var error = new StringWriter();
		var outcome = Program.ParseInvocation(["set-status"], error);

		Assert.AreEqual(ExitCode.ParseError, outcome.ExitCode);
	}

	[TestMethod]
	public void Help_offers_the_new_name_and_keeps_the_legacy_one_out_of_sight()
	{
		var help = new HelpVerb { HelpType = "set-status" }.GetHelpText().ToString();

		StringAssert.Contains(help, "--download-pending");
		Assert.IsFalse(
			help.Contains("--not-downloaded"),
			"The legacy alias should keep working without being advertised as a second way to do this.");
	}

	[TestMethod]
	public void Asins_are_still_read_alongside_the_new_flag()
	{
		var options = Parse("set-status", "-p", "B017V4IM1G");

		Assert.IsTrue(options.SetPending);
		CollectionAssert.AreEqual(new[] { "B017V4IM1G" }, options.Asins?.ToArray());
	}
}
