using Dinah.Core.Threading;
using LibationWinForms.BookLiberation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue
{
	internal partial class ProcessBookQueue : UserControl, ILogForm
	{
		private TrackedQueue<ProcessBook> Queue = new();
		private readonly LogMe Logger;

		private int QueuedCount
		{
			set
			{
				queueNumberLbl.Text = value.ToString();
				queueNumberLbl.Visible = value > 0;
			}
		}
		private int ErrorCount
		{
			set
			{
				errorNumberLbl.Text = value.ToString();
				errorNumberLbl.Visible = value > 0;
			}
		}

		private int CompletedCount
		{
			set
			{
				completedNumberLbl.Text = value.ToString();
				completedNumberLbl.Visible = value > 0;
			}
		}


		public Task QueueRunner { get; private set; }
		public bool Running => !QueueRunner?.IsCompleted ?? false;

		public ToolStripButton popoutBtn = new();

		private int FirstVisible = 0;
		private int NumVisible = 0;
		private IReadOnlyList<ProcessBookControl> Panels;

		public ProcessBookQueue()
		{
			InitializeComponent();
			Logger = LogMe.RegisterForm(this);

			popoutBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
			popoutBtn.Name = "popoutBtn";
			popoutBtn.Text = "Pop Out";
			popoutBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			popoutBtn.Alignment = ToolStripItemAlignment.Right;
			popoutBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			statusStrip1.Items.Add(popoutBtn);

			virtualFlowControl2.RequestData += VirtualFlowControl1_RequestData;
			virtualFlowControl2.ButtonClicked += VirtualFlowControl2_ButtonClicked;

			Queue.QueuededCountChanged += Queue_QueuededCountChanged;
			Queue.CompletedCountChanged += Queue_CompletedCountChanged;

			QueuedCount = 0;
			ErrorCount = 0;
			CompletedCount = 0;
		}

		private void Queue_CompletedCountChanged(object sender, int e)
		{
			int errCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.FailedAbort or ProcessBookResult.FailedSkip or ProcessBookResult.ValidationFail);
			int completeCount = Queue.Completed.Count(p => p.Result is ProcessBookResult.Success);

			ErrorCount = errCount;
			CompletedCount = completeCount;
			UpdateProgressBar();
		}
		private void Queue_QueuededCountChanged(object sender, int cueCount)
		{
			QueuedCount = cueCount;
			virtualFlowControl2.VirtualControlCount = Queue.Count;
			UpdateProgressBar();
		}
		private void UpdateProgressBar()
		{
			toolStripProgressBar1.Maximum = Queue.Count;
			toolStripProgressBar1.Value = Queue.Completed.Count;
		}
		private void VirtualFlowControl2_ButtonClicked(int itemIndex, string buttonName, ProcessBookControl panelClicked)
		{
			ProcessBook item = Queue[itemIndex];
			if (buttonName == "cancelBtn")
			{
				item.Cancel();
				Queue.RemoveQueued(item);
				virtualFlowControl2.VirtualControlCount = Queue.Count;
				UpdateControl(itemIndex);
			}
			else if (buttonName == "moveFirstBtn")
			{
				Queue.MoveQueuePosition(item, QueuePosition.Fisrt);
				UpdateAllControls();
			}
			else if (buttonName == "moveUpBtn")
			{
				Queue.MoveQueuePosition(item, QueuePosition.OneUp);
				UpdateControl(itemIndex - 1);
				UpdateControl(itemIndex);
			}
			else if (buttonName == "moveDownBtn")
			{
				Queue.MoveQueuePosition(item, QueuePosition.OneDown);
				UpdateControl(itemIndex + 1);
				UpdateControl(itemIndex);
			}
			else if (buttonName == "moveLastBtn")
			{
				Queue.MoveQueuePosition(item, QueuePosition.Last);
				UpdateAllControls();
			}
		}

		private void UpdateControl(int queueIndex)
		{
			int i = queueIndex - FirstVisible;

			if (i < 0 || i > NumVisible) return;

			var proc = Queue[queueIndex];

			Panels[i].Invoke(() =>
			{
				Panels[i].SuspendLayout();
				Panels[i].SetCover(proc.Cover);
				Panels[i].SetBookInfo(proc.BookText);

				if (proc.Result != ProcessBookResult.None)
				{
					Panels[i].SetResult(proc.Result);
					return;
				}

				Panels[i].SetStatus(proc.Status);
				Panels[i].SetProgrss(proc.Progress);
				Panels[i].SetRemainingTime(proc.TimeRemaining);
				Panels[i].ResumeLayout();
			});
		}

		private void UpdateAllControls()
		{
			int numToShow = Math.Min(NumVisible, Queue.Count - FirstVisible);

			for (int i = 0; i < numToShow; i++)
				UpdateControl(FirstVisible + i);
		}

		private void VirtualFlowControl1_RequestData(int firstIndex, int numVisible, IReadOnlyList<ProcessBookControl> panelsToFill)
		{
			FirstVisible = firstIndex;
			NumVisible = numVisible;
			Panels = panelsToFill;
			UpdateAllControls();
		}

		public void AddDownloadDecrypt(IEnumerable<GridEntry> entries)
		{
			foreach (var entry in entries)
				AddDownloadDecrypt(entry);
		}

		public void AddDownloadDecrypt(GridEntry gridEntry)
		{
			if (Queue.Any(b => b?.LibraryBook?.Book?.AudibleProductId == gridEntry.AudibleProductId))
				return;

			ProcessBook pbook = new(gridEntry.LibraryBook, gridEntry.Cover, Logger);
			pbook.DataAvailable += Pbook_DataAvailable;

			pbook.AddDownloadDecryptBook();
			pbook.AddDownloadPdf();

			Queue.Enqueue(pbook);

			if (!Running)
			{
				QueueRunner = QueueLoop();
			}
		}

		private void Pbook_DataAvailable(object sender, EventArgs e)
		{
			int index = Queue.IndexOf((ProcessBook)sender);
			UpdateControl(index);
		}

		private async Task QueueLoop()
		{
			while (Queue.MoveNext())
			{
				var nextBook = Queue.Current;

				var result = await nextBook.ProcessOneAsync();

				if (result == ProcessBookResult.FailedRetry)
					Queue.Enqueue(nextBook);
				else if (result == ProcessBookResult.FailedAbort)
					return;
			}

			Queue_CompletedCountChanged(this, 0);
		}

		private void cancelAllBtn_Click(object sender, EventArgs e)
		{
			Queue.ClearQueue();
			Queue.Current?.Cancel();
			virtualFlowControl2.VirtualControlCount = Queue.Count;
			UpdateAllControls();
		}

		private void btnCleanFinished_Click(object sender, EventArgs e)
		{
			Queue.ClearCompleted();
			virtualFlowControl2.VirtualControlCount = Queue.Count;
			UpdateAllControls();
		}

		private void clearLogBtn_Click(object sender, EventArgs e)
		{
			logMeTbox.Clear();
		}

		public void WriteLine(string text)
		{
			if (!IsDisposed)
				logMeTbox.UIThreadAsync(() => logMeTbox.AppendText($"{DateTime.Now} {text}{Environment.NewLine}"));
		}
	}
}
