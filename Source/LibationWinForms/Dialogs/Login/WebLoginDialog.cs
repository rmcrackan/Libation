using Dinah.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Login
{
	public partial class WebLoginDialog : Form
	{
		public string ResponseUrl { get; private set; }
		private readonly string accountID;
		private readonly WebView2 webView;
		public WebLoginDialog()
		{
			InitializeComponent();
			webView = new WebView2();

			webView.Dock = DockStyle.Fill;
			Controls.Add(webView);

			webView.NavigationStarting += WebView_NavigationStarting;
			webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
			this.SetLibationIcon();
		
		}

		public WebLoginDialog(string accountID, string loginUrl) : this()
		{
			this.accountID = ArgumentValidator.EnsureNotNullOrWhiteSpace(accountID, nameof(accountID));
			webView.Source = new Uri(ArgumentValidator.EnsureNotNullOrWhiteSpace(loginUrl, nameof(loginUrl)));
		}

		private void WebView_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
		{
			if (e.Uri.Contains("/ap/maplanding") is true)
			{
				ResponseUrl = e.Uri;
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private void WebView_CoreWebView2InitializationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
		{
			webView.CoreWebView2.DOMContentLoaded -= CoreWebView2_DOMContentLoaded;
			webView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
		}

		private async void CoreWebView2_DOMContentLoaded(object sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs e)
		{
			await webView.ExecuteScriptAsync(getScript(accountID));
		}

		private static string getScript(string accountID) => $$"""
			(function() {
				var email = document.querySelector("input[name='email']");
				if (email !== null)
					email.value = '{{accountID}}';
			
				var pass = document.querySelector("input[name='password']");
				if (pass !== null)
					pass.focus();
			})()
			""";
	}
}
