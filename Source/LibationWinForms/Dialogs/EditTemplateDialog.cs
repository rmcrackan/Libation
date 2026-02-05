using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Dinah.Core;
using LibationFileManager;
using LibationFileManager.Templates;

namespace LibationWinForms.Dialogs;

public partial class EditTemplateDialog : Form
{
	private void resetTextBox(string? value) => this.templateTb.Text = value;
	private Configuration config { get; } = Configuration.Instance;
	private ITemplateEditor? templateEditor { get; }

	public EditTemplateDialog()
	{
		InitializeComponent();
		this.SetLibationIcon();
	}

	public EditTemplateDialog(ITemplateEditor templateEditor) : this()
	{
		this.templateEditor = ArgumentValidator.EnsureNotNull(templateEditor, nameof(templateEditor));
	}

	private void EditTemplateDialog_Load(object sender, EventArgs e)
	{
		if (this.DesignMode)
			return;

		if (templateEditor is null)
		{
			MessageBoxLib.ShowAdminAlert(this, $"Programming error. {nameof(EditTemplateDialog)} was not created correctly", "Edit template error", new NullReferenceException($"{nameof(templateEditor)} is null"));
			return;
		}

		warningsLbl.Text = "";

		this.Text = $"Edit {templateEditor.TemplateName}";

		this.templateLbl.Text = templateEditor.TemplateDescription;
		resetTextBox(templateEditor.EditingTemplate.TemplateText);

		// populate list view
		foreach (TemplateTags tag in templateEditor.EditingTemplate.TagsRegistered)
			listView1.Items.Add(new ListViewItem(new[] { tag.Display, tag.Description }) { Tag = tag.DefaultValue });

		listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
	}

	private void resetToDefaultBtn_Click(object sender, EventArgs e) => resetTextBox(templateEditor?.DefaultTemplate);

	private void templateTb_TextChanged(object sender, EventArgs e)
	{
		if (templateEditor is null)
			return;
		templateEditor.SetTemplateText(templateTb.Text);

		const char ZERO_WIDTH_SPACE = '\u200B';
		var sing = $"{Path.DirectorySeparatorChar}";

		// result: can wrap long paths. eg:
		// |-- LINE WRAP BOUNDARIES --|
		// \books\author with a very     <= normal line break on space between words
		// long name\narrator narrator   
		// \title                        <= line break on the zero-with space we added before slashes
		string? slashWrap(string? val) => val?.Replace(sing, $"{ZERO_WIDTH_SPACE}{sing}");

		warningsLbl.Text
			= !templateEditor.EditingTemplate.HasWarnings
			? ""
			: "Warning:\r\n" +
				templateEditor
				.EditingTemplate
				.Warnings
				.Select(err => $"- {err}")
				.Aggregate((a, b) => $"{a}\r\n{b}");

		var bold = new System.Drawing.Font(richTextBox1.Font, System.Drawing.FontStyle.Bold);
		var reg = new System.Drawing.Font(richTextBox1.Font, System.Drawing.FontStyle.Regular);

		richTextBox1.Clear();
		richTextBox1.SelectionFont = reg;

		if (!templateEditor.IsFilePath)
		{
			richTextBox1.SelectionFont = bold;
			richTextBox1.AppendText(templateEditor.GetName());
			return;
		}

		var folder = templateEditor.GetFolderName();
		var file = templateEditor.GetFileName();
		var ext = config.DecryptToLossy ? "mp3" : "m4b";

		richTextBox1.AppendText(slashWrap(templateEditor.BaseDirectory.PathWithoutPrefix));
		richTextBox1.AppendText(sing);

		if (templateEditor.IsFolder)
			richTextBox1.SelectionFont = bold;

		richTextBox1.AppendText(slashWrap(folder));

		if (templateEditor.IsFolder)
			richTextBox1.SelectionFont = reg;

		richTextBox1.AppendText(sing);

		if (templateEditor.IsFilePath && !templateEditor.IsFolder)
			richTextBox1.SelectionFont = bold;

		richTextBox1.AppendText(file);

		richTextBox1.SelectionFont = reg;
		richTextBox1.AppendText($".{ext}");
	}

	private void saveBtn_Click(object sender, EventArgs e)
	{
		if (templateEditor?.EditingTemplate.IsValid is true)
		{
			var errors = templateEditor
				.EditingTemplate
				.Errors
				.Select(err => $"- {err}")
				.Aggregate((a, b) => $"{a}\r\n{b}");
			MessageBox.Show($"This template text is not valid. Errors:\r\n{errors}", "Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		this.DialogResult = DialogResult.OK;
		this.Close();
	}

	private void cancelBtn_Click(object sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
		this.Close();
	}

	private void listView1_DoubleClick(object sender, EventArgs e)
	{
		var itemText = listView1.SelectedItems[0].Tag as string;

		if (string.IsNullOrEmpty(itemText)) return;

		var text = templateTb.Text;
		var selStart = Math.Min(Math.Max(0, templateTb.SelectionStart), text.Length);

		templateTb.Text = text.Insert(selStart, itemText);
		templateTb.SelectionStart = selStart + itemText.Length;
		templateTb.Focus();
	}

	private void llblGoToWiki_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		Go.To.Url(@"ht" + "tps://github.com/rmcrackan/Libation/blob/master/Documentation/NamingTemplates.md");
		e.Link?.Visited = true;
	}
}
