using ApplicationServices;
using CommandLine;
using DataLayer;
using FileLiberator;
using LibationCli.Options;
using LibationFileManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli;

[Verb("liberate", HelpText = "Liberate: book and pdf backups. Default: download and decrypt all un-liberated titles and download pdfs.\n"
	+ "Optional: specify product id(s) via --id or positional ASIN(s) to liberate those book(s), including re-download if already liberated.\n"
	+ "Optional: reads a license file from standard input.\n"
	+ "Optional: stop this run early with --limit-books, --limit-mb or --limit-gb.")]
public class LiberateOptions : ProcessableOptionsBase
{
	[Option(shortName: 'p', longName: "pdf", Required = false, Default = false, HelpText = "Flag to only download pdfs")]
	public bool PdfOnly { get; set; }

	[Option(shortName: 'f', longName: "force", Required = false, Default = false, HelpText = "Force the book to re-download")]
	public bool Force { get; set; }


	[Option(shortName: 'l', longName: "license", Required = false, Default = null, HelpText = "A license file from the get-license command. Either a file path or dash ('-') to read from standard input.")]
	public string? LicenseInput { get; set; }

	#region per-run limit
	// Distinct SetName values make these three mutually exclusive. See ExportOptions for the same pattern.

	[Option(longName: "limit-books", Required = false, Default = null, SetName = "limit-books",
		HelpText = "Stop this run after downloading this many books. A daily download limit, if set, still applies.")]
	public int? LimitBooks { get; set; }

	[Option(longName: "limit-mb", Required = false, Default = null, SetName = "limit-mb",
		HelpText = "Stop this run after downloading about this many MB. Approximate: a title's size is unknown until it is downloaded.")]
	public int? LimitMB { get; set; }

	[Option(longName: "limit-gb", Required = false, Default = null, SetName = "limit-gb",
		HelpText = "Stop this run after downloading about this many GB. Approximate: a title's size is unknown until it is downloaded.")]
	public int? LimitGB { get; set; }

	private RunDownloadLimit? runLimit;
	protected override RunDownloadLimit? RunLimit => runLimit;

	#endregion

	/// <summary>
	/// --force means "attempt everything", which includes the titles Audible recently refused. A --pdf run is
	/// held back like any other: a PDF is fetched through the same license request as the audiobook, so a title
	/// Libation is waiting on would be refused for its PDF exactly as it was for its audio.
	/// </summary>
	internal override bool HonorsDeferredRetries => !Force;

	/// <summary>
	/// Audible will not license a title the last scan did not find, so a bulk run has nothing to gain by asking.
	/// --force means "attempt everything"; a run that names its titles never reaches the bulk selection at all.
	/// </summary>
	internal override bool SkipsTitlesAbsentFromLastScan => !Force;

	protected override async Task ProcessAsync()
	{
		if (!RunDownloadLimit.TryCreate(LimitBooks, LimitMB, LimitGB, PdfOnly, out runLimit, out var limitError))
		{
			PrintVerbUsage("ERROR", "=====", limitError);
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		if (AudibleFileStorage.BooksDirectory is null)
		{
			Console.Error.WriteLine("Error: Books directory is not set. Please configure the 'Books' setting in Settings.json.");
			return;
		}

		if (LicenseInput is string licenseInput)
		{
			await LiberateFromLicense(licenseInput);
		}
		else
		{
			var isTargetedRun = GetProductIds().Any();

			await RunAsync(
				GetProcessable(),
				lb => PrepareBookForLiberate(lb, isTargetedRun),
				bulkFollowUp: BackFillsPdfs ? CreateProcessable<DownloadPdf>() : null);
		}
	}

	/// <summary>
	/// Whether this run also picks up titles that need nothing but their PDF. The verb is "book and pdf
	/// backups", but the main pass only selects titles that need downloading, so on its own it never reaches
	/// one whose audio it already has. A --pdf run selects those titles to begin with, and a run that names
	/// its titles re-downloads them and gets their PDFs from that.
	/// </summary>
	internal bool BackFillsPdfs => !PdfOnly && !GetProductIds().Any();

	private async Task LiberateFromLicense(string licPath)
	{
		var licenseInfo = licPath is "-" ? ReadLicenseFromStdIn()
			: ReadLicenseFromFile(licPath);

		if (licenseInfo is null)
			return;

		if (licenseInfo?.ContentMetadata?.ContentReference?.Asin is not string asin)
		{
			Console.Error.WriteLine("Error: License file is missing ASIN information.");
			return;
		}

		if (DbContexts.GetLibraryBook_Flat_NoTracking(asin) is not LibraryBook libraryBook)
		{
			Console.Error.WriteLine($"Book not found with asin={asin}");
			return;
		}

		PrepareBookForLiberate(libraryBook, isTargetedRun: true);

		var processable = GetProcessable(licenseInfo);
		if (IsSkippedByDailyLimit(processable, libraryBook))
		{
			Console.WriteLine(DailyDownloadLimitUserMessage.BuildCliSkippedSummary(1));
			return;
		}

		await ProcessOneAsync(processable, libraryBook, true);
	}

	private static DownloadOptions.LicenseInfo? ReadLicenseFromFile(string licFile)
	{
		if (!File.Exists(licFile))
		{
			Console.Error.WriteLine("File does not exist: " + licFile);
			return null;
		}

		Console.WriteLine("Reading license from file.");
		try
		{
			var serializer = CreateLicenseInfoSerializer();
			using var reader = new JsonTextReader(new StreamReader(licFile));
			return serializer.Deserialize<DownloadOptions.LicenseInfo>(reader);
		}
		catch (Exception ex)
		{
			Serilog.Log.Error(ex, "Failed to read license file: {LicenseFile}", licFile);
			Console.Error.WriteLine("Error: Failed to read license file. Please ensure the file is a valid license file in JSON format.");
		}
		return null;
	}

	private static DownloadOptions.LicenseInfo? ReadLicenseFromStdIn()
	{
		if (!Console.IsInputRedirected)
		{
			Console.Error.WriteLine("Ther is nothing in standard input to read.");
			return null;
		}

		Console.WriteLine("Reading license from standard input.");
		try
		{
			var serializer = CreateLicenseInfoSerializer();
			using var reader = new JsonTextReader(new StreamReader(Console.OpenStandardInput()));
			return serializer.Deserialize<DownloadOptions.LicenseInfo>(reader);
		}
		catch (Exception ex)
		{
			Serilog.Log.Error(ex, "Failed to read license from standard input");
			Console.Error.WriteLine("Error: Failed to read license file from standard input. Please ensure the input is a valid license file in JSON format.");
		}
		return null;
	}

	private static JsonSerializer CreateLicenseInfoSerializer()
	{
		var jsonSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			Converters = [new StringEnumConverter(), new ByteArrayHexConverter()]
		};

		return JsonSerializer.Create(jsonSettings);
	}

