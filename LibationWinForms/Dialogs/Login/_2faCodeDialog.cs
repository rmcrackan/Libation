using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login
{
	public partial class _2faCodeDialog : Form
	{
		public string Code { get; private set; }

		public _2faCodeDialog()
		{
			InitializeComponent();
		}

		private void submitBtn_Click(object sender, EventArgs e)
		{
			Code = this.codeTb.Text.Trim();

			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { Code });

			DialogResult = DialogResult.OK;
		}
	}
}
