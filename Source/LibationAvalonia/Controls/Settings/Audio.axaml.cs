using Avalonia.Controls;
using Avalonia.ReactiveUI;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using System.Threading.Tasks;

namespace LibationAvalonia.Controls.Settings
{
	public partial class Audio : ReactiveUserControl<AudioSettingsVM>
	{
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
			var newTemplate = await editTemplate(TemplateEditor<Templates.ChapterTitleTemplate>.CreateNameEditor(ViewModel.ChapterTitleTemplate));
			if (newTemplate is not null)
				ViewModel.ChapterTitleTemplate = newTemplate;
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
