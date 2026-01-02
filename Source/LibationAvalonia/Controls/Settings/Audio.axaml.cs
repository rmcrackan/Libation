using AudibleUtilities;
using Avalonia.Controls;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Controls.Settings
{
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

		private async void UseWidevine_IsCheckedChanged(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (sender is CheckBox cbox && cbox.IsChecked is true)
			{
				using var accounts = AudibleApiStorage.GetAccountsSettingsPersister();

				if (!accounts.AccountsSettings.Accounts.All(a => a.IdentityTokens.DeviceType == AudibleApi.Resources.DeviceType))
				{
					if (VisualRoot is Window parent)
					{
						var choice = await MessageBox.Show(parent,
						"In order to enable widevine content, Libation will need to log into your accounts again.\r\n\r\n" +
						"Do you want Libation to clear your current account settings and prompt you to login before the next download?",
						"Widevine Content Unavailable",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2);

						if (choice == DialogResult.Yes)
						{
							foreach (var account in accounts.AccountsSettings.Accounts.ToArray())
							{
								if (account.IdentityTokens.DeviceType != AudibleApi.Resources.DeviceType)
								{
									accounts.AccountsSettings.Delete(account);
									var acc = accounts.AccountsSettings.Upsert(account.AccountId, account.Locale.Name);
									acc.AccountName = account.AccountName;
								}
							}
							return;
						}
					}

					_viewModel?.UseWidevine = false;
				}
			}
			else
			{
				_viewModel?.Request_xHE_AAC = _viewModel.RequestSpatial = false;
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
}
