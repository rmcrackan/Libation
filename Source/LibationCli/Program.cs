using CommandLine;
using CommandLine.Text;
using Dinah.Core;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli;

file static class GlobalCliHelp
{
	internal static bool IsGlobalHelpToken(string token)
	{
		if (string.IsNullOrWhiteSpace(token))
			return false;
		return token.Equals("--help", StringComparison.OrdinalIgnoreCase)
			|| token.Equals("-h", StringComparison.OrdinalIgnoreCase)
			|| token.Equals("/?", StringComparison.OrdinalIgnoreCase)
			|| token.Equals("/h", StringComparison.OrdinalIgnoreCase)
			|| token.Equals("/help", StringComparison.OrdinalIgnoreCase);
	}
}

public enum ExitCode
{
	ProcessCompletedSuccessfully = 0,
	NonRunNonError = 1,
	ParseError = 2,
	RunTimeError = 3
}

internal sealed record CliParseOutcome(ParserResult<object>? Result, ExitCode? ExitCode);

class Program
{
	public static readonly Type[] VerbTypes = Setup.LoadVerbs();
	static async Task Main(string[] args)
	{
		Console.OutputEncoding = Console.InputEncoding = System.Text.Encoding.UTF8;
#if DEBUG
		string input = "";

		//input = "  set-status -n --force B017V4IM1G";
		//input = "  liberate B017V4IM1G";
		//input = "  convert B017V4IM1G";
		//input = "  search \"-liberated\"";
		//input = "  export --help";
		//input = "  version --check";
		//input = "  scan  rmcrackan";
		//input = "  help  set-status";
		//input = "  liberate ";
		//input = "get-setting -o Replace_OpenQuote=[ ";
		//input = "get-setting ";
		//input = "liberate B017V4NOZ0 --force -o Books=\"./Books\"";

		// note: this hack will fail for quoted file paths with spaces because it will break on those spaces
		if (!string.IsNullOrWhiteSpace(input))
			args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		var setBreakPointHere = args;
#endif
		var outcome = ParseInvocation(args, Console.Error);
		if (outcome.ExitCode is { } exitCode)
		{
			Environment.ExitCode = (int)exitCode;
			return;
		}

		//Everything parsed correctly, so execute the command
		// async: run parsed options
		await outcome.Result!.WithParsedAsync<OptionsBase>(opt => opt.Run());
	}

	internal static CliParseOutcome ParseInvocation(string[] args, TextWriter error)
	{
		var route = CliCommandRouter.Route(args, CliCommandGroups.All);
		if (route.Kind == CliRouteKind.GroupHelp)
		{
			error.WriteLine(HelpVerb.GetGroupHelpText(route.Group!));
			return new(null, ExitCode.ProcessCompletedSuccessfully);
		}

		args = route.ParserArgs;
		if (route.Kind == CliRouteKind.UnknownSubcommand)
		{
			error.WriteLine($"Unknown ABS command '{args[1]}'.");
			error.WriteLine(HelpVerb.GetGroupHelpText(route.Group!));
			return new(null, ExitCode.ParseError);
		}

		if (TryPrintGlobalHelpOnly(args, error))
			return new(null, ExitCode.ProcessCompletedSuccessfully);

		args = NormalizeVerbShortHelpAliases(args);

		var result = new Parser(ConfigureParser).ParseArguments(args, VerbTypes);

		if (result.Value is HelpVerb helper)
		{
			error.WriteLine(helper.GetHelpText());
			return new(result, ExitCode.ProcessCompletedSuccessfully);
		}

		if (result.TypeInfo.Current == typeof(HelpVerb))
		{
			//Error parsing the command, but the verb type was identified as HelpVerb
			//Print LibationCli usage
			var helpText = HelpVerb.CreateHelpText();
			helpText.AddVerbs(VerbTypes);
			error.WriteLine(helpText);
			return new(result, ExitCode.ProcessCompletedSuccessfully);
		}

		if (result.Errors.Any())
			return new(result, HandleErrors(result, error));

		return new(result, null);
	}

