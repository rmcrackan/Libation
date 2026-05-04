using CommandLine;
using CommandLine.Text;
using Dinah.Core;
using System;
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

		if (TryPrintGlobalHelpOnly(args))
			return;

		args = NormalizeVerbShortHelpAliases(args);

		var result = new Parser(ConfigureParser).ParseArguments(args, VerbTypes);

		if (result.Value is HelpVerb helper)
			Console.Error.WriteLine(helper.GetHelpText());
		else if (result.TypeInfo.Current == typeof(HelpVerb))
		{
			//Error parsing the command, but the verb type was identified as HelpVerb
			//Print LibationCli usage
			var helpText = HelpVerb.CreateHelpText();
			helpText.AddVerbs(VerbTypes);
			Console.Error.WriteLine(helpText);
		}
		else if (result.Errors.Any())
			HandleErrors(result);
		else
		{
			//Everything parsed correctly, so execute the command
			// async: run parsed options
			await result.WithParsedAsync<OptionsBase>(opt => opt.Run());
		}
	}

	private static void HandleErrors(ParserResult<object> result)
	{
		var errorsList = result.Errors.ToList();

		if (errorsList.Any(e => e.Tag == ErrorType.HelpRequestedError))
		{
			Environment.ExitCode = (int)ExitCode.ProcessCompletedSuccessfully;
			WriteVerbOptionsHelp(result);
			return;
		}

		if (errorsList.OfType<HelpVerbRequestedError>().FirstOrDefault() is { } helpVerbErr)
		{
			Environment.ExitCode = (int)ExitCode.ProcessCompletedSuccessfully;
			WriteHelpForVerbRequestedError(helpVerbErr);
			return;
		}

		if (errorsList.Any(e => e.Tag == ErrorType.VersionRequestedError))
		{
			Environment.ExitCode = (int)ExitCode.ProcessCompletedSuccessfully;
			Console.Error.WriteLine(HelpVerb.CreateHelpText().Heading);
			return;
		}

		Environment.ExitCode = (int)ExitCode.ParseError;
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
		Console.Error.WriteLine(helpText);
	}

	/// <summary>
	/// Multi-verb parsing treats the first token as a verb name, so bare <c>--help</c> / <c>-h</c> must be handled here.
	/// </summary>
	private static bool TryPrintGlobalHelpOnly(string[] args)
	{
		if (args is not { Length: 1 } || !GlobalCliHelp.IsGlobalHelpToken(args[0]))
			return false;

		WriteGlobalVerbListHelp();
		Environment.ExitCode = (int)ExitCode.ProcessCompletedSuccessfully;
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

	private static void WriteGlobalVerbListHelp(string? preOptionsLine = null)
	{
		var helpText = HelpVerb.CreateHelpText();
		if (preOptionsLine is not null)
			helpText.AddPreOptionsLine(preOptionsLine);
		helpText.AddVerbs(VerbTypes);
		Console.Error.WriteLine(helpText);
	}

	private static void WriteVerbOptionsHelp(ParserResult<object> result)
	{
		var helpText = HelpVerb.CreateHelpText();
		helpText.AddDashesToOption = true;
		helpText.AutoHelp = true;
		helpText.AddOptions(result);
		Console.Error.WriteLine(helpText);
	}

	private static void WriteHelpForVerbRequestedError(HelpVerbRequestedError helpVerbErr)
	{
		if (!helpVerbErr.Matched || helpVerbErr.Type is null || string.IsNullOrWhiteSpace(helpVerbErr.Verb))
		{
			WriteGlobalVerbListHelp();
			return;
		}

		var subResult = new Parser(ConfigureParser).ParseArguments(new[] { helpVerbErr.Verb }, VerbTypes);
		if (subResult.TypeInfo.Current != typeof(NullInstance))
			WriteVerbOptionsHelp(subResult);
		else
			WriteGlobalVerbListHelp();
	}

	private static void ConfigureParser(ParserSettings settings)
	{
		settings.AllowMultiInstance = true;
		settings.AutoVersion = false;
		settings.AutoHelp = true;
	}
}
