using AudibleApi;
using AudibleUtilities;
using Avalonia.Threading;
using LibationFileManager;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs.Login
{
	public class AvaloniaLoginChoiceEager : ILoginChoiceEager
	{
		/// <summary>Convenience method. Recommended when wiring up Winforms to <see cref="ApplicationServices.LibraryCommands.ImportAccountAsync"/></summary>
		public static async Task<ApiExtended> ApiExtendedFunc(Account account)
			=> await ApiExtended.CreateAsync(account, new AvaloniaLoginChoiceEager(account));

		public ILoginCallback LoginCallback { get; }

		private readonly Account _account;

		public AvaloniaLoginChoiceEager(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
			LoginCallback = new AvaloniaLoginCallback(_account);
		}

		public async Task<ChoiceOut> StartAsync(ChoiceIn choiceIn)
		{
			if (Configuration.IsWindows && Environment.OSVersion.Version.Major >= 10)
			{
				try
				{
					var weblogin = new WebLoginDialog(_account.AccountId, choiceIn.LoginUrl);
					if (await weblogin.ShowDialog<DialogResult>(App.MainWindow) is DialogResult.OK)
						return ChoiceOut.External(weblogin.ResponseUrl);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, $"Failed to run {nameof(WebLoginDialog)}");
				}
			}

			var dialog = new LoginChoiceEagerDialog(_account);

			if (await dialog.ShowDialogAsync() is not DialogResult.OK ||
				(dialog.LoginMethod is LoginMethod.Api && string.IsNullOrWhiteSpace(dialog.Password)))
				return null;

			switch (dialog.LoginMethod)
			{
				case LoginMethod.Api:
					return ChoiceOut.WithApi(dialog.Account.AccountId, dialog.Password);
				case LoginMethod.External:
					{
						var externalDialog = new LoginExternalDialog(_account, choiceIn.LoginUrl);
						return await externalDialog.ShowDialogAsync() is DialogResult.OK
							? ChoiceOut.External(externalDialog.ResponseUrl)
							: null;
					}
				default:
					throw new Exception($"Unknown {nameof(LoginMethod)} value");
			}
		}

	}
}
