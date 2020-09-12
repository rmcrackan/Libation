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

		[Name("Pdf url")]
		public string PdfUrl { get; set; }
		
		[Name("Series Names")]
		public string SeriesNames{ get; set; }

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
				PdfUrl = a.Book.Supplements?.FirstOrDefault()?.Url,
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
				MyLibationTags = a.Book.UserDefinedItem.Tags
			}).ToList();
	}
	public static class LibraryExporter
	{
		private static List<acct> GetAccts()
		{
			var ia = true;
			var userAccounts = new List<acct>();
			for (var i = 0; i < 7; i++)
				userAccounts.Add(new acct { UserName = "u" + i, Email = "e" + i, CreationDate = DateTime.Now.AddDays(-i * 2), LastLoginDate = DateTime.Now.AddDays(-i), IsApproved = (ia = !ia), Comment = "c [ ] * % , ' \" \\ \n " + i });

			return userAccounts;
		}

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

			var library = context.GetLibrary_Flat_NoTracking();
		}

		public static void TEST_ToXlsx(string saveFilePath)
		{
			// https://steemit.com/utopian-io/@haig/how-to-create-excel-spreadsheets-using-npoi

			var workbook = new XSSFWorkbook();
			var sheet = workbook.CreateSheet("Library");

			var detailSubtotalFont = workbook.CreateFont();
			detailSubtotalFont.IsBold = true;

			var detailSubtotalCellStyle = workbook.CreateCellStyle();
			detailSubtotalCellStyle.SetFont(detailSubtotalFont);

			// headers
			var rowIndex = 0;
			var row = sheet.CreateRow(rowIndex);

			{
				var cell = row.CreateCell(0);
				cell.SetCellValue("Username");
				cell.CellStyle = detailSubtotalCellStyle;
			}
			{
				var cell = row.CreateCell(1);
				cell.SetCellValue("Email");
				cell.CellStyle = detailSubtotalCellStyle;
			}
			{
				var cell = row.CreateCell(2);
				cell.SetCellValue("Joined");
				cell.CellStyle = detailSubtotalCellStyle;
			}
			{
				var cell = row.CreateCell(3);
				cell.SetCellValue("Last Login");
				cell.CellStyle = detailSubtotalCellStyle;
			}
			{
				var cell = row.CreateCell(4);
				cell.SetCellValue("Approved?");
				cell.CellStyle = detailSubtotalCellStyle;
			}
			{
				var cell = row.CreateCell(5);
				cell.SetCellValue("Comments");
				cell.CellStyle = detailSubtotalCellStyle;
			}

			rowIndex++;

			// Add data rows
			foreach (acct account in GetAccts())
			{
				row = sheet.CreateRow(rowIndex);
				row.CreateCell(0).SetCellValue(account.UserName);
				row.CreateCell(1).SetCellValue(account.Email);
				row.CreateCell(2).SetCellValue(account.CreationDate.ToShortDateString());
				row.CreateCell(3).SetCellValue(account.LastLoginDate.ToShortDateString());
				row.CreateCell(4).SetCellValue(account.IsApproved);
				row.CreateCell(5).SetCellValue(account.Comment);
				rowIndex++;
			}

			using var fileData = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);
			workbook.Write(fileData);
		}
		class acct
		{
			public string UserName { get; set; }
			public string Email { get; set; }
			public DateTime CreationDate { get; set; }
			public DateTime LastLoginDate { get; set; }
			public bool IsApproved { get; set; }
			public string Comment { get; set; }
		}
	}
}
