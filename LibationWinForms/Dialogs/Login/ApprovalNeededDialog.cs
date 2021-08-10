using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login
{
	public partial class ApprovalNeededDialog : Form
	{
		public ApprovalNeededDialog()
		{
			InitializeComponent();
		}

		private void approvedBtn_Click(object sender, EventArgs e)
		{
			Serilog.Log.Logger.Information("Submit button clicked");

			DialogResult = DialogResult.OK;
		}
	}
}
