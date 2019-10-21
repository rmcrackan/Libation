using System;
using System.Windows.Forms;
using ScrapingDomainServices;

namespace LibationWinForm.BookLiberation
{
    public partial class NoLongerAvailableForm : Form
    {
        public ScrapeBookDetails.NoLongerAvailableEnum EnumResult { get; private set; }

        public NoLongerAvailableForm(string title, string url) : this()
        {
            this.Text += ": " + title;
            this.label1.Text = string.Format(this.label1.Text, title);
            this.textBox1.Text = url;
        }
        public NoLongerAvailableForm() => InitializeComponent();

        private void missingBtn_Click(object sender, EventArgs e) => complete(ScrapeBookDetails.NoLongerAvailableEnum.MarkAsMissing);
        private void abortBtn_Click(object sender, EventArgs e) => complete(ScrapeBookDetails.NoLongerAvailableEnum.Abort);

        private void complete(ScrapeBookDetails.NoLongerAvailableEnum nlaEnum)
        {
            EnumResult = nlaEnum;
            Close();
        }
    }
}
