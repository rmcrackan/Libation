using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudibleApi;
using AudibleUtilities;
using LibationWinForms.Dialogs.Login;

namespace LibationWinForms.Login
{
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

		public Task<ChoiceOut> StartAsync(ChoiceIn choiceIn)
			=> Owner.Invoke(() => StartAsyncInternal(choiceIn));

		private Task<ChoiceOut> StartAsyncInternal(ChoiceIn choiceIn)
		{
			if (Environment.OSVersion.Version.Major >= 10)
			{
				try
				{
					using var weblogin = new WebLoginDialog(_account.AccountId, choiceIn);
					if (ShowDialog(weblogin))
						return Task.FromResult(ChoiceOut.External(weblogin.ResponseUrl));
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, $"Failed to run {nameof(WebLoginDialog)}");
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
}
