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

				Console.WriteLine("ERROR");
				Console.WriteLine("=====");
				Console.WriteLine(ex.Message);
				Console.WriteLine();
				Console.WriteLine(ex.StackTrace);
			}
		}

		protected abstract Task ProcessAsync();
	}
}
