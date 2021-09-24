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
		protected IStreamable Streamable { get; private set; }
		protected LogMe LogMe { get; private set; }
		private SynchronizeInvoker Invoker { get; init; } 

		public LiberationBaseForm()
		{
			//SynchronizationContext.Current will be null until the process contains a Form.
			//If this is the first form created, it will not exist until after execution
			//reaches inside the constructor (after base class has been initialized).
			Invoker = new SynchronizeInvoker();
		}

		public void RegisterFileLiberator(IStreamable streamable, LogMe logMe = null)
		{
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

			Disposed += UnsubscribeStreamable;
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
			Disposed -= UnsubscribeStreamable;

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

		#region IStreamable event handlers
		public virtual void OnStreamingBegin(object sender, string beginString)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IStreamable.StreamingBegin), Message = beginString });		
		public virtual void OnStreamingProgressChanged(object sender, DownloadProgress downloadProgress) { }
		public virtual void OnStreamingTimeRemaining(object sender, TimeSpan timeRemaining) { }
		public virtual void OnStreamingCompleted(object sender, string completedString)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IStreamable.StreamingCompleted), Message = completedString });
		
		#endregion

		#region IProcessable event handlers
		public virtual void OnBegin(object sender, LibraryBook libraryBook)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IProcessable.Begin), Book = libraryBook.LogFriendly() });		
		public virtual void OnStatusUpdate(object sender, string statusUpdate)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IProcessable.StatusUpdate), Status = statusUpdate });		
		public virtual void OnCompleted(object sender, LibraryBook libraryBook)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IProcessable.Completed), Book = libraryBook.LogFriendly() });
		
		#endregion

		#region IAudioDecodable event handlers
		public virtual void OnRequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IAudioDecodable.RequestCoverArt) });		
		public virtual void OnTitleDiscovered(object sender, string title)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IAudioDecodable.TitleDiscovered), Title = title });		
		public virtual void OnAuthorsDiscovered(object sender, string authors)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IAudioDecodable.AuthorsDiscovered), Authors = authors });		
		public virtual void OnNarratorsDiscovered(object sender, string narrators)
		=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IAudioDecodable.NarratorsDiscovered), Narrators = narrators });		
		public virtual void OnCoverImageDiscovered(object sender, byte[] coverArt)
			=> Serilog.Log.Logger.Debug("Event fired {@DebugInfo}", new { Name = nameof(IAudioDecodable.CoverImageDiscovered), CoverImageBytes = coverArt?.Length });
		#endregion
	}
}
