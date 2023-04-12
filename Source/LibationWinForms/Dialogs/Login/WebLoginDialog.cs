using Dinah.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Login
{
	public partial class WebLoginDialog : Form
	{
		public string ResponseUrl { get; private set; }
		private readonly string loginUrl;
		private readonly string accountID;
		private readonly WebView2 webView = new();
		public WebLoginDialog()
		{
			InitializeComponent();
			webView.Dock = DockStyle.Fill;
			Controls.Add(webView);
			Shown += WebLoginDialog_Shown;
			this.SetLibationIcon();
		}

		public WebLoginDialog(string accountID, string loginUrl) : this()
		{
			this.accountID = ArgumentValidator.EnsureNotNullOrWhiteSpace(accountID, nameof(accountID));
			this.loginUrl = ArgumentValidator.EnsureNotNullOrWhiteSpace(loginUrl, nameof(loginUrl));
		}

		private async void WebLoginDialog_Shown(object sender, EventArgs e)
		{
			await webView.EnsureCoreWebView2Async();
			webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
			webView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
			webView.CoreWebView2.Navigate(loginUrl);
		}

		private async void CoreWebView2_DOMContentLoaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e)
		{
			await webView.CoreWebView2.ExecuteScriptAsync(getScript(accountID));
		}

		private static string getScript(string accountID) => $$"""
			(function() {
				var inputs = document.getElementsByTagName('input');
				for (index = 0; index < inputs.length; ++index) {
					if (inputs[index].name.includes('email')) {
						inputs[index].value = '{{accountID}}';
					}
					if (inputs[index].name.includes('password')) {
						inputs[index].focus();
					}
				}
			})()
			""";

		private void CoreWebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
		{
			if (new Uri(e.Uri).AbsolutePath.Contains("/ap/maplanding"))
			{
				ResponseUrl = e.Uri;
				e.Cancel = true;
				DialogResult = DialogResult.OK;
				Close();
			}
		}
	}
}
