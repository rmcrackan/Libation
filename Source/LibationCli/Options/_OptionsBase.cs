using CommandLine;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace LibationCli
{
	public abstract class OptionsBase
	{
		public async Task Run()
		{
			try
			{
				await ProcessAsync();
			}
			catch (Exception ex)
			{
				Environment.ExitCode = (int)ExitCode.RunTimeError;
				PrintVerbUsage(new string[]
				{
					"ERROR",
					"=====",
					ex.Message,
					"",
					ex.StackTrace
				});
			}
		}

		protected void PrintVerbUsage(params string[] linesBeforeUsage)
		{
			var verb = GetType().GetCustomAttribute<VerbAttribute>().Name;
			var helpText = new HelpVerb { HelpType = verb }.GetHelpText();
			helpText.AddPreOptionsLines(linesBeforeUsage);
			helpText.AddPreOptionsLine("");
			helpText.AddPreOptionsLine($"{verb} Usage:");
			Console.Error.WriteLine(helpText);
		}

		protected static void ReplaceConsoleText(TextWriter writer, int previousLength, string newText)
		{
			writer.Write(new string('\b', previousLength));
			writer.Write(newText);
			writer.Write(new string(' ', int.Max(0, previousLength - newText.Length)));
		}

		protected abstract Task ProcessAsync();
	}
}
