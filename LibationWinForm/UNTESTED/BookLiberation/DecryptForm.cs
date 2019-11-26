using System;
using System.Windows.Forms;
using Dinah.Core.Drawing;
using Dinah.Core.IO;
using Dinah.Core.Windows.Forms;

namespace LibationWinForm.BookLiberation
{
    public partial class DecryptForm : Form
    {
		class SerilogTextWriter : System.IO.TextWriter
		{
			public override System.Text.Encoding Encoding => System.Text.Encoding.ASCII;
			public override void WriteLine(string value) => Serilog.Log.Logger.Debug(value);
		}

		public DecryptForm()
        {
            InitializeComponent();
        }

        System.IO.TextWriter origOut { get; } = Console.Out;
        private void DecryptForm_Load(object sender, EventArgs e)
        {
            // redirect Console.WriteLine to console, textbox
            var controlWriter = new RichTextBoxTextWriter(this.rtbLog);
            var multiLogger = new MultiTextWriter(origOut, controlWriter, new SerilogTextWriter());
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

		public void UpdateProgress(int percentage) => progressBar1.UIThread(() => progressBar1.Value = percentage);
    }
}
