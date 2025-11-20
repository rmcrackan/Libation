using CommandLine;
using CommandLine.Text;
using Dinah.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli
{
	public enum ExitCode
	{
		ProcessCompletedSuccessfully = 0,
		NonRunNonError = 1,
		ParseError = 2,
		RunTimeError = 3
	}
	class Program
	{
		public readonly static Type[] VerbTypes = Setup.LoadVerbs();
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
			if (errorsList.Any(e => e.Tag.In(ErrorType.HelpRequestedError, ErrorType.VersionRequestedError, ErrorType.HelpVerbRequestedError)))
			{
				Environment.ExitCode = (int)ExitCode.NonRunNonError;
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

		private static void ConfigureParser(ParserSettings settings)
		{
			settings.AllowMultiInstance = true;
			settings.AutoVersion = false;
			settings.AutoHelp = false;
		}
	}
}
