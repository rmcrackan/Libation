using LibationSearchEngine;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class SearchSyntaxDialog : Form
{
	public event EventHandler<string>? TagDoubleClicked;
	public SearchSyntaxDialog()
	{
		InitializeComponent();

		lboxNumberFields.Items.AddRange(SearchEngine.FieldIndexRules.NumberFieldNames.ToArray());
		lboxStringFields.Items.AddRange(SearchEngine.FieldIndexRules.StringFieldNames.ToArray());
		lboxBoolFields.Items.AddRange(SearchEngine.FieldIndexRules.BoolFieldNames.ToArray());
		lboxIdFields.Items.AddRange(SearchEngine.FieldIndexRules.IdFieldNames.ToArray());
		this.SetLibationIcon();
		this.RestoreSizeAndLocation(LibationFileManager.Configuration.Instance);
	}
	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		base.OnFormClosing(e);
		this.SaveSizeAndLocation(LibationFileManager.Configuration.Instance);
	}

	private void lboxFields_DoubleClick(object sender, EventArgs e)
	{
		if (sender is ListBox { SelectedItem: string tagName })
		{
			TagDoubleClicked?.Invoke(this, tagName);
		}
	}
}
