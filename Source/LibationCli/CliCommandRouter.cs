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

internal sealed record CliRoute(CliRouteKind Kind, string[] ParserArgs, CliCommandGroup? Group, CliSubcommand? Subcommand = null);

internal sealed record CliSubcommand(string Name, string ParserVerb, string HelpText, Type OptionsType);

internal sealed record CliCommandGroup(string Name, string HelpText, IReadOnlyList<CliSubcommand> Subcommands);

internal static class CliCommandRouter
{
	public static CliRoute Route(string[] args, IReadOnlyList<CliCommandGroup> groups)
	{
		if (args.Length == 3 && Matches("help", args[0]))
		{
			var nestedHelpGroup = groups.FirstOrDefault(group => Matches(group.Name, args[1]));
			var nestedHelpSubcommand = nestedHelpGroup?.Subcommands.FirstOrDefault(subcommand => Matches(subcommand.Name, args[2]));
			if (nestedHelpSubcommand is not null)
				return new(CliRouteKind.RewrittenCommand, [nestedHelpSubcommand.ParserVerb, "--help"], nestedHelpGroup, nestedHelpSubcommand);
		}

		if (args.Length == 2 && Matches("help", args[0]))
		{
			var helpGroup = groups.FirstOrDefault(group => Matches(group.Name, args[1]));
			if (helpGroup is not null)
				return new(CliRouteKind.GroupHelp, args, helpGroup);
		}

		var group = groups.FirstOrDefault(group => Matches(group.Name, args.FirstOrDefault()));
		if (group is null)
			return new(CliRouteKind.PassThrough, args, null);

		if (args.Length == 1 || (args.Length == 2 && Matches("--help", args[1])))
			return new(CliRouteKind.GroupHelp, args, group);

		var subcommand = group.Subcommands.FirstOrDefault(subcommand => Matches(subcommand.Name, args[1]));
		if (subcommand is null)
			return new(CliRouteKind.UnknownSubcommand, args, group);

		var parserArgs = new string[args.Length - 1];
		parserArgs[0] = subcommand.ParserVerb;
		Array.Copy(args, 2, parserArgs, 1, parserArgs.Length - 1);

		return new(CliRouteKind.RewrittenCommand, parserArgs, group, subcommand);
	}

	private static bool Matches(string expected, string? value)
		=> expected.Equals(value, StringComparison.OrdinalIgnoreCase);
}
