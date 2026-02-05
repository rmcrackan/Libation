using ApplicationServices;
using CommandLine;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Threading.Tasks;

namespace LibationCli.Options;

[Verb("get-license", HelpText = "Get the license information for a book.")]
internal class GetLicenseOptions : OptionsBase
{

	[Value(0, MetaName = "[asin]", HelpText = "Product ID of book to request license for.", Required = true)]
	public string? Asin { get; set; }
	protected override async Task ProcessAsync()
	{
		if (string.IsNullOrWhiteSpace(Asin))
		{
			Console.Error.WriteLine("ASIN is required.");
			return;
		}

		if (DbContexts.GetLibraryBook_Flat_NoTracking(Asin) is not LibraryBook libraryBook)
		{
			Console.Error.WriteLine($"Book not found with asin={Asin}");
			return;
		}

		var api = await libraryBook.GetApiAsync();
		var license = await DownloadOptions.GetDownloadLicenseAsync(api, libraryBook, Configuration.Instance, default);

		var jsonSettings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			Converters = [new StringEnumConverter(), new ByteArrayHexConverter()]
		};

		var licenseJson = JsonConvert.SerializeObject(license, Formatting.Indented, jsonSettings);
		Console.WriteLine(licenseJson);
	}
}

class ByteArrayHexConverter : JsonConverter
{
	public override bool CanConvert(Type objectType) => objectType == typeof(byte[]);

	public override bool CanRead => false;
	public override bool CanWrite => true;

	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		=> throw new NotSupportedException();

	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value is byte[] array)
		{
			writer.WriteValue(Convert.ToHexStringLower(array));
		}
	}
}
