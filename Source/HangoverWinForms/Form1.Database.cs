using HangoverBase;

namespace HangoverWinForms;

public partial class Form1
{
	private DatabaseTab _tab;

	private void Load_databaseTab()
	{
		_tab = new(new(() => sqlTb.Text, sqlResultsTb.AppendText, s => sqlResultsTb.Text = s));

		_tab.LoadDatabaseFile();
		if (_tab.DbFile is null)
		{
			databaseFileLbl.Text = $"Database file not found";
			return;
		}

		databaseFileLbl.Text = $"Database file: {_tab.DbFile}";
	}

	private void databaseTab_VisibleChanged(object sender, EventArgs e)
	{
		if (!databaseTab.Visible)
			return;
	}

	private void sqlExecuteBtn_Click(object sender, EventArgs e) => _tab.ExecuteQuery();
}
