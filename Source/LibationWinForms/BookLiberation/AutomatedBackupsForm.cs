using System;
using System.Windows.Forms;
using Dinah.Core.Threading;

namespace LibationWinForms.BookLiberation
{
	public interface ILogForm
	{
		void WriteLine(string text);
	}
	public partial class AutomatedBackupsForm : Form, ILogForm
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
				logTb.UIThreadAsync(() => logTb.AppendText($"{DateTime.Now} {text}{Environment.NewLine}"));
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
