using System;
using System.Threading.Tasks;
using AudibleApi;
using AudibleUtilities;

namespace LibationAvalonia.Dialogs.Login
{
	public class AvaloniaLoginCallback : AvaloniaLoginBase, ILoginCallback
	{
		private Account _account { get; }

		public AvaloniaLoginCallback(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
		}

		public async Task<string> Get2faCodeAsync()
		{
			var dialog = new _2faCodeDialog();
			if (await ShowDialog(dialog))
				return dialog.Code;

			return null;
		}

		public async Task<string> GetCaptchaAnswerAsync(byte[] captchaImage)
		{
			var dialog = new CaptchaDialog(captchaImage);
			if (await ShowDialog(dialog))
				return dialog.Answer;
			return null;
		}

		public async Task<(string name, string value)> GetMfaChoiceAsync(MfaConfig mfaConfig)
		{
			var dialog = new MfaDialog(mfaConfig);
			if (await ShowDialog(dialog))
				return (dialog.SelectedName, dialog.SelectedValue);
			return (null, null);
		}

		public async Task<(string email, string password)> GetLoginAsync()
		{
			var dialog = new LoginCallbackDialog(_account);
			if (await ShowDialog(dialog))
				return (_account.AccountId, dialog.Password);
			return (null, null);
		}

		public async Task ShowApprovalNeededAsync()
		{
			var dialog = new ApprovalNeededDialog();
			await ShowDialog(dialog);
		}
	}
}