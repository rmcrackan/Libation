using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class SetupDialog : Form
	{
		public bool IsNewUser { get; private set; }
		public bool IsReturningUser { get; private set; }

		public SetupDialog() => InitializeComponent();

		private void newUserBtn_Click(object sender, EventArgs e)
		{
			IsNewUser = true;

			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void returningUserBtn_Click(object sender, EventArgs e)
		{
			IsReturningUser = true;

			this.DialogResult = DialogResult.OK;
			Close();
		}
	}
}
