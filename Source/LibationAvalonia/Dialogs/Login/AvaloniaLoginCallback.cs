using AudibleApi;
using AudibleUtilities;
using Avalonia.Threading;
using LibationUiBase.Forms;
using System.Threading.Tasks;

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
			=> await Dispatcher.UIThread.InvokeAsync(async () =>
			{
				var dialog = new _2faCodeDialog(prompt);
				if (await dialog.ShowDialogAsync() is DialogResult.OK)
					return dialog.Code;
				return null;
			});

		public async Task<(string password, string guess)> GetCaptchaAnswerAsync(string password, byte[] captchaImage)
			=> await Dispatcher.UIThread.InvokeAsync(async () =>
			{
				var dialog = new CaptchaDialog(password, captchaImage);
				if (await dialog.ShowDialogAsync() is DialogResult.OK)
					return (dialog.Password, dialog.Answer);
				return (null, null);
			});

		public async Task<(string name, string value)> GetMfaChoiceAsync(MfaConfig mfaConfig)
			=> await Dispatcher.UIThread.InvokeAsync(async () =>
			{
				var dialog = new MfaDialog(mfaConfig);
				if (await dialog.ShowDialogAsync() is DialogResult.OK)
					return (dialog.SelectedName, dialog.SelectedValue);
				return (null, null);
			});

		public async Task<(string email, string password)> GetLoginAsync()
			=> await Dispatcher.UIThread.InvokeAsync(async () =>
			{
				var dialog = new LoginCallbackDialog(_account);
				if (await dialog.ShowDialogAsync() is DialogResult.OK)
					return (_account.AccountId, dialog.Password);
				return (null, null);
			});

		public async Task ShowApprovalNeededAsync()
			=> await Dispatcher.UIThread.InvokeAsync(async () =>
			{
				var dialog = new ApprovalNeededDialog();
				await dialog.ShowDialogAsync();
			});
	}
}