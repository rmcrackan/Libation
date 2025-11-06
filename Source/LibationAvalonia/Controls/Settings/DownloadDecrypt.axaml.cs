using Avalonia.Controls;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase.Forms;
using System.Threading.Tasks;

namespace LibationAvalonia.Controls.Settings
{
	public partial class DownloadDecrypt : UserControl
	{
		private DownloadDecryptSettingsVM _viewModel => DataContext as DownloadDecryptSettingsVM;
		public DownloadDecrypt()
		{
			InitializeComponent();
			if (Design.IsDesignMode)
			{
				DataContext = new DownloadDecryptSettingsVM(Configuration.CreateMockInstance());
			}
		}

		public async void EditFolderTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (_viewModel is null) return;
			var newTemplate = await editTemplate(TemplateEditor<Templates.FolderTemplate>.CreateFilenameEditor(_viewModel.Config.Books, _viewModel.FolderTemplate));
			if (newTemplate is not null)
				_viewModel.FolderTemplate = newTemplate;
		}

		public async void EditFileTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (_viewModel is null) return;
			var newTemplate = await editTemplate(TemplateEditor<Templates.FileTemplate>.CreateFilenameEditor(_viewModel.Config.Books, _viewModel.FileTemplate));
			if (newTemplate is not null)
				_viewModel.FileTemplate = newTemplate;
		}

		public async void EditChapterFileTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (_viewModel is null) return;
			var newTemplate = await editTemplate(TemplateEditor<Templates.ChapterFileTemplate>.CreateFilenameEditor(_viewModel.Config.Books, _viewModel.ChapterFileTemplate));
			if (newTemplate is not null)
				_viewModel.ChapterFileTemplate = newTemplate;
		}

		public async void EditCharReplacementButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (_viewModel is null) return;
			var form = new EditReplacementChars(_viewModel.Config);
			await form.ShowDialog<DialogResult>(this.GetParentWindow());
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