	private Processable GetProcessable(DownloadOptions.LicenseInfo? licenseInfo = null)
		=> PdfOnly ? CreateProcessable<DownloadPdf>() : CreateBackupBook(licenseInfo);

	/// <summary>
	/// Runs the steps that follow a book download for the same title. Reached through the audiobook step's
	/// Completed event, which fires whether or not the download worked, so what follows has to decide for
	/// itself whether there is anything left to do.
	/// </summary>
	private void OnBookProcessed(DownloadPdf downloadPdf, UploadToAudiobookshelf uploadToAudiobookshelf, object? sender, LibraryBook libraryBook)
	{
		// The supplement is fetched from the license the audiobook download used - the same request, asked once.
		// A step that obtained no license attempted nothing worth following up: asking for one here would put a
		// second identical request to Audible, and where the first was refused, collect a second refusal.
		if (sender is ILicensedDownload { ObtainedLicense: DownloadOptions.LicenseInfo license }
			&& downloadPdf.Validate(libraryBook))
		{
			downloadPdf.LicenseInfo = license;
			try
			{
				// Through the shared per-book handling so a refusal is recorded and reported the same way here
				// as in a bulk pass. Run as sync for easy exception catching; this is fast anyway.
				ProcessOneAsync(downloadPdf, libraryBook, validate: false).GetAwaiter().GetResult();
			}
			finally
			{
				// Processable instances are reused across books, so a license left behind would be applied to
				// the next title, which it is not for.
				downloadPdf.LicenseInfo = null;
			}
		}

		uploadToAudiobookshelf.TryProcessAsync(libraryBook).GetAwaiter().GetResult();
	}

	private void PrepareBookForLiberate(LibraryBook lb, bool isTargetedRun)
	{
		// Targeted runs (explicit ASIN/id) always re-download. --force is for re-downloading the whole library.
		if (Force || isTargetedRun)
		{
			lb.Book.UserDefinedItem.BookStatus = LiberatedStatus.NotLiberated;
			lb.Book.UserDefinedItem.SetPdfStatus(LiberatedStatus.NotLiberated);

			// The status above is set on an untracked copy, so the central clear in updateUserDefinedItem
			// never sees it. Asking for this title is the user overriding any wait Libation was observing,
			// and the wait must restart from the beginning if the attempt fails again.
			DownloadAttemptFailureStore.Clear(lb);
		}
	}

	private Processable CreateBackupBook(DownloadOptions.LicenseInfo? licenseInfo)
	{
		var downloadPdf = CreateProcessable<DownloadPdf>();
		var uploadToAudiobookshelf = CreateProcessable<UploadToAudiobookshelf>();

		// Chain pdf download and audiobookshelf upload on DownloadDecryptBook.Completed
		var downloadDecryptBook = CreateProcessable<DownloadDecryptBook>(
			(sender, e) => OnBookProcessed(downloadPdf, uploadToAudiobookshelf, sender, e));
		downloadDecryptBook.LicenseInfo = licenseInfo;
		return downloadDecryptBook;
	}
}
