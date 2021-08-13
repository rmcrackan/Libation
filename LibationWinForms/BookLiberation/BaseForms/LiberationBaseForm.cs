using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Windows.Forms;
using FileLiberator;
using System;
using System.Windows.Forms;

namespace LibationWinForms.BookLiberation.BaseForms
{
	public class LiberationBaseForm : Form
	{
		protected IStreamable Streamable { get; private set; }
		protected LogMe LogMe { get; private set; }
		private CrossThreadSync<Action> FormSync { get; } = new CrossThreadSync<Action>();

		public LiberationBaseForm()
		{
			//SynchronizationContext.Current will be null until the process contains a Form.
			//If this is the first form created, it will not exist until after execution
			//reaches inside the constructor. So need to reset the context here.
			FormSync.ResetContext();
			FormSync.ObjectReceived += (_, action) => action();
		}

		public void RegisterFileLiberator(IStreamable streamable, LogMe logMe = null)
		{
			//IFileLiberator must at least be IStreamable, otherwise the Form won't ever Show()
			if (streamable is null) return;

			Streamable = streamable;
			LogMe = logMe;

			Subscribe(streamable);

			if (Streamable is IProcessable processable)
				Subscribe(processable);
			if (Streamable is IAudioDecodable audioDecodable)
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
			FormClosed -= UnsubscribeStreamable;

			Streamable.StreamingBegin -= OnStreamingBeginShow;
			Streamable.StreamingBegin -= OnStreamingBegin;
			Streamable.StreamingProgressChanged -= OnStreamingProgressChanged;
			Streamable.StreamingTimeRemaining -= OnStreamingTimeRemaining;
			Streamable.StreamingCompleted -= OnStreamingCompleted;
			Streamable.StreamingCompleted -= OnStreamingCompletedClose;
		}
		private void UnsubscribeProcessable(object sender, LibraryBook e)
		{
			if (Streamable is not IProcessable processable)
				return;

			processable.Completed -= UnsubscribeProcessable;
			processable.Completed -= OnCompletedDispose;
			processable.Completed -= OnCompleted;
			processable.StatusUpdate -= OnStatusUpdate;
			processable.Begin -= OnBegin;
		}
		private void UnsubscribeAudioDecodable(object sender, EventArgs e)
		{
			if (Streamable is not IAudioDecodable audioDecodable)
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
		/// If StreamingBegin is fired from a worker thread, the window will be created on that
		/// worker thread. We need to make certain that we show the window on the UI thread (same 
		/// thread that created form), otherwise the renderer will be on a worker thread which 
		/// could cause it to freeze. Form.BeginInvoke won't work until the form is created 
		/// (ie. shown) because Control doesn't get a window handle until it is Shown.
		/// </summary>
		private void OnStreamingBeginShow(object sender, string beginString) => FormSync.Send(Show);
		
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
}
