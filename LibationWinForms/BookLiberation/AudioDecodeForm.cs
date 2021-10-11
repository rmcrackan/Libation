using System;
using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Threading;
using LibationFileManager;
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

		#region Processable event handler overrides
		public override void Processable_Begin(object sender, LibraryBook libraryBook)
		{
			base.Processable_Begin(sender, libraryBook);

			GetCoverArtDelegate = () => PictureStorage.GetPictureSynchronously(
						new PictureDefinition(
							libraryBook.Book.PictureId,
							PictureSize._500x500));

			//Set default values from library
			AudioDecodable_TitleDiscovered(sender, libraryBook.Book.Title);
			AudioDecodable_AuthorsDiscovered(sender, string.Join(", ", libraryBook.Book.Authors));
			AudioDecodable_NarratorsDiscovered(sender, string.Join(", ", libraryBook.Book.NarratorNames));
			AudioDecodable_CoverImageDiscovered(sender,
					PictureStorage.GetPicture(
						new PictureDefinition(
							libraryBook.Book.PictureId,
							PictureSize._80x80)).bytes);
		}
		#endregion

		#region Streamable event handler overrides
		public override void Streamable_StreamingProgressChanged(object sender, DownloadProgress downloadProgress)
		{
			base.Streamable_StreamingProgressChanged(sender, downloadProgress);
			if (!downloadProgress.ProgressPercentage.HasValue)
				return;

			if (downloadProgress.ProgressPercentage == 0)
				updateRemainingTime(0);
			else
				progressBar1.UIThreadAsync(() => progressBar1.Value = (int)downloadProgress.ProgressPercentage);
		}

		public override void Streamable_StreamingTimeRemaining(object sender, TimeSpan timeRemaining)
		{
			base.Streamable_StreamingTimeRemaining(sender, timeRemaining);
			updateRemainingTime((int)timeRemaining.TotalSeconds);
		}

		#endregion

		#region AudioDecodable event handlers
		public override void AudioDecodable_RequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate)
		{
			base.AudioDecodable_RequestCoverArt(sender, setCoverArtDelegate);
			setCoverArtDelegate(GetCoverArtDelegate?.Invoke());
		}

		public override void AudioDecodable_TitleDiscovered(object sender, string title)
		{
			base.AudioDecodable_TitleDiscovered(sender, title);
			this.UIThreadAsync(() => this.Text = DecodeActionName + " " + title);
			this.title = title;
			updateBookInfo();
		}

		public override void AudioDecodable_AuthorsDiscovered(object sender, string authors)
		{
			base.AudioDecodable_AuthorsDiscovered(sender, authors);
			authorNames = authors;
			updateBookInfo();
		}

		public override void AudioDecodable_NarratorsDiscovered(object sender, string narrators)
		{
			base.AudioDecodable_NarratorsDiscovered(sender, narrators);
			narratorNames = narrators;
			updateBookInfo();
		}

		public override void AudioDecodable_CoverImageDiscovered(object sender, byte[] coverArt)
		{
			base.AudioDecodable_CoverImageDiscovered(sender, coverArt);
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
