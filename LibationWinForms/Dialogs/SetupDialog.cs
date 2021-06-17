using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class SetupDialog : Form
	{
		public event EventHandler NoQuestionsBtn_Click
		{
			add => noQuestionsBtn.Click += value;
			remove => noQuestionsBtn.Click -= value;
		}

		public event EventHandler BasicBtn_Click
		{
			add => basicBtn.Click += value;
			remove => basicBtn.Click -= value;
		}

		public event EventHandler AdvancedBtn_Click
		{
			add => advancedBtn.Click += value;
			remove => advancedBtn.Click -= value;
		}

		public SetupDialog()
		{
			InitializeComponent();

			noQuestionsBtn.Click += btn_Click;
			basicBtn.Click += btn_Click;
			advancedBtn.Click += btn_Click;
		}

		private void btn_Click(object sender, EventArgs e) => Close();
	}
}
