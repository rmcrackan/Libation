using AaxDecrypter;
using ApplicationServices;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Net.Http;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase;

public class FindBetterQualityBooksViewModel : ReactiveObject
{
	public const string StartScanBtnText = "Scan Audible for Higher Quality Audio";
	public const string StopScanBtnText = "Stop Scanning";
	public const string UseWidevineSboxText = "Use Widevine?";
	public const string InitialMessage = """
		This tool will scan your liberated audiobooks to see if Audible
		has a higher quality version available.

		For each liberated audiobook in your library, it will try to read the existing audio file to determine its codec and bitrate. If no local file is found, it will use the 'Last Downloaded' format information stored in the database.
		
		It will then query Audible's API to get the highest quality format currently available for that audiobook.
		
		If you check the 'Use Widevine' option, it will query for Widevine-protected formats, which may or may not be xHE-AAC. If unchecked, it will query for Audible DRM-protected formats, which are typically AAC-LC.

		Click 'Scan Audible for Higher Quality Audio' to begin.

		When done, click the 'Mark X books as Not Liberated' to allow Libation to re-download those books in the higher.

		Note: make sure you adjust your download quality settings before re-liberating the books.
		""";

	public event EventHandler<BookDataViewModel>? BookScanned;
	public IList<BookDataViewModel>? Books { get => field; set => RaiseAndSetIfChanged(ref field, value); }

	public bool ScanWidevine { get; set; }
	public int SignificantCount
	{
		get => field;
		set
		{
			RaiseAndSetIfChanged(ref field, value);
			MarkBooksButtonText = value == 0 ? null
				: value == 1 ? "Mark 1 book as 'Not Liberated'"
				: $"Mark {value} books as 'Not Liberated'";
		}
	}
	public bool IsScanning { get => field; set { RaiseAndSetIfChanged(ref field, value); ScanButtonText = field ? StopScanBtnText : StartScanBtnText; } }
	public string? MarkBooksButtonText { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	public string? ScanCount { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	public string ScanButtonText { get => field; set => RaiseAndSetIfChanged(ref field, value); } = StartScanBtnText;

	private CancellationTokenSource? cts;

	public FindBetterQualityBooksViewModel()
	{
		ScanWidevine = Configuration.Instance.UseWidevine;
	}

	public static bool ShouldScan(LibraryBook lb)
		=> lb.Book.ContentType is ContentType.Product //only scan books, not podcasts
		&& !lb.Book.IsSpatial //skip spatial audio books. When querying the /metadata endpoint, it will only show ac-4 data for spatial audiobooks.
		&& lb.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated;

	public void StopScan()
	{
		cts?.Cancel();
	}

	public async Task MarkBooksAsync()
	{
		var significant = Books?.Where(b => b.IsSignificant).ToArray() ?? [];

		await significant.Select(b => b.LibraryBook).UpdateBookStatusAsync(LiberatedStatus.NotLiberated);
		Array.ForEach(significant, b => Books?.Remove(b));

		SignificantCount = Books?.Count(b => b.IsSignificant) ?? 0;
	}

	public async Task ScanAsync()
	{
		if (cts?.IsCancellationRequested is true || Books is null)
			return;
		IsScanning = true;

		foreach (var b in Books)
		{
			b.AvailableBitrate = 0;
			b.AvailableCodec = null;
			b.ScanStatus = BookScanStatus.None;
		}
		ScanCount = $"0 of {Books.Count:N0} scanned";

		try
		{
			using var cli = new HttpClient();
			cts = new CancellationTokenSource();
			for (int i = 0; i < Books.Count; i++)
			{
				var b = Books[i];
				var url = GetUrl(b.LibraryBook);
				try
				{
					//Don't re-scan a file if we have already loaded existing audio codec and bitrate.
					if (b.Bitrate == 0 && b.Codec == null)
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
							b.ScanStatus = BookScanStatus.Error;
							continue;
						}
					}

					var resp = await cli.GetAsync(url, cts.Token);
					var (codecString, bitrate) = await ReadAudioInfoAsync(resp.EnsureSuccessStatusCode());

					b.AvailableCodec = codecString;
					b.AvailableBitrate = bitrate;
					b.ScanStatus = BookScanStatus.Completed;
				}
				catch (OperationCanceledException)
				{
					b.ScanStatus = BookScanStatus.Cancelled;
					break;
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error checking for better quality for {@Asin}", b.Asin);
					b.FoundFile = $"Error: {ex.Message}";
					b.ScanStatus = BookScanStatus.Error;
				}
				finally
				{
					SignificantCount = Books.Count(b => b.IsSignificant);
					ScanCount = $"{i + 1:N0} of {Books.Count:N0} scanned";
					BookScanned?.Invoke(this, b);
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
