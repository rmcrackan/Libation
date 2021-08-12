using Dinah.Core.Net.Http;
using FileLiberator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.BookLiberation
{
	public class StreamBaseForm : Form
	{
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
		public virtual void OnStreamingBegin(object sender, string beginString) => Show();
		public virtual void OnStreamingProgressChanged(object sender, DownloadProgress downloadProgress) { }
		public virtual void OnStreamingTimeRemaining(object sender, TimeSpan timeRemaining) { }
		public virtual void OnStreamingCompleted(object sender, string completedString)
		{
			Close();
			Dispose();
		}
		#endregion
	}
}
