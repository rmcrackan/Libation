using AudibleUtilities;
using Avalonia.Controls;
using Dinah.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login;

public partial class LoginExternalDialog : DialogWindow
{
	public Account? Account { get; }
	public string? ExternalLoginUrl { get; }
	public string? ResponseUrl { get; set; }

	public LoginExternalDialog() : base(saveAndRestorePosition: false)
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			Account = new Account("someemail.somedomain.co")
			{
				IdentityTokens = new AudibleApi.Authorization.Identity(AudibleApi.Localization.Locales.First())
			};
			ExternalLoginUrl = "ht" + "tps://us.audible.com/Test_url";
			DataContext = this;
		}
	}
	public LoginExternalDialog(Account account, string loginUrl) : this()
	{
		Account = account;
		ExternalLoginUrl = loginUrl;
		DataContext = this;
	}

	public LoginExternalDialog(Account account)
	{
		Account = account;
		DataContext = this;
	}

	protected override async Task SaveAndCloseAsync()
	{
		Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { ResponseUrl });
		if (!Uri.TryCreate(ResponseUrl, UriKind.Absolute, out _))
		{
			await MessageBox.Show("Invalid response URL");
			return;
		}
		await base.SaveAndCloseAsync();
	}


	public async void CopyUrlToClipboard_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (App.MainWindow?.Clipboard is not null)
			await App.MainWindow.Clipboard.SetTextAsync(ExternalLoginUrl);
	}

	public void LaunchInBrowser_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> Go.To.Url(ExternalLoginUrl);

	public async void Submit_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await SaveAndCloseAsync();

}
