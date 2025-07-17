using AudibleUtilities;
using Avalonia.Controls;
using LibationUiBase.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs
{
	public partial class ScanAccountsDialog : DialogWindow
	{
		public List<Account> CheckedAccounts { get; } = new();
		private List<listItem> _accounts { get; } = new();
		public IList Accounts => _accounts;
		private class listItem
		{
			public Account Account { get; set; }
			public string Text { get; set; }
			public bool IsChecked { get; set; } = true;
			public override string ToString() => Text;
		}

		public ScanAccountsDialog()
		{
			InitializeComponent();

			ControlToFocusOnShow = this.FindControl<Button>(nameof(ImportButton));

			LoadAccounts();
		}

		private void LoadAccounts()
		{
			_accounts.Clear();
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var accounts = persister.AccountsSettings.Accounts;

			foreach (var account in accounts)
				_accounts.Add(new listItem
				{
					Account = account,
					IsChecked = account.LibraryScan,
					Text = $"{account.AccountName} ({account.AccountId} - {account.Locale.Name})"
				});

			DataContext = this;
		}

		public async Task EditAccountsAsync()
		{
			if (await new AccountsDialog().ShowDialog<DialogResult>(this) == DialogResult.OK)
			{
				// reload grid and default checkboxes
				LoadAccounts();
			}
		}

		protected override void SaveAndClose()
		{
			foreach (listItem item in _accounts.Where(a => a.IsChecked))
				CheckedAccounts.Add(item.Account);

			base.SaveAndClose();
		}
	}
}
