using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DataLayer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ApplicationServices
{
	public class ExportDto
	{
		public static string GetName(string fieldName)
		{
			var property = typeof(ExportDto).GetProperty(fieldName);
			var attribute = property.GetCustomAttributes(typeof(NameAttribute), true)[0];
			var description = (NameAttribute)attribute;
			var text = description.Names;
			return text[0];
		}

		[Name("Account")]
		public string Account { get; set; }

		[Name("Date Added to library")]
		public DateTime DateAdded { get; set; }

		[Name("Audible Product Id")]
		public string AudibleProductId { get; set; }

		[Name("Locale")]
		public string Locale { get; set; }

		[Name("Title")]
		public string Title { get; set; }

		[Name("Subtitle")]
		public string Subtitle { get; set; }

		[Name("Authors")]
		public string AuthorNames { get; set; }

		[Name("Narrators")]
		public string NarratorNames { get; set; }

		[Name("Length In Minutes")]
		public int LengthInMinutes { get; set; }

		[Name("Description")]
		public string Description { get; set; }

		[Name("Publisher")]
		public string Publisher { get; set; }

		[Name("Has PDF")]
		public bool HasPdf { get; set; }

		[Name("Series Names")]
		public string SeriesNames { get; set; }

		[Name("Series Order")]
		public string SeriesOrder { get; set; }

		[Name("Community Rating: Overall")]
		public float? CommunityRatingOverall { get; set; }

		[Name("Community Rating: Performance")]
		public float? CommunityRatingPerformance { get; set; }

		[Name("Community Rating: Story")]
		public float? CommunityRatingStory { get; set; }

		[Name("Cover Id")]
		public string PictureId { get; set; }

		[Name("Is Abridged?")]
		public bool IsAbridged { get; set; }

		[Name("Date Published")]
		public DateTime? DatePublished { get; set; }

		[Name("Categories")]
		public string CategoriesNames { get; set; }

		[Name("My Rating: Overall")]
		public float? MyRatingOverall { get; set; }

		[Name("My Rating: Performance")]
		public float? MyRatingPerformance { get; set; }

		[Name("My Rating: Story")]
		public float? MyRatingStory { get; set; }

		[Name("My Libation Tags")]
		public string MyLibationTags { get; set; }

		[Name("Book Liberated Status")]
		public string BookStatus { get; set; }

		[Name("PDF Liberated Status")]
		public string PdfStatus { get; set; }

		[Name("Content Type")]
		public string ContentType { get; set; }

		[Name("Language")]
        public string Language { get; set; }

		[Name("LastDownloaded")]
		public DateTime? LastDownloaded { get; set; }

		[Name("LastDownloadedVersion")]
        public string LastDownloadedVersion { get; set; }

		[Name("IsFinished")]
        public bool IsFinished { get; set; }

		[Name("IsSpatial")]
		public bool IsSpatial { get; set; }

		[Name("Last Downloaded File Version")]
		public string LastDownloadedFileVersion { get; set; }

		[Ignore /* csv ignore */]
		public AudioFormat LastDownloadedFormat { get; set; }

		[Name("Last Downloaded Codec"), JsonIgnore]
		public string CodecString => LastDownloadedFormat?.CodecString ?? "";

		[Name("Last Downloaded Sample rate"), JsonIgnore]
		public int? SampleRate => LastDownloadedFormat?.SampleRate;

		[Name("Last Downloaded Audio Channels"), JsonIgnore]
		public int? ChannelCount => LastDownloadedFormat?.ChannelCount;

		[Name("Last Downloaded Bitrate"), JsonIgnore]
		public int? BitRate => LastDownloadedFormat?.BitRate;
	}

	public static class LibToDtos
	{
		public static List<ExportDto> ToDtos(this IEnumerable<LibraryBook> library)
			=> library.Select(a => new ExportDto
			{
				Account = a.Account,
				DateAdded = a.DateAdded,
				AudibleProductId = a.Book.AudibleProductId,
				Locale = a.Book.Locale,
				Title = a.Book.Title,
				Subtitle = a.Book.Subtitle,
				AuthorNames = a.Book.AuthorNames(),
				NarratorNames = a.Book.NarratorNames(),
				LengthInMinutes = a.Book.LengthInMinutes,
				Description = a.Book.Description,
				Publisher = a.Book.Publisher,
				HasPdf = a.Book.HasPdf(),
				SeriesNames = a.Book.SeriesNames(),
				SeriesOrder = a.Book.SeriesLink.Any() ? a.Book.SeriesLink?.Select(sl => $"{sl.Order} : {sl.Series.Name}").Aggregate((a, b) => $"{a}, {b}") : "",
				CommunityRatingOverall = a.Book.Rating?.OverallRating.ZeroIsNull(),
				CommunityRatingPerformance = a.Book.Rating?.PerformanceRating.ZeroIsNull(),
				CommunityRatingStory = a.Book.Rating?.StoryRating.ZeroIsNull(),
				PictureId = a.Book.PictureId,
				IsAbridged = a.Book.IsAbridged,
				DatePublished = a.Book.DatePublished,
				CategoriesNames = string.Join("; ", a.Book.LowestCategoryNames()),
				MyRatingOverall = a.Book.UserDefinedItem.Rating.OverallRating.ZeroIsNull(),
				MyRatingPerformance = a.Book.UserDefinedItem.Rating.PerformanceRating.ZeroIsNull(),
				MyRatingStory = a.Book.UserDefinedItem.Rating.StoryRating.ZeroIsNull(),
				MyLibationTags = a.Book.UserDefinedItem.Tags,
				BookStatus = a.Book.UserDefinedItem.BookStatus.ToString(),
				PdfStatus = a.Book.UserDefinedItem.PdfStatus.ToString(),
				ContentType = a.Book.ContentType.ToString(),
				Language = a.Book.Language,
				LastDownloaded = a.Book.UserDefinedItem.LastDownloaded,
				LastDownloadedVersion = a.Book.UserDefinedItem.LastDownloadedVersion?.ToString() ?? "",
				IsFinished = a.Book.UserDefinedItem.IsFinished,
				IsSpatial = a.Book.IsSpatial,
				LastDownloadedFileVersion = a.Book.UserDefinedItem.LastDownloadedFileVersion ?? "",
				LastDownloadedFormat = a.Book.UserDefinedItem.LastDownloadedFormat
			}).ToList();

		private static float? ZeroIsNull(this float value) => value is 0 ? null : value;
	}
	public static class LibraryExporter
	{
		public static void ToCsv(string saveFilePath)
		{
			var dtos = DbContexts.GetLibrary_Flat_NoTracking().ToDtos();
			if (!dtos.Any())
				return;
			using var writer = new System.IO.StreamWriter(saveFilePath);
			using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture);

			csv.WriteHeader(typeof(ExportDto));
			csv.NextRecord();
			csv.WriteRecords(dtos);
		}

		public static void ToJson(string saveFilePath)
		{
			var dtos = DbContexts.GetLibrary_Flat_NoTracking().ToDtos();
			var json = JsonConvert.SerializeObject(dtos, Formatting.Indented);
			System.IO.File.WriteAllText(saveFilePath, json);
		}

		public static void ToXlsx(string saveFilePath)
		{
			var dtos = DbContexts.GetLibrary_Flat_NoTracking().ToDtos();

			using var workbook = new XLWorkbook();
			var sheet = workbook.AddWorksheet("Library");


			// headers
			var columns = new[] {
				nameof(ExportDto.Account),
				nameof(ExportDto.DateAdded),
				nameof(ExportDto.AudibleProductId),
				nameof(ExportDto.Locale),
				nameof(ExportDto.Title),
				nameof(ExportDto.Subtitle),
				nameof(ExportDto.AuthorNames),
				nameof(ExportDto.NarratorNames),
				nameof(ExportDto.LengthInMinutes),
				nameof(ExportDto.Description),
				nameof(ExportDto.Publisher),
				nameof(ExportDto.HasPdf),
				nameof(ExportDto.SeriesNames),
				nameof(ExportDto.SeriesOrder),
				nameof(ExportDto.CommunityRatingOverall),
				nameof(ExportDto.CommunityRatingPerformance),
				nameof(ExportDto.CommunityRatingStory),
				nameof(ExportDto.PictureId),
				nameof(ExportDto.IsAbridged),
				nameof(ExportDto.DatePublished),
				nameof(ExportDto.CategoriesNames),
				nameof(ExportDto.MyRatingOverall),
				nameof(ExportDto.MyRatingPerformance),
				nameof(ExportDto.MyRatingStory),
				nameof(ExportDto.MyLibationTags),
				nameof(ExportDto.BookStatus),
				nameof(ExportDto.PdfStatus),
				nameof(ExportDto.ContentType),
                nameof(ExportDto.Language),
                nameof(ExportDto.LastDownloaded),
                nameof(ExportDto.LastDownloadedVersion),
                nameof(ExportDto.IsFinished),
                nameof(ExportDto.IsSpatial),
                nameof(ExportDto.LastDownloadedFileVersion),
                nameof(ExportDto.CodecString),
                nameof(ExportDto.SampleRate),
                nameof(ExportDto.ChannelCount),
                nameof(ExportDto.BitRate)
            };

			int rowIndex = 1, col = 1;
			var headerRow = sheet.Row(rowIndex++);
			foreach (var c in columns)
			{
				var headerCell = headerRow.Cell(col++);
				headerCell.Value = ExportDto.GetName(c);
				headerCell.Style.Font.Bold = true;
			}

			var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " HH:mm:ss";

			// Add data rows
			foreach (var dto in dtos)
			{
				col = 1;
				var row = sheet.Row(rowIndex++);

				row.Cell(col++).Value = dto.Account;
				row.Cell(col++).SetDate(dto.DateAdded, dateFormat);
				row.Cell(col++).Value = dto.AudibleProductId;
				row.Cell(col++).Value = dto.Locale;
				row.Cell(col++).Value = dto.Title;
				row.Cell(col++).Value = dto.Subtitle;
				row.Cell(col++).Value = dto.AuthorNames;
				row.Cell(col++).Value = dto.NarratorNames;
				row.Cell(col++).Value = dto.LengthInMinutes;
				row.Cell(col++).Value = dto.Description;
				row.Cell(col++).Value = dto.Publisher;
				row.Cell(col++).Value = dto.HasPdf;
				row.Cell(col++).Value = dto.SeriesNames;
				row.Cell(col++).Value = dto.SeriesOrder;
				row.Cell(col++).Value = dto.CommunityRatingOverall;
				row.Cell(col++).Value = dto.CommunityRatingPerformance;
				row.Cell(col++).Value = dto.CommunityRatingStory;
				row.Cell(col++).Value = dto.PictureId;
				row.Cell(col++).Value = dto.IsAbridged;
				row.Cell(col++).SetDate(dto.DatePublished, dateFormat);
				row.Cell(col++).Value = dto.CategoriesNames;
				row.Cell(col++).Value = dto.MyRatingOverall;
				row.Cell(col++).Value = dto.MyRatingPerformance;
				row.Cell(col++).Value = dto.MyRatingStory;
				row.Cell(col++).Value = dto.MyLibationTags;
				row.Cell(col++).Value = dto.BookStatus;
				row.Cell(col++).Value = dto.PdfStatus;
				row.Cell(col++).Value = dto.ContentType;
				row.Cell(col++).Value = dto.Language;
				row.Cell(col++).SetDate(dto.LastDownloaded, dateFormat);
				row.Cell(col++).Value = dto.LastDownloadedVersion;
				row.Cell(col++).Value = dto.IsFinished;
				row.Cell(col++).Value = dto.IsSpatial;
				row.Cell(col++).Value = dto.LastDownloadedFileVersion;
				row.Cell(col++).Value = dto.CodecString;
				row.Cell(col++).Value = dto.SampleRate;
				row.Cell(col++).Value = dto.ChannelCount;
				row.Cell(col++).Value = dto.BitRate;
			}

			workbook.SaveAs(saveFilePath);
		}

		private static void SetDate(this IXLCell cell, DateTime? value, string dateFormat)
		{
			cell.Value = value;
			cell.Style.DateFormat.Format = dateFormat;
		}
	}
}
