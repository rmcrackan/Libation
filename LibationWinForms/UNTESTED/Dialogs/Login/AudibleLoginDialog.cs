using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login
{
	public partial class AudibleLoginDialog : Form
	{
		public string Email { get; private set; }
		public string Password { get; private set; }

		public AudibleLoginDialog()
		{
			InitializeComponent();
		}

		private void submitBtn_Click(object sender, EventArgs e)
		{
			Email = this.emailTb.Text;
			Password = this.passwordTb.Text;
			DialogResult = DialogResult.OK;
		}
	}
}
