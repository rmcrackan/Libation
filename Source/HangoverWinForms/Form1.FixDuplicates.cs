namespace HangoverWinForms;

public partial class Form1
{
	private void Load_fixDuplicatesTab()
	{
		if (_tab.DbFile is null)
		{
			duplicateAsinStatusLbl.Text = "Duplicate ASIN cleanup unavailable (database not found).";
			scanDuplicateAsinsBtn.Enabled = false;
			removeDuplicateAsinsBtn.Enabled = false;
			return;
		}

		refreshDuplicateAsinStatus();
	}

	private void refreshDuplicateAsinStatus()
	{
		duplicateAsinStatusLbl.Text = _tab.GetDuplicateAsinStatusText();
		scanDuplicateAsinsBtn.Enabled = true;
		removeDuplicateAsinsBtn.Enabled = _tab.CanRemoveDuplicateAsins();
	}

	private void fixDuplicatesTab_VisibleChanged(object sender, EventArgs e)
	{
		if (!fixDuplicatesTab.Visible)
			return;

		if (_tab.DbFile is not null)
			refreshDuplicateAsinStatus();
	}

	private void scanDuplicateAsinsBtn_Click(object sender, EventArgs e)
	{
		_tab.ScanDuplicateAsins();
		refreshDuplicateAsinStatus();
	}

	private void removeDuplicateAsinsBtn_Click(object sender, EventArgs e)
	{
		if (!HangoverMutationConfirm.Confirm(this, HangoverBase.HangoverDbMutation.RemoveDuplicateAsinsDescription))
			return;

		_tab.RemoveDuplicateAsins();
		refreshDuplicateAsinStatus();
	}
}
