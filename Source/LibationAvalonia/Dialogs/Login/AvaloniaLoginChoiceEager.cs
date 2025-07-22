using AudibleApi;
using AudibleUtilities;
using Avalonia.Threading;
using LibationFileManager;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia.Dialogs.Login
{
	public class AvaloniaLoginChoiceEager : ILoginChoiceEager
	{
		public ILoginCallback LoginCallback { get; }

		private readonly Account _account;

		public AvaloniaLoginChoiceEager(Account account)
		{
			_account = Dinah.Core.ArgumentValidator.EnsureNotNull(account, nameof(account));
			LoginCallback = new AvaloniaLoginCallback(_account);
		}

		public async Task<ChoiceOut?> StartAsync(ChoiceIn choiceIn)
			=> await Dispatcher.UIThread.InvokeAsync(() => StartAsyncInternal(choiceIn));

		private async Task<ChoiceOut?> StartAsyncInternal(ChoiceIn choiceIn)
		{
			if (Configuration.IsWindows && Environment.OSVersion.Version.Major >= 10)
			{
				try
				{
					var weblogin = new WebLoginDialog(_account.AccountId, choiceIn.LoginUrl);
					if (await weblogin.ShowDialogAsync(App.MainWindow) is DialogResult.OK)
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
