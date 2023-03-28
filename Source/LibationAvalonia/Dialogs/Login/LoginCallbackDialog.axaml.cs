using AudibleUtilities;
using Avalonia.Controls;
using Dinah.Core;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login
{
	public partial class LoginCallbackDialog : DialogWindow
	{
		public Account Account { get; }
		public string Password { get; set; }

		public LoginCallbackDialog()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
			{
				using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
				var accounts = persister.AccountsSettings.Accounts;
				Account = accounts.FirstOrDefault();
				DataContext = this;
			}
		}
		public LoginCallbackDialog(Account account) : this()
		{
			Account = account;
			DataContext = this;
		}

		protected override Task SaveAndCloseAsync()
		{
			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { email = Account?.AccountId?.ToMask(), passwordLength = Password?.Length });

			return base.SaveAndCloseAsync();
		}

		public async void Submit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();
	}
}
