#if DEBUG
using LibationUiBase.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	public async Task SimulateBadBookFailuresAsync()
	{
		var books = ProductsDisplay.GetVisibleBookEntries().Take(5).ToArray();
		if (books.Length == 0)
		{
			await MessageBox.Show(
				"No books are visible in the grid.\n\nClear your filter or widen it, then try again.",
				"Test bad book dialog",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
			return;
		}

		var confirm = await MessageBox.Show(
			$"Queue {books.Length} visible book(s) with simulated failures?\n\n"
			+ "No files will be downloaded. Each book will immediately show the bad-book error dialog.\n\n"
			+ "Set error handling to \"Ask each time\" in Settings > Download/Decrypt before testing.",
			"Test bad book dialog",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question,
			MessageBoxDefaultButton.Button1);

		if (confirm != DialogResult.Yes)
			return;

		ProcessQueue.QueueSimulatedBadBookFailures(books);
		setQueueCollapseState(false);
	}
}
#endif
