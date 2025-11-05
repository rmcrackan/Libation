using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Styling;
using Dinah.Core;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public partial class EditTemplateDialog : DialogWindow
{
	private EditTemplateViewModel _viewModel;

	public EditTemplateDialog()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			var mockInstance = Configuration.CreateMockInstance();
			mockInstance.Books = Configuration.DefaultBooksDirectory;
			RequestedThemeVariant  = ThemeVariant.Dark;
			var editor = TemplateEditor<Templates.FileTemplate>.CreateFilenameEditor(mockInstance.Books, mockInstance.FileTemplate);
			_viewModel = new(mockInstance, editor);
			_viewModel.ResetTextBox(editor.EditingTemplate.TemplateText);
			Title = $"Edit {editor.TemplateName}";
			DataContext = _viewModel;
		}
	}

	public EditTemplateDialog(ITemplateEditor templateEditor) : this()
	{
		ArgumentValidator.EnsureNotNull(templateEditor, nameof(templateEditor));

		_viewModel = new EditTemplateViewModel(Configuration.Instance, templateEditor);
		_viewModel.ResetTextBox(templateEditor.EditingTemplate.TemplateText);
		Title = $"Edit {templateEditor.TemplateName}";
		DataContext = _viewModel;
	}


	public void EditTemplateViewModel_DoubleTapped(object sender, Avalonia.Input.TappedEventArgs e)
	{
		var dataGrid = sender as DataGrid;

		var item = (dataGrid.SelectedItem as Tuple<string, string, string>).Item3;
		if (string.IsNullOrWhiteSpace(item)) return;

		var text = userEditTbox.Text;

		userEditTbox.Text = text.Insert(Math.Min(Math.Max(0, userEditTbox.CaretIndex), text.Length), item);
		userEditTbox.CaretIndex += item.Length;
	}

	protected override async Task SaveAndCloseAsync()
	{
		if (!await _viewModel.Validate())
			return;

		await base.SaveAndCloseAsync();
	}

	public async void SaveButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await SaveAndCloseAsync();

	internal class EditTemplateViewModel : ViewModels.ViewModelBase
	{
		private readonly Configuration config;
		public InlineCollection Inlines { get; } = new();
		public ITemplateEditor TemplateEditor { get; }
		public EditTemplateViewModel(Configuration configuration, ITemplateEditor templates)
		{
			config = configuration;
			TemplateEditor = templates;
			Description = templates.TemplateDescription;
			ListItems
			= new AvaloniaList<Tuple<string, string, string>>(
				TemplateEditor
				.EditingTemplate
				.TagsRegistered
				.Cast<TemplateTags>()
				.Select(
					t => new Tuple<string, string, string>(
						$"<{t.TagName}>",
						t.Description,
						t.DefaultValue)
					)
				);

		}

		public void GoToNamingTemplateWiki()
			=> Go.To.Url(@"ht" + "tps://github.com/rmcrackan/Libation/blob/master/Documentation/NamingTemplates.md");

		// hold the work-in-progress value. not guaranteed to be valid
		private string _userTemplateText;
		public string UserTemplateText
		{
			get => _userTemplateText;
			set
			{
				this.RaiseAndSetIfChanged(ref _userTemplateText, value);
				templateTb_TextChanged();
			}
		}

		private string _warningText;
		public string WarningText { get => _warningText; set => this.RaiseAndSetIfChanged(ref _warningText, value); }

		public string Description { get; }

		public AvaloniaList<Tuple<string, string, string>> ListItems { get; set; }

		public void ResetTextBox(string value) => UserTemplateText = value;
		public void ResetToDefault() => ResetTextBox(TemplateEditor.DefaultTemplate);

		public async Task<bool> Validate()
		{
			if (TemplateEditor.EditingTemplate.IsValid)
				return true;

			var errors
				= TemplateEditor
					.EditingTemplate
					.Errors
					.Select(err => $"- {err}")
					.Aggregate((a, b) => $"{a}\r\n{b}");
			await MessageBox.Show($"This template text is not valid. Errors:\r\n{errors}", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}

		private void templateTb_TextChanged()
		{
			TemplateEditor.SetTemplateText(UserTemplateText);

			const char ZERO_WIDTH_SPACE = '\u200B';
			var sing = $"{Path.DirectorySeparatorChar}";

			// result: can wrap long paths. eg:
			// |-- LINE WRAP BOUNDARIES --|
			// \books\author with a very     <= normal line break on space between words
			// long name\narrator narrator   
			// \title                        <= line break on the zero-with space we added before slashes
			string slashWrap(string val) => val.Replace(sing, $"{ZERO_WIDTH_SPACE}{sing}");

			WarningText
				= !TemplateEditor.EditingTemplate.HasWarnings
				? ""
				: "Warning:\r\n" +
					TemplateEditor
					.EditingTemplate
					.Warnings
					.Select(err => $"- {err}")
					.Aggregate((a, b) => $"{a}\r\n{b}");

			var bold = FontWeight.Bold;
			var reg = FontWeight.Normal;

			Inlines.Clear();

			if (!TemplateEditor.IsFilePath)
			{
				Inlines.Add(new Run(TemplateEditor.GetName()) { FontWeight = bold });
				return;
			}

			var folder = TemplateEditor.GetFolderName();
			var file = TemplateEditor.GetFileName();
			var ext = config.DecryptToLossy ? "mp3" : "m4b";

			Inlines.Add(new Run(slashWrap(TemplateEditor.BaseDirectory.PathWithoutPrefix)) { FontWeight = reg });
			Inlines.Add(new Run(sing) { FontWeight = reg });

			Inlines.Add(new Run(slashWrap(folder)) { FontWeight = TemplateEditor.IsFolder ? bold : reg });

			Inlines.Add(new Run(sing));

			Inlines.Add(new Run(slashWrap(file)) { FontWeight = TemplateEditor.IsFolder ? reg : bold });

			Inlines.Add(new Run($".{ext}"));
		}
	}
}
