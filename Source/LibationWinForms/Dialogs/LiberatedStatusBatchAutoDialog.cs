using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
    public partial class LiberatedStatusBatchAutoDialog : Form
    {
        public bool SetDownloaded { get; private set; }
        public bool SetNotDownloaded { get; private set; }

        public LiberatedStatusBatchAutoDialog()
        {
            InitializeComponent();
            this.SetLibationIcon();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            SetDownloaded = this.setDownloadedCb.Checked;
            SetNotDownloaded = this.setNotDownloadedCb.Checked;

            this.DialogResult = DialogResult.OK;
        }
    }
}
