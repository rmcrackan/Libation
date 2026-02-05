using AudibleApi;
using Dinah.Core;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Login;

public partial class WebLoginDialog : Form
{
	public string? ResponseUrl { get; private set; }
	private readonly string? accountID;
	private readonly WebView2 webView;
	public WebLoginDialog()
	{
		InitializeComponent();
		webView = new WebView2();

		webView.Dock = DockStyle.Fill;
		Controls.Add(webView);

		webView.NavigationStarting += WebView_NavigationStarting;
		this.SetLibationIcon();
	}

	public WebLoginDialog(string accountID, ChoiceIn choiceIn) : this()
	{
		this.accountID = ArgumentValidator.EnsureNotNullOrWhiteSpace(accountID, nameof(accountID));
		ArgumentValidator.EnsureNotNullOrWhiteSpace(choiceIn?.LoginUrl, nameof(choiceIn));
		this.Load += async (_, _) =>
		{
			//enable private browsing
			var env = await CoreWebView2Environment.CreateAsync();
			var options = env.CreateCoreWebView2ControllerOptions();
			options.IsInPrivateModeEnabled = true;
			await webView.EnsureCoreWebView2Async(env, options);

			webView.CoreWebView2.Settings.UserAgent = Resources.User_Agent;

			//Load init cookies
			foreach (System.Net.Cookie cookie in choiceIn.SignInCookies ?? [])
			{
				try
				{
					webView.CoreWebView2.CookieManager.AddOrUpdateCookie(webView.CoreWebView2.CookieManager.CreateCookieWithSystemNetCookie(cookie));
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, $"Failed to set cookie {cookie.Name} for domain {cookie.Domain}");
				}
			}

			webView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
			Invoke(() => webView.Source = new Uri(choiceIn.LoginUrl));
		};
	}

	private void WebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
	{
		if (e.Uri.Contains("/ap/maplanding") is true)
		{
			ResponseUrl = e.Uri;
			DialogResult = DialogResult.OK;
			Close();
		}
	}

	private async void CoreWebView2_DOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs e)
	{
		await webView.ExecuteScriptAsync(getScript(accountID));
	}

	private static string getScript(string? accountID) => $$"""
		(function() {
			var email = document.querySelector("input[id='ap_email_login']");
			if (email !== null)
				email.value = '{{accountID}}';
		
			var pass = document.querySelector("input[name='password']");
			if (pass !== null)
				pass.focus();
		})()
		""";
}
