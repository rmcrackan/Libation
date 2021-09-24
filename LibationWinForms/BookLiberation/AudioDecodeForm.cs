using System;
using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Threading;
using LibationWinForms.BookLiberation.BaseForms;

namespace LibationWinForms.BookLiberation
{
	public partial class AudioDecodeForm : LiberationBaseForm
	{
		public virtual string DecodeActionName { get; } = "Decoding";
		public AudioDecodeForm() => InitializeComponent();

		private Func<byte[]> GetCoverArtDelegate;

		// book info
		private string title;
		private string authorNames;
		private string narratorNames;

		#region IProcessable event handler overrides
		public override void OnBegin(object sender, LibraryBook libraryBook)
		{
			base.OnBegin(sender, libraryBook);

			GetCoverArtDelegate = () => FileManager.PictureStorage.GetPictureSynchronously(
						new FileManager.PictureDefinition(
							libraryBook.Book.PictureId,
							FileManager.PictureSize._500x500));

			//Set default values from library
			OnTitleDiscovered(sender, libraryBook.Book.Title);
			OnAuthorsDiscovered(sender, string.Join(", ", libraryBook.Book.Authors));
			OnNarratorsDiscovered(sender, string.Join(", ", libraryBook.Book.NarratorNames));
			OnCoverImageDiscovered(sender,
					FileManager.PictureStorage.GetPicture(
						new FileManager.PictureDefinition(
							libraryBook.Book.PictureId,
							FileManager.PictureSize._80x80)).bytes);
		}
		#endregion

		#region IStreamable event handler overrides
		public override void OnStreamingProgressChanged(object sender, DownloadProgress downloadProgress)
		{
			base.OnStreamingProgressChanged(sender, downloadProgress);
			if (!downloadProgress.ProgressPercentage.HasValue)
				return;

			if (downloadProgress.ProgressPercentage == 0)
				updateRemainingTime(0);
			else
				progressBar1.UIThreadAsync(() => progressBar1.Value = (int)downloadProgress.ProgressPercentage);
		}

		public override void OnStreamingTimeRemaining(object sender, TimeSpan timeRemaining)
		{
			base.OnStreamingTimeRemaining(sender, timeRemaining);
			updateRemainingTime((int)timeRemaining.TotalSeconds);
		}

		#endregion

		#region IAudioDecodable event handlers
		public override void OnRequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate)
		{
			base.OnRequestCoverArt(sender, setCoverArtDelegate);
			setCoverArtDelegate(GetCoverArtDelegate?.Invoke());
		}

		public override void OnTitleDiscovered(object sender, string title)
		{
			base.OnTitleDiscovered(sender, title);
			this.UIThreadAsync(() => this.Text = DecodeActionName + " " + title);
			this.title = title;
			updateBookInfo();
		}

		public override void OnAuthorsDiscovered(object sender, string authors)
		{
			base.OnAuthorsDiscovered(sender, authors);
			authorNames = authors;
			updateBookInfo();
		}

		public override void OnNarratorsDiscovered(object sender, string narrators)
		{
			base.OnNarratorsDiscovered(sender, narrators);
			narratorNames = narrators;
			updateBookInfo();
		}

		public override void OnCoverImageDiscovered(object sender, byte[] coverArt)
		{
			base.OnCoverImageDiscovered(sender, coverArt);
			pictureBox1.UIThreadAsync(() => pictureBox1.Image = Dinah.Core.Drawing.ImageReader.ToImage(coverArt));
		}
		#endregion

		// thread-safe UI updates
		private void updateBookInfo()
			=> bookInfoLbl.UIThreadAsync(() => bookInfoLbl.Text = $"{title}\r\nBy {authorNames}\r\nNarrated by {narratorNames}");

		private void updateRemainingTime(int remaining)
			=> remainingTimeLbl.UIThreadAsync(() => remainingTimeLbl.Text = $"ETA:\r\n{remaining} sec");
	}
}
