using System;
using System.Windows.Forms;
using Dinah.Core;
using InternalUtilities;

namespace LibationWinForms.Dialogs.Login
{
	public partial class LoginChoiceEagerDialog : Form
	{
		private string accountId { get; }

		public AudibleApi.LoginMethod LoginMethod { get; private set; }

		public string Email { get; private set; }
		public string Password { get; private set; }

		public LoginChoiceEagerDialog(Account account)
		{
			InitializeComponent();

			accountId = account.AccountId;

			// do not allow user to change login id here. if they do then jsonpath will fail
			this.localeLbl.Text = string.Format(this.localeLbl.Text, account.Locale.Name);
			this.usernameLbl.Text = string.Format(this.usernameLbl.Text, accountId);
		}

		private void externalLoginLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			LoginMethod = AudibleApi.LoginMethod.External;
			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void submitBtn_Click(object sender, EventArgs e)
		{
			Email = accountId;
			Password = this.passwordTb.Text;

			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { email = Email?.ToMask(), passwordLength = Password.Length });

			LoginMethod = AudibleApi.LoginMethod.Api;
			DialogResult = DialogResult.OK;
			// Close() not needed for AcceptButton
		}
	}
}
