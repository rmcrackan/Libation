using AudibleApi;
using AudibleUtilities;
using LibationUiBase;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace LibationWinForms.Dialogs;

public partial class AccountsDialog : Form
{
	private const string COL_Delete = nameof(DeleteAccount);
	private const string COL_Export = nameof(ExportAccount);
	private const string COL_LibraryScan = nameof(LibraryScan);
	private const string COL_AccountId = nameof(AccountId);
	private const string COL_AccountName = nameof(AccountName);
	private const string COL_Locale = nameof(Locale);
	private const string COL_Marketplaces = nameof(Marketplaces);

	public AccountsDialog()
	{
		InitializeComponent();
		dataGridView1.EnableHeadersVisualStyles = !Application.IsDarkModeEnabled;
		dataGridView1.Columns[COL_AccountName]?.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

		dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
		dataGridView1.CurrentCellDirtyStateChanged += DataGridView1_CurrentCellDirtyStateChanged;

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
		var additional = account.AdditionalLocales.Select(l => l.Name).ToList();

		var row = dataGridView1.Rows.Add(
			"X",
			"Export",
			account.LibraryScan,
			account.AccountId,
			account.Locale?.Name ?? "",
			MarketplacesUi.ButtonText(additional.Count + 1),
			account.AccountName ?? "");

		// the extra marketplaces are a list, not a cell value, so the row carries them alongside its cells
		dataGridView1.Rows[row].Tag = additional;

		dataGridView1[COL_Export, row].ToolTipText = "Export account authorization to audible-cli";
		UpdateExportCellState(dataGridView1.Rows[row]);
	}

	private void dataGridView1_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
	{
		e.Row.Cells[COL_Delete].Value = "X";
		e.Row.Cells[COL_LibraryScan].Value = true;
		e.Row.Cells[COL_Export].ReadOnly = true;
		e.Row.Cells[COL_Marketplaces].Value = MarketplacesUi.ButtonText(1);
		e.Row.Cells[COL_Marketplaces].ReadOnly = true;
		e.Row.Tag = new List<string>();
	}

