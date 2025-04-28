using LibationUiBase;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class AboutDialog : Form
	{
		public AboutDialog()
		{
			InitializeComponent();
			this.SetLibationIcon();
			releaseNotesLbl.Text = $"Libation {AppScaffolding.LibationScaffolding.Variety} v{AppScaffolding.LibationScaffolding.BuildVersion}";

			rmcrackanLbl.Tag = LibationContributor.PrimaryContributors.Single(c => c.Name == rmcrackanLbl.Text);
			MBucariLbl.Tag = LibationContributor.PrimaryContributors.Single(c => c.Name == MBucariLbl.Text);

			foreach (var contributor in LibationContributor.AdditionalContributors)
			{
				var label = new LinkLabel { Tag = contributor, Text = contributor.Name, AutoSize = true };
				label.LinkClicked += ContributorLabel_LinkClicked;
				flowLayoutPanel1.Controls.Add(label);
			}

			var toolTip = new ToolTip();
			toolTip.SetToolTip(releaseNotesLbl, "View Release Notes");
		}

		private void ContributorLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (sender is LinkLabel lbl && lbl.Tag is LibationContributor contributor)
			{
				Dinah.Core.Go.To.Url(contributor.Link.AbsoluteUri);
				e.Link.Visited = true;
			}
		}

		private void releaseNotesLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
			=> Dinah.Core.Go.To.Url($"{AppScaffolding.LibationScaffolding.RepositoryUrl}/releases/tag/v{AppScaffolding.LibationScaffolding.BuildVersion.ToString(3)}");

		private async void checkForUpgradeBtn_Click(object sender, EventArgs e)
		{
			var form1 = Owner as Form1;
			var upgrader = new Upgrader();
			upgrader.DownloadBegin += (_, _) => form1.Invoke(() => form1.upgradeLbl.Visible = form1.upgradePb.Visible = true);
			upgrader.DownloadProgress += (_, e) => form1.Invoke(() => form1.upgradePb.Value = int.Max(0, int.Min(100, (int)(e.ProgressPercentage ?? 0))));
			upgrader.DownloadCompleted += (_, _) => form1.Invoke(() => form1.upgradeLbl.Visible = form1.upgradePb.Visible = false);

			checkForUpgradeBtn.Enabled = false;
			Version latestVersion = null;
			await upgrader.CheckForUpgradeAsync(OnUpgradeAvailable);

			checkForUpgradeBtn.Enabled = latestVersion is null;

			checkForUpgradeBtn.Text = latestVersion is null ? "Libation is up to date. Check Again." : $"Version {latestVersion:3} is available";

			Task OnUpgradeAvailable(UpgradeEventArgs e)
			{
				var notificationResult = new UpgradeNotificationDialog(e.UpgradeProperties).ShowDialog(this);

				e.Ignore = notificationResult == DialogResult.Ignore;
				e.InstallUpgrade = notificationResult == DialogResult.Yes;
				latestVersion = e.UpgradeProperties.LatestRelease;

				return Task.CompletedTask;
			}
		}

		private void getLibationLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
			=> Dinah.Core.Go.To.Url(AppScaffolding.LibationScaffolding.WebsiteUrl);
	}
}
