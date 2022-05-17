using System;
using System.Windows.Forms;
using Dinah.Core.Net.Http;
using Dinah.Core.Threading;
using FileLiberator;

namespace LibationWinForms.BookLiberation
{
	public partial class DownloadForm : Form
	{
		protected Streamable Streamable { get; private set; }
		protected LogMe LogMe { get; private set; }
		private SynchronizeInvoker Invoker { get; init; }

		public DownloadForm()
		{
			//SynchronizationContext.Current will be null until the process contains a Form.
			//If this is the first form created, it will not exist until after execution
			//reaches inside the constructor (after base class has been initialized).
			Invoker = new SynchronizeInvoker();
			InitializeComponent();

			this.SetLibationIcon();
			progressLbl.Text = "";
			filenameLbl.Text = "";
		}

		public void RegisterFileLiberator(Streamable streamable, LogMe logMe = null)
		{
			if (streamable is null) return;
			streamable.StreamingBegin += Streamable_StreamingBegin;
			streamable.StreamingProgressChanged += Streamable_StreamingProgressChanged;
			streamable.StreamingCompleted += (_, _) => this.UIThreadAsync(Close);
			Streamable = streamable;
			LogMe = logMe;
		}


		#region Streamable event handler overrides
		public void Streamable_StreamingBegin(object sender, string beginString)
		{
			Invoker.UIThreadAsync(Show);
			filenameLbl.UIThreadAsync(() => filenameLbl.Text = beginString);
		}
		public void Streamable_StreamingProgressChanged(object sender, DownloadProgress downloadProgress)
		{
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