	private static ExitCode HandleErrors(ParserResult<object> result, TextWriter error)
	{
		var errorsList = result.Errors.ToList();

		if (errorsList.Any(e => e.Tag == ErrorType.HelpRequestedError))
		{
			WriteVerbOptionsHelp(result, error);
			return ExitCode.ProcessCompletedSuccessfully;
		}

		if (errorsList.OfType<HelpVerbRequestedError>().FirstOrDefault() is { } helpVerbErr)
		{
			WriteHelpForVerbRequestedError(helpVerbErr, error);
			return ExitCode.ProcessCompletedSuccessfully;
		}

		if (errorsList.Any(e => e.Tag == ErrorType.VersionRequestedError))
		{
			error.WriteLine(HelpVerb.CreateHelpText().Heading);
			return ExitCode.ProcessCompletedSuccessfully;
		}

		var helpText = HelpVerb.CreateHelpText();

		if (errorsList.OfType<NoVerbSelectedError>().Any())
		{
			//Print LibationCli usage
			helpText.AddPreOptionsLine("No verb selected");
			helpText.AddVerbs(VerbTypes);
		}
		else
		{
			//print the specified verb's usage
			helpText.AddDashesToOption = true;
			helpText.AutoHelp = true;

			if (!errorsList.OfType<UnknownOptionError>().Any(o => o.Token.ToLower() == "help"))
			{
				//verb was not executed with the "--help" option,
				//so print verb option parsing error info.
				helpText = HelpText.DefaultParsingErrorsHandler(result, helpText);
			}

			helpText.AddOptions(result);
		}
		error.WriteLine(helpText);
		return ExitCode.ParseError;
	}

	/// <summary>
	/// Multi-verb parsing treats the first token as a verb name, so bare <c>--help</c> / <c>-h</c> must be handled here.
	/// </summary>
	private static bool TryPrintGlobalHelpOnly(string[] args, TextWriter error)
	{
		if (args is not { Length: 1 } || !GlobalCliHelp.IsGlobalHelpToken(args[0]))
			return false;

		WriteGlobalVerbListHelp(error);
		return true;
	}

	/// <summary>
	/// CommandLineParser's implicit help is <c>--help</c> only; map the first <c>-h</c> after the verb (case-insensitive, so <c>-H</c> too) to <c>--help</c>.
	/// </summary>
	private static string[] NormalizeVerbShortHelpAliases(string[] args)
	{
		if (args.Length < 2)
			return args;

		var copy = (string[])args.Clone();
		for (var i = 1; i < copy.Length; i++)
		{
			if (copy[i].Equals("-h", StringComparison.OrdinalIgnoreCase))
			{
				copy[i] = "--help";
				break;
			}
		}

		return copy;
	}

	private static void WriteGlobalVerbListHelp(TextWriter error, string? preOptionsLine = null)
	{
		var helpText = HelpVerb.CreateHelpText();
		if (preOptionsLine is not null)
			helpText.AddPreOptionsLine(preOptionsLine);
		helpText.AddVerbs(VerbTypes);
		error.WriteLine(helpText);

		foreach (var group in CliCommandGroups.All)
			error.WriteLine($"  {group.Name,-21}{group.HelpText}");
	}

	private static void WriteVerbOptionsHelp(ParserResult<object> result, TextWriter error)
	{
		var helpText = HelpVerb.CreateHelpText();
		helpText.AddDashesToOption = true;
		helpText.AutoHelp = true;
		helpText.AddOptions(result);
		error.WriteLine(helpText);
	}

	private static void WriteHelpForVerbRequestedError(HelpVerbRequestedError helpVerbErr, TextWriter error)
	{
		if (!helpVerbErr.Matched || helpVerbErr.Type is null || string.IsNullOrWhiteSpace(helpVerbErr.Verb))
		{
			WriteGlobalVerbListHelp(error);
			return;
		}

		var subResult = new Parser(ConfigureParser).ParseArguments(new[] { helpVerbErr.Verb }, VerbTypes);
		if (subResult.TypeInfo.Current != typeof(NullInstance))
			WriteVerbOptionsHelp(subResult, error);
		else
			WriteGlobalVerbListHelp(error);
	}

	private static void ConfigureParser(ParserSettings settings)
	{
		settings.AllowMultiInstance = true;
		settings.AutoVersion = false;
		settings.AutoHelp = true;
	}
}
