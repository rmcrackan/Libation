using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DomainServices;
using Dinah.Core;

namespace LibationWinForm
{
    public partial class ScanLibraryDialog : Form, IIndexLibraryDialog
    {
        public ScanLibraryDialog()
        {
            InitializeComponent();
        }

        public string StringBasedValidate() => null;

        List<string> successMessages = new List<string>();
        public string SuccessMessage => string.Join("\r\n", successMessages);

        public int NewBooksAdded { get; private set; }
        public int TotalBooksProcessed { get; private set; }

        public async Task DoMainWorkAsync()
        {
			using var pageRetriever = websiteProcessorControl1.GetPageRetriever();
			var jsonFilepaths = await DownloadLibrary.DownloadLibraryAsync(pageRetriever).ConfigureAwait(false);

            successMessages.Add($"Downloaded {"library page".PluralizeWithCount(jsonFilepaths.Count)}");

            (TotalBooksProcessed, NewBooksAdded) = await Indexer
                .IndexLibraryAsync(jsonFilepaths)
                .ConfigureAwait(false);

            successMessages.Add($"Total processed: {TotalBooksProcessed}");
            successMessages.Add($"New: {NewBooksAdded}");
        }
    }
}
