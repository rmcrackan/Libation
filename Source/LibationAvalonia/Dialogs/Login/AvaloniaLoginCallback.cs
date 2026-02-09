using AudibleApi;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login;

public class AvaloniaLoginCallback : ILoginCallback
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