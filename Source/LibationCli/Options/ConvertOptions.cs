using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;

namespace LibationCli
{
	[Verb("convert", HelpText = "Convert mp4 to mp3.")]
	public class ConvertOptions : ProcessableOptionsBase
	{
		protected override Task ProcessAsync() => RunAsync(CreateProcessable<FileLiberator.ConvertToMp3>());
	}
}
