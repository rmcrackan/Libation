using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class TagsBatchDialog : Form
{
	public string? NewTags { get; private set; }

	public TagsBatchDialog()
	{
		InitializeComponent();
		this.SetLibationIcon();
	}

	private void saveBtn_Click(object sender, EventArgs e)
	{
		NewTags = this.newTagsTb.Text;
		this.DialogResult = DialogResult.OK;
	}

	private void cancelBtn_Click(object sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
		this.Close();
	}
}
