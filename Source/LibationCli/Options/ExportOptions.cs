using ApplicationServices;
using CommandLine;
using DataLayer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli
{
	[Verb("export", HelpText = "Must include path and flag for export file type: --xlsx , --csv , --json")]
	public class ExportOptions : OptionsBase
	{
		[Option(shortName: 'p', longName: "path", Required = true, HelpText = "Path to save file to.")]
		public string? FilePath { get; set; }

		#region explanation of mutually exclusive options
		/*
		giving these SetName values makes them mutually exclusive. they are in different sets. eg:
		class Options
		{
		    [Option("username", SetName = "auth")]
		    public string Username { get; set; }
		    [Option("password", SetName = "auth")]
		    public string Password { get; set; }

		    [Option("guestaccess", SetName = "guest")]
		    public bool GuestAccess { get; set; }
		}
		*/
		#endregion
		[Option(shortName: 'x', longName: "xlsx", HelpText = "Microsoft Excel Spreadsheet", SetName = "xlsx")]
		public bool xlsx { get; set; }

		[Option(shortName: 'c', longName: "csv", HelpText = "Comma-separated values", SetName = "csv")]
		public bool csv { get; set; }

		[Option(shortName: 'j', longName: "json", HelpText = "JavaScript Object Notation", SetName = "json")]
		public bool json { get; set; }

		[Value(0, MetaName = "[asins]", HelpText = "Optional product IDs of books to process.")]
		public IEnumerable<string>? Asins { get; set; }

		protected override Task ProcessAsync()
		{
			Action<string, IEnumerable<LibraryBook>?>? exporter
				= csv ? LibraryExporter.ToCsv
				: json ? LibraryExporter.ToJson
				: xlsx ? LibraryExporter.ToXlsx
				: Path.GetExtension(FilePath)?.ToLower() switch
				{
					".xlsx" => LibraryExporter.ToXlsx,
					".csv" => LibraryExporter.ToCsv,
					".json" => LibraryExporter.ToJson,
					_ => null
				};

			if (exporter is null)
			{
				PrintVerbUsage($"Undefined export format for file type \"{Path.GetExtension(FilePath)}\"");
			}
			else if (FilePath is null)
			{
				PrintVerbUsage($"Undefined export file name");
			}
			else
			{
				IEnumerable<LibraryBook>? booksToScan = null;
				if (Asins?.Any() is true)
				{
					booksToScan = DbContexts.GetLibrary_Flat_NoTracking().IntersectBy(Asins, l => l.Book.AudibleProductId);
				}
				exporter(FilePath, booksToScan);
				Console.WriteLine($"Library exported to: {FilePath}");
			}
			return Task.CompletedTask;
		}
	}
}
