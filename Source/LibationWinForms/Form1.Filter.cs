using ApplicationServices;
using Dinah.Core.Threading;
using LibationUiBase;
using LibationWinForms.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms;

public partial class Form1
{
	protected void Configure_Filter() { }

	private void filterHelpBtn_Click(object sender, EventArgs e) => ShowSearchSyntaxDialog();

	private void filterSearchTb_TextCleared(object sender, EventArgs e)
	{
		performFilter(string.Empty);
	}
	private void filterSearchTb_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == (char)Keys.Return)
		{
			performFilter(this.filterSearchTb.Text);

			// silence the 'ding'
			e.Handled = true;
		}
	}

	private void filterBtn_Click(object sender, EventArgs e) => performFilter(this.filterSearchTb.Text);

	private string? lastGoodFilter = null;
	private void performFilter(string? filterString)
	{
		if (applyFilter(filterString) is not Exception failure)
			return;

		Serilog.Log.Logger.Error(failure, "Error performing filtering. {@DebugInfo}", new { filterString, lastGoodFilter });

		if (SearchIndexRecovery.IsIndexUnavailable(failure))
			MessageBox.Show(this, SearchIndexRecovery.ManualRecoveryInstructions, SearchIndexRecovery.Caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
		else
			MessageBox.Show(this, $"Bad filter string:\r\n\r\n{failure.Message}", "Bad filter string", MessageBoxButtons.OK, MessageBoxIcon.Error);

		// Restore the last filter that worked, then give up on filtering entirely. Recursing into performFilter
		// here never terminated when the search index rather than the query was at fault, because that fails for
		// every filter including the one being restored. An empty filter never reaches the search engine.
		if (!string.IsNullOrEmpty(lastGoodFilter) && applyFilter(lastGoodFilter) is null)
			return;

		applyFilter(string.Empty);
	}

	/// <summary>Applies a filter, returning the exception that stopped it, or null when it worked.</summary>
	private Exception? applyFilter(string? filterString)
	{
		this.filterSearchTb.Text = filterString;

		try
		{
			productsDisplay.Filter(filterString);
			lastGoodFilter = filterString;
			refreshGridEmptyState(filterString);
			return null;
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

	private int visibleCount;
	private bool libraryIsEmpty;
	private bool anyAccounts;
	private int booksInTrash;

	/// <summary>
	/// Decide which explanation the empty grid should carry, if any. An empty library takes precedence over
	/// an empty filter result: "no books match" is true but useless when there are no books at all.
	/// </summary>
	private async void refreshGridEmptyState(string? filterString)
	{
		// Someone searching is not someone getting started, so a filter always answers for itself.
		var hasFilter = !string.IsNullOrWhiteSpace(filterString);
		var gettingStarted = libraryIsEmpty && !hasFilter && !LibraryCommands.Scanning;
		var noMatches = hasFilter && visibleCount == 0;

		this.UIThreadSync(() =>
		{
			emptyLibraryLbl.Text = $"{GridEmptyStateUi.EmptyLibraryHeadline(anyAccounts)}\r\n{GridEmptyStateUi.EmptyLibraryDetail(anyAccounts)}";
			emptyLibraryActionLink.Text = anyAccounts ? GridEmptyStateUi.ScanLibraryButton : GridEmptyStateUi.AddAccountButton;
			emptyLibraryTourLink.Text = GridEmptyStateUi.TakeTheTourButton;
			emptyLibraryLbl.Visible = gettingStarted;
			emptyLibraryActionLink.Visible = gettingStarted;
			emptyLibraryTourLink.Visible = gettingStarted;

			// The books are not gone, they are in the trash. Saying so keeps the headline above from
			// reading as though they were never there.
			emptyLibraryTrashLink.Visible = gettingStarted && booksInTrash > 0;
			if (emptyLibraryTrashLink.Visible)
				SetTrashLink(emptyLibraryTrashLink, GridEmptyStateUi.EmptyLibraryTrashHintText(booksInTrash));

			noMatchesLbl.Text = GridEmptyStateUi.NoMatchesText(filterString);
			noMatchesLbl.Visible = noMatches;
			noMatchesTrashLink.Visible = false;

			noMatchesPanel.Visible = gettingStarted || noMatches;
			if (noMatchesPanel.Visible)
				noMatchesPanel.BringToFront();
		});

		if (!noMatches)
			return;

		// A trashed book is filtered out of the library and out of the search index, so searching for one
		// looks exactly like searching for a book that was never imported. Say when that is what happened.
		var matches = await Task.Run(() => TrashBinSearch.Search(filterString));
		if (matches.Count == 0)
			return;

		this.UIThreadSync(() =>
		{
			SetTrashLink(noMatchesTrashLink, GridEmptyStateUi.NoMatchesTrashHintText(matches.Count));
			noMatchesTrashLink.Visible = true;
		});
	}

	/// <summary>Sentence plus a trailing "Open Trash Bin" link, which is the clickable part.</summary>
	private static void SetTrashLink(LinkLabel link, string sentence)
	{
		link.Text = $"{sentence}  {GridEmptyStateUi.OpenTrashBinButton}";
		link.LinkArea = new LinkArea(
			link.Text.Length - GridEmptyStateUi.OpenTrashBinButton.Length,
			GridEmptyStateUi.OpenTrashBinButton.Length);
	}

	private void noMatchesTrashLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		=> openTrashBinToolStripMenuItem_Click(sender, e);

	private void emptyLibraryActionLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		// Adding an account comes first; telling someone without one to scan is a dead end.
		if (anyAccounts)
			scanLibraryOfAllAccountsToolStripMenuItem_Click(sender, e);
		else
			noAccountsYetAddAccountToolStripMenuItem_Click(sender, e);
	}

	private void emptyLibraryTourLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		=> tourToolStripMenuItem_Click(sender, e);

	/// <summary>Re-evaluate against the filter already applied, for changes that came from somewhere else.</summary>
	private void refreshGridEmptyState() => refreshGridEmptyState(lastGoodFilter);

	/// <summary>Whether the library has any books at all, from the counts that were just taken.</summary>
	private void setLibraryIsEmpty(bool isEmpty)
	{
		libraryIsEmpty = isEmpty;
		refreshGridEmptyState();
	}

	private void setAnyAccounts(bool any)
	{
		anyAccounts = any;
		refreshGridEmptyState();
	}

	private void setBooksInTrash(int count)
	{
		booksInTrash = count;
		refreshGridEmptyState();
	}

	private void emptyLibraryTrashLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		=> openTrashBinToolStripMenuItem_Click(sender, e);

	public SearchSyntaxDialog ShowSearchSyntaxDialog()
	{
		var dialog = new SearchSyntaxDialog();
		dialog.TagDoubleClicked += Dialog_TagDoubleClicked;
		dialog.FormClosed += Dialog_Closed;
		filterHelpBtn.Enabled = false;
		dialog.Show(this);
		return dialog;

		void Dialog_Closed(object? sender, FormClosedEventArgs e)
		{
			dialog.TagDoubleClicked -= Dialog_TagDoubleClicked;
			filterHelpBtn.Enabled = true;
		}
		void Dialog_TagDoubleClicked(object? sender, string tag)
		{
			if (string.IsNullOrEmpty(tag)) return;

			var text = filterSearchTb.Text;
			var selStart = Math.Min(Math.Max(0, filterSearchTb.SelectionStart), text.Length);

			filterSearchTb.Text = text.Insert(selStart, tag);
			filterSearchTb.SelectionStart = selStart + tag.Length;
			filterSearchTb.Focus();
		}
	}
}
