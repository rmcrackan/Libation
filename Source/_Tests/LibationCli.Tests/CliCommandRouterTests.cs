using AssertionHelper;
using LibationCli;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

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
			new[] { "upload", "B017V4IM1G", "--id", "B000000001", "--id", "B000000002" },
			route.ParserArgs);
	}

	[DataTestMethod]
	[DataRow(new[] { "help", "abs", "upload" }, new[] { "upload", "--help" })]
	[DataRow(new[] { "abs", "upload", "--help" }, new[] { "upload", "--help" })]
	[DataRow(new[] { "abs", "upload", "-h" }, new[] { "upload", "-h" })]
	public void Nested_help_forms_resolve_to_upload(string[] input, string[] expected)
		=> CollectionAssert.AreEqual(expected, CliCommandRouter.Route(input, CliCommandGroups.All).ParserArgs);

	[DataTestMethod]
	[DataRow("abs-upload", "--help")]
	[DataRow("upload", "--help")]
	public void Root_upload_aliases_are_rejected(params string[] args)
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(args, error);

		Assert.AreEqual(ExitCode.ParseError, outcome.ExitCode);
		Assert.IsFalse(error.ToString().Contains("--id", StringComparison.Ordinal));
	}

	[TestMethod]
	public void Global_help_lists_the_abs_group_without_legacy_upload_verb()
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(["--help"], error);

		Assert.AreEqual(ExitCode.ProcessCompletedSuccessfully, outcome.ExitCode);
		StringAssert.Contains(error.ToString(), "  abs                  Audiobookshelf commands.");
		Assert.IsFalse(error.ToString().Contains("abs-upload", StringComparison.Ordinal));
	}

	[TestMethod]
	[DataRow("abs")]
	[DataRow("help", "abs")]
	[DataRow("abs", "--help")]
	public void Abs_group_help_forms_are_handled_by_program(params string[] args)
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(args, error);

		Assert.AreEqual(ExitCode.ProcessCompletedSuccessfully, outcome.ExitCode);
		Assert.IsNull(outcome.Result);
		StringAssert.Contains(error.ToString(), "Audiobookshelf commands.");
		StringAssert.Contains(error.ToString(), "abs upload");
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
	public void Nested_abs_upload_parses_positional_and_repeatable_ids()
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(["abs", "upload", "B017V4IM1G", "--id", "B000000001", "--id", "B000000002"], error);

		Assert.IsNull(outcome.ExitCode);
		var options = outcome.Result!.Value as AbsUploadOptions;
		Assert.IsNotNull(options);
		CollectionAssert.AreEqual(new[] { "B017V4IM1G" }, options.Asins!.ToArray());
		CollectionAssert.AreEqual(new[] { "B000000001", "B000000002" }, options.Ids!.ToArray());
	}

	[TestMethod]
	public void Abs_upload_group_and_subcommand_are_case_insensitive()
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(["ABS", "UPLOAD", "B017V4IM1G"], error);

		Assert.IsNull(outcome.ExitCode);
		Assert.AreEqual(typeof(AbsUploadOptions), outcome.Result!.TypeInfo.Current);
	}

	[TestMethod]
	[DataRow("help", "abs", "upload")]
	[DataRow("abs", "upload", "-h")]
	public void Additional_nested_upload_help_forms_are_handled_by_program(params string[] args)
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(args, error);

		Assert.AreEqual(ExitCode.ProcessCompletedSuccessfully, outcome.ExitCode);
		StringAssert.Contains(error.ToString(), "--id");
	}

	[TestMethod]
	public void Unknown_abs_subcommand_writes_error_before_group_help_and_exits_parse_error()
	{
		using var error = new StringWriter();
		var outcome = Program.ParseInvocation(["abs", "download"], error);
		var output = error.ToString();

		Assert.AreEqual(ExitCode.ParseError, outcome.ExitCode);
		Assert.AreEqual("Unknown ABS command 'download'.", new StringReader(output).ReadLine());
		StringAssert.Contains(output, "Audiobookshelf commands.");
	}

	[TestMethod]
	public void Inverse_abs_help_upload_form_is_rejected()
	{
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(["abs", "help", "upload"], error);
		var output = error.ToString();

		Assert.AreEqual(ExitCode.ParseError, outcome.ExitCode);
		Assert.AreEqual("Unknown ABS command 'help'.", new StringReader(output).ReadLine());
		StringAssert.Contains(output, "Audiobookshelf commands.");
	}

	[TestMethod]
	public void Unknown_subcommand_uses_the_routed_group_name_in_diagnostic()
	{
		var group = new CliCommandGroup(
			"library",
			"Library commands.",
			[new("sync", "library-sync", "Sync the library.", typeof(AbsUploadOptions))]);
		using var error = new StringWriter();

		var outcome = Program.ParseInvocation(["library", "delete"], error, [group]);
		var output = error.ToString();

		Assert.AreEqual(ExitCode.ParseError, outcome.ExitCode);
		Assert.IsTrue(output.StartsWith($"Unknown LIBRARY command 'delete'.{Environment.NewLine}", StringComparison.Ordinal));
		StringAssert.Contains(output, "Library commands.");
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
