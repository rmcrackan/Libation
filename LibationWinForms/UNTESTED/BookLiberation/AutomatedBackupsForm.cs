using System;
using System.Windows.Forms;
using Dinah.Core.Windows.Forms;

namespace LibationWinForms.BookLiberation
{
    public partial class AutomatedBackupsForm : Form
    {
        public bool KeepGoingVisible
        {
            get => keepGoingCb.Visible;
            set => keepGoingCb.Visible = value;
        }

        public bool KeepGoingChecked => keepGoingCb.Checked;

        public bool KeepGoing
            => keepGoingCb.Visible
            && keepGoingCb.Enabled
            && keepGoingCb.Checked;

        public AutomatedBackupsForm()
        {
            InitializeComponent();
        }

		public void AppendError(Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Automated backup: error");
			appendText("ERROR: " + ex.Message);
		}
		public void AppendText(string text)
		{
			Serilog.Log.Logger.Debug($"Automated backup: {text}");
			appendText(text);
		}
		private void appendText(string text)
			=> logTb.UIThread(() => logTb.AppendText($"{DateTime.Now} {text}{Environment.NewLine}"));

		public void FinalizeUI()
        {
            keepGoingCb.Enabled = false;
            logTb.AppendText("");
            AppendText("DONE");
        }

        private void AutomatedBackupsForm_FormClosing(object sender, FormClosingEventArgs e) => keepGoingCb.Checked = false;
    }
}
