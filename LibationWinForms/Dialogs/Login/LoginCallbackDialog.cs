using System;
using System.Windows.Forms;
using AudibleUtilities;
using Dinah.Core;

namespace LibationWinForms.Dialogs.Login
{
	public partial class LoginCallbackDialog : Form
	{
		private string accountId { get; }

		public string Email { get; private set; }
		public string Password { get; private set; }

		public LoginCallbackDialog(Account account)
		{
			InitializeComponent();

			accountId = account.AccountId;

			// do not allow user to change login id here. if they do then jsonpath will fail
			this.localeLbl.Text = string.Format(this.localeLbl.Text, account.Locale.Name);
			this.usernameLbl.Text = string.Format(this.usernameLbl.Text, accountId);
		}

		private void submitBtn_Click(object sender, EventArgs e)
		{
			Email = accountId;
			Password = this.passwordTb.Text;

			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { email = Email?.ToMask(), passwordLength = Password.Length });

			DialogResult = DialogResult.OK;
			// Close() not needed for AcceptButton
		}
	}
}
