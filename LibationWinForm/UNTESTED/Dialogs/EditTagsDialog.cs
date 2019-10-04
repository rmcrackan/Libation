using System;
using System.Windows.Forms;

namespace LibationWinForm
{
    public partial class EditTagsDialog : Form
    {
        public string NewTags { get; private set; }

        public EditTagsDialog()
        {
            InitializeComponent();
        }
        public EditTagsDialog(string title, string rawTags) : this()
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
