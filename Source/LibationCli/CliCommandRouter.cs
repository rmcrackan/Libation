using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationCli;

internal enum CliRouteKind
{
	PassThrough,
	GroupHelp,
	RewrittenCommand,
	UnknownSubcommand,
}

internal sealed record CliRoute(CliRouteKind Kind, string[] ParserArgs, CliCommandGroup? Group);

internal sealed record CliSubcommand(string Name, string FlatVerb, string HelpText);

internal sealed record CliCommandGroup(string Name, string HelpText, IReadOnlyList<CliSubcommand> Subcommands);

internal static class CliCommandRouter
{
	public static CliRoute Route(string[] args, IReadOnlyList<CliCommandGroup> groups)
	{
		var group = groups.FirstOrDefault(group => group.Name == args.FirstOrDefault());
		if (group is null)
			return new(CliRouteKind.PassThrough, args, null);

		if (args.Length == 1)
			return new(CliRouteKind.GroupHelp, args, group);

		var subcommand = group.Subcommands.FirstOrDefault(subcommand => subcommand.Name == args[1]);
		if (subcommand is null)
			return new(CliRouteKind.UnknownSubcommand, args, group);

		var parserArgs = new string[args.Length - 1];
		parserArgs[0] = subcommand.FlatVerb;
		Array.Copy(args, 2, parserArgs, 1, parserArgs.Length - 1);

		return new(CliRouteKind.RewrittenCommand, parserArgs, group);
	}
}
