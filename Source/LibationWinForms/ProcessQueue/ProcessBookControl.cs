using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Threading;
using FileLiberator;
using LibationFileManager;
using LibationWinForms.BookLiberation;
using LibationWinForms.ProcessQueue;

namespace LibationWinForms.ProcessQueue
{
	internal interface ILiberatiofffnBaseForm
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

	internal partial class ProcessBookControl : UserControl
	{
		public ProcessBookStatus Status { get; private set; } = ProcessBookStatus.Queued;
		public Action CancelAction { get; set; }
		public Func<QueuePositionRequest, QueuePosition?> RequestMoveAction { get; set; }
		public string DecodeActionName { get; } = "Decoding";
		private Func<byte[]> GetCoverArtDelegate;
		protected Processable Processable { get; private set; }
		protected LogMe LogMe { get; private set; }
		public ProcessBookControl()
		{
			InitializeComponent();
			statusLbl.Text = "Queued";
			remainingTimeLbl.Visible = false;
			progressBar1.Visible = false;
			etaLbl.Visible = false;
		}

		public void SetCover(Image cover)
		{
			pictureBox1.Image = cover;
		}
		public void SetTitle(string title)
		{
			bookInfoLbl.Text = title;
		}

		public void SetResult(ProcessBookResult result)
		{
			string statusText = default;
			switch (result)
			{
				case ProcessBookResult.Success:
					statusText = "Finished";
					Status = ProcessBookStatus.Completed;
					break;
				case ProcessBookResult.Cancelled:
					statusText = "Cancelled";
					Status = ProcessBookStatus.Cancelled;
					break;
				case ProcessBookResult.FailedRetry:
					statusText = "Queued";
					Status = ProcessBookStatus.Queued;
					break;
				case ProcessBookResult.FailedSkip:
					statusText = "Error, Skip";
					Status = ProcessBookStatus.Failed;
					break;
				case ProcessBookResult.FailedAbort:
					statusText = "Error, Abort";
					Status = ProcessBookStatus.Failed;
					break;
				case ProcessBookResult.ValidationFail:
					statusText = "Validate fail";
					Status = ProcessBookStatus.Failed;
					break;
				case ProcessBookResult.None:
					statusText = "UNKNOWN";
					Status = ProcessBookStatus.Failed;
					break;
			}

			SetStatus(Status, statusText);
		}

		public void SetStatus(ProcessBookStatus status, string statusText)
		{
			Color backColor = default;
			switch (status)
			{
				case ProcessBookStatus.Completed:
					backColor = Color.PaleGreen;
					Status = ProcessBookStatus.Completed;
					break;
				case ProcessBookStatus.Cancelled:
					backColor = Color.Khaki;
					Status = ProcessBookStatus.Cancelled;
					break;
				case ProcessBookStatus.Queued:
					backColor = SystemColors.Control;
					Status = ProcessBookStatus.Queued;
					break;
				case ProcessBookStatus.Working:
					backColor = SystemColors.Control;
					Status = ProcessBookStatus.Working;
					break;
				case ProcessBookStatus.Failed:
					backColor = Color.LightCoral;
					Status = ProcessBookStatus.Failed;
					break;
			}

			this.UIThreadAsync(() =>
			{
				SuspendLayout();

				cancelBtn.Visible = Status is ProcessBookStatus.Queued or ProcessBookStatus.Working;
				moveLastBtn.Visible = Status == ProcessBookStatus.Queued;
				moveDownBtn.Visible = Status == ProcessBookStatus.Queued;
				moveUpBtn.Visible = Status == ProcessBookStatus.Queued;
				moveFirstBtn.Visible = Status == ProcessBookStatus.Queued;
				remainingTimeLbl.Visible = Status == ProcessBookStatus.Working;
				progressBar1.Visible = Status == ProcessBookStatus.Working;
				etaLbl.Visible = Status == ProcessBookStatus.Working;
				statusLbl.Visible = Status != ProcessBookStatus.Working;
				statusLbl.Text = statusText;
				BackColor = backColor;

				if (status == ProcessBookStatus.Working)
				{
					bookInfoLbl.Width += moveLastBtn.Width + moveLastBtn.Padding.Left + moveLastBtn.Padding.Right;
				}
				else
				{
					bookInfoLbl.Width -= moveLastBtn.Width + moveLastBtn.Padding.Left + moveLastBtn.Padding.Right;

				}
				ResumeLayout();
			});
		}

		public ProcessBookControl(string title, Image cover) : this()
		{
			this.title = title;
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
			Status = ProcessBookStatus.Working;

			LogMe.Info($"{Environment.NewLine}{Processable.Name} Step, Begin: {libraryBook.Book}");

			SetStatus(ProcessBookStatus.Working, "");

			GetCoverArtDelegate = () => PictureStorage.GetPictureSynchronously(
						new PictureDefinition(
							libraryBook.Book.PictureId,
							PictureSize._500x500));

			//Set default values from library
			AudioDecodable_TitleDiscovered(sender, libraryBook.Book.Title);
			AudioDecodable_AuthorsDiscovered(sender, libraryBook.Book.AuthorNames());
			AudioDecodable_NarratorsDiscovered(sender, libraryBook.Book.NarratorNames());
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

		private void moveLastBtn_Click(object sender, EventArgs e)
		{
			RequestMoveAction?.Invoke(QueuePositionRequest.Last);
		}

		private void moveFirstBtn_Click(object sender, EventArgs e)
		{
			RequestMoveAction?.Invoke(QueuePositionRequest.Fisrt);
		}

		private void moveUpBtn_Click_1(object sender, EventArgs e)
		{
			RequestMoveAction?.Invoke(QueuePositionRequest.OneUp);
		}

		private void moveDownBtn_Click(object sender, EventArgs e)
		{
			RequestMoveAction?.Invoke(QueuePositionRequest.OneDown);
		}

		public override string ToString()
		{
			return title ?? "NO TITLE";
		}
	}
}
