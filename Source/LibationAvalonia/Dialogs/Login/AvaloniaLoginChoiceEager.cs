using System;
using System.Threading.Tasks;
using AudibleApi;
using AudibleUtilities;

namespace LibationAvalonia.Dialogs.Login
{
	public class AvaloniaLoginChoiceEager : AvaloniaLoginBase, ILoginChoiceEager
	{
		/// <summary>Convenience method. Recommended when wiring up Winforms to <see cref="ApplicationServices.LibraryCommands.ImportAccountAsync"/></summary>
		public static async Task<ApiExtended> ApiExtendedFunc(Account account) => await ApiExtended.CreateAsync(account, new AvaloniaLoginChoiceEager(account));

		public ILoginCallback LoginCallback { get; private set; }

		private Account _account { get; }

		public AvaloniaLoginChoiceEager(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
			LoginCallback = new AvaloniaLoginCallback(_account);
		}

		public async Task<ChoiceOut> StartAsync(ChoiceIn choiceIn)
		{
			var dialog = new LoginChoiceEagerDialog(_account);

			if (!await ShowDialog(dialog))
				return null;


			switch (dialog.LoginMethod)
			{
				case LoginMethod.Api:
					return ChoiceOut.WithApi(dialog.Account.AccountId, dialog.Password);
				case LoginMethod.External:
					{
						var externalDialog = new LoginExternalDialog(_account, choiceIn.LoginUrl);
						return await ShowDialog(externalDialog)
							? ChoiceOut.External(externalDialog.ResponseUrl)
							: null;
					}
				default:
					throw new Exception($"Unknown {nameof(LoginMethod)} value");
			}
		}

	}
}
