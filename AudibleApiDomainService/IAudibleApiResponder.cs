namespace AudibleApiDomainService
{
	public interface IAudibleApiResponder
	{
		(string email, string password) GetLogin();
		string GetCaptchaAnswer(byte[] captchaImage);
		string Get2faCode();
	}
}