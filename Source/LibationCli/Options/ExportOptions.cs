using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServices;
using AudibleUtilities;
using CommandLine;

namespace LibationCli
{
	[Verb("export", HelpText = "Must include path and flag for export file type: --xlsx , --csv , --json")]
	public class ExportOptions : OptionsBase
	{
		[Option(shortName: 'p', longName: "path", Required = true, HelpText = "Path to save file to.")]
		public string FilePath { get; set; }

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
		[Option(shortName: 'x', longName: "xlsx", SetName = "xlsx", Required = true)]
		public bool xlsx { get; set; }

		[Option(shortName: 'c', longName: "csv", SetName = "csv", Required = true)]
		public bool csv { get; set; }

		[Option(shortName: 'j', longName: "json", SetName = "json", Required = true)]
		public bool json { get; set; }

		protected override Task ProcessAsync()
		{
			if (xlsx)
				LibraryExporter.ToXlsx(FilePath);
			if (csv)
				LibraryExporter.ToCsv(FilePath);
			if (json)
				LibraryExporter.ToJson(FilePath);

			Console.WriteLine($"Library exported to: {FilePath}");

			return Task.CompletedTask;
		}
	}
}
