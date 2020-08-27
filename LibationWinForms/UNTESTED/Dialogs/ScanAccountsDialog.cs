using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using InternalUtilities;

namespace LibationWinForms.Dialogs
{
	public partial class ScanAccountsDialog : Form
	{
		public List<Account> CheckedAccounts { get; } = new List<Account>();

		Form1 _parent { get; }

		public ScanAccountsDialog(Form1 parent)
		{
			_parent = parent;

			InitializeComponent();
		}

		class listItem
		{
			public Account Account { get; set; }
			public string Text { get; set; }
			public override string ToString() => Text;
		}
		private void ScanAccountsDialog_Load(object sender, EventArgs e)
		{
			// DO NOT dispose of this connection.
			// Here we (clumsily) connect to the persistent file. Accounts returned from here are used throughout library scan INCLUDING updates. eg: identity tokens
			var accounts = AudibleApiStorage.GetPersistentAccountsSettings().Accounts;

			foreach (var account in accounts)
			{
				var item = new listItem { Account=account,Text = $"{account.AccountName} ({account.AccountId} - {account.Locale.Name})" };
				this.accountsClb.Items.Add(item, account.LibraryScan);
			}
		}

		private void editBtn_Click(object sender, EventArgs e)
		{
			if (new AccountsDialog(_parent).ShowDialog()== DialogResult.OK)
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
				CheckedAccounts.Add(item.Account);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e) => this.Close();
	}
}
