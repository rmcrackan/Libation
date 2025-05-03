using AudibleUtilities;
using Avalonia.Controls;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationFileManager.Templates;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Controls.Settings
{
	public partial class Audio : UserControl
	{
		private AudioSettingsVM _viewModel => DataContext as AudioSettingsVM; 
		public Audio()
		{
			InitializeComponent();
			if (Design.IsDesignMode)
			{
				_ = Configuration.Instance.LibationFiles;
				DataContext = new AudioSettingsVM(Configuration.Instance);
			}
		}

		private async void UseWidevine_IsCheckedChanged(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (sender is CheckBox cbox && cbox.IsChecked is true)
			{
				using var accounts = AudibleApiStorage.GetAccountsSettingsPersister();

				if (!accounts.AccountsSettings.Accounts.Any(a => a.IdentityTokens.DeviceType == AudibleApi.Resources.DeviceType))
				{
					if (VisualRoot is Window parent)
						await MessageBox.Show(parent,
						"Your must remove account(s) from Libation and then re-add them to enable widwvine content.",
						"Widevine Content Unavailable",
						MessageBoxButtons.OK);

					_viewModel.UseWidevine = false;
				}
			}
		}

		public async void EditChapterTitleTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (_viewModel is null) return;
			var newTemplate = await editTemplate(TemplateEditor<Templates.ChapterTitleTemplate>.CreateNameEditor(_viewModel.ChapterTitleTemplate));
			if (newTemplate is not null)
				_viewModel.ChapterTitleTemplate = newTemplate;
		}

		private async Task<string> editTemplate(ITemplateEditor template)
		{
			var form = new EditTemplateDialog(template);
			if (await form.ShowDialog<DialogResult>(this.GetParentWindow()) == DialogResult.OK)
				return template.EditingTemplate.TemplateText;
			else return null;
		}
	}
}
