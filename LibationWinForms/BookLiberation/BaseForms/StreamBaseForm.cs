using Dinah.Core.Net.Http;
using Dinah.Core.Windows.Forms;
using FileLiberator;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace LibationWinForms.BookLiberation
{
	public class StreamBaseForm : Form
	{
		private int InstanceThreadId { get; } = Thread.CurrentThread.ManagedThreadId;
		public new bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != InstanceThreadId;
		private SynchronizationContext SyncContext { get; } 

		public StreamBaseForm()
		{
			//Will not work if set outside constructor.
			SyncContext = SynchronizationContext.Current;
		}

		protected IStreamable Streamable { get; private set; }
		public void SetStreamable(IStreamable streamable)
		{
			Streamable = streamable;

			if (Streamable is null) return;

			OnUnsubscribeAll(this, EventArgs.Empty);

			Streamable.StreamingBegin += OnStreamingBegin;
			Streamable.StreamingCompleted += OnStreamingCompleted;
			Streamable.StreamingProgressChanged += OnStreamingProgressChanged;
			Streamable.StreamingTimeRemaining += OnStreamingTimeRemaining;

			Disposed += OnUnsubscribeAll;
		}

		private void OnUnsubscribeAll(object sender, EventArgs e)
		{
			Disposed -= OnUnsubscribeAll;

			Streamable.StreamingBegin -= OnStreamingBegin;
			Streamable.StreamingCompleted -= OnStreamingCompleted;
			Streamable.StreamingProgressChanged -= OnStreamingProgressChanged;
			Streamable.StreamingTimeRemaining -= OnStreamingTimeRemaining;
		}

		#region IStreamable event handlers
		public virtual void OnStreamingBegin(object sender, string beginString)
		{
			//If StreamingBegin is fired from a worker thread, the form will be created on
			//that UI thread. Form.BeginInvoke won't work until the form is created (ie. shown),
			//so we need to make certain that we show the form on the same thread that created
			//this StreamBaseForm.

			static void sendCallback(object asyncArgs)
			{
				var e = asyncArgs as AsyncCompletedEventArgs;
				((Action)e.UserState)();
			}

			Action code = Show;

			if (InvokeRequired)
			{
				var args = new AsyncCompletedEventArgs(null, false, code);
				SyncContext.Send(
						sendCallback,
						args);
			}
			else
			{
				code();
			}
		}
		public virtual void OnStreamingProgressChanged(object sender, DownloadProgress downloadProgress) { }
		public virtual void OnStreamingTimeRemaining(object sender, TimeSpan timeRemaining) { }
		public virtual void OnStreamingCompleted(object sender, string completedString)
			=> this.UIThread(() =>
			{
				Close();
				Dispose();
			});
		#endregion
	}
}
