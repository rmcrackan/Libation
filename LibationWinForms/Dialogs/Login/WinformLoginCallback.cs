using System;
using AudibleApi;
using InternalUtilities;
using LibationWinForms.Dialogs.Login;

namespace LibationWinForms.Login
{
	public class WinformLoginCallback : WinformLoginBase, ILoginCallback
	{
		private Account _account { get; }

		public WinformLoginCallback(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
		}

		public string Get2faCode()
		{
			using var dialog = new _2faCodeDialog();
			if (ShowDialog(dialog))
				return dialog.Code;
			return null;
		}

		public string GetCaptchaAnswer(byte[] captchaImage)
		{
			using var dialog = new CaptchaDialog(captchaImage);
			if (ShowDialog(dialog))
				return dialog.Answer;
			return null;
		}

		public (string name, string value) GetMfaChoice(MfaConfig mfaConfig)
		{
			using var dialog = new MfaDialog(mfaConfig);
			if (ShowDialog(dialog))
				return (dialog.SelectedName, dialog.SelectedValue);
			return (null, null);
		}

		public (string email, string password) GetLogin()
		{
			using var dialog = new LoginCallbackDialog(_account);
			if (ShowDialog(dialog))
				return (dialog.Email, dialog.Password);
			return (null, null);
		}

		public void ShowApprovalNeeded()
		{
			using var dialog = new ApprovalNeededDialog();
			ShowDialog(dialog);
		}
	}
}