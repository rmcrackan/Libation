using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core;

namespace LibationWinForms.Dialogs
{
	public partial class BookDetailsDialog : Form
	{
		public string NewTags { get; private set; }

		private LibraryBook _libraryBook { get; }
		private Book Book => _libraryBook.Book;

		public BookDetailsDialog()
		{
			InitializeComponent();
		}
		public BookDetailsDialog(LibraryBook libraryBook) : this()
		{
			_libraryBook = ArgumentValidator.EnsureNotNull(libraryBook, nameof(libraryBook));
			initDetails();
			initTags();
//initLiberated();
		}
		// 1st draft: lazily cribbed from GridEntry.ctor()
		private void initDetails()
		{
			this.Text = Book.Title;

			(var isDefault, var picture) = FileManager.PictureStorage.GetPicture(new FileManager.PictureDefinition(Book.PictureId, FileManager.PictureSize._80x80));
			this.coverPb.Image = Dinah.Core.Drawing.ImageReader.ToImage(picture);

			var t = @$"
Title: {Book.Title}
Author(s): {Book.AuthorNames}
Narrator(s): {Book.NarratorNames}
Length: {(Book.LengthInMinutes == 0 ? "" : $"{Book.LengthInMinutes / 60} hr {Book.LengthInMinutes % 60} min")}
Category: {string.Join(" > ", Book.CategoriesNames)}
Purchase Date: {_libraryBook.DateAdded.ToString("d")}
".Trim();

			if (!string.IsNullOrWhiteSpace(Book.SeriesNames))
				t += $"\r\nSeries: {Book.SeriesNames}";

			var bookRating = Book.Rating?.ToStarString();
			if (!string.IsNullOrWhiteSpace(bookRating))
				t += $"\r\nBook Rating:\r\n{bookRating}";

			var myRating = Book.UserDefinedItem.Rating?.ToStarString();
			if (!string.IsNullOrWhiteSpace(myRating))
				t += $"\r\nMy Rating:\r\n{myRating}";

			this.detailsTb.Text = t;
		}
		private void initTags() => this.newTagsTb.Text = Book.UserDefinedItem.Tags;

		private void saveBtn_Click(object sender, EventArgs e)
		{
			NewTags = this.newTagsTb.Text;
			this.DialogResult = DialogResult.OK;
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
