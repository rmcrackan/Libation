using AudibleUtilities;
using Avalonia.Collections;
using LibationUiBase.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public partial class ScanAccountsDialog : DialogWindow
{
	public IEnumerable<Account> CheckedAccounts => Accounts.Where(a => a.IsChecked).Select(a => a.Account);
	public AvaloniaList<ListItem> Accounts { get; } = new();
	public class ListItem
	{
		public ListItem(Account account)
		{
			Account = account;
			IsChecked = account.LibraryScan;
			Text = $"{account.AccountName} ({account.AccountId} - {account.Locale?.Name})";
		}
		public Account Account { get; }
		public string Text { get; }
		public bool IsChecked { get; set; }
		public override string ToString() => Text;
	}

	public ScanAccountsDialog()
	{
		InitializeComponent();
		ControlToFocusOnShow = ImportButton;
		DataContext = this;
		LoadAccounts();
	}

	private void LoadAccounts()
	{
		Accounts.Clear();
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		var accounts = persister.AccountsSettings.Accounts;
		Accounts.AddRange(accounts.Select(account => new ListItem(account)));
	}

	public async Task EditAccountsAsync()
	{
		if (await new AccountsDialog().ShowDialog<DialogResult>(this) == DialogResult.OK)
		{
			// reload grid and default checkboxes
			LoadAccounts();
		}
	}

	public new void SaveAndClose()
		=> base.SaveAndClose();
}
