using ApplicationServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationAvalonia.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace LibationAvalonia.Dialogs
{
	public partial class BookDetailsDialog : DialogWindow
	{
		private LibraryBook _libraryBook;
		private BookDetailsDialogViewModel _viewModel;
		public LibraryBook LibraryBook
		{
			get => _libraryBook;
			set
			{
				_libraryBook = value;
				Title = _libraryBook.Book.Title;
				DataContext = _viewModel = new BookDetailsDialogViewModel(_libraryBook);
			}
		}

		public string NewTags => _viewModel.Tags;
		public LiberatedStatus BookLiberatedStatus => _viewModel.BookLiberatedSelectedItem.Status;
		public LiberatedStatus? PdfLiberatedStatus => _viewModel.PdfLiberatedSelectedItem?.Status;

		public BookDetailsDialog()
		{
			InitializeComponent();
			ControlToFocusOnShow = this.Find<TextBox>(nameof(tagsTbox));

			if (Design.IsDesignMode)
			{
				using var context = DbContexts.GetContext();
				LibraryBook = context.GetLibraryBook_Flat_NoTracking("B017V4IM1G");
			}
		}
		public BookDetailsDialog(LibraryBook libraryBook) :this()
		{
			LibraryBook = libraryBook;
		}

		protected override void SaveAndClose()
        {
            LibraryBook.Book.UpdateUserDefinedItem(NewTags, bookStatus: BookLiberatedStatus, pdfStatus: PdfLiberatedStatus);
			base.SaveAndClose();
		}

		public void GoToAudible_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		{
			var locale = AudibleApi.Localization.Get(_libraryBook.Book.Locale);
			var link = $"https://www.audible.{locale.TopDomain}/pd/{_libraryBook.Book.AudibleProductId}";
			Go.To.Url(link);
		}

		public void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
            => SaveAndClose();

        private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		private class BookDetailsDialogViewModel : ViewModelBase
		{
			public class liberatedComboBoxItem
			{
				public LiberatedStatus Status { get; set; }
				public string Text { get; set; }
				public override string ToString() => Text;
			}

			public Bitmap Cover { get; set; }
			public string DetailsText { get; set; }
			public string Tags { get; set; }

			public bool HasPDF => PdfLiberatedItems?.Count > 0;

			private liberatedComboBoxItem _bookLiberatedSelectedItem;
			public ObservableCollection<liberatedComboBoxItem> BookLiberatedItems { get; } = new();
			public List<liberatedComboBoxItem> PdfLiberatedItems { get; } = new();
			public liberatedComboBoxItem PdfLiberatedSelectedItem { get; set; }

			public liberatedComboBoxItem BookLiberatedSelectedItem
			{
				get => _bookLiberatedSelectedItem;
				set
				{
					_bookLiberatedSelectedItem = value;
					if (value?.Status is not LiberatedStatus.Error)
					{
						BookLiberatedItems.Remove(BookLiberatedItems.SingleOrDefault(s => s.Status == LiberatedStatus.Error));
					}
				}
			}

			public BookDetailsDialogViewModel(LibraryBook libraryBook)
			{
				//init tags
				Tags = libraryBook.Book.UserDefinedItem.Tags;

				//init cover image
				var picture = PictureStorage.GetPictureSynchronously(new PictureDefinition(libraryBook.Book.PictureId, PictureSize._80x80));
				using var ms = new System.IO.MemoryStream(picture);
				Cover = new Bitmap(ms);

				//init book details
				DetailsText = @$"
Title: {libraryBook.Book.Title}
Author(s): {libraryBook.Book.AuthorNames()}
Narrator(s): {libraryBook.Book.NarratorNames()}
Length: {(libraryBook.Book.LengthInMinutes == 0 ? "" : $"{libraryBook.Book.LengthInMinutes / 60} hr {libraryBook.Book.LengthInMinutes % 60} min")}
Audio Bitrate: {libraryBook.Book.AudioFormat}
Category: {string.Join(" > ", libraryBook.Book.CategoriesNames())}
Purchase Date: {libraryBook.DateAdded:d}
Audible ID: {libraryBook.Book.AudibleProductId}
".Trim();

				var seriesNames = libraryBook.Book.SeriesNames();
				if (!string.IsNullOrWhiteSpace(seriesNames))
					DetailsText += $"\r\nSeries: {seriesNames}";

				var bookRating = libraryBook.Book.Rating?.ToStarString();
				if (!string.IsNullOrWhiteSpace(bookRating))
					DetailsText += $"\r\nBook Rating:\r\n{bookRating}";

				var myRating = libraryBook.Book.UserDefinedItem.Rating?.ToStarString();
				if (!string.IsNullOrWhiteSpace(myRating))
					DetailsText += $"\r\nMy Rating:\r\n{myRating}";


				//init book status
				{
					var status = libraryBook.Book.UserDefinedItem.BookStatus;

					BookLiberatedItems.Add(new() { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
					BookLiberatedItems.Add(new() { Status = LiberatedStatus.NotLiberated, Text = "Not Downloaded" });

					if (status == LiberatedStatus.Error)
						BookLiberatedItems.Add(new() { Status = LiberatedStatus.Error, Text = "Error" });

					BookLiberatedSelectedItem = BookLiberatedItems.SingleOrDefault(s => s.Status == status);
				}

				//init pdf status
				{
					var status = libraryBook.Book.UserDefinedItem.PdfStatus;

					if (status is not null)
					{
						PdfLiberatedItems.Add(new() { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
						PdfLiberatedItems.Add(new() { Status = LiberatedStatus.NotLiberated, Text = "Not Downloaded" });

						PdfLiberatedSelectedItem = PdfLiberatedItems.SingleOrDefault(s => s.Status == status);
					}
				}
			}
		}
	}
}
