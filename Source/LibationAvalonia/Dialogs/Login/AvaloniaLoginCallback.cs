using System;
using System.Threading.Tasks;
using AudibleApi;
using AudibleUtilities;

namespace LibationAvalonia.Dialogs.Login
{
	public class AvaloniaLoginCallback : ILoginCallback
	{
		private Account _account { get; }

		public string DeviceName { get; } = "Libation";

		public AvaloniaLoginCallback(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
		}

		public async Task<string> Get2faCodeAsync(string prompt)
		{
			var dialog = new _2faCodeDialog(prompt);
			if (await dialog.ShowDialogAsync() is DialogResult.OK)
				return dialog.Code;

			return null;
		}

		public async Task<(string password, string guess)> GetCaptchaAnswerAsync(string password, byte[] captchaImage)
		{
			var dialog = new CaptchaDialog(password, captchaImage);
			if (await dialog.ShowDialogAsync() is DialogResult.OK)
				return (dialog.Password, dialog.Answer);
			return (null, null);
		}

		public async Task<(string name, string value)> GetMfaChoiceAsync(MfaConfig mfaConfig)
		{
			var dialog = new MfaDialog(mfaConfig);
			if (await dialog.ShowDialogAsync() is DialogResult.OK)
				return (dialog.SelectedName, dialog.SelectedValue);
			return (null, null);
		}

		public async Task<(string email, string password)> GetLoginAsync()
		{
			var dialog = new LoginCallbackDialog(_account);
			if (await dialog.ShowDialogAsync() is DialogResult.OK)
				return (_account.AccountId, dialog.Password);
			return (null, null);
		}

		public async Task ShowApprovalNeededAsync()
		{
			var dialog = new ApprovalNeededDialog();
			await dialog.ShowDialogAsync();
		}
	}
}