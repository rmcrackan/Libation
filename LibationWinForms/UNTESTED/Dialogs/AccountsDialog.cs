using System;
using System.Linq;
using System.Windows.Forms;
using AudibleApi;
using InternalUtilities;

namespace LibationWinForms.Dialogs
{
	public partial class AccountsDialog : Form
	{
		const string NON_BREAKING_SPACE = "\u00a0";

		const string COL_Original = nameof(Original);
		const string COL_Delete = nameof(DeleteAccount);
		const string COL_LibraryScan = nameof(LibraryScan);
		const string COL_AccountId = nameof(AccountId);
		const string COL_AccountName = nameof(AccountName);
		const string COL_Locale = nameof(Locale);

		public AccountsDialog()
		{
			InitializeComponent();

			dataGridView1.Columns[COL_AccountName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			populateDropDown();

			populateGridValues();
		}

		private void populateDropDown()
			=> (dataGridView1.Columns[COL_Locale] as DataGridViewComboBoxColumn).DataSource
				= Localization.Locales
					.Select(l => l.Name)
					.OrderBy(a => a).ToList();

		private void populateGridValues()
		{
			// WARNING
			// behind the scenes this returns a AccountsSettings
			// accounts persister will write ANY EDIT to object immediately to file
			// here: copy strings
			// only persist in 'save' step
			var accounts = AudibleApiStorage.GetAccountsSettings().Accounts;
			if (!accounts.Any())
				return;

			foreach (var account in accounts)
				dataGridView1.Rows.Add(
					new { account.AccountId, account.Locale.Name },
					"X",
					account.LibraryScan,
					account.AccountId,
					account.Locale.Name,
					account.AccountName);
		}

		private void dataGridView1_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
		{
			e.Row.Cells[COL_Delete].Value = "X";
			e.Row.Cells[COL_LibraryScan].Value = true;
		}

		private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			var dgv = (DataGridView)sender;

			var col = dgv.Columns[e.ColumnIndex];
			if (col is DataGridViewButtonColumn && e.RowIndex >= 0)
			{
				var row = dgv.Rows[e.RowIndex];
				switch (col.Name)
				{
					case COL_Delete:
						// if final/edit row: do nothing
						if (e.RowIndex < dgv.RowCount - 1)
							dgv.Rows.Remove(row);
						break;
						//case COL_MoveUp:
						//	// if top: do nothing
						//	if (e.RowIndex < 1)
						//		break;
						//	dgv.Rows.Remove(row);
						//	dgv.Rows.Insert(e.RowIndex - 1, row);
						//	break;
						//case COL_MoveDown:
						//	// if final/edit row or bottom filter row: do nothing
						//	if (e.RowIndex >= dgv.RowCount - 2)
						//		break;
						//	dgv.Rows.Remove(row);
						//	dgv.Rows.Insert(e.RowIndex + 1, row);
						//	break;
				}
			}
		}

		private void cancelBtn_Click(object sender, EventArgs e) => this.Close();

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var accounts = AudibleApiStorage.GetAccountsSettings()
				.Accounts;

			// find added
			// validate

			// find deleted

			// find edited
			// validate

			// persist

		}
	}
}
