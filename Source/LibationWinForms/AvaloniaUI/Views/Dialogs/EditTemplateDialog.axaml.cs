using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Dinah.Core;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{
	class BracketEscapeConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string str && str[0] != '<' && str[^1] != '>')
				return $"<{str}>";
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is string str && str[0] == '<' && str[^1] == '>')
				return str[1..^2];
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}
	}
	public partial class EditTemplateDialog : DialogWindow
	{
		// final value. post-validity check
		public string TemplateText { get; private set; }

		private EditTemplateViewModel _viewModel;

		public EditTemplateDialog()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
			_viewModel = new(Configuration.Instance, this.Find<WrapPanel>(nameof(wrapPanel)));
		}

		public EditTemplateDialog(Templates template, string inputTemplateText) : this()
		{
			_viewModel.template = ArgumentValidator.EnsureNotNull(template, nameof(template));
			Title = $"Edit {_viewModel.template.Name}";
			_viewModel.Description = _viewModel.template.Description;
			_viewModel.resetTextBox(inputTemplateText);

			_viewModel.ListItems = _viewModel.template.GetTemplateTags();

			DataContext = _viewModel;
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
		protected override async Task SaveAndCloseAsync()
		{
			if (!await _viewModel.Validate())
				return;

			TemplateText = _viewModel.workingTemplateText;
			await base.SaveAndCloseAsync();
		}

		public async void SaveButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();

		public void ResetButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> _viewModel.resetTextBox(_viewModel.template.DefaultTemplate);

		private class EditTemplateViewModel : ViewModels.ViewModelBase
		{
			WrapPanel WrapPanel;
			public Configuration config { get; }
			public EditTemplateViewModel(Configuration configuration, WrapPanel panel)
			{
				config = configuration;
				WrapPanel = panel;
			}
			// hold the work-in-progress value. not guaranteed to be valid
			private string _workingTemplateText;
			public string workingTemplateText
			{
				get => _workingTemplateText;
				set
				{
					_workingTemplateText = template.Sanitize(value);
					templateTb_TextChanged();

				}
			}
			private string _warningText;
			public string WarningText
			{
				get => _warningText;
				set
				{
					this.RaiseAndSetIfChanged(ref _warningText, value);
				}
			}

			public Templates template { get; set; }
			public string Description { get; set; }

			public IEnumerable<TemplateTags> ListItems { get; set; }

			public void resetTextBox(string value) => workingTemplateText = value;

			public async Task<bool> Validate()
			{
				if (template.IsValid(workingTemplateText))
					return true;
				var errors = template
					.GetErrors(workingTemplateText)
					.Select(err => $"- {err}")
					.Aggregate((a, b) => $"{a}\r\n{b}");
				await MessageBox.Show($"This template text is not valid. Errors:\r\n{errors}", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}

			private void templateTb_TextChanged()
			{
				var isChapterTitle = template == Templates.ChapterTitle;
				var isFolder = template == Templates.Folder;

				var libraryBookDto = new LibraryBookDto
				{
					Account = "my account",
					AudibleProductId = "123456789",
					Title = "A Study in Scarlet: A Sherlock Holmes Novel",
					Locale = "us",
					Authors = new List<string> { "Arthur Conan Doyle", "Stephen Fry - introductions" },
					Narrators = new List<string> { "Stephen Fry" },
					SeriesName = "Sherlock Holmes",
					SeriesNumber = "1"
				};
				var chapterName = "A Flight for Life";
				var chapterNumber = 4;
				var chaptersTotal = 10;

				var partFileProperties = new AaxDecrypter.MultiConvertFileProperties()
				{
					OutputFileName = "",
					PartsPosition = chapterNumber,
					PartsTotal = chaptersTotal,
					Title = chapterName
				};

				var books = config.Books;
				var folder = Templates.Folder.GetPortionFilename(
					libraryBookDto,
					isFolder ? workingTemplateText : config.FolderTemplate);

				var file
					= template == Templates.ChapterFile
					? Templates.ChapterFile.GetPortionFilename(
						libraryBookDto,
						workingTemplateText,
						partFileProperties,
						"")
					: Templates.File.GetPortionFilename(
						libraryBookDto,
						isFolder ? config.FileTemplate : workingTemplateText);
				var ext = config.DecryptToLossy ? "mp3" : "m4b";

				var chapterTitle = Templates.ChapterTitle.GetPortionTitle(libraryBookDto, workingTemplateText, partFileProperties);

				const char ZERO_WIDTH_SPACE = '\u200B';
				var sing = $"{Path.DirectorySeparatorChar}";

				// result: can wrap long paths. eg:
				// |-- LINE WRAP BOUNDARIES --|
				// \books\author with a very     <= normal line break on space between words
				// long name\narrator narrator   
				// \title                        <= line break on the zero-with space we added before slashes
				string slashWrap(string val) => val.Replace(sing, $"{ZERO_WIDTH_SPACE}{sing}");

				WarningText
					= !template.HasWarnings(workingTemplateText)
					? ""
					: "Warning:\r\n" +
						template
						.GetWarnings(workingTemplateText)
						.Select(err => $"- {err}")
						.Aggregate((a, b) => $"{a}\r\n{b}");

				var list = new List<TextCharacters>();

				var bold = new Typeface(Typeface.Default.FontFamily, FontStyle.Normal, FontWeight.Bold);
				var normal = new Typeface(Typeface.Default.FontFamily, FontStyle.Normal, FontWeight.Normal);

				var stringList = new List<(string, FontWeight)>();

				if (isChapterTitle)
				{
					stringList.Add((chapterTitle, FontWeight.Bold));
				}
				else
				{

					stringList.Add((slashWrap(books), FontWeight.Normal));
					stringList.Add((sing, FontWeight.Normal));

					stringList.Add((slashWrap(folder), isFolder ? FontWeight.Bold : FontWeight.Normal));

					stringList.Add((sing, FontWeight.Normal));

					stringList.Add((file, !isFolder ? FontWeight.Bold : FontWeight.Normal));

					stringList.Add(($".{ext}", FontWeight.Normal));
				}

				WrapPanel.Children.Clear();

				//Avalonia doesn't yet support anything like rich text, so add a new textblock for every word/style
				foreach (var item in stringList)
				{
					var wordsSplit = item.Item1.Split(' ');

					for(int i = 0; i < wordsSplit.Length; i++)
					{
						var tb = new TextBlock();
						tb.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;

						tb.Text = wordsSplit[i] + (i == wordsSplit.Length - 1 ? "" : " ");
						tb.FontWeight = item.Item2;

						WrapPanel.Children.Add(tb);
					}
				}

			}
		}

	}
}
