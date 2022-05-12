using System;
using System.Drawing;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Threading;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.BookLiberation;
using LibationWinForms.NewUI;

namespace LibationWinForms
{
	internal interface ILiberationBaseForm
	{
		Action CancelAction { get; set; }
		Func<QueuePosition?> MoveUpAction { get; set; }
		Func<QueuePosition?> MoveDownAction { get; set; }
		void SetResult(ProcessBookResult status);
		void SetQueuePosition(QueuePosition status);
		void RegisterFileLiberator(Processable streamable, LogMe logMe);
		void Processable_Begin(object sender, LibraryBook libraryBook);
		int Width { get; set; }
		int Height { get; set; }
		Padding Margin { get; set; }
	}

	internal partial class ProcessBookControl : UserControl, ILiberationBaseForm
	{
		public Action CancelAction { get; set; }
		public Func<QueuePosition?> MoveUpAction { get; set; }
		public Func<QueuePosition?> MoveDownAction { get; set; }
		public string DecodeActionName { get; } = "Decoding";
		private Func<byte[]> GetCoverArtDelegate;
		protected Processable Processable { get; private set; }
		protected LogMe LogMe { get; private set; }
		public ProcessBookControl()
		{
			InitializeComponent();
			label1.Text = "Queued";
			remainingTimeLbl.Visible = false;
			progressBar1.Visible = false;
		}

		public void SetResult(ProcessBookResult status)
		{
			var statusTxt = status switch
			{
				ProcessBookResult.Success => "Finished",
				ProcessBookResult.Cancelled => "Cancelled",
				ProcessBookResult.FailedRetry => "Error, Retry",
				ProcessBookResult.FailedSkip => "Error, Skip",
				ProcessBookResult.FailedAbort => "Error, Abort",
				_ => throw new NotImplementedException(),
			};

			Color backColor = status switch
			{
				ProcessBookResult.Success => Color.PaleGreen,
				ProcessBookResult.Cancelled => Color.Khaki,
				ProcessBookResult.FailedRetry => Color.LightCoral,
				ProcessBookResult.FailedSkip => Color.LightCoral,
				ProcessBookResult.FailedAbort => Color.Firebrick,
				_ => throw new NotImplementedException(),
			};

			this.UIThreadAsync(() =>
			{
				cancelBtn.Visible = false;
				moveDownBtn.Visible = false;
				moveUpBtn.Visible = false;
				remainingTimeLbl.Visible = false;
				progressBar1.Visible = false;
				label1.Text = statusTxt;
				BackColor = backColor;
			});
		}

		public ProcessBookControl(string title, Image cover) : this()
		{
			pictureBox1.Image = cover;
			bookInfoLbl.Text = title;
		}

		public void RegisterFileLiberator(Processable processable, LogMe logMe = null)
		{
			if (processable is null) return;

			Processable = processable;
			LogMe = logMe;

			Subscribe((Streamable)processable);
			Subscribe(processable);
			if (processable is AudioDecodable audioDecodable)
				Subscribe(audioDecodable);
		}


