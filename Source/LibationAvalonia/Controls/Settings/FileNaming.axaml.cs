using Avalonia.Controls;
using FileManager;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase.Forms;
using System.Threading.Tasks;

namespace LibationAvalonia.Controls.Settings;

public partial class FileNaming : UserControl
{
	private SettingsVM? _settings => DataContext as SettingsVM;
	private DownloadDecryptSettingsVM? _viewModel => _settings?.DownloadDecryptSettings;

	public FileNaming()
	{
		InitializeComponent();
		if (Design.IsDesignMode)
		{
			DataContext = new SettingsVM(Configuration.CreateMockInstance());
		}
	}

	public async void EditFolderTemplateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Config.Books is not LongPath books) return;
		var newTemplate = await editTemplate(TemplateEditor<Templates.FolderTemplate>.CreateFilenameEditor(books, _viewModel.FolderTemplate));
		if (newTemplate is not null)
			_viewModel.FolderTemplate = newTemplate;
	}

	public async void EditFileTemplateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Config.Books is not LongPath books) return;
		var newTemplate = await editTemplate(TemplateEditor<Templates.FileTemplate>.CreateFilenameEditor(books, _viewModel.FileTemplate));
		if (newTemplate is not null)
			_viewModel.FileTemplate = newTemplate;
	}

	public async void EditChapterFileTemplateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (_viewModel is null || _viewModel.Config.Books is not LongPath books) return;
		var newTemplate = await editTemplate(TemplateEditor<Templates.ChapterFileTemplate>.CreateFilenameEditor(books, _viewModel.ChapterFileTemplate));
		if (newTemplate is not null)
			_viewModel.ChapterFileTemplate = newTemplate;
	}

	public async void EditChapterTitleTemplateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (_settings?.AudioSettings is not { } audio) return;
		var newTemplate = await editTemplate(TemplateEditor<Templates.ChapterTitleTemplate>.CreateNameEditor(audio.ChapterTitleTemplate));
		if (newTemplate is not null)
			audio.ChapterTitleTemplate = newTemplate;
	}

	public async void EditCharReplacementButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (_viewModel is null) return;
		var form = new EditReplacementChars(_viewModel.Config);
		await form.ShowDialog<DialogResult>(this.GetParentWindow());
	}

	private async Task<string?> editTemplate(ITemplateEditor template)
	{
		var form = new EditTemplateDialog(template);
		if (await form.ShowDialog<DialogResult>(this.GetParentWindow()) == DialogResult.OK)
			return template.EditingTemplate.TemplateText;
		else return null;
	}
}
