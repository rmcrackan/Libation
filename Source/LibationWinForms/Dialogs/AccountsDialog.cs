using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AudibleApi;
using AudibleUtilities;

namespace LibationWinForms.Dialogs
{
	public partial class AccountsDialog : Form
	{
		private const string COL_Delete = nameof(DeleteAccount);
		private const string COL_LibraryScan = nameof(LibraryScan);
		private const string COL_AccountId = nameof(AccountId);
		private const string COL_AccountName = nameof(AccountName);
		private const string COL_Locale = nameof(Locale);

		private Form1 _parent { get; }

		public AccountsDialog(Form1 parent)
		{
			_parent = parent;

			InitializeComponent();

			dataGridView1.Columns[COL_AccountName].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

			populateDropDown();

			populateGridValues();
			this.SetLibationIcon();
		}

		private void populateDropDown()
			=> (dataGridView1.Columns[COL_Locale] as DataGridViewComboBoxColumn).DataSource
				= Localization.Locales
					.Select(l => l.Name)
					.OrderBy(a => a).ToList();

		private void populateGridValues()
		{
			// WARNING: accounts persister will write ANY EDIT to object immediately to file
			// here: copy strings and dispose of persister
			// only persist in 'save' step
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var accounts = persister.AccountsSettings.Accounts;
			if (!accounts.Any())
				return;

			foreach (var account in accounts)
				dataGridView1.Rows.Add(
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

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private class AccountDto
		{
			public string AccountId { get; set; }
			public string AccountName { get; set; }
			public string LocaleName { get; set; }
			public bool LibraryScan { get; set; }
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			try
			{
				if (!inputIsValid())
					return;

				// without transaction, accounts persister will write ANY EDIT immediately to file
				using var persister = AudibleApiStorage.GetAccountsSettingsPersister();

				persister.BeginTransation();
				persist(persister.AccountsSettings);
				persister.CommitTransation();

				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert("Error attempting to save accounts", "Error saving accounts", ex);
			}
		}

		private bool inputIsValid()
		{
			var dtos = getRowDtos();

			foreach (var dto in dtos)
			{
				if (string.IsNullOrWhiteSpace(dto.AccountId))
				{
					MessageBox.Show("Account id cannot be blank. Please enter an account id for all accounts.", "Blank account", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				if (string.IsNullOrWhiteSpace(dto.LocaleName))
				{
					MessageBox.Show("Please select a locale (i.e.: country or region) for all accounts.", "Blank region", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
			}

			return true;
		}

		private void persist(AccountsSettings accountsSettings)
		{
			var existingAccounts = accountsSettings.Accounts;
			var dtos = getRowDtos();

			// editing account id is a special case. an account is defined by its account id, therefore this is really a different account. the user won't care about this distinction though.
			// these will be caught below by normal means and re-created minus the convenience of persisting identity tokens

			// delete
			for (var i = existingAccounts.Count - 1; i >= 0; i--)
			{
				var existing = existingAccounts[i];
				if (!dtos.Any(dto =>
					dto.AccountId?.ToLower().Trim() == existing.AccountId.ToLower()
					&& dto.LocaleName == existing.Locale?.Name))
				{
					accountsSettings.Delete(existing);
				}
			}

			// upsert each. validation occurs through Account and AccountsSettings
			foreach (var dto in dtos)
			{
				var acct = accountsSettings.Upsert(dto.AccountId, dto.LocaleName);
				acct.LibraryScan = dto.LibraryScan;
				acct.AccountName
					= string.IsNullOrWhiteSpace(dto.AccountName)
					? $"{dto.AccountId} - {dto.LocaleName}"
					: dto.AccountName.Trim();
			}
		}

		private List<AccountDto> getRowDtos()
			=> dataGridView1.Rows
				.Cast<DataGridViewRow>()
				.Where(r => !r.IsNewRow)
				.Select(r => new AccountDto
				{
					AccountId = (string)r.Cells[COL_AccountId].Value,
					AccountName = (string)r.Cells[COL_AccountName].Value,
					LocaleName = (string)r.Cells[COL_Locale].Value,
					LibraryScan = (bool)r.Cells[COL_LibraryScan].Value
				})
				.ToList();
	}
}
