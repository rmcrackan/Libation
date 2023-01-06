using AudibleApi.Common;
using CsvHelper;
using DataLayer;
using Newtonsoft.Json.Linq;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApplicationServices
{
	public static class RecordExporter
	{
		public static void ToXlsx(string saveFilePath, IEnumerable<IRecord> records)
		{
			if (!records.Any())
				return;

			using var workbook = new XSSFWorkbook();
			var sheet = workbook.CreateSheet("Records");

			var detailSubtotalFont = workbook.CreateFont();
			detailSubtotalFont.IsBold = true;

			var detailSubtotalCellStyle = workbook.CreateCellStyle();
			detailSubtotalCellStyle.SetFont(detailSubtotalFont);

			// headers
			var rowIndex = 0;
			var row = sheet.CreateRow(rowIndex);

			var columns = new List<string>
			{
				nameof(IRecord.RecordType),
				nameof(IRecord.Created),
				nameof(IRecord.Start) + "_ms",
			};

			if (records.OfType<IAnnotation>().Any())
			{
				columns.Add(nameof(IAnnotation.AnnotationId));
				columns.Add(nameof(IAnnotation.LastModified));
			}
			if (records.OfType<IRangeAnnotation>().Any())
			{
				columns.Add(nameof(IRangeAnnotation.End) + "_ms");
				columns.Add(nameof(IRangeAnnotation.Text));
			}
			if (records.OfType<Clip>().Any())
				columns.Add(nameof(Clip.Title));

			var col = 0;
			foreach (var c in columns)
			{
				var cell = row.CreateCell(col++);
				cell.SetCellValue(c);
				cell.CellStyle = detailSubtotalCellStyle;
			}

			var dateFormat = workbook.CreateDataFormat();
			var dateStyle = workbook.CreateCellStyle();
			dateStyle.DataFormat = dateFormat.GetFormat("MM/dd/yyyy HH:mm:ss");

			// Add data rows
			foreach (var record in records)
			{
				col = 0;

				row = sheet.CreateRow(++rowIndex);

				row.CreateCell(col++).SetCellValue(record.RecordType);

				var dateCreatedCell = row.CreateCell(col++);
				dateCreatedCell.CellStyle = dateStyle;
				dateCreatedCell.SetCellValue(record.Created.DateTime);

				row.CreateCell(col++).SetCellValue(record.Start.TotalMilliseconds);

				if (record is IAnnotation annotation)
				{
					row.CreateCell(col++).SetCellValue(annotation.AnnotationId);

					var lastModifiedCell = row.CreateCell(col++);
					lastModifiedCell.CellStyle = dateStyle;
					lastModifiedCell.SetCellValue(annotation.LastModified.DateTime);

					if (annotation is IRangeAnnotation rangeAnnotation)
					{
						row.CreateCell(col++).SetCellValue(rangeAnnotation.End.TotalMilliseconds);
						row.CreateCell(col++).SetCellValue(rangeAnnotation.Text);

						if (rangeAnnotation is Clip clip)
							row.CreateCell(col++).SetCellValue(clip.Title);
					}
				}
			}

			using var fileData = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);
			workbook.Write(fileData);
		}

		public static void ToJson(string saveFilePath, LibraryBook libraryBook, IEnumerable<IRecord> records)
		{
			if (!records.Any())
				return;

			var recordsObj = new JObject
			{
				{ "title", libraryBook.Book.Title},
				{ "asin", libraryBook.Book.AudibleProductId},
				{ "exportTime", DateTime.Now},
				{ "records", JArray.FromObject(records) }
			};

			System.IO.File.WriteAllText(saveFilePath, recordsObj.ToString(Newtonsoft.Json.Formatting.Indented));
		}

		public static void ToCsv(string saveFilePath, IEnumerable<IRecord> records)
		{
			if (!records.Any())
				return;

			using var writer = new System.IO.StreamWriter(saveFilePath);
			using var csv = new CsvWriter(writer, System.Globalization.CultureInfo.CurrentCulture);

			//Write headers for the present type that has the most properties
			if (records.OfType<Clip>().Any())
				csv.WriteHeader(typeof(Clip));
			else if (records.OfType<IRangeAnnotation>().Any())
				csv.WriteHeader(typeof(IRangeAnnotation));
			else if (records.OfType<IAnnotation>().Any())
				csv.WriteHeader(typeof(IAnnotation));
			else
				csv.WriteHeader(typeof(IRecord));

			csv.NextRecord();
			csv.WriteRecords(records.OfType<Clip>());
			csv.WriteRecords(records.OfType<Note>());
			csv.WriteRecords(records.OfType<Bookmark>());
			csv.WriteRecords(records.OfType<LastHeard>());
		}
	}
}
