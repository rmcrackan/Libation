using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using AudibleApi;
using AudibleUtilities;
using LibationWinForms.Dialogs.Login;

namespace LibationWinForms.Login
{
	public class WinformLoginChoiceEager : WinformLoginBase, ILoginChoiceEager
	{
		/// <summary>Convenience method. Recommended when wiring up Winforms to <see cref="ApplicationServices.LibraryCommands.ImportAccountAsync"/></summary>
		public static Func<Account, Task<ApiExtended>> CreateApiExtendedFunc(IWin32Window owner) => a => ApiExtendedFunc(a, owner);

		private static async Task<ApiExtended> ApiExtendedFunc(Account account, IWin32Window owner)
			=> await ApiExtended.CreateAsync(account, new WinformLoginChoiceEager(account, owner));

		public ILoginCallback LoginCallback { get; private set; }

		private Account _account { get; }

		private WinformLoginChoiceEager(Account account, IWin32Window owner) : base(owner)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
			LoginCallback = new WinformLoginCallback(_account, owner);
		}

		public Task<ChoiceOut> StartAsync(ChoiceIn choiceIn)
		{
			if (Environment.OSVersion.Version.Major >= 10)
			{
				try
				{
					using var weblogin = new WebLoginDialog(_account.AccountId, choiceIn.LoginUrl);
					if (ShowDialog(weblogin))
						return Task.FromResult(ChoiceOut.External(weblogin.ResponseUrl));
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, $"Failed to run {nameof(WebLoginDialog)}");
				}
			}

			using var dialog = new LoginChoiceEagerDialog(_account);

			if (!ShowDialog(dialog) || (dialog.LoginMethod is LoginMethod.Api && string.IsNullOrWhiteSpace(dialog.Password)))
				return null;

			switch (dialog.LoginMethod)
			{
				case LoginMethod.Api:
					return Task.FromResult(ChoiceOut.WithApi(dialog.Email, dialog.Password));
				case LoginMethod.External:
					{
						using var externalDialog = new LoginExternalDialog(_account, choiceIn.LoginUrl);
						return Task.FromResult(
							ShowDialog(externalDialog)
							? ChoiceOut.External(externalDialog.ResponseUrl)
							: null);
					}
				default:
					throw new Exception($"Unknown {nameof(LoginMethod)} value");
			}
		}
	}
}
