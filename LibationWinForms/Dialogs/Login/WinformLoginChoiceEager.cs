using System;
using AudibleApi;
using InternalUtilities;
using LibationWinForms.Dialogs.Login;

namespace LibationWinForms.Login
{
	public class WinformLoginChoiceEager : WinformLoginBase, ILoginChoiceEager
	{
		public ILoginCallback LoginCallback { get; private set; }

		private Account _account { get; }

		public WinformLoginChoiceEager(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
			LoginCallback = new WinformLoginCallback(_account);
		}

		public ChoiceOut Start(ChoiceIn choiceIn)
		{
			using var dialog = new LoginChoiceEagerDialog(_account);

			if (!ShowDialog(dialog))
				return null;

			switch (dialog.LoginMethod)
			{
				case LoginMethod.Api:
					return ChoiceOut.WithApi(dialog.Email, dialog.Password);
				case LoginMethod.External:
				{
					using var externalDialog = new LoginExternalDialog(_account, choiceIn.LoginUrl);
					return ShowDialog(externalDialog)
						? ChoiceOut.External(externalDialog.ResponseUrl)
						: null;
				}
				default:
					throw new Exception($"Unknown {nameof(LoginMethod)} value");
			}
		}
	}
}
