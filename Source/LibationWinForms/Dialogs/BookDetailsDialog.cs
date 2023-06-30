using System;
using System.Linq;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core;
using LibationFileManager;

namespace LibationWinForms.Dialogs
{
	public partial class BookDetailsDialog : Form
	{
		public class liberatedComboBoxItem
		{
			public LiberatedStatus Status { get; set; }
			public string Text { get; set; }
			public override string ToString() => Text;
		}

		public string NewTags { get; private set; }
		public LiberatedStatus BookLiberatedStatus { get; private set; }
		public LiberatedStatus? PdfLiberatedStatus { get; private set; }

		private LibraryBook _libraryBook { get; }
		private Book Book => _libraryBook.Book;

		public BookDetailsDialog()
		{
			InitializeComponent();
			this.SetLibationIcon();
		}
		public BookDetailsDialog(LibraryBook libraryBook) : this()
		{
			_libraryBook = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook));
			initDetails();
			initTags();
			initLiberated();
		}
		// 1st draft: lazily cribbed from GridEntry.ctor()
		private void initDetails()
		{
			this.Text = Book.TitleWithSubtitle;

			(_, var picture) = PictureStorage.GetPicture(new PictureDefinition(Book.PictureId, PictureSize._80x80));
			this.coverPb.Image = WinFormsUtil.TryLoadImageOrDefault(picture, PictureSize._80x80);

			var title = string.IsNullOrEmpty(Book.Subtitle) ? Book.Title : $"{Book.Title}\r\n        {Book.Subtitle}";
			var t = @$"
Title: {title}
Author(s): {Book.AuthorNames()}
Narrator(s): {Book.NarratorNames()}
Length: {(Book.LengthInMinutes == 0 ? "" : $"{Book.LengthInMinutes / 60} hr {Book.LengthInMinutes % 60} min")}
Audio Bitrate: {Book.AudioFormat}
Category: {string.Join(" > ", Book.CategoriesNames())}
Purchase Date: {_libraryBook.DateAdded:d}
Language: {Book.Language}
Audible ID: {Book.AudibleProductId}
".Trim();

			var seriesNames = Book.SeriesNames();
			if (!string.IsNullOrWhiteSpace(seriesNames))
				t += $"\r\nSeries: {seriesNames}";

			var bookRating = Book.Rating?.ToStarString();
			if (!string.IsNullOrWhiteSpace(bookRating))
				t += $"\r\nBook Rating:\r\n{bookRating}";

			var myRating = Book.UserDefinedItem.Rating?.ToStarString();
			if (!string.IsNullOrWhiteSpace(myRating))
				t += $"\r\nMy Rating:\r\n{myRating}";

			this.detailsTb.Text = t;
		}
		private void initTags() => this.newTagsTb.Text = Book.UserDefinedItem.Tags;
		private void initLiberated()
		{
			{
				var status = Book.UserDefinedItem.BookStatus;

				this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
				this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.NotLiberated, Text = "Not Downloaded" });

				// this should only appear if is already an error. User should not be able to set status to error, only away from error
				if (status == LiberatedStatus.Error)
					this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.Error, Text = "Error" });

				setDefaultComboBox(this.bookLiberatedCb, status);
			}

			{
				var status = Book.UserDefinedItem.PdfStatus;

				if (status is null)
					this.pdfLiberatedCb.Enabled = false;
				else
				{
					this.pdfLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
					this.pdfLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.NotLiberated, Text = "Not Downloaded" });

					setDefaultComboBox(this.pdfLiberatedCb, status);
				}
			}
		}
		private static void setDefaultComboBox(ComboBox comboBox, LiberatedStatus? status)
		{
			if (!status.HasValue)
			{
				comboBox.SelectedIndex = 0;
				return;
			}

			var item = comboBox.Items.Cast<liberatedComboBoxItem>().SingleOrDefault(item => item.Status == status.Value);
			if (item is not null)
				comboBox.SelectedItem = item;
			else
				comboBox.SelectedIndex = 0;
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			NewTags = this.newTagsTb.Text;

			BookLiberatedStatus = ((liberatedComboBoxItem)this.bookLiberatedCb.SelectedItem).Status;

			if (this.pdfLiberatedCb.Enabled)
				PdfLiberatedStatus = ((liberatedComboBoxItem)this.pdfLiberatedCb.SelectedItem).Status;

			this.DialogResult = DialogResult.OK;
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

        private void audibleLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
			var locale = AudibleApi.Localization.Get(_libraryBook.Book.Locale);
			var link = $"https://www.audible.{locale.TopDomain}/pd/{Book.AudibleProductId}";
			Go.To.Url(link);
		}
    }
}
