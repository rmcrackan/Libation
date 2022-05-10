using System;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Threading;
using FileLiberator;

namespace LibationWinForms.BookLiberation.BaseForms
{
	public class LiberationBaseForm : Form
	{
		protected Streamable Streamable { get; private set; }
		protected LogMe LogMe { get; private set; }
		private SynchronizeInvoker Invoker { get; init; } 

		public LiberationBaseForm()
		{
			//SynchronizationContext.Current will be null until the process contains a Form.
			//If this is the first form created, it will not exist until after execution
			//reaches inside the constructor (after base class has been initialized).
			Invoker = new SynchronizeInvoker();
			this.SetLibationIcon();
		}

		public void RegisterFileLiberator(Streamable streamable, LogMe logMe = null)
		{
			if (streamable is null) return;

			Streamable = streamable;
			LogMe = logMe;

			Subscribe(streamable);

			if (Streamable is Processable processable)
				Subscribe(processable);
			if (Streamable is AudioDecodable audioDecodable)
				Subscribe(audioDecodable);
		}

		#region Event Subscribers and Unsubscribers
		private void Subscribe(Streamable streamable)
		{
			UnsubscribeStreamable(this, EventArgs.Empty);

			streamable.StreamingBegin += OnStreamingBeginShow;
			streamable.StreamingBegin += Streamable_StreamingBegin;
			streamable.StreamingProgressChanged += Streamable_StreamingProgressChanged;
			streamable.StreamingTimeRemaining += Streamable_StreamingTimeRemaining;
			streamable.StreamingCompleted += Streamable_StreamingCompleted;
			streamable.StreamingCompleted += OnStreamingCompletedClose;

			Disposed += UnsubscribeStreamable;
		}
		private void Subscribe(Processable processable)
		{
			UnsubscribeProcessable(this, null);

			processable.Begin += Processable_Begin;
			processable.StatusUpdate += Processable_StatusUpdate;
			processable.Completed += Processable_Completed;

			//The form is created on Processable.Begin and we
			//dispose of it on Processable.Completed
			processable.Completed += OnCompletedDispose;

			//Don't unsubscribe from Dispose because it fires when
			//Streamable.StreamingCompleted closes the form, and
			//the Processable events need to live past that event.
			processable.Completed += UnsubscribeProcessable;
		}
		private void Subscribe(AudioDecodable audioDecodable)
		{
			UnsubscribeAudioDecodable(this, EventArgs.Empty);

			audioDecodable.RequestCoverArt += AudioDecodable_RequestCoverArt;
			audioDecodable.TitleDiscovered += AudioDecodable_TitleDiscovered;
			audioDecodable.AuthorsDiscovered += AudioDecodable_AuthorsDiscovered;
			audioDecodable.NarratorsDiscovered += AudioDecodable_NarratorsDiscovered;
			audioDecodable.CoverImageDiscovered += AudioDecodable_CoverImageDiscovered;

			Disposed += UnsubscribeAudioDecodable;
		}
		private void UnsubscribeStreamable(object sender, EventArgs e)
		{
			Disposed -= UnsubscribeStreamable;

			Streamable.StreamingBegin -= OnStreamingBeginShow;
			Streamable.StreamingBegin -= Streamable_StreamingBegin;
			Streamable.StreamingProgressChanged -= Streamable_StreamingProgressChanged;
			Streamable.StreamingTimeRemaining -= Streamable_StreamingTimeRemaining;
			Streamable.StreamingCompleted -= Streamable_StreamingCompleted;
			Streamable.StreamingCompleted -= OnStreamingCompletedClose;
		}
		private void UnsubscribeProcessable(object sender, LibraryBook e)
		{
			if (Streamable is not Processable processable)
				return;

			processable.Completed -= UnsubscribeProcessable;
			processable.Completed -= OnCompletedDispose;
			processable.Completed -= Processable_Completed;
			processable.StatusUpdate -= Processable_StatusUpdate;
			processable.Begin -= Processable_Begin;
		}
		private void UnsubscribeAudioDecodable(object sender, EventArgs e)
		{
			if (Streamable is not AudioDecodable audioDecodable)
				return;

			Disposed -= UnsubscribeAudioDecodable;
			audioDecodable.RequestCoverArt -= AudioDecodable_RequestCoverArt;
			audioDecodable.TitleDiscovered -= AudioDecodable_TitleDiscovered;
			audioDecodable.AuthorsDiscovered -= AudioDecodable_AuthorsDiscovered;
			audioDecodable.NarratorsDiscovered -= AudioDecodable_NarratorsDiscovered;
			audioDecodable.CoverImageDiscovered -= AudioDecodable_CoverImageDiscovered;

			audioDecodable.Cancel();
		}
		#endregion

		#region Form creation and disposal handling

		/// <summary>
		/// If the form was shown using Show (not ShowDialog), Form.Close calls Form.Dispose
		/// </summary>
		private void OnStreamingCompletedClose(object sender, string completedString) => this.UIThreadAsync(Close);
		private void OnCompletedDispose(object sender, LibraryBook e) => this.UIThreadAsync(Dispose);

		/// <summary>
		/// If StreamingBegin is fired from a worker thread, the window will be created on that
		/// worker thread. We need to make certain that we show the window on the UI thread (same 
		/// thread that created form), otherwise the renderer will be on a worker thread which 
		/// could cause it to freeze. Form.BeginInvoke won't work until the form is created 
		/// (ie. shown) because Control doesn't get a window handle until it is Shown.
		/// </summary>
		private void OnStreamingBeginShow(object sender, string beginString) => Invoker.UIThreadAsync(Show);

		#endregion

		#region Streamable event handlers
		public virtual void Streamable_StreamingBegin(object sender, string beginString) { }
		public virtual void Streamable_StreamingProgressChanged(object sender, DownloadProgress downloadProgress) { }
		public virtual void Streamable_StreamingTimeRemaining(object sender, TimeSpan timeRemaining) { }
		public virtual void Streamable_StreamingCompleted(object sender, string completedString) { }

		#endregion

		#region Processable event handlers
		public virtual void Processable_Begin(object sender, LibraryBook libraryBook) { }
		public virtual void Processable_StatusUpdate(object sender, string statusUpdate) { }
		public virtual void Processable_Completed(object sender, LibraryBook libraryBook) { }

		#endregion

		#region AudioDecodable event handlers
		public virtual void AudioDecodable_TitleDiscovered(object sender, string title) { }
		public virtual void AudioDecodable_AuthorsDiscovered(object sender, string authors) { }
		public virtual void AudioDecodable_NarratorsDiscovered(object sender, string narrators) { }

		public virtual void AudioDecodable_CoverImageDiscovered(object sender, byte[] coverArt) { }
		public virtual void AudioDecodable_RequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate) { }
		#endregion
	}
}
