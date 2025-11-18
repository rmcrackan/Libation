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

#nullable enable
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

		protected override async Task ProcessAsync()
		{
			if (AudibleFileStorage.BooksDirectory is null)
			{
				Console.Error.WriteLine("Error: Books directory is not set. Please configure the 'Books' setting in Settings.json.");
				return;
			}

			if (Console.IsInputRedirected)
			{
				Console.WriteLine("Reading license file from standard input.");
				using var reader = new StreamReader(Console.OpenStandardInput());
				var stdIn = await reader.ReadToEndAsync();
				try
				{

					var jsonSettings = new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Ignore,
						Converters = [new StringEnumConverter(), new ByteArrayHexConverter()]
					};
					var licenseInfo = JsonConvert.DeserializeObject<DownloadOptions.LicenseInfo>(stdIn, jsonSettings);

					if (licenseInfo?.ContentMetadata?.ContentReference?.Asin is not string asin)
					{
						Console.Error.WriteLine("Error: License file is missing ASIN information.");
						return;
					}

					LibraryBook libraryBook;
					using (var dbContext = DbContexts.GetContext())
					{
						if (dbContext.GetLibraryBook_Flat_NoTracking(asin) is not LibraryBook lb)
						{
							Console.Error.WriteLine($"Book not found with asin={asin}");
							return;
						}
						libraryBook = lb;
					}

					SetDownloadedStatus(libraryBook);
					await ProcessOneAsync(GetProcessable(licenseInfo), libraryBook, true);
				}
				catch
				{
					Console.Error.WriteLine("Error: Failed to read license file from standard input. Please ensure the input is a valid license file in JSON format.");
				}
			}
			else
			{
				await RunAsync(GetProcessable(), SetDownloadedStatus);
			}
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
