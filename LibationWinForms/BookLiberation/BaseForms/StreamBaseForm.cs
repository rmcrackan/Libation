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
			//Will be null if set outside constructor.
			SyncContext = SynchronizationContext.Current;
		}

		protected IStreamable Streamable { get; private set; }
		public void SetStreamable(IStreamable streamable)
		{
			Streamable = streamable;

			if (Streamable is null) return;

			OnUnsubscribeAll(this, EventArgs.Empty);

			Streamable.StreamingBegin += ShowFormHandler;
			Streamable.StreamingBegin += OnStreamingBegin;
			Streamable.StreamingProgressChanged += OnStreamingProgressChanged;
			Streamable.StreamingTimeRemaining += OnStreamingTimeRemaining;
			Streamable.StreamingCompleted += OnStreamingCompleted;
			Streamable.StreamingCompleted += CloseFormHandler;

			FormClosed += OnUnsubscribeAll;
		}

		private void OnUnsubscribeAll(object sender, EventArgs e)
		{
			FormClosed -= OnUnsubscribeAll;

			Streamable.StreamingBegin -= ShowFormHandler;
			Streamable.StreamingBegin -= OnStreamingBegin;
			Streamable.StreamingProgressChanged -= OnStreamingProgressChanged;
			Streamable.StreamingTimeRemaining -= OnStreamingTimeRemaining;
			Streamable.StreamingCompleted -= OnStreamingCompleted;
			Streamable.StreamingCompleted -= CloseFormHandler;
		}

		private void ShowFormHandler(object sender, string beginString)
		{
			//If StreamingBegin is fired from a worker thread, the window will be created on
			//that UI thread. We need to make certain that we show the window on the same
			//thread that created form, otherwise the form and the window handle will be on
			//different threads. Form.BeginInvoke won't work until the form is created
			//(ie. shown) because control doesn't get a window handle until it is Shown.
			static void sendCallback(object asyncArgs)
			{
				var e = asyncArgs as AsyncCompletedEventArgs;
				((Action)e.UserState)();
			}

			Action show = Show;

			if (InvokeRequired)
				SyncContext.Send(
						sendCallback,
						new AsyncCompletedEventArgs(null, false, show));
			else
				show();
		}

		/// <summary>
		/// If the form was shown using Show (not ShowDialog), Form.Close calls Form.Dispose
		/// </summary>
		private void CloseFormHandler(object sender, string completedString) => this.UIThread(() => Close());


		#region IStreamable event handlers
		public virtual void OnStreamingBegin(object sender, string beginString) { }
		public virtual void OnStreamingProgressChanged(object sender, DownloadProgress downloadProgress) { }
		public virtual void OnStreamingTimeRemaining(object sender, TimeSpan timeRemaining) { }
		public virtual void OnStreamingCompleted(object sender, string completedString) { }
		#endregion
	}
}
