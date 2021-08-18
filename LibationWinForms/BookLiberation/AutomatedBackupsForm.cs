using Dinah.Core.Windows.Forms;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.BookLiberation
{
	public partial class AutomatedBackupsForm : Form
	{
		public bool KeepGoingChecked => keepGoingCb.Checked;

		public bool KeepGoing => keepGoingCb.Enabled && keepGoingCb.Checked;

		public AutomatedBackupsForm()
		{
			InitializeComponent();
		}

		public void WriteLine(string text)
		{
			if (!IsDisposed)
				logTb.UIThread(() => logTb.AppendText($"{DateTime.Now} {text}{Environment.NewLine}"));
		}

		public void FinalizeUI()
		{
			keepGoingCb.Enabled = false;

			if (!IsDisposed)
				logTb.AppendText("");
		}

		private void AutomatedBackupsForm_FormClosing(object sender, FormClosingEventArgs e) => keepGoingCb.Checked = false;
	}
}
