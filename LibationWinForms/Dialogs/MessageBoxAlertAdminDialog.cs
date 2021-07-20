using System;
using System.Drawing;
using System.Windows.Forms;
using Dinah.Core;

namespace LibationWinForms.Dialogs
{
	public partial class MessageBoxAlertAdminDialog : Form
	{
		public MessageBoxAlertAdminDialog() => InitializeComponent();

		/// <summary>
		/// Displays a message box with specified text and caption.
		/// </summary>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="exception">Exception to display</param>
		public MessageBoxAlertAdminDialog(string text, string caption, Exception exception) : this()
		{
			this.descriptionLbl.Text = text;
			this.Text = caption;
			this.exceptionTb.Text = $"{exception.Message}\r\n\r\n{exception.StackTrace}";
		}

		private void MessageBoxAlertAdminDialog_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			System.Media.SystemSounds.Hand.Play();
			pictureBox1.Image = SystemIcons.Error.ToBitmap();
		}

		private void githubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
			=> Go.To.Url("https://github.com/rmcrackan/Libation/issues");

		private void logsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
			=> Go.To.Folder(FileManager.Configuration.Instance.LibationFiles);

		private void okBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
