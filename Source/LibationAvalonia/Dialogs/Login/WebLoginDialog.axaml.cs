using Avalonia.Controls;
using Dinah.Core;
using System;

namespace LibationAvalonia.Dialogs.Login
{
	public partial class WebLoginDialog : Window
	{
		public string ResponseUrl { get; private set; }
		private readonly string accountID;

		public WebLoginDialog()
		{
			InitializeComponent();
			webView.NavigationStarted += WebView_NavigationStarted;
			webView.DOMContentLoaded += WebView_NavigationCompleted;
		}

		public WebLoginDialog(string accountID, string loginUrl) : this()
		{
			this.accountID = ArgumentValidator.EnsureNotNullOrWhiteSpace(accountID, nameof(accountID));
			webView.Source = new Uri(ArgumentValidator.EnsureNotNullOrWhiteSpace(loginUrl, nameof(loginUrl)));
		}
		
		private void WebView_NavigationStarted(object sender, LibationFileManager.WebViewNavigationEventArgs e)
		{
			if (e.Request?.AbsolutePath.Contains("/ap/maplanding") is true)
			{
				ResponseUrl = e.Request.ToString();
				Close(DialogResult.OK);
			}
		}

		private async void WebView_NavigationCompleted(object sender, EventArgs e)
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
