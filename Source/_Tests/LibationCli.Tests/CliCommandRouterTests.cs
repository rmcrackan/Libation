using AssertionHelper;
using LibationCli;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
