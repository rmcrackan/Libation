using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudibleApi;
using AudibleUtilities;
using LibationWinForms.Dialogs.Login;

namespace LibationWinForms.Login
{
	public class WinformLoginCallback : ILoginCallback
	{
		public string DeviceName { get; } = "Libation";

		public Task<string> Get2faCodeAsync(string prompt) => throw new System.NotSupportedException();
		public Task<(string password, string guess)> GetCaptchaAnswerAsync(string password, byte[] captchaImage)
			 => throw new System.NotSupportedException();
		public Task<(string name, string value)> GetMfaChoiceAsync(MfaConfig mfaConfig)
			 => throw new System.NotSupportedException();
		public Task<(string email, string password)> GetLoginAsync()
			 => throw new System.NotSupportedException();
		public Task ShowApprovalNeededAsync() => throw new System.NotSupportedException();
	}
}