	private void DataGridView1_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		if (e.RowIndex < 0 || e.ColumnIndex < 0)
			return;
		var colName = dataGridView1.Columns[e.ColumnIndex].Name;
		if (colName is COL_AccountId or COL_Locale)
			UpdateExportCellState(dataGridView1.Rows[e.RowIndex]);
	}

	private void DataGridView1_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
	{
		if (!dataGridView1.IsCurrentCellDirty || dataGridView1.CurrentCell is null)
			return;
		var colName = dataGridView1.Columns[dataGridView1.CurrentCell.ColumnIndex].Name;
		if (colName == COL_Locale)
			dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
	}

	private static bool AccountRowCanExport(string? accountId, string? localeName)
	{
		if (string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(localeName))
			return false;

		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		var account = persister.AccountsSettings.Accounts.FirstOrDefault(a =>
			a.AccountId == accountId && a.Locale?.Name == localeName);
		return account?.IdentityTokens?.IsValid == true;
	}

	private void UpdateExportCellState(DataGridViewRow row)
	{
		if (row.IsNewRow || !dataGridView1.Columns.Contains(COL_Export))
			return;

		// checking other marketplaces speaks to Audible with this account's stored credentials, so it needs the
		// same thing an export does: an account that has logged in at least once
		var canExport = AccountRowCanExport(GetAccountId(row), GetLocale(row));
		row.Cells[COL_Export].ReadOnly = !canExport;
		row.Cells[COL_Export].ToolTipText = canExport
			? "Export account authorization to audible-cli"
			: "Authenticate this account (e.g. library scan) before exporting to audible-cli.";

		if (!dataGridView1.Columns.Contains(COL_Marketplaces))
			return;

		row.Cells[COL_Marketplaces].ReadOnly = !canExport;
		row.Cells[COL_Marketplaces].ToolTipText = canExport
			? MarketplacesUi.ButtonToolTip
			: MarketplacesUi.NotAuthenticatedToolTip;
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
					if (e.RowIndex < dgv.RowCount - 1
						&& !row.Cells[COL_Export].ReadOnly
						&& RowToAccountDto(row) is AccountDto accountDto)
						Export(accountDto);
					break;
				case COL_Marketplaces:
					// if final/edit row: do nothing
					if (e.RowIndex < dgv.RowCount - 1
						&& !row.Cells[COL_Marketplaces].ReadOnly)
						EditMarketplaces(row);
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

	private record AccountDto(
		string AccountId,
		string? AccountName,
		string LocaleName,
		bool LibraryScan,
		IReadOnlyList<string> AdditionalLocaleNames);

	/// <summary>Opens the marketplaces dialog for one row and takes the result back into the row.</summary>
	private void EditMarketplaces(DataGridViewRow row)
	{
		if (GetAccountId(row) is not string accountId || GetLocale(row) is not string localeName)
			return;

		// the probe speaks to Audible with this account's stored credentials, so it needs the saved account,
		// not the grid's copy of it
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		var account = persister.AccountsSettings.Accounts.FirstOrDefault(a =>
			a.AccountId == accountId && a.Locale?.Name == localeName);

		if (account is null || account.IdentityTokens?.IsValid != true)
		{
			MessageBox.Show(this, MarketplacesUi.NotAuthenticatedToolTip, "Account Not Authenticated");
			return;
		}

		using var dialog = new MarketplacesDialog(account, persister.AccountsSettings, GetAdditionalLocaleNames(row));

		if (dialog.ShowDialog(this) != DialogResult.OK)
			return;

		var selected = dialog.SelectedAdditionalLocaleNames.ToList();
		row.Tag = selected;
		row.Cells[COL_Marketplaces].Value = MarketplacesUi.ButtonText(selected.Count + 1);
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
		var upserted = new List<(AccountDto Dto, Account Account)>();
		foreach (var dto in dtos)
		{
			var acct = accountsSettings.Upsert(dto.AccountId, dto.LocaleName);
			acct.LibraryScan = dto.LibraryScan;
			acct.AccountName
				= string.IsNullOrWhiteSpace(dto.AccountName)
				? $"{dto.AccountId} - {dto.LocaleName}"
				: dto.AccountName.Trim();

			// drop every marketplace before assigning any, so that moving one from one account to another in a
			// single sitting cannot trip the "no two accounts scan one marketplace" rule halfway through
			acct.SetAdditionalMarketplaces([]);
			upserted.Add((dto, acct));
		}

		foreach (var (dto, acct) in upserted)
			acct.SetAdditionalMarketplaces(dto.AdditionalLocaleNames);
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

	private static IReadOnlyList<string> GetAdditionalLocaleNames(DataGridViewRow row)
		=> row.Tag as List<string> ?? [];

	private static AccountDto? RowToAccountDto(DataGridViewRow row)
		=> GetAccountId(row) is string accountId
		&& GetLocale(row) is string localeName
		&& GetLibraryScan(row) is bool libraryScan
		? new AccountDto(accountId, GetAccountName(row), localeName, libraryScan, GetAdditionalLocaleNames(row))
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
			var importResult = await Mkb79AuthImporter.ImportFromJsonTextAsync(jsonText);

			if (importResult.Outcome is Mkb79ImportOutcome.InvalidFile)
			{
				MessageBox.Show(this, importResult.Message ?? "Invalid import file.", "Error Importing Account", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (importResult.Outcome is Mkb79ImportOutcome.DuplicateAccount && importResult.Account is not null)
			{
				MessageBox.Show(this, Mkb79AuthImporter.DuplicateMessage(importResult), "Cannot Add Duplicate Account");
				return;
			}

			if (importResult.Account is { } account)
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

	public class MarketplacesColumn : DataGridViewButtonColumn
	{
		public MarketplacesColumn() : base()
		{
			this.CellTemplate = new MarketplacesColumnCell();
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

	public class ExportColumnCell : DisableableButtonCell
	{
		public ExportColumnCell() : base("Export account to mkb79/audible-cli format") { }
	}

	public class MarketplacesColumnCell : DisableableButtonCell
	{
		public MarketplacesColumnCell() : base("Check which Audible marketplaces this account holds titles in") { }
	}

	/// <summary>
	/// A button cell that looks disabled when it is. A read-only <see cref="DataGridViewButtonCell"/> still
	/// paints as a live button, which invites a click that does nothing.
	/// </summary>
	public abstract class DisableableButtonCell : AccessibleDataGridViewButtonCell
	{
		protected DisableableButtonCell(string accessibilityName) : base(accessibilityName)
		{
			ToolTipText = AccessibilityName;
		}

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object? value, object? formattedValue, string? errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			if (ReadOnly)
			{
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts ^ (DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.SelectionBackground));
				var caption = formattedValue?.ToString() ?? Convert.ToString(value) ?? "";
				ButtonRenderer.DrawButton(graphics, cellBounds, caption, cellStyle.Font, focused: false, PushButtonState.Disabled);
			}
			else
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}
	}
	#endregion
}
