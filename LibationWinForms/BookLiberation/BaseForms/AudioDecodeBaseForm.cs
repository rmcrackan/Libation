using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Windows.Forms;
using FileLiberator;
using System;

namespace LibationWinForms.BookLiberation
{
	public partial class AudioDecodeBaseForm : ProcessBaseForm
	{
		public virtual string DecodeActionName { get; } = "Decoding";
		public AudioDecodeBaseForm() => InitializeComponent();

		private Func<byte[]> GetCoverArtDelegate;

		// book info
		private string title;
		private string authorNames;
		private string narratorNames;

		#region ProcessBaseForm overrides
		public override void SetProcessable(IStreamable streamProcessable, LogMe logMe)
		{
			base.SetProcessable(streamProcessable, logMe);

			if (Streamable is not null && Streamable is IAudioDecodable audioDecodable)
			{
				OnUnsubscribeAll(this, EventArgs.Empty);

				audioDecodable.RequestCoverArt += OnRequestCoverArt;
				audioDecodable.TitleDiscovered += OnTitleDiscovered;
				audioDecodable.AuthorsDiscovered += OnAuthorsDiscovered;
				audioDecodable.NarratorsDiscovered += OnNarratorsDiscovered;
				audioDecodable.CoverImageDiscovered += OnCoverImageDiscovered;

				Disposed += OnUnsubscribeAll;
			}
		}
		#endregion

		private void OnUnsubscribeAll(object sender, EventArgs e)
		{
			Disposed -= OnUnsubscribeAll;
			if (Streamable is IAudioDecodable audioDecodable)
			{
				audioDecodable.RequestCoverArt -= OnRequestCoverArt;
				audioDecodable.TitleDiscovered -= OnTitleDiscovered;
				audioDecodable.AuthorsDiscovered -= OnAuthorsDiscovered;
				audioDecodable.NarratorsDiscovered -= OnNarratorsDiscovered;
				audioDecodable.CoverImageDiscovered -= OnCoverImageDiscovered;

				audioDecodable.Cancel();
			}
		}

		#region IProcessable event handler overrides
		public override void OnBegin(object sender, LibraryBook libraryBook)
		{
			GetCoverArtDelegate = () => FileManager.PictureStorage.GetPictureSynchronously(
						new FileManager.PictureDefinition(
							libraryBook.Book.PictureId,
							FileManager.PictureSize._500x500));

			//Set default values from library
			OnTitleDiscovered(null, libraryBook.Book.Title);
			OnAuthorsDiscovered(null, string.Join(", ", libraryBook.Book.Authors));
			OnNarratorsDiscovered(null, string.Join(", ", libraryBook.Book.NarratorNames));
			OnCoverImageDiscovered(null,
					FileManager.PictureStorage.GetPicture(
						new FileManager.PictureDefinition(
							libraryBook.Book.PictureId,
							FileManager.PictureSize._80x80)).bytes);
		}
		#endregion

		#region IStreamable event handler overrides

		public override void OnStreamingProgressChanged(object sender, DownloadProgress downloadProgress)
		{
			if (!downloadProgress.ProgressPercentage.HasValue)
				return;

			if (downloadProgress.ProgressPercentage == 0)
				updateRemainingTime(0);
			else
				progressBar1.UIThread(() => progressBar1.Value = (int)downloadProgress.ProgressPercentage);
		}

		public override void OnStreamingTimeRemaining(object sender, TimeSpan timeRemaining)
			=> updateRemainingTime((int)timeRemaining.TotalSeconds);

		#endregion

		#region IAudioDecodable event handlers

		public virtual void OnRequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate)
			=> setCoverArtDelegate(GetCoverArtDelegate?.Invoke());

		public virtual void OnTitleDiscovered(object sender, string title)
		{
			this.UIThread(() => this.Text = DecodeActionName + " " + title);
			this.title = title;
			updateBookInfo();
		}

		public virtual void OnAuthorsDiscovered(object sender, string authors)
		{
			authorNames = authors;
			updateBookInfo();
		}

		public virtual void OnNarratorsDiscovered(object sender, string narrators)
		{
			narratorNames = narrators;
			updateBookInfo();
		}

		public virtual void OnCoverImageDiscovered(object sender, byte[] coverArt) 
			=> pictureBox1.UIThread(() => pictureBox1.Image = Dinah.Core.Drawing.ImageReader.ToImage(coverArt));

		#endregion


		// thread-safe UI updates
		private void updateBookInfo()
			=> bookInfoLbl.UIThread(() => bookInfoLbl.Text = $"{title}\r\nBy {authorNames}\r\nNarrated by {narratorNames}");

		private void updateRemainingTime(int remaining)
			=> remainingTimeLbl.UIThread(() => remainingTimeLbl.Text = $"ETA:\r\n{remaining} sec");

	}
}
