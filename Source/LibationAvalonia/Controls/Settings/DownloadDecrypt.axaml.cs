using Avalonia.Controls;
using Avalonia.ReactiveUI;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using System.Threading.Tasks;

namespace LibationAvalonia.Controls.Settings
{
	public partial class DownloadDecrypt : ReactiveUserControl<DownloadDecryptSettingsVM>
	{
		public DownloadDecrypt()
		{
			InitializeComponent();
			if (Design.IsDesignMode)
			{
				_ = Configuration.Instance.LibationFiles;
				DataContext = new DownloadDecryptSettingsVM(Configuration.Instance);
			}
		}

		public async void EditFolderTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = await editTemplate(TemplateEditor<Templates.FolderTemplate>.CreateFilenameEditor(ViewModel.Config.Books, ViewModel.FolderTemplate));
			if (newTemplate is not null)
				ViewModel.FolderTemplate = newTemplate;
		}

		public async void EditFileTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var newTemplate = await editTemplate(TemplateEditor<Templates.FileTemplate>.CreateFilenameEditor(ViewModel.Config.Books, ViewModel.FileTemplate));
			if (newTemplate is not null)
				ViewModel.FileTemplate = newTemplate;
		}

		public async void EditChapterFileTemplateButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{

			var newTemplate = await editTemplate(TemplateEditor<Templates.ChapterFileTemplate>.CreateFilenameEditor(ViewModel.Config.Books, ViewModel.ChapterFileTemplate));
			if (newTemplate is not null)
				ViewModel.ChapterFileTemplate = newTemplate;
		}

		public async void EditCharReplacementButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var form = new EditReplacementChars(ViewModel.Config);
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
