using Dinah.Core;
using LibationFileManager;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Login
{
	public partial class WebLoginDialog : Form
	{
		public string ResponseUrl { get; private set; }
		private readonly string accountID;
		private readonly IWebViewAdapter webView;
		public WebLoginDialog()
		{
			InitializeComponent();
			webView = InteropFactory.Create().CreateWebViewAdapter();

			var webViewControl = webView.NativeWebView as Control;
			webViewControl.Dock = DockStyle.Fill;
			Controls.Add(webViewControl);

			webView.NavigationStarted += WebView_NavigationStarted;
			webView.DOMContentLoaded += WebView_DOMContentLoaded;
			this.SetLibationIcon();
		}

		public WebLoginDialog(string accountID, string loginUrl) : this()
		{
			this.accountID = ArgumentValidator.EnsureNotNullOrWhiteSpace(accountID, nameof(accountID));
			webView.Source = new Uri(ArgumentValidator.EnsureNotNullOrWhiteSpace(loginUrl, nameof(loginUrl)));
		}

		private void WebView_NavigationStarted(object sender, WebViewNavigationEventArgs e)
		{
			if (e.Request?.AbsolutePath.Contains("/ap/maplanding") is true)
			{
				ResponseUrl = e.Request.ToString();
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		private async void WebView_DOMContentLoaded(object sender, EventArgs e)
		{
			await webView.InvokeScriptAsync(getScript(accountID));
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
	}
}
