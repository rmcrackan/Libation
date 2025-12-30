using AaxDecrypter;
using ApplicationServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Net.Http;
using DynamicData;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public partial class FindBetterQualityBooksDialog : DialogWindow
{
    public enum ScanStatus
    {
        None,
		Error,
        Cancelled,
		Completed,
	}

    private ScanVM VM { get; }
	private Dictionary<BookData, IDisposable> Observers { get; } = new();
	public FindBetterQualityBooksDialog()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			var library = Enumerable.Repeat(MockLibraryBook.CreateBook(), 3);
			DataContext = VM = new ScanVM(library);
			VM.Books[0].AvailableCodec = "xHE-AAC";
			VM.Books[0].AvailableBitrate = 256;
			VM.Books[0].ScanStatus = ScanStatus.Completed;
			VM.Books[1].ScanStatus = ScanStatus.Error;
			VM.Books[2].ScanStatus = ScanStatus.Cancelled;
			VM.SignificantCount = 1;
		}
		else
		{
			var library = DbContexts.GetLibrary_Flat_NoTracking();
			DataContext = VM = new ScanVM(library.Where(ShouldScan));
		}

		VM.Books.ForEachItem(OnBookDataAdded, OnBookDataRemoved, OnBooksReset);
	}

	private static bool ShouldScan(LibraryBook lb)
		=> lb.Book.ContentType is ContentType.Product //only scan books, not podcasts
		&& !lb.Book.IsSpatial //skip spatial audio books. When querying the /metadata endpoint, it will only show ac-4 data for spatial audiobooks.
		&& lb.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated;

	private void OnBookDataAdded(BookData bookData)
	{
		var subscriber = bookData.ObservableForProperty(x => x.ScanStatus)
			.Subscribe(v => booksDataGrid.ScrollIntoView(v.Sender, booksDataGrid.Columns[0]));
		Observers[bookData] = subscriber;
	}

	private void OnBookDataRemoved(BookData bookData)
	{
		if (Observers.TryGetValue(bookData, out var subscriber))
		{
			subscriber.Dispose();
			Observers.Remove(bookData);
		}
	}

	private void OnBooksReset()
	{
		foreach (var subscriber in Observers.Values)
		{
			subscriber.Dispose();
		}
		Observers.Clear();
	}

	public static FuncValueConverter<ScanStatus, IBrush?> RowConverter { get; } = new(status =>
    {
        var brush = status switch
        {
            ScanStatus.Completed => "ProcessQueueBookCompletedBrush",
            ScanStatus.Cancelled => "ProcessQueueBookCancelledBrush",
            ScanStatus.Error => "ProcessQueueBookFailedBrush",
            _ => null,
        };
        return brush is not null && App.Current.TryGetResource(brush, App.Current.ActualThemeVariant, out var res) ? res as Brush : null;
    });

	public class ScanVM : ViewModelBase
    {
		public AvaloniaList<BookData> Books { get; }
		public bool ScanWidevine { get; set; }
		public int SignificantCount
		{
			get => field;
			set
			{
				this.RaiseAndSetIfChanged(ref field, value);
				MarkBooksButtonText = value == 0 ? string.Empty
					: value == 1 ? "Mark 1 book as 'Not Liberated'"
					: $"Mark {value} books as 'Not Liberated'";
			}
		}
		public bool IsScanning { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
		public string? MarkBooksButtonText { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
		public string? ScanCount { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }

		private CancellationTokenSource? cts;

		public ScanVM(IEnumerable<LibraryBook> books)
        {
			Books = new(books.Select(lb => new BookData(lb)));
			ScanWidevine = Configuration.Instance.UseWidevine;
		}

        public void StopScan()
        {
            cts?.Cancel();
        }

		public async Task MarkBooksAsync()
		{
			var significant = Books.Where(b => b.IsSignificant).ToArray();

			await significant.Select(b => b.LibraryBook).UpdateBookStatusAsync(LiberatedStatus.NotLiberated);
			Books.RemoveMany(significant);
			SignificantCount = Books.Count(b => b.IsSignificant);
		}

		public async Task ScanAsync()
        {
            if (cts?.IsCancellationRequested is true || Design.IsDesignMode)
                return;
            IsScanning = true;

			foreach (var b in Books)
			{
				b.AvailableBitrate = 0;
				b.AvailableCodec = null;
				b.ScanStatus = ScanStatus.None;
			}
			ScanCount = $"0 of {Books.Count:N0} scanned";

			try
            {
                using var cli = new HttpClient();
                cts = new CancellationTokenSource();
				for(int i = 0; i < Books.Count; i++)
				{
					var b = Books[i];
					var url = GetUrl(b.LibraryBook);
					try
					{
						var (file, bestformat) = FindHighestExistingFormat(b.LibraryBook);

						if (file is not null)
						{
							b.FoundFile = Configuration.Instance.Books?.Path is string booksDir ? Path.GetRelativePath(booksDir, file) : file;
							b.Bitrate = bestformat.BitRate;
							b.Codec = bestformat.CodecString;
						}
						else if (b.LibraryBook.Book.UserDefinedItem.LastDownloadedFormat is not null)
						{
							b.FoundFile = "File not found. Using 'Last Downloaded' format.";
							b.Bitrate = b.LibraryBook.Book.UserDefinedItem.LastDownloadedFormat.BitRate;
							b.Codec = b.LibraryBook.Book.UserDefinedItem.LastDownloadedFormat.CodecString;
						}
						else
						{
							b.FoundFile = "File not found and no 'Last Downloaded' format found.";
							b.ScanStatus = ScanStatus.Error;
							continue;
						}

						var resp = await cli.GetAsync(url, cts.Token);
						var (codecString, bitrate) = await ReadAudioInfoAsync(resp.EnsureSuccessStatusCode());

						b.AvailableCodec = codecString;
						b.AvailableBitrate = bitrate;
						b.ScanStatus = ScanStatus.Completed;
					}
					catch (OperationCanceledException)
					{
						b.ScanStatus = ScanStatus.Cancelled;
						break;
					}
					catch (Exception ex)
					{
						Serilog.Log.Logger.Error(ex, "Error checking for better quality for {@Asin}", b.Asin);
						b.ScanStatus = ScanStatus.Error;
					}
					finally
					{
						SignificantCount = Books.Count(b => b.IsSignificant);
						ScanCount = $"{i:N0} of {Books.Count:N0} scanned";
					}
				}
            }
            finally
            {
                cts?.Dispose();
                cts = null;
                IsScanning = false;
			}
		}

		private static (string? file, AudioFormat format) FindHighestExistingFormat(LibraryBook libraryBook)
		{
			var largestfile
				= AudibleFileStorage.Audio
				.GetPaths(libraryBook.Book.AudibleProductId)
				.Select(p => new FileInfo(p))
				.Where(f => f.Exists && f.Extension.EqualsInsensitive(".m4b"))
				.OrderByDescending(f => f.Length)
				.FirstOrDefault();

			if (largestfile is null)
				return (null, AudioFormat.Default);
			return (largestfile.FullName, AudioFormatDecoder.FromMpeg4(largestfile.FullName));
		}

        static async Task<(string codec, int bitrate)> ReadAudioInfoAsync(HttpResponseMessage response)
        {
			var data = await response.Content.ReadAsJObjectAsync();
			var totalLengthMs = data["content_metadata"]?["chapter_info"]?.Value<long>("runtime_length_ms") ?? throw new InvalidDataException("Missing runtime length");
			var contentReference = data["content_metadata"]?["content_reference"];
			var totalSize = contentReference?.Value<long>("content_size_in_bytes") ?? throw new InvalidDataException("Missing content size");
			var codec = contentReference?.Value<string>("codec") ?? throw new InvalidDataException("Missing codec name");

			var codecString = codec switch
			{
				AudibleApi.Codecs.AAC_LC => "AAC-LC",
				AudibleApi.Codecs.xHE_AAC => "xHE-AAC",
				AudibleApi.Codecs.EC_3 => "EC-3",
				AudibleApi.Codecs.AC_4 => "AC-4",
				_ => codec,
			};

			var bitrate = (int)(totalSize / 1024d * 1000 / totalLengthMs * 8); // in kbps
            return (codecString, bitrate);
		}

        string GetUrl(LibraryBook libraryBook)
        {
            var drm_type = ScanWidevine ? "Widevine" : "Adrm";
			var locale = AudibleApi.Localization.Get(libraryBook.Book.Locale);
            return string.Format(BaseUrl, locale.TopDomain, libraryBook.Book.AudibleProductId, drm_type);
		}

        const string BaseUrl = "ht" + "tps://api.audible.{0}/1.0/content/{1}/metadata?response_groups=chapter_info,content_reference&quality=High&drm_type={2}";
	}

	public class BookData : ViewModelBase
	{
		public LibraryBook LibraryBook { get; }
		public BookData(LibraryBook libraryBook)
		{
			LibraryBook = libraryBook;
			Asin = libraryBook.Book.AudibleProductId;
			Title = libraryBook.Book.Title;
		}
		public string Asin { get; }
		public string Title { get; }
		public string? FoundFile { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
		public string? Codec { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
		public int Bitrate
		{
			get => field;
			set
			{
				this.RaiseAndSetIfChanged(ref field, value);
				BitrateString = GetBitrateString(value);
			}
		}
		public string? BitrateString { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
		public string? AvailableCodec { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
		public int AvailableBitrate
		{
			get => field;
			set
			{
				this.RaiseAndSetIfChanged(ref field, value);
				AvailableBitrateString = GetBitrateString(value);
				var diff = (double)AvailableBitrate / Bitrate;
				IsSignificant = diff >= 1.15;
			}
		}
		public string? AvailableBitrateString { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
		public bool IsSignificant { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
		public ScanStatus ScanStatus { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
		private static string? GetBitrateString(int bitrate) => bitrate > 0 ? $"{bitrate} kbps" : null;
	}
}