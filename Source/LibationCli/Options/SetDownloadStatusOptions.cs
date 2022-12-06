using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServices;
using AudibleUtilities;
using CommandLine;

namespace LibationCli
{
    [Verb("set-status", HelpText = """
        Set download statuses throughout library based on whether each book's audio file can be found. 
        Must include at least one flag: --downloaded , --not-downloaded. 
        Downloaded: If the audio file can be found, set download status to 'Downloaded'. 
        Not Downloaded: If the audio file cannot be found, set download status to 'Not Downloaded'
        """)]
    public class SetDownloadStatusOptions : OptionsBase
    {
        [Option(shortName: 'd', longName: "downloaded", Required = true)]
        public bool SetDownloaded { get; set; }

        [Option(shortName: 'n', longName: "not-downloaded", Required = true)]
        public bool SetNotDownloaded { get; set; }

        protected override async Task ProcessAsync()
        {
            var libraryBooks = DbContexts.GetLibrary_Flat_NoTracking();

            var bulkSetStatus = new BulkSetDownloadStatus(libraryBooks, SetDownloaded, SetNotDownloaded);
            await Task.Run(() => bulkSetStatus.Discover());
            bulkSetStatus.Execute();

            foreach (var msg in bulkSetStatus.Messages)
                Console.WriteLine(msg);
        }
    }
}