		#region Event Subscribers and Unsubscribers
		private void Subscribe(Streamable streamable)
		{
			UnsubscribeStreamable(this, EventArgs.Empty);

			streamable.StreamingProgressChanged += Streamable_StreamingProgressChanged;
			streamable.StreamingTimeRemaining += Streamable_StreamingTimeRemaining;

			Disposed += UnsubscribeStreamable;
		}
		private void Subscribe(Processable processable)
		{
			UnsubscribeProcessable(this, null);

			processable.Begin += Processable_Begin;
			processable.Completed += Processable_Completed;

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

			Processable.StreamingProgressChanged -= Streamable_StreamingProgressChanged;
			Processable.StreamingTimeRemaining -= Streamable_StreamingTimeRemaining;
		}
		private void UnsubscribeProcessable(object sender, LibraryBook e)
		{
			Processable.Completed -= UnsubscribeProcessable;
			Processable.Begin -= Processable_Begin;
			Processable.Completed -= Processable_Completed;
		}
		private void UnsubscribeAudioDecodable(object sender, EventArgs e)
		{
			if (Processable is not AudioDecodable audioDecodable)
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

		#region Streamable event handlers
		public void Streamable_StreamingProgressChanged(object sender, DownloadProgress downloadProgress)
		{
			if (!downloadProgress.ProgressPercentage.HasValue)
				return;

			if (downloadProgress.ProgressPercentage == 0)
				updateRemainingTime(0);
			else
				progressBar1.UIThreadAsync(() => progressBar1.Value = (int)downloadProgress.ProgressPercentage);
		}

		public void Streamable_StreamingTimeRemaining(object sender, TimeSpan timeRemaining)
		{
			updateRemainingTime((int)timeRemaining.TotalSeconds);
		}

		private void updateRemainingTime(int remaining)
			=> remainingTimeLbl.UIThreadAsync(() => remainingTimeLbl.Text = formatTime(remaining));

		private string formatTime(int seconds)
		{
			var timeSpan = TimeSpan.FromSeconds(seconds);
			return $"{timeSpan:mm\\:ss}";
		}

		#endregion

		#region Processable event handlers
		public void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			LogMe.Info($"{Environment.NewLine}{Processable.Name} Step, Begin: {libraryBook.Book}");

			this.UIThreadAsync(() =>
			{
				label1.Text = "ETA:";
				remainingTimeLbl.Visible = true;
				progressBar1.Visible = true;
			});

			GetCoverArtDelegate = () => PictureStorage.GetPictureSynchronously(
						new PictureDefinition(
							libraryBook.Book.PictureId,
							PictureSize._500x500));

			//Set default values from library
			AudioDecodable_TitleDiscovered(sender, libraryBook.Book.Title);
			AudioDecodable_AuthorsDiscovered(sender, libraryBook.Book.AuthorNames);
			AudioDecodable_NarratorsDiscovered(sender, libraryBook.Book.NarratorNames);
			AudioDecodable_CoverImageDiscovered(sender,
					PictureStorage.GetPicture(
						new PictureDefinition(
							libraryBook.Book.PictureId,
							PictureSize._80x80)).bytes);
		}

		public void Processable_Completed(object sender, LibraryBook libraryBook)
		{
			LogMe.Info($"{Processable.Name} Step, Completed: {libraryBook.Book}");
		}

		#endregion

		#region AudioDecodable event handlers

		private string title;
		private string authorNames;
		private string narratorNames;
		public void AudioDecodable_TitleDiscovered(object sender, string title)
		{
			this.UIThreadAsync(() => this.Text = DecodeActionName + " " + title);
			this.title = title;
			updateBookInfo();
		}

		public void AudioDecodable_AuthorsDiscovered(object sender, string authors)
		{
			authorNames = authors;
			updateBookInfo();
		}

		public void AudioDecodable_NarratorsDiscovered(object sender, string narrators)
		{
			narratorNames = narrators;
			updateBookInfo();
		}

		private void updateBookInfo()
			=> bookInfoLbl.UIThreadAsync(() => bookInfoLbl.Text = $"{title}\r\nBy {authorNames}\r\nNarrated by {narratorNames}");

		public void AudioDecodable_RequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate)
		{
			setCoverArtDelegate(GetCoverArtDelegate?.Invoke());
		}

		public void AudioDecodable_CoverImageDiscovered(object sender, byte[] coverArt)
		{
			pictureBox1.UIThreadAsync(() => pictureBox1.Image = Dinah.Core.Drawing.ImageReader.ToImage(coverArt));
		}
		#endregion

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			CancelAction?.Invoke();
		}

		private void moveUpBtn_Click(object sender, EventArgs e)
		{
			HandleMovePositionResult(MoveUpAction?.Invoke());
		}


		private void moveDownBtn_Click(object sender, EventArgs e)
		{
			HandleMovePositionResult(MoveDownAction?.Invoke());
		}

		private void HandleMovePositionResult(QueuePosition? result)
		{
			if (result.HasValue)
				SetQueuePosition(result.Value);
			else
				SetQueuePosition(QueuePosition.Absent);
		}

		public void SetQueuePosition(QueuePosition status)
		{
			if (status is QueuePosition.Absent or QueuePosition.Current)
			{
				moveUpBtn.Visible = false;
				moveDownBtn.Visible = false;
			}

			if (status == QueuePosition.Absent)
				cancelBtn.Enabled = false;

			moveUpBtn.Enabled = status != QueuePosition.Fisrt;
			moveDownBtn.Enabled = status != QueuePosition.Last;
		}
	}
}
