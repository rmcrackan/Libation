using LibationUiBase;
using LibationUiBase.Forms;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class BadBookActionDialog : Form
{
	public BadBookDialogResult Result { get; private set; } = new(LibationUiBase.Forms.DialogResult.Retry, false, false);

	public BadBookActionDialog()
	{
		InitializeComponent();
		this.SetLibationIcon();
	}

	public BadBookActionDialog(string message, string caption) : this()
	{
		Text = caption;
		messageLbl.Text = message;

		SizeChanged += (_, _) => AdjustLayout();
		Shown += (_, _) => AdjustLayout();
		messageLbl.SizeChanged += (_, _) => AdjustLayout();
		AdjustLayout();
	}

	private void AdjustLayout()
	{
		const int margin = 12;
		const int gap = 10;
		const int messageLeft = 50;

		var messageWidth = ClientSize.Width - messageLeft - margin;
		messageLbl.Left = messageLeft;
		messageLbl.Top = margin;
		messageLbl.Width = messageWidth;
		messageLbl.MaximumSize = new Size(messageWidth, 0);

		applyToAllCb.Top = messageLbl.Bottom + gap;
		rememberInSettingsCb.Top = applyToAllCb.Bottom + gap;
		buttonPanel.Top = rememberInSettingsCb.Bottom + gap;
		buttonPanel.Width = ClientSize.Width;

		var desiredHeight = buttonPanel.Bottom + margin;
		if (ClientSize.Height != desiredHeight)
			ClientSize = new Size(ClientSize.Width, desiredHeight);
	}

	private void CloseWith(LibationUiBase.Forms.DialogResult action)
	{
		Result = new BadBookDialogResult(action, applyToAllCb.Checked, rememberInSettingsCb.Checked);
		base.DialogResult = System.Windows.Forms.DialogResult.OK;
		Close();
	}

	private void AbortBtn_Click(object sender, EventArgs e)
		=> CloseWith(LibationUiBase.Forms.DialogResult.Abort);

	private void RetryBtn_Click(object sender, EventArgs e)
		=> CloseWith(LibationUiBase.Forms.DialogResult.Retry);

	private void IgnoreBtn_Click(object sender, EventArgs e)
		=> CloseWith(LibationUiBase.Forms.DialogResult.Ignore);
}
