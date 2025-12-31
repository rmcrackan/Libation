using DataLayer;

namespace LibationUiBase;

public enum BookScanStatus
{
	None,
	Error,
	Cancelled,
	Completed,
}

public class BookDataViewModel : ReactiveObject
{
	public LibraryBook LibraryBook { get; }
	public BookDataViewModel(LibraryBook libraryBook)
	{
		LibraryBook = libraryBook;
		Asin = libraryBook.Book.AudibleProductId;
		Title = libraryBook.Book.Title;
	}
	public string Asin { get; }
	public string Title { get; }
	public string? FoundFile { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	public string? Codec { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	public string? AvailableCodec { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	public int Bitrate
	{
		get => field;
		set
		{
			RaiseAndSetIfChanged(ref field, value);
			BitrateString = GetBitrateString(value);
		}
	}
	public int AvailableBitrate
	{
		get => field;
		set
		{
			RaiseAndSetIfChanged(ref field, value);
			AvailableBitrateString = GetBitrateString(value);
			var diff = (double)AvailableBitrate / Bitrate;
			IsSignificant = diff >= 1.15;
		}
	}

	public string? BitrateString { get => field; private set => RaiseAndSetIfChanged(ref field, value); }
	public string? AvailableBitrateString { get => field; private set => RaiseAndSetIfChanged(ref field, value); }
	public bool IsSignificant { get => field; private set => RaiseAndSetIfChanged(ref field, value); }
	public BookScanStatus ScanStatus { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	private static string? GetBitrateString(int bitrate) => bitrate > 0 ? $"{bitrate} kbps" : null;
}
