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
		public string TemplateText { get; private set; }

		private Configuration config { get; } = Configuration.Instance;

		private Templates template { get; }
		private string inputTemplateText { get; }

		public EditTemplateDialog() => InitializeComponent();
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
				MessageBoxAlertAdmin.Show($"Programming error. {nameof(EditTemplateDialog)} was not created correctly", "Edit template error", new NullReferenceException($"{nameof(template)} is null"));
				return;
			}

			this.Text = $"Edit {template.Name}";

			this.templateLbl.Text = template.Description;
			this.templateTb.Text = inputTemplateText;

			// populate list view
			foreach (var tag in template.GetTemplateTags())
				listView1.Items.Add(new ListViewItem(new[] { $"<{tag.TagName}>", tag.Description }));
		}

		private void resetToDefaultBtn_Click(object sender, EventArgs e) => templateTb.Text = template.DefaultTemplate;

		private void templateTb_TextChanged(object sender, EventArgs e)
		{
			var t = templateTb.Text;

			var warnings
				= !template.HasWarnings(t)
				? ""
				: "Warnings:\r\n" +
					template
					.GetWarnings(t)
					.Select(err => $"- {err}")
					.Aggregate((a, b) => $"{a}\r\n{b}");


			var books = config.Books;
			var folderTemplate = template == Templates.Folder ? t : config.FolderTemplate;
			folderTemplate = folderTemplate.Trim().Trim(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }).Trim();
			var fileTemplate = template == Templates.Folder ? config.FileTemplate : t;
			fileTemplate = fileTemplate.Trim();
			var ext = config.DecryptToLossy ? "mp3" : "m4b";

			var path = Path.Combine(books, folderTemplate, $"{fileTemplate}.{ext}");

			// this logic should be external
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			var sing = $"{Path.DirectorySeparatorChar}";
			var dbl = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}";
			while (path.Contains(dbl))
				path = path.Replace(dbl, sing);

			// once path is finalized
			const char ZERO_WIDTH_SPACE = '\u200B';
			path = path.Replace(sing, $"{ZERO_WIDTH_SPACE}{sing}");
			// result: can wrap long paths. eg:
			// |-- LINE WRAP BOUNDARIES --|
			// \books\author with a very     <= normal line break on space between words
			// long name\narrator narrator   
			// \title                        <= line break on the zero-with space we added before slashes

			var book = new DataLayer.Book(
				new DataLayer.AudibleProductId("123456789"),
				"A Study in Scarlet: A Sherlock Holmes Novel",
				"Fake description",
				1234,
				DataLayer.ContentType.Product,
				new List<DataLayer.Contributor>
				{
					new("Arthur Conan Doyle"),
					new("Stephen Fry - introductions")
				},
				new List<DataLayer.Contributor> { new("Stephen Fry") },
				new DataLayer.Category(new DataLayer.AudibleCategoryId("cat12345"), "Mystery"),
				"us"
				);
			var libraryBook = new DataLayer.LibraryBook(book, DateTime.Now, "my account");

			outputTb.Text = @$"

Example:

{books}
{folderTemplate}
{fileTemplate}
{ext}
{path}

{book.AudibleProductId}
{book.Title}
{book.AuthorNames}
{book.NarratorNames}
series: {"Sherlock Holmes"}

{warnings}

".Trim();
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			if (!template.IsValid(templateTb.Text))
			{
				var errors = template
					.GetErrors(templateTb.Text)
					.Select(err => $"- {err}")
					.Aggregate((a, b) => $"{a}\r\n{b}");
				MessageBox.Show($"This template text is not valid. Errors:\r\n{errors}", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			TemplateText = templateTb.Text;

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
