using System;
using System.Windows.Forms;
using AudibleUtilities;
using Dinah.Core;

namespace LibationWinForms.Dialogs.Login
{
	public partial class LoginExternalDialog : Form
	{
		public string ResponseUrl { get; private set; }

		public LoginExternalDialog(Account account, string loginUrl)
		{
			InitializeComponent();

			// do not allow user to change login id here. if they do then jsonpath will fail
			this.localeLbl.Text = string.Format(this.localeLbl.Text, account.Locale.Name);
			this.usernameLbl.Text = string.Format(this.usernameLbl.Text, account.AccountId);

			this.loginUrlTb.Text = loginUrl;
		}

		private void copyBtn_Click(object sender, EventArgs e) => Clipboard.SetText(this.loginUrlTb.Text);

		private void launchBrowserBtn_Click(object sender, EventArgs e) => Go.To.Url(this.loginUrlTb.Text);

		private void submitBtn_Click(object sender, EventArgs e)
		{
			ResponseUrl = this.responseUrlTb.Text?.Trim();

			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { ResponseUrl });
			if (!Uri.TryCreate(ResponseUrl, UriKind.Absolute, out var result))
			{
				MessageBox.Show("Invalid response URL");
				return;
			}

			DialogResult = DialogResult.OK;
			// Close() not needed for AcceptButton
		}
	}
}
