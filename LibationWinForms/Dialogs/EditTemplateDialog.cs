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
		}

		private void resetToDefaultBtn_Click(object sender, EventArgs e) => templateTb.Text = template.DefaultTemplate;

		private void templateTb_TextChanged(object sender, EventArgs e)
		{
			var books = config.Books;
			var folderTemplate = template == Templates.Folder ? templateTb.Text : config.FolderTemplate;
			folderTemplate = folderTemplate.Trim().Trim(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }).Trim();
			var fileTemplate = template == Templates.Folder ? config.FileTemplate : templateTb.Text;
			fileTemplate = fileTemplate.Trim();
			var ext = config.DecryptToLossy ? "mp3" : "m4b";

			var path = Path.Combine(books, folderTemplate, $"{fileTemplate}.{ext}");

			// this logic should be external
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			var dbl = $"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}";
			while (path.Contains(dbl))
				path = path.Replace(dbl, $"{Path.DirectorySeparatorChar}");

			outputTb.Text = @$"
{books}
{folderTemplate}
{fileTemplate}
{ext}
{path}
";
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			if (!template.IsValid(templateTb.Text))
			{
				MessageBox.Show("This template text is not valid.", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
