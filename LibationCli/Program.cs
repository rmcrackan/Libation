using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Dinah.Core;
using Dinah.Core.Collections;
using Dinah.Core.Collections.Generic;

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
        static async Task<int> Main(string[] args)
		{
			//***********************************************//
			//                                               //
			//   do not use Configuration before this line   //
			//                                               //
			//***********************************************//
			Setup.Initialize();
			Setup.SubscribeToDatabaseEvents();

			var types = Setup.LoadVerbs();

#if DEBUG
			string input = null;

			//input = "  export --help";
			//input = "  scan  rmcrackan";
			//input = "  liberate ";


			// note: this hack will fail for quoted file paths with spaces because it will break on those spaces
			if (!string.IsNullOrWhiteSpace(input))
				args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			var setBreakPointHere = args;
#endif

			var result = Parser.Default.ParseArguments(args, types);

			// if successfully parsed
			// async: run parsed options
			await result.WithParsedAsync<OptionsBase>(opt => opt.Run());

			// if not successfully parsed
			// sync: handle parse errors
			result.WithNotParsed(errors => HandleErrors(result, errors));

			return Environment.ExitCode;
		}

		private static void HandleErrors(ParserResult<object> result, IEnumerable<Error> errors)
		{
			var errorsList = errors.ToList();
			if (errorsList.Any(e => e.Tag.In(ErrorType.HelpRequestedError, ErrorType.VersionRequestedError, ErrorType.HelpVerbRequestedError)))
			{
				Environment.ExitCode = (int)ExitCode.NonRunNonError;
				return;
			}

			Environment.ExitCode = (int)ExitCode.ParseError;

			if (errorsList.Any(e => e.Tag.In(ErrorType.NoVerbSelectedError)))
			{
				Console.WriteLine("No verb selected");
				return;
			}

			var helpText = HelpText.AutoBuild(result,
				h => HelpText.DefaultParsingErrorsHandler(result, h),
				e => e);
			Console.WriteLine(helpText);
		}
	}
}
