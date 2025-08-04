using CommandLine;
using LibationFileManager;
using System;
using System.Threading.Tasks;

namespace LibationCli
{
	[Verb("convert", HelpText = "Convert mp4 to mp3.")]
	public class ConvertOptions : ProcessableOptionsBase
	{
		protected override Task ProcessAsync()
		{
			if (AudibleFileStorage.BooksDirectory is null)
			{
				Console.Error.WriteLine("Error: Books directory is not set. Please configure the 'Books' setting in Settings.json.");
				return Task.CompletedTask;
			}

			return RunAsync(CreateProcessable<FileLiberator.ConvertToMp3>());
		}
	}
}
