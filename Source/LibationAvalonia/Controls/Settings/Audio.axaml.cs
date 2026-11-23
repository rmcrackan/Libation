using AudibleUtilities;
using Avalonia.Controls;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationUiBase.Forms;

namespace LibationAvalonia.Controls.Settings;

public partial class Audio : UserControl
{
	private AudioSettingsVM? _viewModel => DataContext as AudioSettingsVM;
	public Audio()
	{
		InitializeComponent();
		if (Design.IsDesignMode)
		{
			DataContext = new AudioSettingsVM(Configuration.CreateMockInstance());
		}
	}

	private async void UseWidevine_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (sender is Avalonia.Controls.Primitives.ToggleButton cbox && cbox.IsChecked is true)
		{
			using var accounts = AudibleApiStorage.GetAccountsSettingsPersister();

			if (accounts.AccountsSettings.Accounts.Count > 0)
			{
				var choice = await MessageBox.Show(this.GetParentWindow(),
					ThrottledDeviceRecovery.WidevinePromptMessage,
					ThrottledDeviceRecovery.WidevinePromptTitle,
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question,
					MessageBoxDefaultButton.Button1);

				if (choice == DialogResult.Yes)
				{
					ThrottledDeviceRecovery.MarkAllAccountsForWidevine(accounts.AccountsSettings, Configuration.Instance);
					return;
				}

				cbox.IsChecked = false;
				if (_viewModel is not null)
					_viewModel.UseWidevine = false;
			}
			else
			{
				Configuration.Instance.DeviceRegistrationKind = AudibleApi.DeviceRegistrationKind.CurrentAndroid;
			}
		}
		else
		{
			if (_viewModel is not null)
				_viewModel.Request_xHE_AAC = _viewModel.RequestSpatial = false;
		}
	}
}
