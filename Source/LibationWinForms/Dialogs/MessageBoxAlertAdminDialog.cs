using Dinah.Core;
using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using FileManager;

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
			//This is a different (and newer) icon from SystemIcons.Error
			pictureBox1.Image = SystemIcons.GetStockIcon(StockIconId.Error).ToBitmap();
		}

		private void githubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var url = "https://github.com/rmcrackan/Libation/issues";
			try
			{
				Go.To.Url(url);
			}
			catch
			{
				MessageBox.Show(this, $"Error opening url\r\n{url}", "Error opening url", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void logsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				Go.To.File(LibationFileManager.LogFileFilter.LogFilePath);
			}
			catch
			{
				LongPath dir = "";
				try
				{
					dir = LibationFileManager.Configuration.Instance.LibationFiles.Location;
					Go.To.Folder(dir.ShortPathName);
				}
				catch
				{
					MessageBox.Show(this, $"Error opening folder\r\n{dir}", "Error opening folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}


		private void okBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
