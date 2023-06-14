using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;

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

				Console.Error.WriteLine("ERROR");
				Console.Error.WriteLine("=====");
				Console.Error.WriteLine(ex.Message);
				Console.Error.WriteLine();
				Console.Error.WriteLine(ex.StackTrace);
			}
		}

		protected abstract Task ProcessAsync();
	}
}
