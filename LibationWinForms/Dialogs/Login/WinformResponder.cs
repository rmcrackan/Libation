using System;
using AudibleApi;
using InternalUtilities;
using LibationWinForms.Dialogs.Login;

namespace LibationWinForms.Login
{
	public class WinformResponder : ILoginCallback
	{
		private Account _account { get; }

		public WinformResponder(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
		}

		public string Get2faCode()
		{
			using var dialog = new _2faCodeDialog();
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				return dialog.Code;
			return null;
		}

		public string GetCaptchaAnswer(byte[] captchaImage)
		{
			using var dialog = new CaptchaDialog(captchaImage);
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				return dialog.Answer;
			return null;
		}

		public (string email, string password) GetLogin()
		{
			using var dialog = new AudibleLoginDialog(_account);
			if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				return (dialog.Email, dialog.Password);
			return (null, null);
		}

		public void ShowApprovalNeeded()
		{
			using var dialog = new ApprovalNeededDialog();
			dialog.ShowDialog();
		}
	}
}