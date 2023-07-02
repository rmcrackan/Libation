using ApplicationServices;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli.Options
{
	[Verb("search", HelpText = "Search for books in your library")]
	internal class SearchOptions : OptionsBase
	{
		[Value(0, MetaName = "query", Required = true, HelpText = "Lucene search string")]
		public IEnumerable<string> Query { get; set; }

		protected override Task ProcessAsync()
		{
			var query = string.Join(" ", Query).Trim('\"');
			var results = SearchEngineCommands.Search(query).Docs.ToList();

			Console.WriteLine($"Found {results.Count} matching results.");

			const string nextPrompt = "Press any key for the next 10 results or Esc for all results";
			bool waitForNextBatch = true;

			for (int i = 0; i < results.Count; i += 10)
			{
				foreach (var doc in results.Skip(i).Take(10))
					Console.WriteLine(getDocDisplay(doc.Doc));

				if (waitForNextBatch)
				{
					Console.Write(nextPrompt);
					waitForNextBatch = Console.ReadKey(intercept: true).Key != ConsoleKey.Escape;
					ReplaceConsoleText(Console.Out, nextPrompt.Length, "");
					Console.SetCursorPosition(0, Console.CursorTop);
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
}
