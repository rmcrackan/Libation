using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DataLayer;
using Newtonsoft.Json;
using NPOI.XSSF.UserModel;

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

			var workbook = new XSSFWorkbook();
			var sheet = workbook.CreateSheet("Library");

			var detailSubtotalFont = workbook.CreateFont();
			detailSubtotalFont.IsBold = true;

			var detailSubtotalCellStyle = workbook.CreateCellStyle();
			detailSubtotalCellStyle.SetFont(detailSubtotalFont);

			// headers
			var rowIndex = 0;
			var row = sheet.CreateRow(rowIndex);

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
			var col = 0;
			foreach (var c in columns)
			{
				var cell = row.CreateCell(col++);
				var name = ExportDto.GetName(c);
				cell.SetCellValue(name);
				cell.CellStyle = detailSubtotalCellStyle;
			}

			var dateFormat = workbook.CreateDataFormat();
			var dateStyle = workbook.CreateCellStyle();
			dateStyle.DataFormat = dateFormat.GetFormat("MM/dd/yyyy HH:mm:ss");

			rowIndex++;

			// Add data rows
			foreach (var dto in dtos)
			{
				col = 0;
				row = sheet.CreateRow(rowIndex++);

				row.CreateCell(col++).SetCellValue(dto.Account);
				row.CreateCell(col++).SetCellValue(dto.DateAdded).CellStyle = dateStyle;
				row.CreateCell(col++).SetCellValue(dto.AudibleProductId);
				row.CreateCell(col++).SetCellValue(dto.Locale);
				row.CreateCell(col++).SetCellValue(dto.Title);
				row.CreateCell(col++).SetCellValue(dto.Subtitle);
				row.CreateCell(col++).SetCellValue(dto.AuthorNames);
				row.CreateCell(col++).SetCellValue(dto.NarratorNames);
				row.CreateCell(col++).SetCellValue(dto.LengthInMinutes);
				row.CreateCell(col++).SetCellValue(dto.Description);
				row.CreateCell(col++).SetCellValue(dto.Publisher);
				row.CreateCell(col++).SetCellValue(dto.HasPdf);
				row.CreateCell(col++).SetCellValue(dto.SeriesNames);
				row.CreateCell(col++).SetCellValue(dto.SeriesOrder);
				row.CreateCell(col++).SetCellValue(dto.CommunityRatingOverall);
				row.CreateCell(col++).SetCellValue(dto.CommunityRatingPerformance);
				row.CreateCell(col++).SetCellValue(dto.CommunityRatingStory);
				row.CreateCell(col++).SetCellValue(dto.PictureId);
				row.CreateCell(col++).SetCellValue(dto.IsAbridged);
				row.CreateCell(col++).SetCellValue(dto.DatePublished).CellStyle = dateStyle;
				row.CreateCell(col++).SetCellValue(dto.CategoriesNames);
				row.CreateCell(col++).SetCellValue(dto.MyRatingOverall);
				row.CreateCell(col++).SetCellValue(dto.MyRatingPerformance);
				row.CreateCell(col++).SetCellValue(dto.MyRatingStory);
				row.CreateCell(col++).SetCellValue(dto.MyLibationTags);
				row.CreateCell(col++).SetCellValue(dto.BookStatus);
				row.CreateCell(col++).SetCellValue(dto.PdfStatus);
				row.CreateCell(col++).SetCellValue(dto.ContentType);
				row.CreateCell(col++).SetCellValue(dto.Language);
				row.CreateCell(col++).SetCellValue(dto.LastDownloaded).CellStyle = dateStyle;
				row.CreateCell(col++).SetCellValue(dto.LastDownloadedVersion);
				row.CreateCell(col++).SetCellValue(dto.IsFinished);
				row.CreateCell(col++).SetCellValue(dto.IsSpatial);
				row.CreateCell(col++).SetCellValue(dto.LastDownloadedFileVersion);
				row.CreateCell(col++).SetCellValue(dto.CodecString);
				row.CreateCell(col++).SetCellValue(dto.SampleRate);
				row.CreateCell(col++).SetCellValue(dto.ChannelCount);
				row.CreateCell(col++).SetCellValue(dto.BitRate);
			}

			using var fileData = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);
			workbook.Write(fileData);
		}

		private static NPOI.SS.UserModel.ICell SetCellValue(this NPOI.SS.UserModel.ICell cell, DateTime? nullableDate)
			=> nullableDate.HasValue ? cell.SetCellValue(nullableDate.Value)
			: cell.SetCellType(NPOI.SS.UserModel.CellType.Numeric);

		private static NPOI.SS.UserModel.ICell SetCellValue(this NPOI.SS.UserModel.ICell cell, int? nullableInt)
			=> nullableInt.HasValue ? cell.SetCellValue(nullableInt.Value)
			: cell.SetCellType(NPOI.SS.UserModel.CellType.Numeric);

		private static NPOI.SS.UserModel.ICell SetCellValue(this NPOI.SS.UserModel.ICell cell, float? nullableFloat)
			=> nullableFloat.HasValue ? cell.SetCellValue(nullableFloat.Value)
			: cell.SetCellType(NPOI.SS.UserModel.CellType.Numeric);
	}
}
