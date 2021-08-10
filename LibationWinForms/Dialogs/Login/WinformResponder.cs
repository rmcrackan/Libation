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
			if (showDialog(dialog))
				return dialog.Code;
			return null;
		}

		public string GetCaptchaAnswer(byte[] captchaImage)
		{
			using var dialog = new CaptchaDialog(captchaImage);
			if (showDialog(dialog))
				return dialog.Answer;
			return null;
		}

		public (string name, string value) GetMfaChoice(MfaConfig mfaConfig)
		{
			using var dialog = new MfaDialog(mfaConfig);
			if (showDialog(dialog))
				return (dialog.SelectedName, dialog.SelectedValue);
			return (null, null);
		}

		public (string email, string password) GetLogin()
		{
			using var dialog = new AudibleLoginDialog(_account);
			if (showDialog(dialog))
				return (dialog.Email, dialog.Password);
			return (null, null);
		}

		public void ShowApprovalNeeded()
		{
			using var dialog = new ApprovalNeededDialog();
			showDialog(dialog);
		}

		/// <returns>True if ShowDialog's DialogResult == OK</returns>
		private static bool showDialog(System.Windows.Forms.Form dialog)
		{
			var result = dialog.ShowDialog();
			Serilog.Log.Logger.Debug("{@DebugInfo}", new { DialogResult = result });
			return result == System.Windows.Forms.DialogResult.OK;
		}
	}
}