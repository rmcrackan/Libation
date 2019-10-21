using System;
using System.Windows.Forms;

namespace LibationWinForm.Dialogs.Login
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
			Code = this.codeTb.Text;
			DialogResult = DialogResult.OK;
		}
	}
}
