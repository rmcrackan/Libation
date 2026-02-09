using ApplicationServices;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
using System;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

public partial class BookDetailsDialog : Form
{
	public class liberatedComboBoxItem
	{
		public LiberatedStatus Status { get; set; }
		public string? Text { get; set; }
		public override string? ToString() => Text;
	}

	public string? NewTags { get; private set; }
	public LiberatedStatus BookLiberatedStatus { get; private set; }
	public LiberatedStatus? PdfLiberatedStatus { get; private set; }

	private Book? Book => LibraryBook?.Book;

	public BookDetailsDialog()
	{
		InitializeComponent();
		this.SetLibationIcon();
		audibleLink.SetLinkLabelColors();
	}

	public LibraryBook? LibraryBook
	{
		get => field;
		set
		{
			field = value;
			initDetails();
			initTags();
			initLiberated();
		}
	}

	// 1st draft: lazily cribbed from GridEntry.ctor()
	private void initDetails()
	{
		if (Book is null)
			return;
		audibleLink.LinkVisited = false;
		this.Text = Book.TitleWithSubtitle;
		dolbyAtmosPb.Visible = Book.IsSpatial;
		dolbyAtmosPb.Image = Application.IsDarkModeEnabled ? Properties.Resources.Dolby_Atmos_Vertical_80_dark : Properties.Resources.Dolby_Atmos_Vertical_80;

		var picture = (Book.PictureId ?? Book.PictureLarge) is not string picId ? null
			: PictureStorage.GetPicture(new(picId, PictureSize._80x80)).bytes;
		this.coverPb.Image = WinFormsUtil.TryLoadImageOrDefault(picture, PictureSize._80x80);

		var title = string.IsNullOrEmpty(Book.Subtitle) ? Book.Title : $"{Book.Title}\r\n        {Book.Subtitle}";
		var t = $"""
		Title: {title}
		Author(s): {Book.AuthorNames}
		Narrator(s): {Book.NarratorNames}
		Length: {(Book.LengthInMinutes == 0 ? "" : $"{Book.LengthInMinutes / 60} hr {Book.LengthInMinutes % 60} min")}
		Category: {string.Join(", ", Book.LowestCategoryNames())}
		Purchase Date: {LibraryBook!.DateAdded:d}
		Language: {Book.Language}
		Audible ID: {Book.AudibleProductId}
		""";

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
	private void initTags() => this.newTagsTb.Text = Book?.UserDefinedItem.Tags;
	private void initLiberated()
	{
		if (Book is null)
			return;
		{
			var status = Book.UserDefinedItem.BookStatus;
			this.bookLiberatedCb.Items.Clear();
			this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
			this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.NotLiberated, Text = "Not Downloaded" });

			// this should only appear if is already an error. User should not be able to set status to error, only away from error
			if (status == LiberatedStatus.Error)
				this.bookLiberatedCb.Items.Add(new liberatedComboBoxItem { Status = LiberatedStatus.Error, Text = "Error" });

			setDefaultComboBox(this.bookLiberatedCb, status);
		}

		{
			var status = Book.UserDefinedItem.PdfStatus;
			this.pdfLiberatedCb.Items.Clear();
			this.pdfLiberatedCb.Enabled = status is not null;
			if (status is not null)
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

	private async void saveBtn_Click(object sender, EventArgs e)
	{
		NewTags = this.newTagsTb.Text;
		if (bookLiberatedCb.SelectedItem is liberatedComboBoxItem bStatus)
			BookLiberatedStatus = bStatus.Status;

		if (pdfLiberatedCb.Enabled && pdfLiberatedCb.SelectedItem is liberatedComboBoxItem pStatus)
			PdfLiberatedStatus = pStatus.Status;

		if (LibraryBook is null)
			return;

		Invoke(() => saveBtn.Enabled = cancelBtn.Enabled = false);
		await LibraryBook.UpdateUserDefinedItemAsync(NewTags, BookLiberatedStatus, PdfLiberatedStatus);
		Invoke(() => saveBtn.Enabled = cancelBtn.Enabled = true);
	}

	private void cancelBtn_Click(object sender, EventArgs e)
	{
		this.DialogResult = DialogResult.Cancel;
		this.Close();
	}

	private void audibleLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
	{
		if (Book is null)
			return;
		var locale = AudibleApi.Localization.Get(Book.Locale);
		var link = $"https://www.audible.{locale.TopDomain}/pd/{Book.AudibleProductId}";
		Go.To.Url(link);
		e.Link?.Visited = true;
	}
}
