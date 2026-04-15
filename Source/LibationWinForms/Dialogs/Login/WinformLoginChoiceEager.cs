using AudibleApi;
using AudibleUtilities;
using LibationFileManager;
using LibationUiBase;
using LibationWinForms.Dialogs.Login;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Login;

public class WinformLoginChoiceEager : ILoginChoiceEager
{
	public ILoginCallback LoginCallback { get; } = new WinformLoginCallback();

	private Account _account { get; }
	private Control Owner { get; }
	public WinformLoginChoiceEager(Account account, Control owner)
	{
		_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
		Owner = Dinah.Core.ArgumentValidator.EnsureNotNull(owner, nameof(owner));
	}

	public Task<ChoiceOut?> StartAsync(ChoiceIn choiceIn)
		=> Owner.Invoke(() => StartAsyncInternal(choiceIn));

	private Task<ChoiceOut?> StartAsyncInternal(ChoiceIn choiceIn)
	{
		if (Configuration.Instance.UseWebView && Environment.OSVersion.Version.Major >= 10)
		{
			try
			{
				// Time-of-use: user may turn off embedded browser before this runs — do not construct WebView2.
				if (Configuration.Instance.UseWebView)
				{
					using var weblogin = new WebLoginDialog(_account.AccountId, choiceIn);
					if (ShowDialog(weblogin))
						return Task.FromResult(ChoiceOut.External(weblogin.ResponseUrl));
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Failed to run {nameof(WebLoginDialog)}");
				if (WebView2LoginErrorMessage.IsWebView2SignInInfrastructureFailure(ex))
					MessageBoxLib.ShowAdminAlert(Owner, WebView2LoginErrorMessage.ExplainerBody, WebView2LoginErrorMessage.Caption, ex);
			}
		}

		using var externalDialog = new LoginExternalDialog(_account, choiceIn.LoginUrl);
		return Task.FromResult(
			ShowDialog(externalDialog)
			? ChoiceOut.External(externalDialog.ResponseUrl)
			: null);
	}

	/// <returns>True if ShowDialog's DialogResult == OK</returns>
	private bool ShowDialog(Form dialog)
		=> Owner.Invoke(() =>
		{
			var result = dialog.ShowDialog(Owner);
			Serilog.Log.Logger.Debug("{@DebugInfo}", new { DialogResult = result });
			return result == DialogResult.OK;
		});
}
