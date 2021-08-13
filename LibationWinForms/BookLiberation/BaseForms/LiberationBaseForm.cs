using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Windows.Forms;
using FileLiberator;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace LibationWinForms.BookLiberation.BaseForms
{
	public abstract class LiberationBaseForm : Form
	{
		protected IFileLiberator FileLiberator { get; private set; }
		protected LogMe LogMe { get; private set; }

		private int InstanceThreadId { get; } = Thread.CurrentThread.ManagedThreadId;
		public new bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != InstanceThreadId;
		private SynchronizationContext SyncContext { get; }

		public LiberationBaseForm()
		{
			//Will be null if set outside constructor.
			SyncContext = SynchronizationContext.Current;
		}

		public void RegisterFileLiberator(IFileLiberator fileLiberator, LogMe logMe = null)
		{
			//IFileLiberator must at least be IStreamable, otherwise the Form won't ever Show()
			if (fileLiberator is null || fileLiberator is not IStreamable streamable) return;

			FileLiberator = fileLiberator;
			LogMe = logMe;

			Subscribe(streamable);

			if (FileLiberator is IProcessable processable)
				Subscribe(processable);
			if (FileLiberator is IAudioDecodable audioDecodable)
				Subscribe(audioDecodable);
		}

		#region Event Subscribers and Unsubscribers
		private void Subscribe(IStreamable streamable)
		{
			UnsubscribeStreamable(this, EventArgs.Empty);

			streamable.StreamingBegin += OnStreamingBeginShow;
			streamable.StreamingBegin += OnStreamingBegin;
			streamable.StreamingProgressChanged += OnStreamingProgressChanged;
			streamable.StreamingTimeRemaining += OnStreamingTimeRemaining;
			streamable.StreamingCompleted += OnStreamingCompleted;
			streamable.StreamingCompleted += OnStreamingCompletedClose;

			FormClosed += UnsubscribeStreamable;
		}
		private void Subscribe(IProcessable processable)
		{
			UnsubscribeProcessable(this, null);

			processable.Begin += OnBegin;
			processable.StatusUpdate += OnStatusUpdate;
			processable.Completed += OnCompleted;

			//The form is created on IProcessable.Begin and we
			//dispose of it on IProcessable.Completed
			processable.Completed += OnCompletedDispose;

			//Don't unsubscribe from Dispose because it fires when
			//IStreamable.StreamingCompleted closes the form, and
			//the IProcessable events need to live past that event.
			processable.Completed += UnsubscribeProcessable;
		}
		private void Subscribe(IAudioDecodable audioDecodable)
		{
			UnsubscribeAudioDecodable(this, EventArgs.Empty);

			audioDecodable.RequestCoverArt += OnRequestCoverArt;
			audioDecodable.TitleDiscovered += OnTitleDiscovered;
			audioDecodable.AuthorsDiscovered += OnAuthorsDiscovered;
			audioDecodable.NarratorsDiscovered += OnNarratorsDiscovered;
			audioDecodable.CoverImageDiscovered += OnCoverImageDiscovered;

			Disposed += UnsubscribeAudioDecodable;
		}
		private void UnsubscribeStreamable(object sender, EventArgs e)
		{
			if (FileLiberator is not IStreamable streamable)
				return;

			FormClosed -= UnsubscribeStreamable;

			streamable.StreamingBegin -= OnStreamingBeginShow;
			streamable.StreamingBegin -= OnStreamingBegin;
			streamable.StreamingProgressChanged -= OnStreamingProgressChanged;
			streamable.StreamingTimeRemaining -= OnStreamingTimeRemaining;
			streamable.StreamingCompleted -= OnStreamingCompleted;
			streamable.StreamingCompleted -= OnStreamingCompletedClose;
		}
		private void UnsubscribeProcessable(object sender, LibraryBook e)
		{
			if (FileLiberator is not IProcessable processable)
				return;

			processable.Completed -= UnsubscribeProcessable;
			processable.Completed -= OnCompletedDispose;
			processable.Completed -= OnCompleted;
			processable.StatusUpdate -= OnStatusUpdate;
			processable.Begin -= OnBegin;
		}
		private void UnsubscribeAudioDecodable(object sender, EventArgs e)
		{
			if (FileLiberator is not IAudioDecodable audioDecodable)
				return;

			Disposed -= UnsubscribeAudioDecodable;
			audioDecodable.RequestCoverArt -= OnRequestCoverArt;
			audioDecodable.TitleDiscovered -= OnTitleDiscovered;
			audioDecodable.AuthorsDiscovered -= OnAuthorsDiscovered;
			audioDecodable.NarratorsDiscovered -= OnNarratorsDiscovered;
			audioDecodable.CoverImageDiscovered -= OnCoverImageDiscovered;

			audioDecodable.Cancel();
		}
		#endregion

		#region Form creation and disposal handling

		/// <summary>
		/// If the form was shown using Show (not ShowDialog), Form.Close calls Form.Dispose
		/// </summary>
		private void OnStreamingCompletedClose(object sender, string completedString) => this.UIThread(() => Close());
		private void OnCompletedDispose(object sender, LibraryBook e) => this.UIThread(() => Dispose());

		/// <summary>
		/// If StreamingBegin is fired from a worker thread, the window will be created on
		/// that UI thread. We need to make certain that we show the window on the same
		/// thread that created form, otherwise the form and the window handle will be on
		/// different threads, and the renderer will be on a worker thread which could cause
		/// it to freeze. Form.BeginInvoke won't work until the form is created (ie. shown)
		/// because control doesn't get a window handle until it is Shown.
		/// </summary>
		private void OnStreamingBeginShow(object sender, string beginString)
		{
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
		#endregion

		#region IStreamable event handlers
		public virtual void OnStreamingBegin(object sender, string beginString) { }
		public virtual void OnStreamingProgressChanged(object sender, DownloadProgress downloadProgress) { }
		public virtual void OnStreamingTimeRemaining(object sender, TimeSpan timeRemaining) { }
		public virtual void OnStreamingCompleted(object sender, string completedString) { }
		#endregion

		#region IProcessable event handlers
		public virtual void OnBegin(object sender, LibraryBook libraryBook) { }
		public virtual void OnStatusUpdate(object sender, string statusUpdate) { }
		public virtual void OnCompleted(object sender, LibraryBook libraryBook) { }
		#endregion

		#region IAudioDecodable event handlers
		public virtual void OnRequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate) { }
		public virtual void OnTitleDiscovered(object sender, string title) { }
		public virtual void OnAuthorsDiscovered(object sender, string authors) { }
		public virtual void OnNarratorsDiscovered(object sender, string narrators) { }
		public virtual void OnCoverImageDiscovered(object sender, byte[] coverArt) { }
		#endregion
	}

	#region VS Design View Hack
	/// <summary>
	/// This class is a hack so that VS designer will work wif an abstract base class.
	/// https://stackoverflow.com/questions/1620847/how-can-i-get-visual-studio-2008-windows-forms-designer-to-render-a-form-that-im/2406058#2406058
	/// </summary>
	public class DebugIntermediate : LiberationBaseForm { }
	#endregion
}
