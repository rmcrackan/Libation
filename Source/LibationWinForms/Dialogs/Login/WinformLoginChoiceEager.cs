using System;
using System.Threading.Tasks;
using AudibleApi;
using AudibleUtilities;
using LibationWinForms.Dialogs.Login;

namespace LibationWinForms.Login
{
	public class WinformLoginChoiceEager : WinformLoginBase, ILoginChoiceEager
	{
		/// <summary>Convenience method. Recommended when wiring up Winforms to <see cref="ApplicationServices.LibraryCommands.ImportAccountAsync"/></summary>
		public static async Task<ApiExtended> ApiExtendedFunc(Account account) => await ApiExtended.CreateAsync(account, new WinformLoginChoiceEager(account));

		public ILoginCallback LoginCallback { get; private set; }

		private Account _account { get; }

		public WinformLoginChoiceEager(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
			LoginCallback = new WinformLoginCallback(_account);
		}

		public Task<ChoiceOut> StartAsync(ChoiceIn choiceIn)
		{
			using var dialog = new LoginChoiceEagerDialog(_account);

			if (!ShowDialog(dialog) || string.IsNullOrWhiteSpace(dialog.Password))
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
