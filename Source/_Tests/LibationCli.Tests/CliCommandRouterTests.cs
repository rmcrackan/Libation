using AssertionHelper;
using LibationCli;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationCli.Tests;

[TestClass]
public class CliCommandRouterTests
{
	[TestMethod]
	[DataRow("abs")]
	[DataRow("help", "abs")]
	[DataRow("abs", "--help")]
	public void Abs_group_help_forms_request_group_help(params string[] args)
	{
		var route = CliCommandRouter.Route(args, CliCommandGroups.All);

		Assert.AreEqual(CliRouteKind.GroupHelp, route.Kind);
		route.Group!.Name.Should().Be("abs");
	}

	[TestMethod]
	public void Abs_upload_rewrites_only_the_command_path()
	{
		var route = CliCommandRouter.Route(
			["abs", "upload", "B017V4IM1G", "--id", "B000000001", "--id", "B000000002"],
			CliCommandGroups.All);

		CollectionAssert.AreEqual(
			new[] { "abs-upload", "B017V4IM1G", "--id", "B000000001", "--id", "B000000002" },
			route.ParserArgs);
	}

	[DataTestMethod]
	[DataRow(new[] { "help", "abs", "upload" }, new[] { "help", "abs-upload" })]
	[DataRow(new[] { "abs", "upload", "--help" }, new[] { "abs-upload", "--help" })]
	[DataRow(new[] { "abs", "upload", "-h" }, new[] { "abs-upload", "-h" })]
	public void Nested_help_forms_resolve_to_upload(string[] input, string[] expected)
		=> CollectionAssert.AreEqual(expected, CliCommandRouter.Route(input, CliCommandGroups.All).ParserArgs);

	[TestMethod]
	public void Legacy_abs_upload_is_passed_through_unchanged()
	{
		var route = CliCommandRouter.Route(["abs-upload", "--id", "B017V4IM1G"], CliCommandGroups.All);

		Assert.AreEqual(CliRouteKind.PassThrough, route.Kind);
		CollectionAssert.AreEqual(new[] { "abs-upload", "--id", "B017V4IM1G" }, route.ParserArgs);
	}

	[TestMethod]
	public void Global_help_lists_the_abs_group_and_legacy_upload_verb()
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(["--help"], error);

		Assert.AreEqual(ExitCode.ProcessCompletedSuccessfully, outcome.ExitCode);
		StringAssert.Contains(error.ToString(), "  abs                  Audiobookshelf commands.");
		StringAssert.Contains(error.ToString(), "  abs-upload           Upload already-liberated books to Audiobookshelf.");
	}

	[TestMethod]
	public void Nested_abs_upload_help_reaches_upload_help_path()
	{
		using var error = new StringWriter();
		var outcome = Program.ParseInvocation(["abs", "upload", "--help"], error);

		Assert.AreEqual(ExitCode.ProcessCompletedSuccessfully, outcome.ExitCode);
		Assert.AreEqual(typeof(AbsUploadOptions), outcome.Result!.TypeInfo.Current);
		StringAssert.Contains(error.ToString(), "--id");
	}

	[TestMethod]
	public void Unknown_abs_subcommand_writes_error_before_group_help_and_exits_parse_error()
	{
		using var error = new StringWriter();
		var outcome = Program.ParseInvocation(["abs", "download"], error);
		var output = error.ToString();

		Assert.AreEqual(ExitCode.ParseError, outcome.ExitCode);
		Assert.IsTrue(output.StartsWith($"Unknown ABS command 'download'.{Environment.NewLine}", StringComparison.Ordinal));
		StringAssert.Contains(output, "Audiobookshelf commands.");
	}

	[TestMethod]
	public void Non_group_command_passes_through_unchanged()
	{
		var route = CliCommandRouter.Route(["status", "--verbose"], CliCommandGroups.All);

		Assert.AreEqual(CliRouteKind.PassThrough, route.Kind);
		CollectionAssert.AreEqual(new[] { "status", "--verbose" }, route.ParserArgs);
	}

	[TestMethod]
	public void Abs_unknown_subcommand_is_reported()
	{
		var route = CliCommandRouter.Route(["abs", "delete"], CliCommandGroups.All);

		Assert.AreEqual(CliRouteKind.UnknownSubcommand, route.Kind);
		route.Group!.Name.Should().Be("abs");
	}
}
