using LibationUiBase;
using LibationWinForms.Dialogs;
using System;
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
			return null;
		}
		catch (Exception ex)
		{
			return ex;
		}
	}

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
