using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core;
using LibationFileManager;

namespace LibationWinForms.Dialogs
{
    public partial class LiberatedStatusBatchDialog : Form
    {
        public LiberatedStatus BookLiberatedStatus { get; private set; }

        public class liberatedComboBoxItem
        {
            public LiberatedStatus Status { get; set; }
            public string Text { get; set; }
            public override string ToString() => Text;
        }

        public LiberatedStatusBatchDialog()
        {
            InitializeComponent();
            this.SetLibationIcon();

            this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
            this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.NotLiberated, Text = "Not Downloaded" });

            this.bookLiberatedCb.SelectedIndex = 0;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            BookLiberatedStatus = ((liberatedComboBoxItem)this.bookLiberatedCb.SelectedItem).Status;
            this.DialogResult = DialogResult.OK;
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
