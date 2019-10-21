using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LibationWinForm.Dialogs.Login
{
	public partial class CaptchaDialog : Form
	{
		public string Answer { get; private set; }

		private MemoryStream ms { get; }
		private Image image { get; }

		public CaptchaDialog(byte[] captchaImage)
		{
			InitializeComponent();

			this.FormClosed += (_, __) => { ms?.Dispose(); image?.Dispose(); };

			ms = new MemoryStream(captchaImage);
			image = Image.FromStream(ms);
			this.captchaPb.Image = image;

			var h1 = captchaPb.Height;
			var w1 = captchaPb.Width;

			var h2 = captchaPb.Image.Height;
			var w2 = captchaPb.Image.Width;
		}

		private void submitBtn_Click(object sender, EventArgs e)
		{
			Answer = this.answerTb.Text;
			DialogResult = DialogResult.OK;
		}
	}
}