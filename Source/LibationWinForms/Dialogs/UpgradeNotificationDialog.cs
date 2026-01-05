using AppScaffolding;
using Dinah.Core;
using LibationFileManager;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class UpgradeNotificationDialog : Form
	{
		private string PackageUrl { get; }

		public UpgradeNotificationDialog()
		{
			InitializeComponent();
			this.SetLibationIcon();
		}

		public UpgradeNotificationDialog(UpgradeProperties upgradeProperties) : this()
		{
			Text = $"Libation version {upgradeProperties.LatestRelease.ToVersionString()} is now available.";
			PackageUrl = upgradeProperties.ZipUrl;
			packageDlLink.Text = upgradeProperties.ZipName;
			releaseNotesTbox.Text = upgradeProperties.Notes;			

			Shown += (_, _) => yesBtn.Focus();
		}

		private void PackageDlLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
			=> Go.To.Url(PackageUrl);

		private void GoToGithub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
			=> Go.To.Url(LibationScaffolding.RepositoryUrl);

		private void GoToWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
			=> Go.To.Url(LibationScaffolding.WebsiteUrl);

		private void YesBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			Close();
		}

		private void DontRemindBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Ignore;
			Close();
		}

		private void NoBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
			Close();
		}
	}
}
