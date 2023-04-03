using LibationUiBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
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
			var toolTip = new ToolTip();

			toolTip.SetToolTip(releaseNotesLbl, "View Release Notes");
			toolTip.SetToolTip(rmcrackanLbl, "View rmcrackan's GitHub profile");
			toolTip.SetToolTip(MBucariLbl, "View MBucari's GitHub profile");

			var asmNames = AppDomain.CurrentDomain.GetAssemblies().Select(a => new AssemblyName(a.FullName)).Where(a => a.Version.Major + a.Version.Minor + a.Version.Build + a.Version.Revision > 0).OrderBy(a => a.Name).ToList();

			listView1.Items.AddRange(asmNames.Select(a => new ListViewItem(new string[] { a.Name, a.Version.ToString() })).ToArray());
			listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			Resize += (_, _) => listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
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

		private void copyBtn_Click(object sender, EventArgs e)
		{
			var text = string.Join(Environment.NewLine, listView1.Items.OfType<ListViewItem>().Select(i => $"{i.SubItems[0].Text}\t{i.SubItems[1].Text}"));
			Clipboard.SetDataObject(text, false, 5, 150);
		}

		private void getLibationLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
			=> Dinah.Core.Go.To.Url(AppScaffolding.LibationScaffolding.WebsiteUrl);

		private void rmcrackanLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Dinah.Core.Go.To.Url($"ht" + "tps://github.com/rmcrackan");

		private void MBucariLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Dinah.Core.Go.To.Url($"ht" + "tps://github.com/MBucari");
	}
}
