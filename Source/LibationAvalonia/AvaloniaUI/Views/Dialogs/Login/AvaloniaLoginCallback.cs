using System;
using AudibleApi;
using AudibleUtilities;

namespace LibationAvalonia.AvaloniaUI.Views.Dialogs.Login
{
	public class AvaloniaLoginCallback : AvaloniaLoginBase, ILoginCallback
	{
		private Account _account { get; }

		public AvaloniaLoginCallback(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
		}

		public string Get2faCode()
		{
			var dialog = new _2faCodeDialog();
			if (ShowDialog(dialog))
				return dialog.Code;

			return null;
		}

		public string GetCaptchaAnswer(byte[] captchaImage)
		{
			var dialog = new CaptchaDialog(captchaImage);
			if (ShowDialog(dialog))
				return dialog.Answer;
			return null;
		}

		public (string name, string value) GetMfaChoice(MfaConfig mfaConfig)
		{
			var dialog = new MfaDialog(mfaConfig);
			if (ShowDialog(dialog))
				return (dialog.SelectedName, dialog.SelectedValue);
			return (null, null);
		}

		public (string email, string password) GetLogin()
		{
			var dialog = new LoginCallbackDialog(_account);
			if (ShowDialog(dialog))
				return (_account.AccountId, dialog.Password);
			return (null, null);
		}

		public void ShowApprovalNeeded()
		{
			var dialog = new ApprovalNeededDialog();
			ShowDialog(dialog);
		}
	}
}