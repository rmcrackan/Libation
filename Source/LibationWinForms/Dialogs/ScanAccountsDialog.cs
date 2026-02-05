using AudibleUtilities;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class ScanAccountsDialog : Form
{
	public List<Account> CheckedAccounts { get; } = new List<Account>();

	public ScanAccountsDialog()
	{
		InitializeComponent();
		this.SetLibationIcon();
	}

	private class listItem
	{
		public Account? Account { get; set; }
		public string? Text { get; set; }
		public override string? ToString() => Text;
	}
	private void ScanAccountsDialog_Load(object sender, EventArgs e)
	{
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		var accounts = persister.AccountsSettings.Accounts;

		foreach (var account in accounts)
		{
			var item = new listItem
			{
				Account = account,
				Text = $"{account.AccountName} ({account.AccountId} - {account.Locale?.Name})"
			};
			this.accountsClb.Items.Add(item, account.LibraryScan);
		}
	}

	private void editBtn_Click(object sender, EventArgs e)
	{
		if (new AccountsDialog().ShowDialog() == DialogResult.OK)
		{
			// clear grid
			this.accountsClb.Items.Clear();

			// reload grid and default checkboxes
			ScanAccountsDialog_Load(sender, e);
		}
	}

	private void importBtn_Click(object sender, EventArgs e)
	{
		foreach (listItem item in accountsClb.CheckedItems)
		{
			if (item.Account != null)
				CheckedAccounts.Add(item.Account);
		}

		this.DialogResult = DialogResult.OK;
		this.Close();
	}

	private void cancelBtn_Click(object sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
		this.Close();
	}
}
