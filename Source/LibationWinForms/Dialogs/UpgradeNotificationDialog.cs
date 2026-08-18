using AppScaffolding;
using Dinah.Core;
using LibationFileManager;
using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class UpgradeNotificationDialog : Form
{
	private string? PackageUrl { get; }

	public UpgradeNotificationDialog()
	{
		InitializeComponent();
		this.SetLibationIcon();
	}

	public UpgradeNotificationDialog(UpgradeProperties upgradeProperties, bool canUpgrade = true, string? upgradeUnavailableSummary = null) : this()
	{
		Text = $"Libation version {upgradeProperties.LatestRelease.ToVersionString()} is now available.";
		PackageUrl = upgradeProperties.ZipUrl;
		packageDlLink.Text = upgradeProperties.ZipName;
		releaseNotesTbox.Text = upgradeProperties.Notes;

		if (!canUpgrade)
		{
			// Without a Yes button this is a notice, so it must stop asking a question that no
			// longer has an answer. Both labels take one line before the release notes box begins,
			// which is why this takes the one-line summary and not the full explanation.
			promptLbl.Text = "There is a new version available.";
			promptDetailLbl.Text = upgradeUnavailableSummary ?? "Libation cannot install this update itself. Use the download link below.";
			yesBtn.Visible = false;
			noBtn.Text = "OK";
		}

		Shown += (_, _) => (canUpgrade ? yesBtn : noBtn).Focus();
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
