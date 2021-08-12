using DataLayer;
using Dinah.Core.Net.Http;
using Dinah.Core.Windows.Forms;
using LibationWinForms.BookLiberation.BaseForms;
using System;

namespace LibationWinForms.BookLiberation
{
	public partial class AudioDecodeForm
#if DEBUG
		: DebugIntermediate
#else
		: LiberationBaseForm
#endif
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
		public override void OnStreamingBegin(object sender, string beginString) { }
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

		public override void OnStreamingCompleted(object sender, string completedString) { }
		#endregion

		#region IAudioDecodable event handlers
		public override void OnRequestCoverArt(object sender, Action<byte[]> setCoverArtDelegate)
			=> setCoverArtDelegate(GetCoverArtDelegate?.Invoke());

		public override void OnTitleDiscovered(object sender, string title)
		{
			this.UIThread(() => this.Text = DecodeActionName + " " + title);
			this.title = title;
			updateBookInfo();
		}

		public override void OnAuthorsDiscovered(object sender, string authors)
		{
			authorNames = authors;
			updateBookInfo();
		}

		public override void OnNarratorsDiscovered(object sender, string narrators)
		{
			narratorNames = narrators;
			updateBookInfo();
		}

		public override void OnCoverImageDiscovered(object sender, byte[] coverArt) 
			=> pictureBox1.UIThread(() => pictureBox1.Image = Dinah.Core.Drawing.ImageReader.ToImage(coverArt));
		#endregion

		// thread-safe UI updates
		private void updateBookInfo()
			=> bookInfoLbl.UIThread(() => bookInfoLbl.Text = $"{title}\r\nBy {authorNames}\r\nNarrated by {narratorNames}");

		private void updateRemainingTime(int remaining)
			=> remainingTimeLbl.UIThread(() => remainingTimeLbl.Text = $"ETA:\r\n{remaining} sec");
	}
}
