using System;
using System.Windows.Forms;
using Dinah.Core.Drawing;
using Dinah.Core.IO;
using Dinah.Core.Windows.Forms;

namespace LibationWinForm.BookLiberation
{
    public partial class DecryptForm : Form
    {
        public DecryptForm()
        {
            InitializeComponent();
        }

        System.IO.TextWriter origOut = Console.Out;
        private void DecryptForm_Load(object sender, EventArgs e)
        {
            // redirect Console.WriteLine to console, textbox
            System.IO.TextWriter origOut = Console.Out;
            var controlWriter = new RichTextBoxTextWriter(this.rtbLog);
            var multiLogger = new MultiTextWriter(origOut, controlWriter);
            Console.SetOut(multiLogger);
        }

        private void DecryptForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // restore original
            Console.SetOut(origOut);
        }

        // book info
        string title;
        string authorNames;
        string narratorNames;

        public void SetTitle(string title)
        {
            this.UIThread(() => this.Text = " Decrypting " + title);
            this.title = title;
            updateBookInfo();
        }
        public void SetAuthorNames(string authorNames)
        {
            this.authorNames = authorNames;
            updateBookInfo();
        }
        public void SetNarratorNames(string narratorNames)
        {
            this.narratorNames = narratorNames;
            updateBookInfo();
        }

        // thread-safe UI updates
        private void updateBookInfo()
            => bookInfoLbl.UIThread(() => bookInfoLbl.Text = $"{title}\r\nBy {authorNames}\r\nNarrated by {narratorNames}");

        public void SetCoverImage(byte[] coverBytes)
            => pictureBox1.UIThread(() => pictureBox1.Image = ImageReader.ToImage(coverBytes));

        public static void AppendError(Exception ex) => AppendText("ERROR: " + ex.Message);
        public static void AppendText(string text) =>
            // redirected to log textbox
            Console.WriteLine($"{DateTime.Now} {text}")
            //logTb.UIThread(() => logTb.AppendText($"{DateTime.Now} {text}{Environment.NewLine}"))
            ;

        public void UpdateProgress(int percentage) => progressBar1.UIThread(() => progressBar1.Value = percentage);
    }
}
