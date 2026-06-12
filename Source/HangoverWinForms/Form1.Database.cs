using HangoverBase;



namespace HangoverWinForms;



public partial class Form1

{

	private DatabaseTab _tab;



	private void Load_databaseTab()

	{

		_tab = new(new(() => sqlTb.Text, sqlResultsTb.AppendText, s => sqlResultsTb.Text = s, s => duplicateResultsTb.Text = s));



		_tab.LoadDatabaseFile();

		if (_tab.DbFile is null)

		{

			databaseFileLbl.Text = $"Database file not found";

			Load_fixDuplicatesTab();

			return;

		}



		databaseFileLbl.Text = $"Database file: {_tab.DbFile}";

		Load_fixDuplicatesTab();

	}



	private void sqlExecuteBtn_Click(object sender, EventArgs e)
	{
		if (HangoverBase.HangoverDbMutation.IsMutatingSql(sqlTb.Text)
			&& !HangoverMutationConfirm.Confirm(this, HangoverBase.HangoverDbMutation.SqlMutatingDescription))
			return;

		_tab.ExecuteQuery();
	}

}

