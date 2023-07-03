using ApplicationServices;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationCli.Options;

[Verb("search", HelpText = "Search for books in your library")]
internal class SearchOptions : OptionsBase
{
	[Option('n', Default = 10, HelpText = "Number of search results per page")]
	public int NumResultsPerPage { get; set; }

	[Value(0, MetaName = "query", Required = true, HelpText = "Lucene search string")]
	public IEnumerable<string> Query { get; set; }

	protected override Task ProcessAsync()
	{
		var query = string.Join(" ", Query).Trim('\"');
		var results = SearchEngineCommands.Search(query).Docs.ToList();

		Console.WriteLine($"Found {results.Count} matching results.");

		string nextPrompt = "Press any key for the next " + NumResultsPerPage + " results or Esc for all results";
		bool waitForNextBatch = true;

		for (int i = 0; i < results.Count; i += NumResultsPerPage)
		{
			var sb = new StringBuilder();
			for (int j = i; j < int.Min(results.Count, i + NumResultsPerPage); j++)
				sb.AppendLine(getDocDisplay(results[j].Doc));

			Console.Write(sb.ToString());

			if (waitForNextBatch)
			{
				Console.Write(nextPrompt);
				waitForNextBatch = Console.ReadKey(intercept: true).Key != ConsoleKey.Escape;
				ReplaceConsoleText(Console.Out, nextPrompt.Length, "");
				Console.CursorLeft = 0;
			}
		}

		return Task.CompletedTask;
	}

	private static string getDocDisplay(Lucene.Net.Documents.Document doc)
	{
		var title = doc.GetField("title");
		var id = doc.GetField("_ID_");
		return $"[{id.StringValue}] - {title.StringValue}";
	}
}
