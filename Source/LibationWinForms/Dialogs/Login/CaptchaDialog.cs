using System;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login
{
	public partial class CaptchaDialog : Form
	{
		public string Answer { get; private set; }
		public string Password { get; private set; }

		private MemoryStream ms { get; }
		private Image image { get; }

		public CaptchaDialog() => InitializeComponent();
		public CaptchaDialog(string password, byte[] captchaImage) : this()
		{
			this.FormClosed += (_, __) => { ms?.Dispose(); image?.Dispose(); };

			ms = new MemoryStream(captchaImage);
			image = Image.FromStream(ms);
			this.captchaPb.Image = image;

			passwordTb.Text = password;

			(string.IsNullOrEmpty(password) ? passwordTb : answerTb).Select();
		}

		private void submitBtn_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(passwordTb.Text))
			{
				MessageBox.Show(this, "Please re-enter your password");
				return;
			}

			Answer = answerTb.Text;
			Password = passwordTb.Text;

			Serilog.Log.Logger.Information("Submit button clicked: {@DebugInfo}", new { Answer });

			DialogResult = DialogResult.OK;
			// Close() not needed for AcceptButton
		}
	}
}