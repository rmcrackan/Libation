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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibationAvalonia.Dialogs;

public partial class BookDetailsDialog : DialogWindow
{
	private BookDetailsDialogViewModel? _viewModel;
	public LibraryBook? LibraryBook
	{
		get => field;
		set
		{
			field = value;
			Title = field?.Book.TitleWithSubtitle;
			if (field is not null)
				DataContext = _viewModel = new BookDetailsDialogViewModel(field);
		}
	}

	public string? NewTags => _viewModel?.Tags;
	public LiberatedStatus BookLiberatedStatus => _viewModel?.BookLiberatedSelectedItem?.Status ?? default;
	public LiberatedStatus? PdfLiberatedStatus => _viewModel?.PdfLiberatedSelectedItem?.Status;

	public BookDetailsDialog()
	{
		InitializeComponent();
		ControlToFocusOnShow = tagEditor.InputTextBox;

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
		if (LibraryBook is not null)
			await LibraryBook.UpdateUserDefinedItemAsync(NewTags, bookStatus: BookLiberatedStatus, pdfStatus: PdfLiberatedStatus);
		await base.SaveAndCloseAsync();
	}

	public void BookStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (sender is not WheelComboBox { SelectedItem: liberatedComboBoxItem { Status: LiberatedStatus.Error } } &&
			_viewModel?.BookLiberatedItems.SingleOrDefault(s => s.Status == LiberatedStatus.Error) is liberatedComboBoxItem errorItem)
		{
			_viewModel.BookLiberatedItems.Remove(errorItem);
		}
	}

	public void CancelButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> CancelAndClose();

	public async void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await SaveAndCloseAsync();
	public class liberatedComboBoxItem
	{
		public LiberatedStatus Status { get; set; }
		public string? Text { get; set; }
		public override string? ToString() => Text;
	}

	public record DetailItem(string Label, string Value, bool IsMono = false);

	public class BookDetailsDialogViewModel : ViewModelBase
	{
		public Bitmap Cover { get; set; }
		public string TitleText { get; }
		public string? Subtitle { get; }
		public string? SeriesLine { get; }
		public string AuthorNames { get; }
		public IReadOnlyList<DetailItem> Details { get; }
		public string? FilePath { get; }
		public string? ShortFilePath { get; }
		public string ShowFileText { get; }
		public ICommand ShowFileCommand { get; }
		public string StatusText { get; }
		public bool StatusIsSuccess { get; }
		public bool StatusIsWarning { get; }
		public bool StatusIsError { get; }
		public bool StatusIsNeutral { get; }
		public string Tags { get; set; }
		public bool IsSpatial { get; }

		public bool HasPDF => PdfLiberatedItems?.Count > 0;
		public AvaloniaList<liberatedComboBoxItem> BookLiberatedItems { get; } = new();
		public List<liberatedComboBoxItem> PdfLiberatedItems { get; } = new();
		public liberatedComboBoxItem? PdfLiberatedSelectedItem { get; set; }
		public liberatedComboBoxItem? BookLiberatedSelectedItem { get; set; }
		public ICommand OpenInAudibleCommand { get; }

		public BookDetailsDialogViewModel(LibraryBook libraryBook)
		{
			var Book = libraryBook.Book;

			var locale = AudibleApi.Localization.Get(libraryBook.Book.Locale);
			var link = $"https://www.audible.{locale.TopDomain}/pd/{libraryBook.Book.AudibleProductId}";
			OpenInAudibleCommand = ReactiveCommand.Create(() => Go.To.Url(link));
			IsSpatial = libraryBook.Book.IsSpatial;

			Tags = libraryBook.Book.UserDefinedItem.Tags;

			byte[]? picture = libraryBook.Book.PictureId is not string picId ? null
				: PictureStorage.GetPictureSynchronously(new PictureDefinition(picId, PictureSize._80x80));
			Cover = AvaloniaUtils.TryLoadImageOrDefault(picture, PictureSize._80x80);

			TitleText = Book.Title;
			Subtitle = string.IsNullOrWhiteSpace(Book.Subtitle) ? null : Book.Subtitle;
			AuthorNames = Book.AuthorNames;

			var firstSeries = Book.SeriesLink.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.Series.Name));
			if (firstSeries is not null)
				SeriesLine = string.IsNullOrWhiteSpace(firstSeries.Order)
					? firstSeries.Series.Name
					: $"{firstSeries.Series.Name} \u00B7 Book {firstSeries.Order}";

			var details = new List<DetailItem>();
			void Add(string label, string? value, bool mono = false)
			{
				if (!string.IsNullOrWhiteSpace(value))
					details.Add(new(label, value.Trim(), mono));
			}

			Add("Narrator(s)", Book.NarratorNames);
			Add("Length", Book.LengthInMinutes <= 0 ? null
				: Book.LengthInMinutes >= 60 ? $"{Book.LengthInMinutes / 60}h {Book.LengthInMinutes % 60}m" : $"{Book.LengthInMinutes}m");
			Add("Released", Book.DatePublished?.ToString("d"));
			Add("Purchased", libraryBook.DateAdded.ToString("d"));
			Add("Account", string.IsNullOrWhiteSpace(libraryBook.Account) ? null : $"{libraryBook.Account} ({locale.Name})");
			Add("Category", string.Join(", ", Book.LowestCategoryNames()));
			Add("Language", Book.Language);
			Add("Book rating", StarLine(Book.Rating));
			Add("My rating", StarLine(Book.UserDefinedItem.Rating));
			Add("Audible ID", Book.AudibleProductId, mono: true);
			Details = details;

			try
			{
				FilePath = AudibleFileStorage.Audio.GetPath(Book.AudibleProductId)?.PathWithoutPrefix;
			}
			catch
			{
				FilePath = null;
			}
			ShortFilePath = FilePath is null ? null : TrimPath(FilePath);
			ShowFileText = OperatingSystem.IsMacOS() ? "Show in Finder" : OperatingSystem.IsWindows() ? "Show in Explorer" : "Show in folder";
			ShowFileCommand = ReactiveCommand.Create(() => { if (FilePath is not null) Go.To.File(FilePath); });

			var bookStatus = Book.UserDefinedItem.BookStatus;
			var pdfStatus = Book.UserDefinedItem.PdfStatus;
			(StatusText, StatusIsSuccess, StatusIsWarning, StatusIsError, StatusIsNeutral) = LibraryCommands.Liberated_Status(Book) switch
			{
				LiberatedStatus.PartialDownload => ("Partially downloaded", false, true, false, false),
				LiberatedStatus.Liberated => ("Downloaded", true, false, false, false),
				LiberatedStatus.Error => ("Error", false, false, true, false),
				_ => ("Not downloaded", false, false, false, true)
			};

			BookLiberatedItems.Add(new() { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
			BookLiberatedItems.Add(new() { Status = LiberatedStatus.NotLiberated, Text = "Download Pending" });

			if (bookStatus == LiberatedStatus.Error)
				BookLiberatedItems.Add(new() { Status = LiberatedStatus.Error, Text = "Error" });

			BookLiberatedSelectedItem = BookLiberatedItems.SingleOrDefault(s => s.Status == bookStatus);

			if (pdfStatus is not null)
			{
				PdfLiberatedItems.Add(new() { Status = LiberatedStatus.Liberated, Text = "Downloaded" });
				PdfLiberatedItems.Add(new() { Status = LiberatedStatus.NotLiberated, Text = "Download Pending" });

				PdfLiberatedSelectedItem = PdfLiberatedItems.SingleOrDefault(s => s.Status == pdfStatus);
			}
		}

		private static string? StarLine(Rating? rating)
			=> rating?.ToStarString()?.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries) is { Length: > 0 } lines
				? string.Join("   ", lines.Select(l => l.Trim()))
				: null;

		private static string TrimPath(string path)
		{
			const int max = 64;
			return path.Length <= max ? path : "\u2026" + path[^max..];
		}
	}
}
