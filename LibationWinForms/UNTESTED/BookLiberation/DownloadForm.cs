using System;
using System.Windows.Forms;
using Dinah.Core.Windows.Forms;

namespace LibationWinForms.BookLiberation
{
    public partial class DownloadForm : Form
    {
        public DownloadForm()
        {
            InitializeComponent();

            progressLbl.Text = "";
            filenameLbl.Text = "";
        }

        // thread-safe UI updates
        public void UpdateFilename(string title) => filenameLbl.UIThread(() => filenameLbl.Text = title);

        public void DownloadProgressChanged(long BytesReceived, long TotalBytesToReceive)
        {
            // this won't happen with download file. it will happen with download string
            if (TotalBytesToReceive < 0)
                return;

            progressLbl.UIThread(() => progressLbl.Text = $"{BytesReceived:#,##0} of {TotalBytesToReceive:#,##0}");

            var d = double.Parse(BytesReceived.ToString()) / double.Parse(TotalBytesToReceive.ToString()) * 100.0;
            var i = int.Parse(Math.Truncate(d).ToString());
            progressBar1.UIThread(() => progressBar1.Value = i);

            lastDownloadProgress = DateTime.Now;
        }

        #region timer
        Timer timer = new Timer { Interval = 1000 };
        private void DownloadForm_Load(object sender, EventArgs e)
        {
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }
        DateTime lastDownloadProgress = DateTime.Now;
        private void timer_Tick(object sender, EventArgs e)
        {
            // if no update in the last 30 seconds, display frozen label
            lastUpdateLbl.Visible = lastDownloadProgress.AddSeconds(30) < DateTime.Now;
            if (lastUpdateLbl.Visible)
            {
                var diff = lastDownloadProgress - DateTime.Now;
                var min = (int)diff.TotalMinutes;
                var minText = min > 0 ? $"{min}min " : "";
                
                lastUpdateLbl.Text = $"Frozen? Last download activity: {minText}{diff.Seconds}sec ago";
            }
        }
        private void DownloadForm_FormClosing(object sender, FormClosingEventArgs e) => timer.Stop();
        #endregion
    }
}
