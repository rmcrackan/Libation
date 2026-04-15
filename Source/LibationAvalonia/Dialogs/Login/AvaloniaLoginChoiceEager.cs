using AudibleApi;
using AudibleUtilities;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login;

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
			// Embedded browser is optional; honor UseWebView (getter also forces false under Linux Snap).
			if (Configuration.Instance.UseWebView)
			{
				try
				{
					if (await BrowserLoginAsync(choiceIn) is ChoiceOut external)
						return external;
				}
				catch (Exception ex) when (WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(ex))
				{
					// Linux (e.g. missing WebKit2GTK): go straight to external browser — same outcome as turning off embedded sign-in.
					if (OperatingSystem.IsLinux())
					{
						Serilog.Log.Logger.Information(
							ex,
							"Embedded sign-in browser is not available; continuing with external browser sign-in.");
					}
					else
					{
						await MessageBox.ShowAdminAlert(
							App.MainWindow,
							WebView2LoginErrorMessage.ExplainerBody,
							WebView2LoginErrorMessage.Caption,
							ex);
					}
				}
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Warning(ex, "WebView login failed; falling back to external browser");
		}

		var externalDialog = new LoginExternalDialog(_account, choiceIn.LoginUrl);
		return await externalDialog.ShowDialogAsync() is DialogResult.OK
			? ChoiceOut.External(externalDialog.ResponseUrl)
			: null;
	}

	private async Task<ChoiceOut?> BrowserLoginAsync(ChoiceIn shoiceIn)
	{
		// Time-of-use: setting can change before Show (e.g. settings saved) — never open NativeWebView when disabled.
		if (!Configuration.Instance.UseWebView)
			return null;

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
				foreach (System.Net.Cookie c in shoiceIn.SignInCookies ?? [])
				{
					if (string.IsNullOrEmpty(c.Value))
						continue;
					try
					{
						cookieManager.AddOrUpdateCookie(c);
					}
					catch (Exception ex)
					{
						Serilog.Log.Logger.Error(ex, $"Failed to set cookie {c.Name} for domain {c.Domain}");
					}
				}
			}
			//Set the source only after loading cookies
			dialog.Source = new Uri(shoiceIn.LoginUrl);
		};

		if (!Configuration.Instance.UseWebView)
		{
			try
			{
				dialog.Close();
			}
			catch
			{
				/* not shown */
			}
			return null;
		}

		// NativeWebDialog can fault on a posted dispatcher job (e.g. missing libwebkit2gtk on Linux), which bypasses a try/catch around Show().
		void onUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
		{
			if (!WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(e.Exception))
				return;
			e.Handled = true;
			try
			{
				dialog.Close();
			}
			catch
			{
				/* ignore */
			}
			tcs.TrySetException(e.Exception);
		}

		Dispatcher.UIThread.UnhandledException += onUnhandledException;
		try
		{
			if (!Configuration.Instance.UseWebView)
			{
				try
				{
					dialog.Close();
				}
				catch
				{
					/* not shown */
				}
				return null;
			}

			try
			{
				if (!Configuration.IsLinux && App.MainWindow is TopLevel topLevel)
					dialog.Show(topLevel);
				else
					dialog.Show();
			}
			catch (Exception ex) when (WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(ex))
			{
				tcs.TrySetException(ex);
			}

			return await tcs.Task;
		}
		finally
		{
			Dispatcher.UIThread.UnhandledException -= onUnhandledException;
		}

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
