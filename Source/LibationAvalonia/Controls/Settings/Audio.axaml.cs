using Avalonia.Controls;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
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
