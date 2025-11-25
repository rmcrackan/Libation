using ApplicationServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using DataLayer;
using Dinah.Core;
using LibationAvalonia.Controls;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibationAvalonia.Dialogs
{
	public partial class BookDetailsDialog : DialogWindow
	{
		private BookDetailsDialogViewModel _viewModel;
		public LibraryBook LibraryBook
		{
			get => field;
			set
			{
				field = value;
				Title = field.Book.TitleWithSubtitle;
				DataContext = _viewModel = new BookDetailsDialogViewModel(field);
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
				MainVM.Configure_NonUI();
				LibraryBook
					= MockLibraryBook
					.CreateBook(isSpatial: true)
					.AddAuthor("Author 2")
					.AddNarrator("Narrator 2")
					.AddSeries("Series Name", 1)
					.AddCategoryLadder("Parent", "Child Category")
					.AddCategoryLadder("Parent", "Child Category 2")
					.WithBookStatus(LiberatedStatus.NotLiberated)
					.WithPdfStatus(LiberatedStatus.Liberated);
			}
		}
		public BookDetailsDialog(LibraryBook libraryBook) : this()
		{
			LibraryBook = libraryBook;
		}

		protected override async Task SaveAndCloseAsync()
		{
			await LibraryBook.UpdateUserDefinedItemAsync(NewTags, bookStatus: BookLiberatedStatus, pdfStatus: PdfLiberatedStatus);
			await base.SaveAndCloseAsync();
		}

		public void BookStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is not WheelComboBox { SelectedItem: liberatedComboBoxItem { Status: LiberatedStatus.Error } } &&
				_viewModel.BookLiberatedItems.SingleOrDefault(s => s.Status == LiberatedStatus.Error) is liberatedComboBoxItem errorItem)
			{
				_viewModel.BookLiberatedItems.Remove(errorItem);
			}
		}

		public async void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> await SaveAndCloseAsync();
		public class liberatedComboBoxItem
		{
			public LiberatedStatus Status { get; set; }
			public string Text { get; set; }
			public override string ToString() => Text;
		}

		public class BookDetailsDialogViewModel : ViewModelBase
		{
			public Bitmap Cover { get; set; }
			public string DetailsText { get; set; }
			public string Tags { get; set; }
			public bool IsSpatial { get; }

			public bool HasPDF => PdfLiberatedItems?.Count > 0;
			public AvaloniaList<liberatedComboBoxItem> BookLiberatedItems { get; } = new();
			public List<liberatedComboBoxItem> PdfLiberatedItems { get; } = new();
			public liberatedComboBoxItem PdfLiberatedSelectedItem { get; set; }
			public liberatedComboBoxItem BookLiberatedSelectedItem { get; set; }
			public ICommand OpenInAudibleCommand { get; }

			public BookDetailsDialogViewModel(LibraryBook libraryBook)
			{
				var Book = libraryBook.Book;

				var locale = AudibleApi.Localization.Get(libraryBook.Book.Locale);
				var link = $"https://www.audible.{locale.TopDomain}/pd/{libraryBook.Book.AudibleProductId}";
				OpenInAudibleCommand = ReactiveCommand.Create(() => Go.To.Url(link));
				IsSpatial = libraryBook.Book.IsSpatial;

				//init tags
				Tags = libraryBook.Book.UserDefinedItem.Tags;

				//init cover image
				var picture = PictureStorage.GetPictureSynchronously(new PictureDefinition(libraryBook.Book.PictureId, PictureSize._80x80));
				Cover = AvaloniaUtils.TryLoadImageOrDefault(picture, PictureSize._80x80);

				var title = string.IsNullOrEmpty(Book.Subtitle) ? Book.Title : $"{Book.Title}\r\n        {Book.Subtitle}";

				//init book details
				DetailsText = $"""
				Title: {title}
				Author(s): {Book.AuthorNames}
				Narrator(s): {Book.NarratorNames}
				Length: {(Book.LengthInMinutes == 0 ? "" : $"{Book.LengthInMinutes / 60} hr {Book.LengthInMinutes % 60} min")}
				Category: {string.Join(", ", Book.LowestCategoryNames())}
				Purchase Date: {libraryBook.DateAdded:d}
				Language: {Book.Language}
				Audible ID: {Book.AudibleProductId}
				""";

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
