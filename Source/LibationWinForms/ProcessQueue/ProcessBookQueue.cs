using DataLayer;
using Dinah.Core.Threading;
using LibationWinForms.BookLiberation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue
{	
	internal partial class ProcessBookQueue : UserControl, ILogForm
	{
		TrackedQueue<ProcessBook> Queue = new();
		private readonly LogMe Logger;


		public Task QueueRunner { get; private set; }
		public bool Running => !QueueRunner?.IsCompleted ?? false;

		public ToolStripButton popoutBtn = new();

		public ProcessBookQueue()
		{
			InitializeComponent();
			Logger = LogMe.RegisterForm(this);

			this.popoutBtn.DisplayStyle = ToolStripItemDisplayStyle.Text;
			this.popoutBtn.Name = "popoutBtn";
			this.popoutBtn.Text = "Pop Out";
			this.popoutBtn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.popoutBtn.Alignment = ToolStripItemAlignment.Right;
			this.popoutBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

			statusStrip1.Items.Add(popoutBtn);

			virtualFlowControl2.RequestData += VirtualFlowControl1_RequestData;

		}

		private void VirtualFlowControl1_RequestData(int firstIndex, int numVisible, IReadOnlyList<ProcessBookControl> panelsToFill)
		{
			int numToShow = Math.Min(numVisible, Queue.Count - firstIndex);
			for (int i = 0; i < numToShow; i++)
			{
				var proc = Queue[firstIndex + i];

				panelsToFill[i].SetCover(proc.Entry.Cover);
				panelsToFill[i].SetTitle(proc.Entry.Title);
			}
		}

		public async Task AddDownloadDecrypt(IEnumerable<GridEntry> entries)
		{
			SuspendLayout();
			foreach (var entry in entries)
				await AddDownloadDecryptAsync(entry);
			ResumeLayout();
		}
		int count = 0;
		public async Task AddDownloadDecryptAsync(GridEntry gridEntry)
		{
			//if (Queue.Any(b=> b?.Entry?.AudibleProductId == gridEntry.AudibleProductId))
				//return;

			ProcessBook pbook = new ProcessBook(gridEntry, Logger);
			pbook.Completed += Pbook_Completed;
			pbook.Cancelled += Pbook_Cancelled;
			pbook.RequestMove += (o,d) => RequestMove(o, d);

			var libStatus = gridEntry.Liberate;

			if (libStatus.BookStatus != LiberatedStatus.Liberated)
				pbook.AddDownloadDecryptProcessable();

			if (libStatus.PdfStatus != LiberatedStatus.Liberated)
				pbook.AddPdfProcessable();

			Queue.EnqueueBook(pbook);

			//await AddBookControlAsync(pbook.BookControl);
			count++;

			virtualFlowControl2.VirtualControlCount = count;

			if (!Running)
			{
				//QueueRunner = QueueLoop();
			}
			toolStripStatusLabel1.Text = count.ToString();
		}

		private async void Pbook_Cancelled(ProcessBook sender, EventArgs e)
		{
			Queue.Remove(sender);
			//await RemoveBookControlAsync(sender.BookControl);
		}

		/// <summary>
		/// Handles requests by <see cref="ProcessBook"/> to change its order in the queue
		/// </summary>
		/// <param name="sender">The requesting <see cref="ProcessBook"/></param>
		/// <param name="direction">The requested position</param>
		/// <returns>The resultant position</returns>
		private QueuePosition RequestMove(ProcessBook sender, QueuePositionRequest requested)
		{

			var direction = Queue.MoveQueuePosition(sender, requested);

			if (direction is QueuePosition.Absent or QueuePosition.Current or QueuePosition.Completed)
				return direction;
				return direction;

			/*

			var firstQueue = autosizeFlowLayout1.Controls.Cast<ProcessBookControl>().FirstOrDefault(c => c.Status == ProcessBookStatus.Queued);

			if (firstQueue is null) return QueuePosition.Current;

			int firstQueueIndex = autosizeFlowLayout1.Controls.IndexOf(firstQueue);

			var index = autosizeFlowLayout1.Controls.IndexOf(sender.BookControl);

			int newIndex = direction switch
			{
				QueuePosition.Fisrt => firstQueueIndex,
				QueuePosition.OneUp => index - 1,
				QueuePosition.OneDown => index + 1,
				QueuePosition.Last => autosizeFlowLayout1.Controls.Count - 1,
				_ => -1,
			};

			if (newIndex < 0) return direction;

			autosizeFlowLayout1.Controls.SetChildIndex(sender.BookControl, newIndex);

			return direction;
			*/
		}

		private async Task QueueLoop()
		{
			while (Queue.MoveNext())
			{
				var nextBook = Queue.Current;

				var result = await nextBook.ProcessOneAsync();

				switch (result)
				{
					case ProcessBookResult.FailedRetry:
						Queue.EnqueueBook(nextBook);
						break;
					case ProcessBookResult.FailedAbort:
						return;
				}
			}
		}



		private void Pbook_Completed(object sender, EventArgs e)
		{

		}

		private async void cancelAllBtn_Click(object sender, EventArgs e)
		{
			List<ProcessBook> l1 = Queue.QueuedItems();

			Queue.ClearQueue();
			Queue.Current?.Cancel();

			//await RemoveBookControlsAsync(l1.Select(l => l.BookControl));
		}

		private async void btnCleanFinished_Click(object sender, EventArgs e)
		{
			List<ProcessBook> l1 = Queue.CompletedItems();
			Queue.ClearCompleted();
			//await RemoveBookControlsAsync(l1.Select(l => l.BookControl));
		}

		private async Task AddBookControlAsync(ProcessBookControl control)
		{
			await Task.Run(() => Invoke(() =>
			{
				/*
				control.Width = autosizeFlowLayout1.DesiredBookControlWidth;
				autosizeFlowLayout1.Controls.Add(control);
				autosizeFlowLayout1.SetFlowBreak(control, true);
				*/
				//Refresh();
				//System.Threading.Thread.Sleep(1000);
			}));
		}
		
		private async Task RemoveBookControlAsync(ProcessBookControl control)
		{
			await Task.Run(() => Invoke(() =>
			{
				//autosizeFlowLayout1.Controls.Remove(control);
			}));
		}

		private async Task RemoveBookControlsAsync(IEnumerable<ProcessBookControl> control)
		{
			await Task.Run(() => Invoke(() =>
			{
				/*
				SuspendLayout();
				foreach (var l in control)
					autosizeFlowLayout1.Controls.Remove(l);
				ResumeLayout();
				*/
			}));
		}

		public void WriteLine(string text)
		{
			if (!IsDisposed)
				logMeTbox.UIThreadAsync(() => logMeTbox.AppendText($"{DateTime.Now} {text}{Environment.NewLine}"));
		}

		private void clearLogBtn_Click(object sender, EventArgs e)
		{
			logMeTbox.Clear();
		}

	}
}
