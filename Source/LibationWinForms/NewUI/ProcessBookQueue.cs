using DataLayer;
using Dinah.Core.Threading;
using LibationWinForms.BookLiberation;
using LibationWinForms.NewUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms
{	
	internal partial class ProcessBookQueue : UserControl, ILogForm
	{
		private ProcessBook CurrentBook;
		private readonly LinkedList<ProcessBook> BookQueue = new();
		private readonly List<ProcessBook> CompletedBooks = new();
		private readonly LogMe Logger;
		private readonly object lockObject = new();


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
		}
		public async Task AddDownloadDecrypt(IEnumerable<GridEntry> entries)
		{
			foreach (var entry in entries)
				await AddDownloadDecryptAsync(entry);
		}

		public async Task AddDownloadDecryptAsync(GridEntry gridEntry)
		{
			if (BookExists(gridEntry.LibraryBook))
				return;

			ProcessBook pbook = new ProcessBook(gridEntry, Logger);
			pbook.Completed += Pbook_Completed;
			pbook.Cancelled += Pbook_Cancelled;
			pbook.RequestMove += (o,d) => RequestMove(o, d);

			var libStatus = gridEntry.Liberate;

			if (libStatus.BookStatus != LiberatedStatus.Liberated)
				pbook.AddDownloadDecryptProcessable();

			if (libStatus.PdfStatus != LiberatedStatus.Liberated)
				pbook.AddPdfProcessable();

			EnqueueBook(pbook);

			await AddBookControlAsync(pbook.BookControl);

			if (!Running)
			{
				QueueRunner = QueueLoop();
			}
		}

		private async void Pbook_Cancelled(ProcessBook sender, EventArgs e)
		{
			lock (lockObject)
			{
				if (BookQueue.Contains(sender))
					BookQueue.Remove(sender);
			}
			await RemoveBookControlAsync(sender.BookControl);
		}

		/// <summary>
		/// Handles requests by <see cref="ProcessBook"/> to change its order in the queue
		/// </summary>
		/// <param name="sender">The requesting <see cref="ProcessBook"/></param>
		/// <param name="direction">The requested position</param>
		/// <returns>The resultant position</returns>
		private QueuePosition RequestMove(ProcessBook sender, QueuePosition direction)
		{
			var node = BookQueue.Find(sender);

			if (node == null || direction == QueuePosition.Absent)
				return QueuePosition.Absent;
			if (CurrentBook != null && CurrentBook == sender)
				return QueuePosition.Current;
			if ((direction == QueuePosition.Fisrt || direction == QueuePosition.OneUp) && BookQueue.First.Value == sender)
				return QueuePosition.Fisrt;
			if ((direction == QueuePosition.Last || direction == QueuePosition.OneDown) && BookQueue.Last.Value == sender)
				return QueuePosition.Last;

			if (direction == QueuePosition.OneUp)
			{
				var oneUp = node.Previous;
				BookQueue.Remove(node);
				BookQueue.AddBefore(oneUp, node.Value);
			}
			else if (direction == QueuePosition.OneDown)
			{
				var oneDown = node.Next;
				BookQueue.Remove(node);
				BookQueue.AddAfter(oneDown, node.Value);
			}
			else if (direction == QueuePosition.Fisrt)
			{
				BookQueue.Remove(node);
				BookQueue.AddFirst(node);
			}
			else
			{
				BookQueue.Remove(node);
				BookQueue.AddLast(node);
			}

			var index = flowLayoutPanel1.Controls.IndexOf((Control)sender.BookControl);

			index = direction switch
			{
				QueuePosition.Fisrt => 0,
				QueuePosition.OneUp => index - 1,
				QueuePosition.OneDown => index + 1,
				QueuePosition.Last => flowLayoutPanel1.Controls.Count - 1,
				_ => throw new NotImplementedException(),
			};

			flowLayoutPanel1.Controls.SetChildIndex((Control)sender.BookControl, index);

			if (index == 0) return QueuePosition.Fisrt;
			if (index == flowLayoutPanel1.Controls.Count - 1) return QueuePosition.Last;
			return direction;
		}

		private async Task QueueLoop()
		{
			while (MoreInQueue())
			{
				var nextBook = NextBook();
				nextBook.BookControl.SetQueuePosition(QueuePosition.Current);
				PeekBook()?.BookControl.SetQueuePosition(QueuePosition.Fisrt);

				var result = await nextBook.ProcessOneAsync();

				AddCompletedBook(nextBook);

				switch (result)
				{
					case ProcessBookResult.FailedRetry:
						EnqueueBook(nextBook);
						break;
					case ProcessBookResult.FailedAbort:
						return;
				}
			}
		}

		private bool BookExists(LibraryBook libraryBook)
		{
			lock (lockObject)
			{
				return CurrentBook?.Entry?.AudibleProductId == libraryBook.Book.AudibleProductId ||
					CompletedBooks.Union(BookQueue).Any(p => p.Entry.AudibleProductId == libraryBook.Book.AudibleProductId);
			}
		}

		private ProcessBook NextBook()
		{
			lock (lockObject)
			{
				CurrentBook = BookQueue.First.Value;
				BookQueue.RemoveFirst();
				return CurrentBook;
			}
		}
		private ProcessBook PeekBook()
		{
			lock (lockObject)
				return BookQueue.Count > 0 ? BookQueue.First.Value : default;
		}

		private void EnqueueBook(ProcessBook pbook)
		{
			lock (lockObject)
				BookQueue.AddLast(pbook);
		}
		
		private void AddCompletedBook(ProcessBook pbook)
		{
			lock (lockObject)
				CompletedBooks.Add(pbook);
		}

		private bool MoreInQueue()
		{
			lock (lockObject)
				return BookQueue.Count > 0;
		}


		private void Pbook_Completed(object sender, EventArgs e)
		{
			if (CurrentBook == sender)
				CurrentBook = default;
		}

		private async void cancelAllBtn_Click(object sender, EventArgs e)
		{
			List<ProcessBook> l1 = new();
			lock (lockObject)
			{
				l1.AddRange(BookQueue);
				BookQueue.Clear();
			}
			CurrentBook?.Cancel();
			CurrentBook = default;

			await RemoveBookControlsAsync(l1.Select(l => l.BookControl));
		}

		private async void btnCleanFinished_Click(object sender, EventArgs e)
		{
			List<ProcessBook> l1 = new();
			lock (lockObject)
			{
				l1.AddRange(CompletedBooks);
				CompletedBooks.Clear();
			}

			await RemoveBookControlsAsync(l1.Select(l => l.BookControl));
		}

		private async Task AddBookControlAsync(ILiberationBaseForm control)
		{
			await Task.Run(() => Invoke(() =>
			{
				SetBookControlWidth((Control)control);
				flowLayoutPanel1.Controls.Add((Control)control);
				flowLayoutPanel1.SetFlowBreak((Control)control, true);
				Refresh();
			}));
		}
		
		private async Task RemoveBookControlAsync(ILiberationBaseForm control)
		{
			await Task.Run(() => Invoke(() =>
			{
				flowLayoutPanel1.Controls.Remove((Control)control);
			}));
		}

		private async Task RemoveBookControlsAsync(IEnumerable<ILiberationBaseForm> control)
		{
			await Task.Run(() => Invoke(() =>
			{
				SuspendLayout();
				foreach (var l in control)
					flowLayoutPanel1.Controls.Remove((Control)l);
				ResumeLayout();
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

		[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		private static extern long GetWindowLongPtr(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		private static extern bool ShowScrollBar(IntPtr hWnd, SBOrientation bar, bool show);

		public const int WS_VSCROLL = 0x200000;
		public const int WS_HSCROLL  = 0x100000;
		enum SBOrientation : int
		{
			SB_HORZ = 0,
			SB_VERT = 1,
			SB_CTL = 2,
			SB_BOTH = 3
		}

		private void flowLayoutPanel1_ClientSizeChanged(object sender, EventArgs e)
		{
			ReorderControls();
		}

		private void flowLayoutPanel1_Layout(object sender, LayoutEventArgs e)
		{
			ReorderControls();
		}

		bool V_SHOWN = false;

		private void  ReorderControls()
		{
			bool hShown = (GetWindowLongPtr(flowLayoutPanel1.Handle, -16) & WS_HSCROLL) != 0;
			bool vShown = (GetWindowLongPtr(flowLayoutPanel1.Handle, -16) & WS_VSCROLL) != 0;

			if (hShown)
				ShowScrollBar(flowLayoutPanel1.Handle, SBOrientation.SB_HORZ, false);

			if (vShown != V_SHOWN)
			{
				flowLayoutPanel1.SuspendLayout();

				foreach (Control c in flowLayoutPanel1.Controls)
					SetBookControlWidth(c);

				flowLayoutPanel1.ResumeLayout();
				V_SHOWN = vShown;
			}
		}

		private void SetBookControlWidth(Control book)
		{
			book.Width = flowLayoutPanel1.ClientRectangle.Width - book.Margin.Left - book.Margin.Right;
		}

		private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
		{

		}
	}
}
