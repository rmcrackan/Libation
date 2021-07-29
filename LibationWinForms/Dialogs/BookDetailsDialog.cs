using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
    public partial class BookDetailsDialog : Form
    {
        public string NewTags { get; private set; }

        public BookDetailsDialog()
        {
            InitializeComponent();
        }
        public BookDetailsDialog(string title, string rawTags) : this()
        {
            this.Text = $"Edit Tags - {title}";

            this.newTagsTb.Text = rawTags;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            NewTags = this.newTagsTb.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
