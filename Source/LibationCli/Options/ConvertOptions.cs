using CommandLine;
using System.Threading.Tasks;

namespace LibationCli
{
	[Verb("convert", HelpText = "Convert mp4 to mp3.")]
	public class ConvertOptions : ProcessableOptionsBase
	{
		protected override Task ProcessAsync() => RunAsync(CreateProcessable<FileLiberator.ConvertToMp3>());
	}
}
