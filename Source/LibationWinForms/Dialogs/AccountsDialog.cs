using AudibleApi;
using AudibleUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class AccountsDialog : Form
{
	private const string COL_Delete = nameof(DeleteAccount);
	private const string COL_Export = nameof(ExportAccount);
	private const string COL_LibraryScan = nameof(LibraryScan);
	private const string COL_AccountId = nameof(AccountId);
	private const string COL_AccountName = nameof(AccountName);
	private const string COL_Locale = nameof(Locale);

	public AccountsDialog()
	{
		InitializeComponent();
		dataGridView1.EnableHeadersVisualStyles = !Application.IsDarkModeEnabled;
		dataGridView1.Columns[COL_AccountName]?.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

		populateDropDown();

		populateGridValues();
		this.SetLibationIcon();
	}

	private void populateDropDown()
		=> (dataGridView1.Columns[COL_Locale] as DataGridViewComboBoxColumn)?.DataSource
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
			AddAccountToGrid(account);
	}

	private void AddAccountToGrid(Account account)
	{
		var row = dataGridView1.Rows.Add(
			"X",
			"Export",
			account.LibraryScan,
			account.AccountId,
			account.Locale?.Name ?? "",
			account.AccountName ?? "");

		dataGridView1[COL_Export, row].ToolTipText = "Export account authorization to audible-cli";
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
				case COL_Export:
					// if final/edit row: do nothing
					if (e.RowIndex < dgv.RowCount - 1 && RowToAccountDto(row) is AccountDto accountDto)
						Export(accountDto);
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

	private record AccountDto (string AccountId, string? AccountName, string LocaleName,bool LibraryScan);

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
			MessageBoxLib.ShowAdminAlert(this, "Error attempting to save accounts", "Error saving accounts", ex);
		}
	}

	private bool inputIsValid()
	{
		if (getRows().Any(r => GetAccountId(r) is null))
		{
			MessageBox.Show(this, "Account id cannot be blank. Please enter an account id for all accounts.", "Blank account", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}

		if (getRows().Any(r => GetLocale(r) is null))
		{
			MessageBox.Show(this, "Please select a locale (i.e.: country or region) for all accounts.", "Blank region", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
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

	private IEnumerable<DataGridViewRow> getRows()
		=> dataGridView1.Rows
		.Cast<DataGridViewRow>()
		.Where(r => !r.IsNewRow);

	private List<AccountDto> getRowDtos()
		=> getRows()
		.Select(RowToAccountDto)
		.OfType<AccountDto>()
		.ToList();

	private static string? GetAccountId(DataGridViewRow row)
		=> row.Cells[COL_AccountId]?.Value as string;

	private static string? GetLocale(DataGridViewRow row)
		=> row.Cells[COL_Locale]?.Value as string;

	private static bool? GetLibraryScan(DataGridViewRow row)
		=> row.Cells[COL_LibraryScan]?.Value as bool?;

	private static string? GetAccountName(DataGridViewRow row)
		=> row.Cells[COL_AccountName]?.Value as string;

	private static AccountDto? RowToAccountDto(DataGridViewRow row)
		=> GetAccountId(row) is string accountId
		&& GetLocale(row) is string localeName
		&& GetLibraryScan(row) is bool libraryScan
		? new AccountDto(accountId, GetAccountName(row), localeName, libraryScan)
		: null;

	private string GetAudibleCliAppDataPath()
		=> Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Audible");

	private void Export(AccountDto accountDto)
	{
		// without transaction, accounts persister will write ANY EDIT immediately to file
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();

		var account = persister.AccountsSettings.Accounts.FirstOrDefault(a => a.AccountId == accountDto.AccountId && a.Locale?.Name == accountDto.LocaleName);

		if (account is null)
			return;

		if (account.IdentityTokens?.IsValid != true)
		{
			MessageBox.Show(this, "This account hasn't been authenticated yet. First scan your library to log into your account, then try exporting again.", "Account Not Authenticated");
			return;
		}

		SaveFileDialog sfd = new();
		sfd.Filter = "JSON File|*.json";

		string audibleAppDataDir = GetAudibleCliAppDataPath();

		if (Directory.Exists(audibleAppDataDir))
			sfd.InitialDirectory = audibleAppDataDir;

		if (sfd.ShowDialog() != DialogResult.OK) return;

		try
		{
			var mkbAuth = Mkb79Auth.FromAccount(account);
			var jsonText = mkbAuth.ToJson();

			File.WriteAllText(sfd.FileName, jsonText);

			MessageBox.Show(this, $"Successfully exported {account.AccountName} to\r\n\r\n{sfd.FileName}", "Success!");
		}
		catch (Exception ex)
		{
			MessageBoxLib.ShowAdminAlert(
				this,
				$"An error occurred while exporting account:\r\n{account.AccountName}",
				"Error Exporting Account",
				ex);
		}
	}

	private async void importBtn_Click(object sender, EventArgs e)
	{
		OpenFileDialog ofd = new();
		ofd.Filter = "JSON File|*.json";
		ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

		string audibleAppDataDir = GetAudibleCliAppDataPath();

		if (Directory.Exists(audibleAppDataDir))
			ofd.InitialDirectory = audibleAppDataDir;

		if (ofd.ShowDialog() != DialogResult.OK) return;

		try
		{
			var jsonText = File.ReadAllText(ofd.FileName);
			var mkbAuth = Mkb79Auth.FromJson(jsonText) ?? throw new Exception("File did not contain valid mkb79/audible-cli account data.");
			var account = await mkbAuth.ToAccountAsync();

			// without transaction, accounts persister will write ANY EDIT immediately to file
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();

			if (persister.AccountsSettings.Accounts.Any(a => a.AccountId == account.AccountId && a.IdentityTokens?.Locale.Name == account.Locale?.Name))
			{
				MessageBox.Show(this, $"An account with that account id and country already exists.\r\n\r\nAccount ID: {account.AccountId}\r\nCountry: {account.Locale?.Name}", "Cannot Add Duplicate Account");
				return;
			}

			persister.AccountsSettings.Add(account);

			AddAccountToGrid(account);
		}
		catch (Exception ex)
		{
			MessageBoxLib.ShowAdminAlert(
					this,
					$"An error occurred while importing an account from:\r\n{ofd.FileName}\r\n\r\nIs the file encrypted?",
					"Error Importing Account",
					ex);
		}
	}
	#region Accessable Columns

	public class DeleteColumn : DataGridViewButtonColumn
	{
		public DeleteColumn() : base()
		{
			this.CellTemplate = new DeleteColumnCell();
		}
	}

	public class ExportColumn : DataGridViewButtonColumn
	{
		public ExportColumn() : base()
		{
			this.CellTemplate = new ExportColumnCell();
		}
	}

	public class LocaleColumn : DataGridViewComboBoxColumn
	{
		public LocaleColumn() : base()
		{
			this.CellTemplate = new LocaleColumnCell();
		}
	}

	public class DeleteColumnCell : AccessibleDataGridViewButtonCell
	{
		public DeleteColumnCell() : base("Delete account from Libation")
		{
			ToolTipText = AccessibilityName;
		}
	}

	public class LocaleColumnCell : AccessibleDataGridViewComboBoxCell
	{
		public LocaleColumnCell() : base("Select Audible account region")
		{
			ToolTipText = AccessibilityName;
		}
	}

	public class ExportColumnCell : AccessibleDataGridViewButtonCell
	{
		public ExportColumnCell() : base("Export account to mkb79/audible-cli format")
		{
			ToolTipText = AccessibilityName;
		}
	}
	#endregion
}
