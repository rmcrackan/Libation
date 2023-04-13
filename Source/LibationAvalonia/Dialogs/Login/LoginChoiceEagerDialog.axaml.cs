using AudibleApi;
using AudibleUtilities;
using Avalonia.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login
{
	public partial class LoginChoiceEagerDialog : DialogWindow
	{
		public Account Account { get; }
		public string Password { get; set; }
		public LoginMethod LoginMethod { get; private set; }

		public LoginChoiceEagerDialog() : base(saveAndRestorePosition: false)
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
		public LoginChoiceEagerDialog(Account account) : this()
		{
			Account = account;
			DataContext = this;
		}

		protected override async Task SaveAndCloseAsync()
		{
			if (LoginMethod is LoginMethod.Api && string.IsNullOrWhiteSpace(Password))
			{
				await MessageBox.Show(this, "Please enter your password");
				return;
			}

			await base.SaveAndCloseAsync();
		}

		public async void ExternalLoginLink_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		{
			LoginMethod = LoginMethod.External;
			await SaveAndCloseAsync();
		}
	}
}
