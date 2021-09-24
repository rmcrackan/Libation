using System;
using System.Windows.Forms;
using Dinah.Core.Net.Http;
using Dinah.Core.Threading;
using LibationWinForms.BookLiberation.BaseForms;

namespace LibationWinForms.BookLiberation
{
	public partial class DownloadForm : LiberationBaseForm
	{
		public DownloadForm()
		{
			InitializeComponent();

			progressLbl.Text = "";
			filenameLbl.Text = "";
		}


		#region IStreamable event handler overrides
		public override void OnStreamingBegin(object sender, string beginString)
		{
			base.OnStreamingBegin(sender, beginString);
			filenameLbl.UIThreadAsync(() => filenameLbl.Text = beginString);
		}
		public override void OnStreamingProgressChanged(object sender, DownloadProgress downloadProgress)
		{
			base.OnStreamingProgressChanged(sender, downloadProgress);
			// this won't happen with download file. it will happen with download string
			if (!downloadProgress.TotalBytesToReceive.HasValue || downloadProgress.TotalBytesToReceive.Value <= 0)
				return;

			progressLbl.UIThreadAsync(() => progressLbl.Text = $"{downloadProgress.BytesReceived:#,##0} of {downloadProgress.TotalBytesToReceive.Value:#,##0}");

			var d = double.Parse(downloadProgress.BytesReceived.ToString()) / double.Parse(downloadProgress.TotalBytesToReceive.Value.ToString()) * 100.0;
			var i = int.Parse(Math.Truncate(d).ToString());
			progressBar1.UIThreadAsync(() => progressBar1.Value = i);

			lastDownloadProgress = DateTime.Now;
		}
		#endregion

		#region timer
		private Timer timer { get; } = new Timer { Interval = 1000 };
		private void DownloadForm_Load(object sender, EventArgs e)
		{
			timer.Tick += new EventHandler(timer_Tick);
			timer.Start();
		}
		private DateTime lastDownloadProgress = DateTime.Now;
		private void timer_Tick(object sender, EventArgs e)
		{
			// if no update in the last 30 seconds, display frozen label
			lastUpdateLbl.UIThreadAsync(() => lastUpdateLbl.Visible = lastDownloadProgress.AddSeconds(30) < DateTime.Now);
			if (lastUpdateLbl.Visible)
			{
				var diff = DateTime.Now - lastDownloadProgress;
				var min = (int)diff.TotalMinutes;
				var minText = min > 0 ? $"{min}min " : "";

				lastUpdateLbl.UIThreadAsync(() => lastUpdateLbl.Text = $"Frozen? Last download activity: {minText}{diff.Seconds}sec ago");
			}
		}
		private void DownloadForm_FormClosing(object sender, FormClosingEventArgs e) => timer.Stop();
		#endregion
	}
}
