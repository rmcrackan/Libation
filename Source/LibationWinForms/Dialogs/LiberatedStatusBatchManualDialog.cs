using System;
using System.Windows.Forms;
using DataLayer;

namespace LibationWinForms.Dialogs
{
    public partial class LiberatedStatusBatchManualDialog : Form
    {
        public LiberatedStatus BookLiberatedStatus { get; private set; }

        public class liberatedComboBoxItem
        {
            public LiberatedStatus Status { get; set; }
            public string? Text { get; set; }
            public override string? ToString() => Text;
        }

        public LiberatedStatusBatchManualDialog(bool isPdf) : this()
        {
            if (isPdf)
                this.Text = this.Text.Replace("book", "PDF");
        }

        public LiberatedStatusBatchManualDialog()
        {
            InitializeComponent();
            this.SetLibationIcon();

            this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
            this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.NotLiberated, Text = "Not Downloaded" });

            this.bookLiberatedCb.SelectedIndex = 0;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            if (bookLiberatedCb.SelectedItem is liberatedComboBoxItem item)
                BookLiberatedStatus = item.Status;
            this.DialogResult = DialogResult.OK;
        }
    }
}
