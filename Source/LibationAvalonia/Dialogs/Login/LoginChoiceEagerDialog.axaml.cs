using AudibleApi;
using AudibleUtilities;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;

namespace LibationAvalonia.Dialogs.Login
{
	public partial class LoginChoiceEagerDialog : DialogWindow
	{
		public Account Account { get; }
		public string Password { get; set; }
		public LoginMethod LoginMethod { get; private set; }

		public LoginChoiceEagerDialog()
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
		public LoginChoiceEagerDialog(Account account):this()
		{
			Account = account;
			DataContext = this;
		}

		public async void ExternalLoginLink_Tapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			LoginMethod = LoginMethod.External;
			await SaveAndCloseAsync();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
