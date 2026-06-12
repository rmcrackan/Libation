#if DEBUG
using DataLayer;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms;

public partial class Form1
{
	private void Configure_DebugMenu()
	{
		var simulateItem = new ToolStripMenuItem("Simulate bad book failures (test dialog)...");
		simulateItem.Click += async (_, _) => await SimulateBadBookFailuresAsync();

		// Insert before Tour; toolStripSeparator2 above Tour already provides the divider.
		var insertIndex = settingsToolStripMenuItem.DropDownItems.IndexOf(tourToolStripMenuItem);
		if (insertIndex < 0)
			insertIndex = settingsToolStripMenuItem.DropDownItems.Count;

		settingsToolStripMenuItem.DropDownItems.Insert(insertIndex, simulateItem);
	}

	private async Task SimulateBadBookFailuresAsync()
	{
		var books = productsDisplay.GetVisible().Take(5).ToArray();
		if (books.Length == 0)
		{
			MessageBox.Show(
				this,
				"No books are visible in the grid.\n\nClear your filter or widen it, then try again.",
				"Test bad book dialog",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
			return;
		}

		var confirm = MessageBox.Show(
			this,
			$"Queue {books.Length} visible book(s) with simulated failures?\n\n"
			+ "No files will be downloaded. Each book will immediately show the bad-book error dialog.\n\n"
			+ "Set error handling to \"Ask each time\" in Settings > Download/Decrypt before testing.",
			"Test bad book dialog",
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question,
			MessageBoxDefaultButton.Button1);

		if (confirm != System.Windows.Forms.DialogResult.Yes)
			return;

		processBookQueue1.ViewModel.QueueSimulatedBadBookFailures(books);
		SetQueueCollapseState(false);
	}
}
#endif
