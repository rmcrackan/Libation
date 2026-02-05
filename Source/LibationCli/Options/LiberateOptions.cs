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
using System.Threading.Tasks;

namespace LibationCli
{
	[Verb("liberate", HelpText = "Liberate: book and pdf backups. Default: download and decrypt all un-liberated titles and download pdfs.\n"
		+ "Optional: specify asin(s) of book(s) to liberate.\n"
		+ "Optional: reads a license file from standard input.")]
	public class LiberateOptions : ProcessableOptionsBase
	{
		[Option(shortName: 'p', longName: "pdf", Required = false, Default = false, HelpText = "Flag to only download pdfs")]
		public bool PdfOnly { get; set; }

		[Option(shortName: 'f', longName: "force", Required = false, Default = false, HelpText = "Force the book to re-download")]
		public bool Force { get; set; }
		

		[Option(shortName: 'l', longName: "license", Required = false, Default = null, HelpText = "A license file from the get-license command. Either a file path or dash ('-') to read from standard input.")]
		public string? LicenseInput { get; set; }

		protected override async Task ProcessAsync()
		{
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
				await RunAsync(GetProcessable(), SetDownloadedStatus);
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

			SetDownloadedStatus(libraryBook);
			await ProcessOneAsync(GetProcessable(licenseInfo), libraryBook, true);
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

		private void SetDownloadedStatus(LibraryBook lb)
		{
			if (Force)
			{
				lb.Book.UserDefinedItem.BookStatus = LiberatedStatus.NotLiberated;
				lb.Book.UserDefinedItem.SetPdfStatus(LiberatedStatus.NotLiberated);
			}
		}

		private static Processable CreateBackupBook(DownloadOptions.LicenseInfo? licenseInfo)
		{
			var downloadPdf = CreateProcessable<DownloadPdf>();

			//Chain pdf download on DownloadDecryptBook.Completed
			void onDownloadDecryptBookCompleted(object? sender, LibraryBook e)
			{
				// this is fast anyway. run as sync for easy exception catching
				downloadPdf.TryProcessAsync(e).GetAwaiter().GetResult();
			}

			var downloadDecryptBook = CreateProcessable<DownloadDecryptBook>(onDownloadDecryptBookCompleted);
			downloadDecryptBook.LicenseInfo = licenseInfo;
			return downloadDecryptBook;
		}
	}
}
