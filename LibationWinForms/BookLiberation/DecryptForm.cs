using Dinah.Core.Windows.Forms;
using System;
using System.Windows.Forms;

namespace LibationWinForms.BookLiberation
{
	public partial class DecryptForm : Form
	{
		public DecryptForm() => InitializeComponent();

		// book info
		private string title;
		private string authorNames;
		private string narratorNames;

		public void SetTitle(string actionName, string title)
		{
			this.UIThread(() => this.Text = actionName + " " + title);
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

		public void SetCoverImage(System.Drawing.Image coverImage)
			=> pictureBox1.UIThread(() => pictureBox1.Image = coverImage);

		public void UpdateProgress(int percentage)
		{
			if (percentage == 0)
				updateRemainingTime(0);
			else
				progressBar1.UIThread(() => progressBar1.Value = percentage);
		}

		public void UpdateRemainingTime(TimeSpan remaining)
			=> updateRemainingTime((int)remaining.TotalSeconds);

		private void updateRemainingTime(int remaining)
			=> remainingTimeLbl.UIThread(() => remainingTimeLbl.Text = $"ETA:\r\n{remaining} sec");
	}
}
