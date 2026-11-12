using AudibleUtilities;
using Avalonia.Controls;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase.Forms;
using System.Linq;
using System.Threading.Tasks;

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
		if (sender is CheckBox cbox && cbox.IsChecked is true)
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

	public async void EditChapterTitleTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (_viewModel is null) return;
		var newTemplate = await editTemplate(TemplateEditor<Templates.ChapterTitleTemplate>.CreateNameEditor(_viewModel.ChapterTitleTemplate));
		if (newTemplate is not null)
			_viewModel.ChapterTitleTemplate = newTemplate;
	}

	private async Task<string?> editTemplate(ITemplateEditor template)
	{
		var form = new EditTemplateDialog(template);
		if (await form.ShowDialog<DialogResult>(this.GetParentWindow()) == DialogResult.OK)
			return template.EditingTemplate.TemplateText;
		else return null;
	}
}
