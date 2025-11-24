using AudibleApi;
using AudibleUtilities;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using LibationFileManager;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia.Dialogs.Login
{
	public class AvaloniaLoginChoiceEager : ILoginChoiceEager
	{
		public ILoginCallback LoginCallback { get; } = new AvaloniaLoginCallback();

		private readonly Account _account;

		public AvaloniaLoginChoiceEager(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
		}

		public async Task<ChoiceOut?> StartAsync(ChoiceIn choiceIn)
			=> await Dispatcher.UIThread.InvokeAsync(() => StartAsyncInternal(choiceIn));

		private async Task<ChoiceOut?> StartAsyncInternal(ChoiceIn choiceIn)
		{
			try
			{
				if (Configuration.Instance.UseWebView && await BrowserLoginAsync(choiceIn) is ChoiceOut external)
					return external;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Failed to use the {nameof(NativeWebDialog)}");
			}

			var externalDialog = new LoginExternalDialog(_account, choiceIn.LoginUrl);
			return await externalDialog.ShowDialogAsync() is DialogResult.OK
				? ChoiceOut.External(externalDialog.ResponseUrl)
				: null;
		}

		private async Task<ChoiceOut?> BrowserLoginAsync(ChoiceIn shoiceIn)
		{
			TaskCompletionSource<ChoiceOut?> tcs = new();

			NativeWebDialog dialog = new()
			{
				Title = "Audible Login",
				CanUserResize = true
			};
			dialog.EnvironmentRequested += Dialog_EnvironmentRequested;
			dialog.NavigationCompleted += Dialog_NavigationCompleted;
			dialog.Closing += (_, _) => tcs.TrySetResult(null);
			dialog.NavigationStarted += async (_, e) =>
			{
				if (e.Request?.AbsolutePath.StartsWith("/ap/maplanding") is true)
				{
					tcs.TrySetResult(ChoiceOut.External(e.Request.ToString()));
					dialog.Close();
				}
			};
			dialog.AdapterCreated += (s, e) =>
			{
				if (dialog.TryGetCookieManager() is NativeWebViewCookieManager cookieManager)
				{
					foreach (System.Net.Cookie c in shoiceIn.SignInCookies)
						cookieManager.AddOrUpdateCookie(c);
				}
				//Set the source only after loading cookies
				dialog.Source = new Uri(shoiceIn.LoginUrl);
			};

			if (!Configuration.IsLinux && App.MainWindow is TopLevel topLevel)
				dialog.Show(topLevel);
			else
				dialog.Show();

			return await tcs.Task;

			void Dialog_EnvironmentRequested(object? sender, WebViewEnvironmentRequestedEventArgs e)
			{
				// Private browsing & user agent setting
				switch (e)
				{
					case WindowsWebView2EnvironmentRequestedEventArgs webView2Args:
						webView2Args.IsInPrivateModeEnabled = true;
						webView2Args.AdditionalBrowserArguments = "--user-agent=\"" + Resources.User_Agent + "\"";
						break;
					case AppleWKWebViewEnvironmentRequestedEventArgs appleArgs:
						appleArgs.NonPersistentDataStore = true;
						appleArgs.ApplicationNameForUserAgent = Resources.User_Agent;
						break;
					case GtkWebViewEnvironmentRequestedEventArgs gtkArgs:
						gtkArgs.EphemeralDataManager = true;
						gtkArgs.ApplicationNameForUserAgent = Resources.User_Agent;
						break;
				}
			}
		}		 

		private async void Dialog_NavigationCompleted(object? sender, WebViewNavigationCompletedEventArgs e)
		{
			if (e.IsSuccess && sender is NativeWebDialog dialog)
			{
				await dialog.InvokeScript(getScript(_account.AccountId));
			}
		}

		private static string getScript(string accountID) => $$"""
			(function() {
				function populateForm(){
					var email = document.querySelector("input[id='ap_email_login']");
					if (email !== null)
						email.value = '{{accountID}}';
					
					var pass = document.querySelector("input[name='password']");
					if (pass !== null)
						pass.focus();
				}
				window.addEventListener("load", (event) => { populateForm(); });
				populateForm();
			})()
			""";
	}
}
