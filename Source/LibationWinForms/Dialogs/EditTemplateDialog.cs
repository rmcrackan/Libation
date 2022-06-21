using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Dinah.Core;
using LibationFileManager;

namespace LibationWinForms.Dialogs
{
	public partial class EditTemplateDialog : Form
	{
		// final value. post-validity check
		public string TemplateText { get; private set; }

		// hold the work-in-progress value. not guaranteed to be valid
		private string _workingTemplateText;
		private string workingTemplateText
		{
			get => _workingTemplateText;
			set => _workingTemplateText = template.Sanitize(value);
		}

		private void resetTextBox(string value) => this.templateTb.Text = workingTemplateText = value;

		private Configuration config { get; } = Configuration.Instance;

		private Templates template { get; }
		private string inputTemplateText { get; }

		public EditTemplateDialog()
		{
			InitializeComponent();
			this.SetLibationIcon();
		}
		public EditTemplateDialog(Templates template, string inputTemplateText) : this()
		{
			this.template = ArgumentValidator.EnsureNotNull(template, nameof(template));
			this.inputTemplateText = inputTemplateText ?? "";
		}

		private void EditTemplateDialog_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			if (template is null)
			{
				MessageBoxLib.ShowAdminAlert(this, $"Programming error. {nameof(EditTemplateDialog)} was not created correctly", "Edit template error", new NullReferenceException($"{nameof(template)} is null"));
				return;
			}

			warningsLbl.Text = "";

			this.Text = $"Edit {template.Name}";

			this.templateLbl.Text = template.Description;
			resetTextBox(inputTemplateText);

			// populate list view
			foreach (var tag in template.GetTemplateTags())
				listView1.Items.Add(new ListViewItem(new[] { $"<{tag.TagName}>", tag.Description }));
		}

		private void resetToDefaultBtn_Click(object sender, EventArgs e) => resetTextBox(template.DefaultTemplate);

		private void templateTb_TextChanged(object sender, EventArgs e)
		{
			workingTemplateText = templateTb.Text;
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

			warningsLbl.Text
				= !template.HasWarnings(workingTemplateText)
				? ""
				: "Warning:\r\n" +
					template
					.GetWarnings(workingTemplateText)
					.Select(err => $"- {err}")
					.Aggregate((a, b) => $"{a}\r\n{b}");

			var bold = new System.Drawing.Font(richTextBox1.Font, System.Drawing.FontStyle.Bold);
			var reg = new System.Drawing.Font(richTextBox1.Font, System.Drawing.FontStyle.Regular);

			richTextBox1.Clear();
			richTextBox1.SelectionFont = reg;

			if (isChapterTitle)
			{
				richTextBox1.SelectionFont = bold;
				richTextBox1.AppendText(chapterTitle);
				return;
			}

			richTextBox1.AppendText(slashWrap(books));
			richTextBox1.AppendText(sing);

			if (isFolder)
				richTextBox1.SelectionFont = bold;

			richTextBox1.AppendText(slashWrap(folder));

			if (isFolder)
				richTextBox1.SelectionFont = reg;

			richTextBox1.AppendText(sing);

			if (!isFolder)
				richTextBox1.SelectionFont = bold;

			richTextBox1.AppendText(file);

			if (!isFolder)
				richTextBox1.SelectionFont = reg;

			richTextBox1.AppendText($".{ext}");
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			if (!template.IsValid(workingTemplateText))
			{
				var errors = template
					.GetErrors(workingTemplateText)
					.Select(err => $"- {err}")
					.Aggregate((a, b) => $"{a}\r\n{b}");
				MessageBox.Show($"This template text is not valid. Errors:\r\n{errors}", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			TemplateText = workingTemplateText;

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
