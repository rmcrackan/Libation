using ApplicationServices;
using CommandLine;
using DataLayer;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli
{
	[Verb("set-status", HelpText = """
        Set download statuses throughout library based on whether each book's audio file can be found.
        """)]
	public class SetDownloadStatusOptions : OptionsBase
	{
		[Option(shortName: 'd', longName: "downloaded", Group = "Download Status", HelpText = "set download status to 'Downloaded'")]
		public bool SetDownloaded { get; set; }

		[Option(shortName: 'n', longName: "not-downloaded", Group = "Download Status", HelpText = "set download status to 'Not Downloaded'")]
		public bool SetNotDownloaded { get; set; }

		[Option("force", HelpText = "Set the download status regardless of whether the book's audio file can be found. Only one download status option may be used with this option.")]
		public bool Force { get; set; }

		[Value(0, MetaName = "[asins]", HelpText = "Optional product IDs of books on which to set download status.")]
		public IEnumerable<string> Asins { get; set; }

		protected override async Task ProcessAsync()
		{
			if (Force && SetDownloaded && SetNotDownloaded)
			{
				PrintVerbUsage("ERROR:\nWhen run with --force option, only one download status option may be used.");
				return;
			}

			var libraryBooks = DbContexts.GetLibrary_Flat_NoTracking();

			if (Asins.Any())
			{
				var asins = Asins.Select(a => a.TrimStart('[').TrimEnd(']').ToLower()).ToArray();
				libraryBooks = libraryBooks.Where(lb => lb.Book.AudibleProductId.ToLower().In(asins)).ToList();

				if (libraryBooks.Count == 0)
				{
					Console.Error.WriteLine("Could not find any books matching asins");
					return;
				}
			}

			if (Force)
			{
				var status = SetDownloaded ? LiberatedStatus.Liberated : LiberatedStatus.NotLiberated;

				var num = libraryBooks.UpdateBookStatus(status);
				Console.WriteLine($"Set LiberatedStatus to '{status}' on {"book".PluralizeWithCount(num)}");
			}
			else
			{
				var bulkSetStatus = new BulkSetDownloadStatus(libraryBooks, SetDownloaded, SetNotDownloaded);
				await Task.Run(() => bulkSetStatus.Discover());
				bulkSetStatus.Execute();

				foreach (var msg in bulkSetStatus.Messages)
					Console.WriteLine(msg);
			}
		}
	}
}
