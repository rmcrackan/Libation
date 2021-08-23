using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DataLayer;
using NPOI.XSSF.UserModel;
using Serilog;

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

		[Name("Authors")]
		public string AuthorNames { get; set; }

		[Name("Narrators")]
		public string NarratorNames { get; set; }

		[Name("Length In Minutes")]
		public int LengthInMinutes { get; set; }

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

		[Name("Book Liberation Status")]
		public string BookStatus { get; set; }

		[Name("PDF Liberation Status")]
		public string PdfStatus { get; set; }
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
				AuthorNames = a.Book.AuthorNames,
				NarratorNames = a.Book.NarratorNames,
				LengthInMinutes = a.Book.LengthInMinutes,
				Publisher = a.Book.Publisher,
				HasPdf = a.Book.HasPdf,
				SeriesNames = a.Book.SeriesNames,
				SeriesOrder = a.Book.SeriesLink.Any() ? a.Book.SeriesLink?.Select(sl => $"{sl.Index} : {sl.Series.Name}").Aggregate((a, b) => $"{a}, {b}") : "",
				CommunityRatingOverall = a.Book.Rating?.OverallRating,
				CommunityRatingPerformance = a.Book.Rating?.PerformanceRating,
				CommunityRatingStory = a.Book.Rating?.StoryRating,
				PictureId = a.Book.PictureId,
				IsAbridged = a.Book.IsAbridged,
				DatePublished = a.Book.DatePublished,
				CategoriesNames = a.Book.CategoriesNames.Any() ? a.Book.CategoriesNames.Aggregate((a, b) => $"{a}, {b}") : "",
				MyRatingOverall = a.Book.UserDefinedItem.Rating.OverallRating,
				MyRatingPerformance = a.Book.UserDefinedItem.Rating.PerformanceRating,
				MyRatingStory = a.Book.UserDefinedItem.Rating.StoryRating,
				MyLibationTags = a.Book.UserDefinedItem.Tags,
				BookStatus = a.Book.UserDefinedItem.BookStatus.ToString(),
				PdfStatus = a.Book.UserDefinedItem.PdfStatus.ToString()
			}).ToList();
	}
	public static class LibraryExporter
	{
		public static void ToCsv(string saveFilePath)
		{
			using var context = DbContexts.GetContext();
			var dtos = context.GetLibrary_Flat_NoTracking().ToDtos();

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
			using var context = DbContexts.GetContext();
			var dtos = context.GetLibrary_Flat_NoTracking().ToDtos();

			var json = Newtonsoft.Json.JsonConvert.SerializeObject(dtos, Newtonsoft.Json.Formatting.Indented);
			System.IO.File.WriteAllText(saveFilePath, json);
		}

		public static void ToXlsx(string saveFilePath)
		{
			using var context = DbContexts.GetContext();
			var dtos = context.GetLibrary_Flat_NoTracking().ToDtos();

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
				nameof (ExportDto.Account),
				nameof (ExportDto.DateAdded),
				nameof (ExportDto.AudibleProductId),
				nameof (ExportDto.Locale),
				nameof (ExportDto.Title),
				nameof (ExportDto.AuthorNames),
				nameof (ExportDto.NarratorNames),
				nameof (ExportDto.LengthInMinutes),
				nameof (ExportDto.Publisher),
				nameof (ExportDto.HasPdf),
				nameof (ExportDto.SeriesNames),
				nameof (ExportDto.SeriesOrder),
				nameof (ExportDto.CommunityRatingOverall),
				nameof (ExportDto.CommunityRatingPerformance),
				nameof (ExportDto.CommunityRatingStory),
				nameof (ExportDto.PictureId),
				nameof (ExportDto.IsAbridged),
				nameof (ExportDto.DatePublished),
				nameof (ExportDto.CategoriesNames),
				nameof (ExportDto.MyRatingOverall),
				nameof (ExportDto.MyRatingPerformance),
				nameof (ExportDto.MyRatingStory),
				nameof (ExportDto.MyLibationTags),
				nameof (ExportDto.BookStatus),
				nameof (ExportDto.PdfStatus)
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

				row = sheet.CreateRow(rowIndex);

				row.CreateCell(col++).SetCellValue(dto.Account);

				var dateAddedCell = row.CreateCell(col++);
				dateAddedCell.CellStyle = dateStyle;
				dateAddedCell.SetCellValue(dto.DateAdded);

				row.CreateCell(col++).SetCellValue(dto.AudibleProductId);
				row.CreateCell(col++).SetCellValue(dto.Locale);
				row.CreateCell(col++).SetCellValue(dto.Title);
				row.CreateCell(col++).SetCellValue(dto.AuthorNames);
				row.CreateCell(col++).SetCellValue(dto.NarratorNames);
				row.CreateCell(col++).SetCellValue(dto.LengthInMinutes);
				row.CreateCell(col++).SetCellValue(dto.Publisher);
				row.CreateCell(col++).SetCellValue(dto.HasPdf);
				row.CreateCell(col++).SetCellValue(dto.SeriesNames);
				row.CreateCell(col++).SetCellValue(dto.SeriesOrder);

				col = createCell(row, col, dto.CommunityRatingOverall);
				col = createCell(row, col, dto.CommunityRatingPerformance);
				col = createCell(row, col, dto.CommunityRatingStory);

				row.CreateCell(col++).SetCellValue(dto.PictureId);
				row.CreateCell(col++).SetCellValue(dto.IsAbridged);

				var datePubCell = row.CreateCell(col++);
				datePubCell.CellStyle = dateStyle;
				if (dto.DatePublished.HasValue)
					datePubCell.SetCellValue(dto.DatePublished.Value);
				else
					datePubCell.SetCellValue("");

				row.CreateCell(col++).SetCellValue(dto.CategoriesNames);

				col = createCell(row, col, dto.MyRatingOverall);
				col = createCell(row, col, dto.MyRatingPerformance);
				col = createCell(row, col, dto.MyRatingStory);

				row.CreateCell(col++).SetCellValue(dto.MyLibationTags);
				row.CreateCell(col++).SetCellValue(dto.BookStatus);
				row.CreateCell(col++).SetCellValue(dto.PdfStatus);

				rowIndex++;
			}

			using var fileData = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);
			workbook.Write(fileData);
		}
		private static int createCell(NPOI.SS.UserModel.IRow row, int col, float? nullableFloat)
		{
			if (nullableFloat.HasValue)
				row.CreateCell(col++).SetCellValue(nullableFloat.Value);
			else
				row.CreateCell(col++).SetCellValue("");
			return col;
		}
	}
}
