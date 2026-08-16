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
	/// never held back either: the refusal recorded against a title is about its audiobook, and a PDF is a
	/// different request.
	/// </summary>
	internal override bool HonorsDeferredRetries => !Force && !PdfOnly;

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
			await RunAsync(GetProcessable(), lb => PrepareBookForLiberate(lb, isTargetedRun));
		}
	}

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

	private static Processable CreateBackupBook(DownloadOptions.LicenseInfo? licenseInfo)
	{
		var downloadPdf = CreateProcessable<DownloadPdf>();
		var uploadToAudiobookshelf = CreateProcessable<UploadToAudiobookshelf>();

		// Chain pdf download and audiobookshelf upload on DownloadDecryptBook.Completed
		void onDownloadDecryptBookCompleted(object? sender, LibraryBook e)
		{
			// this is fast anyway. run as sync for easy exception catching
			downloadPdf.TryProcessAsync(e).GetAwaiter().GetResult();
			uploadToAudiobookshelf.TryProcessAsync(e).GetAwaiter().GetResult();
		}

		var downloadDecryptBook = CreateProcessable<DownloadDecryptBook>(onDownloadDecryptBookCompleted);
		downloadDecryptBook.LicenseInfo = licenseInfo;
		return downloadDecryptBook;
	}
}
