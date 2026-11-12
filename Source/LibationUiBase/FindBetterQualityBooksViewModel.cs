using AaxDecrypter;
using ApplicationServices;
using AudibleUtilities;
using DataLayer;
using Dinah.Core;
using Dinah.Core.Net.Http;
using LibationFileManager;
using LibationUiBase.ProcessQueue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LibationUiBase;

public record MarkBooksResult(
	int MarkedCount,
	bool WidevineEnabled,
	bool XHeAacRequested,
	bool AccountsMarkedForReRegistration)
{
	public string? GetSummaryMessage()
	{
		if (MarkedCount == 0)
			return null;

		var message = MarkedCount == 1
			? "Marked 1 book as Download Pending."
			: $"Marked {MarkedCount} books as Download Pending.";

		if (WidevineEnabled || AccountsMarkedForReRegistration)
		{
			message += "\r\n\r\nLibation has automatically configured your download settings for higher-quality audio and marked your accounts for a fresh sign-in on your next scan or download so that the higher-quality audio can be acquired.";
		}
		else if (XHeAacRequested)
		{
			message += "\r\n\r\nLibation has automatically enabled xHE-AAC audio requests in your settings.";
		}

		return message;
	}
}

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

		When done, click 'Mark X books as Download Pending' to allow Libation to re-download those books in the higher quality format.

		Marking books will automatically configure Widevine and xHE-AAC settings and prompt for a fresh account sign-in if needed, ensuring the upgraded audio streams are downloaded.

		Note: If you have a large library, downloading many higher-quality audiobooks in quick succession may trigger Audible's daily download limits. You can configure a daily download limit or download delay in Settings > Network if needed.

		Display this help message again in the future?
		""";

	public event EventHandler<BookDataViewModel>? BookScanned;
	public IList<BookDataViewModel>? Books { get => field; set => RaiseAndSetIfChanged(ref field, value); }
	public bool ShowFindBetterQualityBooksHelp { get => Configuration.Instance.GetNonString(defaultValue: true); set => Configuration.Instance.SetNonString(value); }
	public bool ScanWidevine { get; set; }
	public int SignificantCount
	{
		get => field;
		set
		{
			RaiseAndSetIfChanged(ref field, value);
			MarkBooksButtonText = value == 0 ? null
				: value == 1 ? "Mark 1 book as 'Download Pending'"
				: $"Mark {value} books as 'Download Pending'";
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

	public async Task<MarkBooksResult> MarkBooksAsync()
	{
		var significant = Books?.Where(b => b.IsSignificant).ToArray() ?? [];
		if (significant.Length == 0)
			return new MarkBooksResult(0, false, false, false);

		await significant.Select(b => b.LibraryBook).UpdateBookStatusAsync(LiberatedStatus.NotLiberated);
		Array.ForEach(significant, b => Books?.Remove(b));

		SignificantCount = Books?.Count(b => b.IsSignificant) ?? 0;

		bool widevineNewlyEnabled = false;
		bool xHeAacNewlyRequested = false;
		bool accountsMarked = false;

		bool requiresWidevine = ScanWidevine || significant.Any(b => b.AvailableCodec is "xHE-AAC" or "EC-3" or "AC-4");
		if (File.Exists(AudibleApiStorage.AccountsSettingsFile))
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			if (requiresWidevine)
			{
				var recovery = ThrottledDeviceRecovery.ApplyRecovery(persister.AccountsSettings, Configuration.Instance);
				widevineNewlyEnabled = recovery.WidevineEnabled;
				accountsMarked = recovery.MarkedAccounts.Count > 0;
			}
			else
			{
				accountsMarked = ThrottledDeviceRecovery.MarkObsoleteAccounts(persister.AccountsSettings) > 0;
			}
		}
		else if (requiresWidevine)
		{
			widevineNewlyEnabled = !Configuration.Instance.UseWidevine;
			Configuration.Instance.UseWidevine = true;
			Configuration.Instance.DeviceRegistrationKind = AudibleApi.DeviceRegistrationKind.CurrentAndroid;
		}

		if (requiresWidevine && significant.Any(b => b.AvailableCodec == "xHE-AAC") && !Configuration.Instance.Request_xHE_AAC)
		{
			Configuration.Instance.Request_xHE_AAC = true;
			xHeAacNewlyRequested = true;
		}

		return new MarkBooksResult(significant.Length, widevineNewlyEnabled, xHeAacNewlyRequested, accountsMarked);
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
			b.ScanStatus = ProcessBookStatus.Queued;
		}
		ScanCount = $"0 of {Books.Count:N0} scanned";

		try
		{
			using var cli = new HttpClient();
			cts = new CancellationTokenSource();
			for (int i = 0; i < Books.Count; i++)
			{
				var b = Books[i];
				try
				{
					cts.Token.ThrowIfCancellationRequested();
					var url = GetUrl(b.LibraryBook);
					//Don't re-scan a file if we have already loaded existing audio codec and bitrate.
					if (b.Bitrate == 0 && b.Codec == null)
					{
						var (file, bestFormat) = FindHighestExistingFormat(b.LibraryBook);

						if (file is not null)
						{
							b.FoundFile = Configuration.Instance.Books?.Path is string booksDir ? Path.GetRelativePath(booksDir, file) : file;
							b.Bitrate = bestFormat.BitRate;
							b.Codec = bestFormat.CodecString;
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
							b.ScanStatus = ProcessBookStatus.Failed;
							continue;
						}
					}

					var resp = await cli.GetAsync(url, cts.Token);
					var (codecString, bitrate) = await ReadAudioInfoAsync(resp.EnsureSuccessStatusCode());

					b.AvailableCodec = codecString;
					b.AvailableBitrate = bitrate;
					b.ScanStatus = ProcessBookStatus.Completed;
				}
				catch (OperationCanceledException)
				{
					b.ScanStatus = ProcessBookStatus.Cancelled;
					break;
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error checking for better quality for {@Asin}", new { b.Asin });
					b.FoundFile = $"Error: {ex.Message}";
					b.ScanStatus = ProcessBookStatus.Failed;
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
		var largestFile
			= AudibleFileStorage.Audio
			.GetPaths(libraryBook.Book.AudibleProductId)
			.Select(p => new FileInfo(p))
			.Where(f => f.Exists && f.Extension.EqualsInsensitive(".m4b"))
			.OrderByDescending(f => f.Length)
			.FirstOrDefault()
			?.FullName;

		return largestFile is null ? (null, AudioFormat.Default)
			: (largestFile, AudioFormatDecoder.FromMpeg4(largestFile));
	}

	private static async Task<(string codec, int bitrate)> ReadAudioInfoAsync(HttpResponseMessage response)
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

	private string GetUrl(LibraryBook libraryBook)
	{
		var drm_type = ScanWidevine ? "Widevine" : "Adrm";
		var locale = AudibleApi.Localization.Get(libraryBook.Book.Locale);
		return string.Format(BaseUrl, locale.TopDomain, libraryBook.Book.AudibleProductId, drm_type);
	}

	private const string BaseUrl = "ht" + "tps://api.audible.{0}/1.0/content/{1}/metadata?response_groups=chapter_info,content_reference&quality=High&drm_type={2}";
}
