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


		public async void Quality_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_viewModel.SpatialSelected)
			{
				using var accounts = AudibleApiStorage.GetAccountsSettingsPersister();

				if (!accounts.AccountsSettings.Accounts.Any(a => a.IdentityTokens.DeviceType == AudibleApi.Resources.DeviceType))
				{
					await MessageBox.Show(VisualRoot as Window,
						"Your must remove account(s) from Libation and then re-add them to enable spatial audiobook downloads.",
						"Spatial Audio Unavailable",
						MessageBoxButtons.OK);

					_viewModel.FileDownloadQuality = _viewModel.DownloadQualities[1];
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
