using ApplicationServices;
using CommandLine;
using DataLayer;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli;

[Verb("set-status", HelpText = """
        Set download statuses throughout library based on whether each book's audio file can be found.
        """)]
public class SetDownloadStatusOptions : OptionsBase
{
	//https://github.com/commandlineparser/commandline/wiki/Option-Groups
	[Option(shortName: 'd', longName: "downloaded", Group = "Download Status", HelpText = "if the audio file can be found, mark the book 'Downloaded'")]
	public bool SetDownloaded { get; set; }

	[Option(shortName: 'p', longName: "download-pending", Group = "Download Status", HelpText = "if the audio file cannot be found, mark the book 'Download Pending' (previously 'Not Downloaded')")]
	public bool SetDownloadPending { get; set; }

	/// <summary>
	/// What <see cref="SetDownloadPending"/> was called while the status was named "Not Downloaded". Kept
	/// working so scripts written against the old name survive the rename, and kept out of the help so the
	/// new name is the only one offered. <see cref="SetPending"/> is what the verb acts on.
	/// </summary>
	[Option(shortName: 'n', longName: "not-downloaded", Group = "Download Status", Hidden = true)]
	public bool SetNotDownloadedLegacy { get; set; }

	/// <summary>Whether the run was asked for 'Download Pending', under either flag name.</summary>
	internal bool SetPending => SetDownloadPending || SetNotDownloadedLegacy;

	[Option('f', "force", HelpText = "Set the download status regardless of whether the book's audio file can be found. Only one download status option may be used with this option.")]
	public bool Force { get; set; }

	[Value(0, MetaName = "[asins]", HelpText = "Optional product IDs of books on which to set download status.")]
	public IEnumerable<string>? Asins { get; set; }

	protected override async Task ProcessAsync()
	{
		if (Force && SetDownloaded && SetPending)
		{
			PrintVerbUsage("ERROR:\nWhen run with --force option, only one download status option may be used.");
			return;
		}

		var libraryBooks = DbContexts.GetLibrary_Flat_NoTracking();

		if (Asins?.Any() is true)
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

			var num = await libraryBooks.UpdateBookStatusAsync(status);
			Console.WriteLine($"Set LiberatedStatus to '{status}' on {"book".PluralizeWithCount(num)}");
		}
		else
		{
			var bulkSetStatus = new BulkSetDownloadStatus(libraryBooks, SetDownloaded, SetPending);
			await Task.Run(() => bulkSetStatus.Discover());
			await bulkSetStatus.ExecuteAsync();

			foreach (var msg in bulkSetStatus.Messages)
				Console.WriteLine(msg);
		}
	}
}
