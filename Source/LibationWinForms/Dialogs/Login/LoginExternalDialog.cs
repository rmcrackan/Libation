using AudibleUtilities;
using Dinah.Core;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login;

public partial class LoginExternalDialog : Form
{
	public string? ResponseUrl { get; private set; }

	public LoginExternalDialog(Account account, string loginUrl)
	{
		InitializeComponent();

		// do not allow user to change login id here. if they do then jsonpath will fail
		this.localeLbl.Text = string.Format(this.localeLbl.Text, account.Locale?.Name);
		this.usernameLbl.Text = string.Format(this.usernameLbl.Text, account.AccountId);

		this.loginUrlTb.Text = loginUrl;

		SizeChanged += (_, _) => AdjustInstructionAndResponseLayout();
		Shown += (_, _) => AdjustInstructionAndResponseLayout();
		instructionsLbl.SizeChanged += (_, _) => AdjustInstructionAndResponseLayout();
		AdjustInstructionAndResponseLayout();
	}

	private void AdjustInstructionAndResponseLayout()
	{
		const int margin = 14;
		const int gap = 8;

		tldrLbl.Left = margin;
		tldrLbl.Top = copyBtn.Bottom + gap;

		instructionsLbl.Left = margin;
		instructionsLbl.Top = tldrLbl.Bottom + gap;
		instructionsLbl.Width = ClientSize.Width - 2 * margin;
		instructionsLbl.MaximumSize = new System.Drawing.Size(ClientSize.Width - 2 * margin, 0);

		var responseTop = instructionsLbl.Bottom + gap;
		var responseBottom = submitBtn.Top - gap;
		var h = System.Math.Max(1, responseBottom - responseTop);
		responseUrlTb.SetBounds(margin, responseTop, ClientSize.Width - 2 * margin, h);
	}

	private void copyBtn_Click(object sender, EventArgs e) => Clipboard.SetText(this.loginUrlTb.Text);

	private void launchBrowserBtn_Click(object sender, EventArgs e) => Go.To.Url(this.loginUrlTb.Text);

	private void submitBtn_Click(object sender, EventArgs e)
	{
		ResponseUrl = this.responseUrlTb.Text?.Trim();

		Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { ResponseUrl });
		if (!Uri.TryCreate(ResponseUrl, UriKind.Absolute, out var result))
		{
			MessageBox.Show("Invalid response URL");
			return;
		}

		DialogResult = DialogResult.OK;
		// Close() not needed for AcceptButton
	}
}
