using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using DataLayer;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ApplicationServices;

public static class LibraryExporter
{
	public static void ToCsv(string saveFilePath, IEnumerable<LibraryBook>? libraryBooks = null)
	{
		libraryBooks ??= DbContexts.GetLibrary_Flat_NoTracking();
		var dtos = libraryBooks.ToDtos();
		if (dtos.Count == 0)
			return;

		using var csv = new CsvWriter(new System.IO.StreamWriter(saveFilePath), CultureInfo.CurrentCulture);
		csv.WriteHeader(typeof(ExportDto));
		csv.NextRecord();
		csv.WriteRecords(dtos);
	}

	public static void ToJson(string saveFilePath, IEnumerable<LibraryBook>? libraryBooks = null)
	{
		libraryBooks ??= DbContexts.GetLibrary_Flat_NoTracking();
		var dtos = libraryBooks.ToDtos();
		var serializer = new JsonSerializer();
		using var writer = new JsonTextWriter(new System.IO.StreamWriter(saveFilePath)) { Formatting = Formatting.Indented };
		serializer.Serialize(writer, dtos);
	}

	public static void ToXlsx(string saveFilePath, IEnumerable<LibraryBook>? libraryBooks = null)
	{
		libraryBooks ??= DbContexts.GetLibrary_Flat_NoTracking();
		var dtos = libraryBooks.ToDtos();

		using var workbook = new XLWorkbook();
		var sheet = workbook.AddWorksheet("Library");
		var columns = typeof(ExportDto).GetProperties().Where(p => p.GetCustomAttribute<NameAttribute>() is not null).ToArray();

		// headers
		var currentRow = sheet.FirstRow();
		var currentCell = currentRow.FirstCell();
		foreach (var column in columns)
		{
			currentCell.Value = GetColumnName(column);
			currentCell.Style.Font.Bold = true;
			currentCell = currentCell.CellRight();
		}

		var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " HH:mm:ss";

		// Add data rows
		foreach (var dto in dtos)
		{
			currentRow = currentRow.RowBelow();
			currentCell = currentRow.FirstCell();

			foreach (var column in columns)
			{
				var value = column.GetValue(dto);
				currentCell.Value = XLCellValue.FromObject(value);
				currentCell.Style.DateFormat.Format = currentCell.DataType is XLDataType.DateTime ? dateFormat : string.Empty;
				currentCell = currentCell.CellRight();
			}
		}

		workbook.SaveAs(saveFilePath);
	}

	private static List<ExportDto> ToDtos(this IEnumerable<LibraryBook> library)
		=> library.Select(a => new ExportDto(a)).ToList();

	private static string GetColumnName(PropertyInfo property)
		=> property.GetCustomAttribute<NameAttribute>()?.Names?.FirstOrDefault() ?? property.Name;
}
